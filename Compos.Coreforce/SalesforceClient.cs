using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Compos.Coreforce.Authorization;
using Compos.Coreforce.Filter;
using Compos.Coreforce.Models;
using Compos.Coreforce.Models.Configuration;
using Compos.Coreforce.Models.Description;
using Newtonsoft.Json;
using System.Linq;
using Compos.Coreforce.Models.Authorization;
using Compos.Coreforce.Attributes;
using Compos.Coreforce.Extensions;
using System.Reflection;
using Compos.Coreforce.ContractResolver;
using System.Net;
using Newtonsoft.Json.Serialization;

namespace Compos.Coreforce
{
    public class SalesforceClient : ISalesforceClient
    {
        private static readonly ConcurrentBag<Description> descriptionList = new ConcurrentBag<Description>();
        public ISalesforceOpenAuthorization SalesforceOpenAuthorization { get; private set; }

        public SalesforceClient(
            ISalesforceOpenAuthorization salesforceOpenAuthorization
            )
        {
            SalesforceOpenAuthorization = salesforceOpenAuthorization;

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new Newtonsoft.Json.Converters.IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd" } }
            };
        }

        public async Task<List<T>> Get<T>()
        {
            var select = BuildSelectUrl<T>(null, null);
            return await Query<T>(select);
        }

        public async Task<List<T>> Get<T>(params Expression<Func<T, object>>[] fields)
        {
            var select = BuildSelectUrl<T>(null, fields);
            return await Query<T>(select);
        }

        public async Task<List<T>> Get<T>(FilterCollection filterCollection)
        {
            var select = BuildSelectUrl<T>(filterCollection, null);
            return await Query<T>(select);
        }

        public async Task<List<T>> Get<T>(FilterCollection filterCollection, params Expression<Func<T, object>>[] fields)
        {
            var select = BuildSelectUrl<T>(filterCollection, fields);
            return await Query<T>(select);
        }

        private string BuildSelectUrl<T>(
            FilterCollection filterCollection,
            Expression<Func<T, object>>[] fields
            )
        {
            List<MemberExpression> memberExpressions = new List<MemberExpression>();
            string requestUrl = $"Select+";
            var properties = typeof(T).GetProperties().Where(x => !x.CustomAttributes.Any(y =>
                y.AttributeType == typeof(JsonIgnoreAttribute))
                ).ToList();

            bool firstElement = true;

            if (fields != null && fields.Length > 0)
            {
                foreach (var field in fields)
                {
                    memberExpressions.Add(field.GetMemberExpression());
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

            if (filterCollection is null)
                return requestUrl;

            requestUrl = $"{requestUrl}+where{filterCollection.Get()}";

            return requestUrl;
        }

        public async Task<List<dynamic>> Get(string sObject)
        {
            var fields = await GetSalesforceObjectFields(sObject, false);
            StringBuilder fieldNames = new StringBuilder();

            foreach (var field in fields)
                fieldNames.Append(fieldNames.Length == 0 ? $"{field}" : $", {field}");

            return await Query($"select {fieldNames.ToString()} from {sObject}");
        }

        public async Task<List<dynamic>> Get(string sObject, params string[] fields)
        {
            StringBuilder fieldNames = new StringBuilder();

            foreach (var field in fields)
                fieldNames.Append(fieldNames.Length == 0 ? $"{field}" : $", {field}");

            return await Query($"select {fieldNames.ToString()} from {sObject}");
        }

        public async Task<List<dynamic>> Get(string sObject, FilterCollection filterCollection)
        {
            var fieldNames = await GetSalesforceObjectFields(sObject, false);
            StringBuilder fields = new StringBuilder();

            foreach (var fieldName in fieldNames)
                fields.Append(fields.Length == 0 ? $"{fieldName}" : $", {fieldName}");

            return await Query($"select {fields.ToString()} from {sObject} WHERE {filterCollection.Get()}");
        }

        public async Task<List<dynamic>> Get(string sObject, FilterCollection filterCollection, params string[] fields)
        {
            StringBuilder fieldNames = new StringBuilder();

            foreach (var field in fields)
                fieldNames.Append(fieldNames.Length == 0 ? $"{field}" : $", {field}");

            return await Query($"select {fieldNames.ToString()} from {sObject} WHERE {filterCollection.Get()}");
        }

        public async Task<T> GetById<T>(string id)
        {
            var select = BuildSelectUrl<T>(
                new FilterCollection(
                    new List<IFilter>()
                    {
                        new Filter.Filter("id", "=", id)
                    }), null);

            var objs = await Query<T>(select);
            return objs.FirstOrDefault();
        }

        public async Task<dynamic> GetById(string sObject, string id)
        {
            var fieldNames = await GetSalesforceObjectFields(sObject, false);
            StringBuilder fields = new StringBuilder();

            foreach (var fieldName in fieldNames)
                fields.Append(fields.Length == 0 ? $"{fieldName}" : $", {fieldName}");

            var objs = await Query($"select {fields.ToString()} from {sObject} WHERE id='{id}'");
            return objs.FirstOrDefault();
        }

        public async Task<List<T>> Query<T>(string command)
        {
            try
            {
                if (string.IsNullOrEmpty(command))
                    throw new CoreforceException(CoreforceError.CommandIsEmpty);

                var authorizationResult = await SalesforceOpenAuthorization.Authorize();

                if (authorizationResult == null)
                    throw new CoreforceException(CoreforceError.AuthorizationError);

                var selectResult = new SelectResult<T>();
                var records = new List<T>();

                do
                {
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authorizationResult.AccessToken);

                        using (var message =
                                    await client.GetAsync(
                                        string.Concat(authorizationResult.InstanceUrl,
                                        records.Count == 0 ?
                                        $"/services/data/{CoreforceConfiguration.ApiVersion}/query/?q={command.Replace(" ", "+")}" :
                                        selectResult.NextRecordsUrl)))
                        {
                            var responseString = await message.Content.ReadAsStringAsync();
                            selectResult = JsonConvert.DeserializeObject<SelectResult<T>>(responseString);

                            if (selectResult?.Records != null)
                                records.AddRange(selectResult.Records);
                        }
                    }
                } while (!selectResult.Done);

                return records;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<dynamic>> Query(string command)
        {
            try
            {
                if (string.IsNullOrEmpty(command))
                    throw new CoreforceException(CoreforceError.CommandIsEmpty);

                var authorizationResult = await SalesforceOpenAuthorization.Authorize();

                if (authorizationResult == null)
                    throw new CoreforceException(CoreforceError.AuthorizationError);
                
                var selectResult = new SelectResult<ExpandoObject>();
                var records = new List<dynamic>();

                do
                {
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authorizationResult.AccessToken);

                        using (var message =
                                    await client.GetAsync(
                                        string.Concat(authorizationResult.InstanceUrl,
                                        records.Count == 0 ?
                                        $"/services/data/{CoreforceConfiguration.ApiVersion}/query/?q={command.Replace(" ", "+")}" :
                                        selectResult.NextRecordsUrl)))
                        {
                            var responseString = await message.Content.ReadAsStringAsync();
                            selectResult = JsonConvert.DeserializeObject<SelectResult<ExpandoObject>>(responseString);

                            if (selectResult?.Records != null)
                                records.AddRange(selectResult.Records);
                        }
                    }
                } while (!selectResult.Done);

                return records;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> Insert<T>(T obj)
        {
            return await Insert(typeof(T).Name, obj);
        }

        public async Task<string> Insert(string sObject, dynamic obj)
        {
            dynamic expandoObject = new ExpandoObject();
            var updateableFields = await GetSalesforceObjectFields(sObject, true);

            foreach (var property in obj as IDictionary<string, object>)
            {
                if (updateableFields.Any(x => x == property.Key))
                    ((IDictionary<string, object>)expandoObject).Add(property.Key, property.Value);
            }

            return await Insert(sObject, expandoObject);
        }

        private async Task<string> Insert<T>(string sObject, T obj)
        {
            try
            {
                var authorizationResult = await SalesforceOpenAuthorization.Authorize();

                if (authorizationResult is null)
                    throw new CoreforceException(CoreforceError.AuthorizationError);

                var url = $"{authorizationResult.InstanceUrl}/services/data/{CoreforceConfiguration.ApiVersion}/sobjects/{sObject}";

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authorizationResult.AccessToken);

                    using (var message =
                                await client.PostAsync(
                                    url,
                                    new StringContent(
                                        JsonConvert.SerializeObject(
                                            obj,
                                            new JsonSerializerSettings
                                            {
                                                ContractResolver = new ImportContractResolver()
                                            }),
                                        Encoding.UTF8,
                                        "application/json"
                                        )))
                    {
                        var responseString = await message.Content.ReadAsStringAsync();
                        var insertResponse = JsonConvert.DeserializeObject<InsertResult>(responseString);

                        if (insertResponse is null)
                            throw new CoreforceException(CoreforceError.NoInsertResponse);

                        if (string.IsNullOrEmpty(insertResponse.Id))
                            throw new CoreforceException(CoreforceError.InsertError, insertResponse);

                        return insertResponse.Id;
                    }
                }
            }
            catch (Exception exception)
            {
                throw new CoreforceException($"Error while inserting the object {JsonConvert.SerializeObject(obj)}. Please check the inner exception.", exception);
            }
        }

        public async Task Update<T>(T obj)
        {
            await Update(obj, null);
        }

        public async Task Update<T>(T obj, params Expression<Func<T, object>>[] fields)
        {
            try
            {
                var objAsDictionary = new Dictionary<string, object>();

                var propertyId = typeof(T).GetProperties().SingleOrDefault(x => x.Name.ToLower().Equals("id"));

                if (propertyId is null)
                    throw new CoreforceException(CoreforceError.SalesforceObjectWithoutId);

                var id = propertyId.GetValue(obj);

                if (id is null)
                    throw new CoreforceException(CoreforceError.SalesforceObjectIdIsNull);

                List<string> fieldsToUpdate = new List<string>();

                if (fields != null)
                {
                    foreach (var field in fields)
                    {
                        var memberExpression = field.GetMemberExpression();

                        if (memberExpression?.Member?.Name != "")
                            fieldsToUpdate.Add(memberExpression?.Member?.Name);
                    }
                }

                foreach (PropertyInfo propertyInfo in obj.GetType().GetProperties()
                    .Where(x =>
                        !x.CustomAttributes.Any(y =>
                            y.AttributeType == typeof(Readonly))).ToList())
                {
                    var value = propertyInfo.GetValue(obj);

                    if (value == null ||
                        (propertyInfo.GetType() == typeof(string) && string.IsNullOrEmpty((string)value)) ||
                        (!fieldsToUpdate.Contains(propertyInfo.Name) && fieldsToUpdate.Count > 0) ||
                        (propertyInfo.PropertyType == typeof(DateTime) && (DateTime)value == DateTime.MinValue) ||
                        (propertyInfo.PropertyType == typeof(DateTime?) && (DateTime?)value == DateTime.MinValue))
                    {
                        continue;
                    }

                    else
                        objAsDictionary.Add(propertyInfo.Name, Convert.ChangeType(value, propertyInfo.PropertyType));
                }

                await Update<T>(typeof(T).Name, objAsDictionary, id.ToString());
            }
            catch(Exception exception)
            {
                throw;
            }
        }

        public async Task Update(string sObject, dynamic obj)
        {
            await Update(sObject, obj, null);
        }

        public async Task Update(string sObject, dynamic obj, params string[] fields)
        {
            try
            {
                var objAsDictionary = new Dictionary<string, object>();
                var updateableFields = await GetSalesforceObjectFields(sObject, true);
                var id = string.Empty;

                foreach(var property in obj as IDictionary<string, object>)
                {
                    if ((fields != null && fields.Any(x => x == property.Key) && updateableFields.Any(x => x == property.Key)) ||
                        (fields is null && updateableFields.Any(x => x == property.Key)))
                    {
                        if ((property.Value.GetType() == typeof(DateTime) && (DateTime)property.Value == DateTime.MinValue) ||
                            (property.Value.GetType() == typeof(DateTime?) && (DateTime?)property.Value == DateTime.MinValue))
                            continue;

                        objAsDictionary.Add(property.Key, property.Value);
                    }
                    else if (property.Key.ToLower() == "id")
                        id = property.Value.ToString();
                }
                    
                await Update<dynamic>(sObject, objAsDictionary, id.ToString());
            }
            catch (Exception exception)
            {
                throw;
            }
        }

        private async Task Update<T>(string sObject, Dictionary<string, object> objAsDictionary, string id)
        {
            try
            {
                var authorizationResult = await SalesforceOpenAuthorization.Authorize();

                if (authorizationResult is null)
                    throw new CoreforceException(CoreforceError.AuthorizationError);

                var json = JsonConvert.SerializeObject(objAsDictionary);

                var url = $"{authorizationResult.InstanceUrl}/services/data/{CoreforceConfiguration.ApiVersion}/sobjects/{sObject}/{id}";

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authorizationResult.AccessToken);

                    using (var message =
                                await client.PatchAsync(
                                    url,
                                    new StringContent(
                                        JsonConvert.SerializeObject(objAsDictionary),
                                        Encoding.UTF8,
                                        "application/json"
                                        )))
                    {
                        if (message.StatusCode == HttpStatusCode.NoContent)
                            return;

                        var responseString = await message.Content.ReadAsStringAsync();
                        var apiErrors = JsonConvert.DeserializeObject<List<ApiError>>(responseString);

                        if (apiErrors is null)
                            throw new CoreforceException(CoreforceError.NoUpdateResponse);

                        throw new CoreforceException(CoreforceError.UpdateError, apiErrors);
                    }
                }
            }
            catch (Exception exception)
            {
                throw new CoreforceException($"Error while delete the sObject {id}. Please check the inner exception.", exception);
            }
        }

        public async Task Delete<T>(string id)
        {
            await Delete(typeof(T).Name, id);
        }

        public async Task Delete(string sObject, string id)
        {
            try
            {
                var authorizationResult = await SalesforceOpenAuthorization.Authorize();

                if (authorizationResult is null)
                    throw new CoreforceException(CoreforceError.AuthorizationError);

                var url = $"{authorizationResult.InstanceUrl}/services/data/{CoreforceConfiguration.ApiVersion}/sobjects/{sObject}/{id}";

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authorizationResult.AccessToken);

                    using (var message =
                                await client.DeleteAsync(url))
                    {
                        if (message.StatusCode == HttpStatusCode.NoContent)
                            return;

                        var responseString = await message.Content.ReadAsStringAsync();
                        var apiErrors = JsonConvert.DeserializeObject<List<ApiError>>(responseString);

                        if (apiErrors is null)
                            throw new CoreforceException(CoreforceError.NoDeleteResponse);

                        throw new CoreforceException(CoreforceError.DeleteError, apiErrors);
                    }
                }
            }
            catch (Exception exception)
            {
                throw new CoreforceException($"Error while delete the sObject {id}. Please check the inner exception.", exception);
            }
        }

        private async Task<List<string>> GetSalesforceObjectFields(string sObject, bool onlyUpdateableFields)
        {
            try
            {
                Description sObjectDescription = descriptionList.FirstOrDefault(x => x.name.ToLower() == sObject.ToLower());
                var sObjectFields = new List<string>();

                if (sObjectDescription is null)
                {
                    var authorizationResult = await SalesforceOpenAuthorization.Authorize();

                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authorizationResult.AccessToken);

                        using (var message =
                                    await client.GetAsync($"{authorizationResult.InstanceUrl}/services/data/{CoreforceConfiguration.ApiVersion}/sobjects/{sObject}/describe"))
                        {
                            var responseString = await message.Content.ReadAsStringAsync();
                            sObjectDescription = JsonConvert.DeserializeObject<Description>(responseString);

                            if (sObjectDescription is null)
                                throw new CoreforceException(CoreforceError.SalesforceObjectNotFound);

                            descriptionList.Add(sObjectDescription);
                        }
                    }
                }

                foreach(var field in sObjectDescription.fields)
                {
                    if ((onlyUpdateableFields && !field.updateable) ||
                        !field.filterable)
                        continue;

                    sObjectFields.Add(field.name);
                }

                return sObjectFields;
            }
            catch (Exception exception)
            {
                throw new CoreforceException(CoreforceError.ProcessingError, exception);
            }
        }
    }
}
