using Compos.Coreforce.Authorization;
using Compos.Coreforce.ContractResolver;
using Compos.Coreforce.Extensions;
using Compos.Coreforce.Models;
using Compos.Coreforce.Models.Authorization;
using Compos.Coreforce.Models.Collections;
using Compos.Coreforce.Models.Configuration;
using Compos.Coreforce.Models.Soql;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Compos.Coreforce
{
    public class SalesforceRepository<T> : ISalesforceRepository<T>
    {
        public ISalesforceOpenAuthorization SalesforceOpenAuthorization { get; set; }

        public SalesforceRepository(
            ISalesforceOpenAuthorization salesforceOpenAuthorization
            )
        {
            SalesforceOpenAuthorization = salesforceOpenAuthorization;

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new Newtonsoft.Json.Converters.IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd" } }
            };
        }

        #region Get

        public async Task<List<T>> GetAsync()
        {
            return await ManageGetThreads(null, null);
        }

        public async Task<List<T>> GetAsync(params Expression<Func<T, object>>[] selectItems)
        {
            return await ManageGetThreads(null, selectItems);
        }

        public async Task<List<T>> GetAsync(FilterItemCollection<T> filterCollection)
        {
            return await ManageGetThreads(filterCollection, null);
        }

        public async Task<List<T>> GetAsync(FilterItemCollection<T> filterCollection, params Expression<Func<T, object>>[] selectItems)
        {
            return await ManageGetThreads(filterCollection, selectItems);
        }

        private async Task<List<T>> ManageGetThreads(
            FilterItemCollection<T> filterCollection,
            Expression<Func<T, object>>[] selectItems,
            string statement = ""
            )
        {
            var authorizationResult = await SalesforceOpenAuthorization.Authorize();

            if (authorizationResult == null)
                throw new NullReferenceException("Authorization result is null.");

            var selectUrl = string.IsNullOrEmpty(statement) ? 
                BuildSelectUrl(authorizationResult, filterCollection, selectItems) :
                $"{authorizationResult.InstanceUrl}/services/data/{CoreforceConfiguration.ApiVersion}/query/?q={statement}";

            var result = await SendSelectStatementAsync(authorizationResult, selectUrl);

            if (result is null)
                throw new NullReferenceException("Response object is null.");

            if (result.Done)
                return result.Records;

            int numOfThreads = Convert.ToInt32(Math.Floor((double)result.TotalSize / 2000));
            var selectResultCollection = new Task<SelectResult<T>>[numOfThreads];

            for (int i = 1; i <= numOfThreads; i++)
            {
                var url = $"{authorizationResult.InstanceUrl}{result.NextRecordsUrl.Substring(0, result.NextRecordsUrl.Length - 4)}{i * 2000}";
                selectResultCollection[i - 1] = Task.Run(() => SendSelectStatementAsync(authorizationResult, url));
            }

            await Task.WhenAll(selectResultCollection);
            var resultList = new List<T>();
            resultList.AddRange(result.Records);

            foreach (var taskResult in selectResultCollection)
            {
                resultList.AddRange(taskResult.Result.Records);
            }

            return resultList;
        }

        private async Task<SelectResult<T>> SendSelectStatementAsync(
            AuthorizationResult authorizationResult,
            string url
            )
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authorizationResult.AccessToken);

                    using (var message =
                                await client.GetAsync(url))
                    {
                        var responseString = await message.Content.ReadAsStringAsync();
                        var selectResult = JsonConvert.DeserializeObject<SelectResult<T>>(responseString);

                        if (selectResult?.Records != null && selectResult.Records.Count > 0)
                            return selectResult;
                    }
                }
            }
            catch (Exception exception)
            {
                throw new ApplicationException($"Error while send select statement for object {typeof(T).Name} to salesforce. Please check the inner exception.", exception);
            }

            return null;
        }

        private string BuildSelectUrl(
            AuthorizationResult authorizationResult,
            FilterItemCollection<T> filterCollection,
            Expression<Func<T, object>>[] selectItems
            )
        {
            List<MemberExpression> memberExpressions = new List<MemberExpression>();
            string requestUrl = $"{authorizationResult.InstanceUrl}/services/data/{CoreforceConfiguration.ApiVersion}/query/?q=Select+";
            var properties = typeof(T).GetProperties().Where(x => !x.CustomAttributes.Any(y =>
                y.AttributeType == typeof(JsonIgnoreAttribute))
                ).ToList();

            bool firstElement = true;

            if (selectItems != null && selectItems.Length > 0)
            {
                foreach (var expression in selectItems)
                {
                    memberExpressions.Add(expression.GetMemberExpression());
                }
            }

            foreach (PropertyInfo property in properties)
            {
                if (memberExpressions.Any(x => x.Member.Name == property.Name) || memberExpressions.Count == 0)
                {
                    if (firstElement)
                    {
                        requestUrl = $"{requestUrl}{property.Name}";
                        firstElement = false;
                    }

                    else
                        requestUrl = $"{requestUrl}+,+{property.Name}";
                }
            }

            requestUrl = $"{requestUrl}+from+{typeof(T).Name}";

            if (filterCollection is null || filterCollection.Collection is null || filterCollection.Collection.Count == 0)
                return requestUrl;

            requestUrl = $"{requestUrl}+where{filterCollection.BuildStatement()}";

            return requestUrl;
        }

        public async Task<T> GetByIdAsync(string id)
        {
            var authorizationResult = await SalesforceOpenAuthorization.Authorize();

            if (authorizationResult == null)
                throw new NullReferenceException("Authorization result is null.");

            string salesForceUrl = $"{authorizationResult.InstanceUrl}/services/data/{CoreforceConfiguration.ApiVersion}/sobjects/{typeof(T).Name}/{id}";
            T salesForceObject = default(T);

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authorizationResult.AccessToken);

                    using (var message =
                                await client.GetAsync(salesForceUrl))
                    {
                        var responseString = await message.Content.ReadAsStringAsync();
                        salesForceObject = JsonConvert.DeserializeObject<T>(responseString);
                    }
                }
            }
            catch (Exception exception)
            {
                throw new ApplicationException($"Error while asking for {typeof(T).Name} object with id {id}. Please check the inner exception.", exception);
            }

            return salesForceObject;
        }

        #endregion

        #region Insert

        public async Task<InsertResult> InsertAsync(T obj)
        {
            var authorizationResult = await SalesforceOpenAuthorization.Authorize();

            if (authorizationResult == null)
                throw new NullReferenceException("Authorization result is null.");

            string salesForceUrl = $"{authorizationResult.InstanceUrl}/services/data/{CoreforceConfiguration.ApiVersion}/sobjects/{typeof(T).Name}";
            var insertResponse = new InsertResult();

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authorizationResult.AccessToken);

                    using (var message =
                                await client.PostAsync(salesForceUrl, new StringContent(JsonConvert.SerializeObject(obj,
                                    new JsonSerializerSettings { ContractResolver = new ImportContractResolver() }), Encoding.UTF8, "application/json")))
                    {
                        var responseString = await message.Content.ReadAsStringAsync();
                        insertResponse = JsonConvert.DeserializeObject<InsertResult>(responseString);

                        if (!string.IsNullOrEmpty(insertResponse.Id))
                        {
                            var id = typeof(T).GetProperties().SingleOrDefault(x => x.Name.ToLower().Equals("id"));

                            if (id != null)
                                id.SetValue(obj, insertResponse.Id);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                throw new ApplicationException($"Error while inserting the object {JsonConvert.SerializeObject(obj)}. Please check the inner exception.", exception);
            }

            return insertResponse;
        }

        public async Task<SObjectTreeResult> InsertAsync<U>(List<U> objList, bool stopByException = true, bool parallelProcessing = false)
            where U : InsertAttributes, T, new()
        {
            return await ManageInsertThreads(objList, stopByException, parallelProcessing);
        }

        private async Task<SObjectTreeResult> ManageInsertThreads<U>(List<U> objList, bool stopByException, bool parallelProcessing)
            where U : InsertAttributes, new()
        {
            var authorizationResult = await this.SalesforceOpenAuthorization.Authorize();

            if (authorizationResult == null)
                throw new ApplicationException("Authorization result is null.");

            int parallelThreads = parallelProcessing ? CoreforceConfiguration.MaxNumberOfParallelRunningTasks : 1;

            // Calculate threads
            int numOfThreads = Convert.ToInt32(Math.Ceiling((double)objList.Count / 200));
            int numOfThreadCollections = Convert.ToInt32(Math.Ceiling((double)numOfThreads / parallelThreads));
            SObjectTreeResult sObjectTreeResponse = new SObjectTreeResult();

            RecordCollection<U>[] importObjectCollection = new RecordCollection<U>[numOfThreads];
            Task<SObjectTreeResult>[] importResultCollections;

            // Split import objects into smaller record collections
            try
            {
                for (int threadCollectionCounter = 0; threadCollectionCounter < numOfThreadCollections; threadCollectionCounter++)
                {
                    for (int i = threadCollectionCounter * parallelThreads, threadCounter = 0; i < threadCollectionCounter * parallelThreads + parallelThreads && i < numOfThreads; i++, threadCounter++)
                    {
                        importObjectCollection[i] = new RecordCollection<U>();

                        for (int j = i * 200; (j < i * 200 + 200) && (j < objList.Count); j++)
                        {
                            importObjectCollection[i].Records.Add(objList[j]);
                        }
                    }
                }

                // Run threads
                for (int threadCollectionCounter = 0; threadCollectionCounter < numOfThreadCollections; threadCollectionCounter++)
                {
                    authorizationResult = await this.SalesforceOpenAuthorization.Authorize();

                    if (threadCollectionCounter + 1 < numOfThreadCollections)
                        importResultCollections = new Task<SObjectTreeResult>[parallelThreads];

                    else
                        importResultCollections = new Task<SObjectTreeResult>[numOfThreads - (threadCollectionCounter * parallelThreads)];

                    for (int i = threadCollectionCounter * parallelThreads, threadCounter = 0; i < threadCollectionCounter * parallelThreads + parallelThreads && i < numOfThreads; i++, threadCounter++)
                    {
                        var localThreadCounter = i;
                        importResultCollections[threadCounter] = Task.Run(() => SendImportAsync(importObjectCollection[localThreadCounter], authorizationResult, stopByException));

                        // There should be only max. 10 parallely threads
                        if (i + 1 >= threadCollectionCounter * parallelThreads + parallelThreads || i + 1 >= numOfThreads)
                        {
                            await Task.WhenAll(importResultCollections);

                            foreach (Task<SObjectTreeResult> result in importResultCollections)
                            {
                                sObjectTreeResponse.HasErrors = sObjectTreeResponse.HasErrors ? true : result.Result.HasErrors;
                                sObjectTreeResponse.Results.AddRange(result.Result.Results);
                            }

                            authorizationResult = await this.SalesforceOpenAuthorization.Authorize();

                            if (authorizationResult == null)
                            {
                                throw new ApplicationException("Authorization result is null.");
                            }
                        }
                    }
                }

                if (sObjectTreeResponse != null && sObjectTreeResponse.Results != null && sObjectTreeResponse.Results.Count > 0)
                    AddIdToList(objList, sObjectTreeResponse);
            }
            catch (Exception)
            {
                throw; // Please add exception message
            }

            return sObjectTreeResponse;
        }

        private async Task<SObjectTreeResult> SendImportAsync<U>(RecordCollection<U> recordCollection, AuthorizationResult authorizationResult, bool stopByException)
            where U : InsertAttributes, new()
        {
            var importResult = new SObjectTreeResult();
            bool importError = false;

            do
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authorizationResult.AccessToken);

                    using (var message =
                                await client.PostAsync(authorizationResult.InstanceUrl + $"/services/data/{CoreforceConfiguration.ApiVersion}/composite/tree/{typeof(U).Name}",
                                    new StringContent(JsonConvert.SerializeObject(recordCollection,
                                    new JsonSerializerSettings { ContractResolver = new ImportContractResolver() }), Encoding.UTF8, "application/json")))
                    {
                        try
                        {
                            var responseString = await message.Content.ReadAsStringAsync();
                            var multipleRecordsImportResult =
                                JsonConvert.DeserializeObject<SObjectTreeResult>(responseString);

                            importError = multipleRecordsImportResult.HasErrors;

                            if (stopByException && importError)
                                return multipleRecordsImportResult;

                            foreach (var result in multipleRecordsImportResult.Results)
                            {
                                importResult.Results.Add(result);

                                if (result.Errors != null && result.Errors.Count >= 0)
                                {
                                    recordCollection.Records =
                                        recordCollection.Records.Where(
                                            x => x.ToString() != result.ReferenceId).ToList();
                                }
                            }
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                }
            } while (importError == true);

            return importResult;
        }

        private void AddIdToList<U>(List<U> objs, SObjectTreeResult sObjectTreeResponse)
            where U : InsertAttributes, new()
        {
            var id = typeof(T).GetProperties().SingleOrDefault(x => x.Name.ToLower().Equals("id"));

            if (id == null)
                return;

            var successfulResults = sObjectTreeResponse.Results.Where(x => !string.IsNullOrEmpty(x.Id)).ToList();

            foreach (var result in successfulResults)
            {
                var obj = objs.SingleOrDefault(x => x.attributes.ReferenceId == result.ReferenceId.ToLower());

                if (obj != null)
                    id.SetValue(obj, result.Id);
            }
        }

        #endregion

        public async Task<ApiError> UpdateAsync(T obj, params Expression<Func<T, object>>[] relatingObject)
        {
            var authorizationResult = await SalesforceOpenAuthorization.Authorize();

            if (authorizationResult == null)
                throw new NullReferenceException("Authorization result is null.");

            string salesForceUrl = $"{authorizationResult.InstanceUrl}/services/data/{CoreforceConfiguration.ApiVersion}/sobjects/{typeof(T).Name}";
            var updateResponse = new ApiError();

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authorizationResult.AccessToken);

                    using (var message =
                                await client.PatchAsync(salesForceUrl, new StringContent(JsonConvert.SerializeObject(obj,
                                    new JsonSerializerSettings { ContractResolver = new ImportContractResolver() }), Encoding.UTF8, "application/json")))
                    {
                        if (message.StatusCode == System.Net.HttpStatusCode.NoContent)
                            return null;

                        var responseString = await message.Content.ReadAsStringAsync();

                        if (string.IsNullOrEmpty(responseString))
                            updateResponse = JsonConvert.DeserializeObject<ApiError>(responseString);

                        return updateResponse;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<BatchResult> UpdateAsync(List<T> objList, params Expression<Func<T, object>>[] fieldsToUpdate)
        {
            return await this.ManageBatchThreads(objList, BatchMethod.PATCH, false, fieldsToUpdate);
        }

        public async Task<BatchResult> DeleteAsync(T obj)
        {
            return await this.ManageBatchThreads(new List<T>() { obj }, BatchMethod.DELETE, false);
        }

        public async Task<BatchResult> DeleteAsync(List<T> objList)
        {
            return await this.ManageBatchThreads(objList, BatchMethod.DELETE, false);
        }

        private async Task<BatchResult> ManageBatchThreads(List<T> objList, BatchMethod batchMethod, bool parallelProcessing, params Expression<Func<T, object>>[] fieldsToUpdate)
        {
            AuthorizationResult authorizationResult = await this.SalesforceOpenAuthorization.Authorize();

            if (authorizationResult == null)
                throw new ApplicationException("Authorization result is null.");

            int parallelThreads = parallelProcessing ? CoreforceConfiguration.MaxNumberOfParallelRunningTasks : 1;

            int numOfThreads = Convert.ToInt32(Math.Ceiling((double)objList.Count / 25));
            int numOfThreadCollections = Convert.ToInt32(Math.Ceiling((double)numOfThreads / parallelThreads));

            BatchCollection<T>[] batchCollection = new BatchCollection<T>[numOfThreads];
            Task<BatchResult>[] batchResults;
            BatchResult batchResult = new BatchResult();

            // Split objList into batch collections
            try
            {
                for (int threadCollectionCounter = 0; threadCollectionCounter < numOfThreadCollections; threadCollectionCounter++)
                {
                    for (int i = threadCollectionCounter * parallelThreads, threadCounter = 0; i < threadCollectionCounter * parallelThreads + parallelThreads && i < numOfThreads; i++, threadCounter++)
                    {
                        batchCollection[i] = new BatchCollection<T>();

                        for (int j = i * 25; (j < i * 25 + 25) && (j < objList.Count); j++)
                        {
                            batchCollection[i].BatchRequests.Add(
                                new BatchComponent<T>(
                                    objList[j],
                                    batchMethod,
                                    fieldsToUpdate));
                        }
                    }
                }

                // Run threads
                for (int threadCollectionCounter = 0; threadCollectionCounter < numOfThreadCollections; threadCollectionCounter++)
                {
                    authorizationResult = await SalesforceOpenAuthorization.Authorize();

                    if (threadCollectionCounter + 1 < numOfThreadCollections)
                        batchResults = new Task<BatchResult>[parallelThreads];

                    else
                        batchResults = new Task<BatchResult>[numOfThreads - (threadCollectionCounter * parallelThreads)];

                    for (int i = threadCollectionCounter * parallelThreads, threadCounter = 0; i < threadCollectionCounter * parallelThreads + parallelThreads && i < numOfThreads; i++, threadCounter++)
                    {
                        var localThreadCounter = i;
                        batchResults[threadCounter] = System.Threading.Tasks.Task.Run(() => SendBatchAsync(batchCollection[localThreadCounter], authorizationResult, fieldsToUpdate));

                        // There should be only max. 10 parallely threads
                        if (i + 1 >= threadCollectionCounter * parallelThreads + parallelThreads || i + 1 >= numOfThreads)
                        {
                            await System.Threading.Tasks.Task.WhenAll(batchResults);

                            foreach (Task<BatchResult> result in batchResults)
                            {
                                batchResult.Results.AddRange(result.Result.Results);
                                batchResult.HasErrors = batchResult.HasErrors == false ? result.Result.HasErrors : false;
                            }

                            authorizationResult = await SalesforceOpenAuthorization.Authorize();

                            if (authorizationResult == null)
                            {
                                throw new ApplicationException("Authorization result is null.");
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return batchResult;
        }

        private async Task<BatchResult> SendBatchAsync(BatchCollection<T> batchCollection, AuthorizationResult authorizationResult, Expression<Func<T, object>>[] fields)
        {
            var batchResult = new BatchResult();

            string responseString = string.Empty;

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authorizationResult.AccessToken);

                    using (var message =
                                await client.PostAsync(authorizationResult.InstanceUrl + $"/services/data/{CoreforceConfiguration.ApiVersion}/composite/batch",
                                    new StringContent(JsonConvert.SerializeObject(batchCollection,
                                        new JsonSerializerSettings
                                        {
                                            ContractResolver = new AllowNullableFieldsContractResolver<T>(fields)
                                        }), Encoding.UTF8, "application/json")))
                    {
                        responseString = await message.Content.ReadAsStringAsync();
                        batchResult = JsonConvert.DeserializeObject<BatchResult>(responseString);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return batchResult;
        }

        public async Task<List<T>> Query(string statement)
        {
            return await ManageGetThreads(null, null, statement.Replace(' ', '+'));
        }
    }
}
