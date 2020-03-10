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
using Compos.Coreforce.Attributes;
using Compos.Coreforce.Extensions;
using System.Reflection;
using Compos.Coreforce.ContractResolver;
using System.Net;
using Compos.Coreforce.Cache;

namespace Compos.Coreforce
{
    public class SalesforceClient : ISalesforceClient
    {
        private const string UNABLE_TO_LOCK_ROW = "UNABLE_TO_LOCK_ROW";
        public static int cnt = 0;
        // Only for tests
        protected HttpClient _client = null;

        protected static readonly ConcurrentBag<Description> descriptionList = new ConcurrentBag<Description>();
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

        public async Task<Description> GetDescriptionAsync<T>()
        {
            return await GetDescription(AttributeReader.GetSalesforceObjectName<T>());
        }

        public async Task<Description> GetDescriptionAsync(string sObject)
        {
            return await GetDescription(sObject);
        }

        public async Task<List<T>> GetAsync<T>()
        {
            var select = BuildSelectUrl<T>(null, null);
            return await QueryAsync<T>(select);
        }

        public async Task<List<T>> GetAsync<T>(params Expression<Func<T, object>>[] fields)
        {
            var select = BuildSelectUrl<T>(null, fields);
            return await QueryAsync<T>(select);
        }

        public async Task<List<T>> GetAsync<T>(FilterCollection filterCollection)
        {
            var select = BuildSelectUrl<T>(filterCollection, null);
            return await QueryAsync<T>(select);
        }

        public async Task<List<T>> GetAsync<T>(FilterCollection filterCollection, params Expression<Func<T, object>>[] fields)
        {
            var select = BuildSelectUrl<T>(filterCollection, fields);
            return await QueryAsync<T>(select);
        }

        protected string BuildSelectUrl<T>(
            FilterCollection filterCollection,
            Expression<Func<T, object>>[] fields
            )
        {
            List<MemberExpression> memberExpressions = new List<MemberExpression>();
            var sObjectFields = GetSalesforceObjectFields(
                AttributeReader.GetSalesforceObjectName<T>(), 
                SalesforceObjectFieldAccessibility.All
                ).GetAwaiter().GetResult();

            string requestUrl = $"select+";
            var properties = typeof(T).GetProperties().Where(x => !x.CustomAttributes.Any(y =>
                y.AttributeType == typeof(JsonIgnoreAttribute) || y.AttributeType == typeof(NoSalesforceField))
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
                if (
                    (memberExpressions.Any(x => x.Member.Name == property.Name) || 
                    memberExpressions.Count == 0) &&
                    sObjectFields.Any(x => x.ToLower() == property.Name.ToLower()))
                {
                    if (firstElement)
                    {
                        requestUrl = $"{requestUrl}{property.Name}";
                        firstElement = false;
                    }

                    else
                        requestUrl = $"{requestUrl}+,+{property.Name}";
                }
                else
                {
                    var jsonPropertyNames = GetJsonPropertyNames(property);

                    foreach (var jsonPropertyName in jsonPropertyNames)
                    {
                        if (
                        (memberExpressions.Any(x => x.Member.Name == jsonPropertyName) ||
                        memberExpressions.Count == 0) &&
                        sObjectFields.Any(x => x.ToLower() == jsonPropertyName.ToLower()))
                        {
                            if (firstElement)
                            {
                                requestUrl = $"{requestUrl}{jsonPropertyName}";
                                firstElement = false;
                            }

                            else
                                requestUrl = $"{requestUrl}+,+{jsonPropertyName}";
                        }
                    }
                }
            }

            requestUrl = $"{requestUrl}+from+{AttributeReader.GetSalesforceObjectName<T>()}";

            if (filterCollection is null)
                return requestUrl;

            requestUrl = $"{requestUrl}+where{filterCollection.Get()}";

            return requestUrl;
        }

        private List<string> GetJsonPropertyNames(PropertyInfo propertyInfo)
        {
            var jsonPropertyNames = new List<string>();

            CustomAttributeData attribute = propertyInfo.CustomAttributes
                .FirstOrDefault(x => x.AttributeType == typeof(JsonPropertyAttribute));

            if (attribute is null)
                return jsonPropertyNames;

            var jsonPropertyArguments = attribute.ConstructorArguments
                .Where(x => x.ArgumentType == typeof(string));

            foreach (var jsonPropertyArgument in jsonPropertyArguments)
                if(jsonPropertyArgument.Value != null)
                    jsonPropertyNames.Add(jsonPropertyArgument.Value.ToString());

            return jsonPropertyNames;
        }

        public async Task<List<dynamic>> GetAsync(string sObject)
        {
            var fields = await GetSalesforceObjectFields(
                sObject,
                SalesforceObjectFieldAccessibility.All
                );
            StringBuilder fieldNames = new StringBuilder();

            foreach (var field in fields)
                fieldNames.Append(fieldNames.Length == 0 ? $"{field}" : $"+,+{field}");

            return await QueryAsync($"select+{fieldNames.ToString()}+from+{sObject}");
        }

        public async Task<List<dynamic>> GetAsync(string sObject, params string[] fields)
        {
            StringBuilder fieldNames = new StringBuilder();

            foreach (var field in fields)
                fieldNames.Append(fieldNames.Length == 0 ? $"{field}" : $"+,+{field}");

            return await QueryAsync($"select+{fieldNames.ToString()}+from+{sObject}");
        }

        public async Task<List<dynamic>> GetAsync(string sObject, FilterCollection filterCollection)
        {
            var fieldNames = await GetSalesforceObjectFields(sObject, SalesforceObjectFieldAccessibility.All);
            StringBuilder fields = new StringBuilder();

            foreach (var fieldName in fieldNames)
                fields.Append(fields.Length == 0 ? $"{fieldName}" : $"+,+{fieldName}");

            return await QueryAsync($"select+{fields.ToString()}+from+{sObject}+where{filterCollection.Get()}");
        }

        public async Task<List<dynamic>> GetAsync(string sObject, FilterCollection filterCollection, params string[] fields)
        {
            StringBuilder fieldNames = new StringBuilder();

            foreach (var field in fields)
                fieldNames.Append(fieldNames.Length == 0 ? $"{field}" : $"+,+{field}");

            return await QueryAsync($"select+{fieldNames.ToString()}+from+{sObject}+where{filterCollection.Get()}");
        }

        public async Task<T> GetByIdAsync<T>(string id) where T : class
        {
            var cachedObj = CacheHelper.Get<T>(id);

            if (cachedObj != null)
                return cachedObj;

            var select = BuildSelectUrl<T>(
                new FilterCollection(
                    new Filter.Filter("id", "=", id)
                    ), null);

            var objs = new List<T>();
            objs.AddCollection(await QueryAsync<T>(select));

            return objs.FirstOrDefault();
        }

        public async Task<dynamic> GetByIdAsync(string sObject, string id)
        {
            var cachedObj = CacheHelper.Get<dynamic>(id);

            if (cachedObj != null)
                return cachedObj as ExpandoObject;

            var fieldNames = await GetSalesforceObjectFields(sObject, SalesforceObjectFieldAccessibility.All);
            StringBuilder fields = new StringBuilder();

            foreach (var fieldName in fieldNames)
                fields.Append(fields.Length == 0 ? $"{fieldName}" : $", {fieldName}");

            var objs = new List<dynamic>();
            objs.AddCollection(await QueryAsync($"select {fields.ToString()} from {sObject} WHERE id='{id}'"));

            return objs.FirstOrDefault();
        }

        public async Task<List<T>> QueryAsync<T>(string command)
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
                    using (var client = _client ?? new HttpClient())
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

                            try
                            {
                                selectResult = JsonConvert.DeserializeObject<SelectResult<T>>(responseString);
                            }
                            catch (JsonException exception)
                            {
                                throw new CoreforceException(
                                    CoreforceError.JsonDeserializationError,
                                    $"Response: {responseString}",
                                    exception
                                    );
                            }

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

        public async Task<List<dynamic>> QueryAsync(string command)
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
                    using (var client = _client ?? new HttpClient())
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

                            try
                            {
                                selectResult = JsonConvert.DeserializeObject<SelectResult<ExpandoObject>>(responseString);
                            }
                            catch(JsonException exception)
                            {
                                throw new CoreforceException(
                                    CoreforceError.JsonDeserializationError,
                                    $"Response: {responseString}", 
                                    exception
                                    );
                            }

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

        public async Task<string> InsertAsync<T>(T obj)
        {
            return await Insert(AttributeReader.GetSalesforceObjectName<T>(), obj);
        }

        public async Task<string> InsertAsync(string sObject, dynamic obj)
        {
            dynamic expandoObject = new ExpandoObject();
            var updateableFields = await GetSalesforceObjectFields(sObject, SalesforceObjectFieldAccessibility.Insert);

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

                using (var client = _client ?? new HttpClient())
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
                                                ContractResolver = new CoreforceContractResolver()
                                            }),
                                        Encoding.UTF8,
                                        "application/json"
                                        )))
                    {
                        var responseString = await message.Content.ReadAsStringAsync();
                        InsertResult insertResponse = null;

                        try
                        {
                            insertResponse = JsonConvert.DeserializeObject<InsertResult>(responseString);
                        }
                        catch (JsonException exception)
                        {
                            throw new CoreforceException(
                                CoreforceError.JsonDeserializationError,
                                $"Response: {responseString}",
                                exception
                                );
                        }

                        if (insertResponse is null)
                            throw new CoreforceException(CoreforceError.NoInsertResponse);

                        if (string.IsNullOrEmpty(insertResponse.Id))
                            throw new CoreforceException(CoreforceError.InsertError, insertResponse.Errors);

                        return insertResponse.Id;
                    }
                }
            }
            catch (Exception exception)
            {
                throw new CoreforceException(CoreforceError.InsertError, $"Error while inserting the object {JsonConvert.SerializeObject(obj)}. Please check the inner exception.", exception);
            }
        }

        public async Task UpdateAsync<T>(T obj)
        {
            await UpdateAsync(obj, null);
        }

        public async Task UpdateAsync<T>(T obj, params Expression<Func<T, object>>[] fields)
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
                            y.AttributeType == typeof(Readonly)) && x.Name.ToLower() != "id").ToList())
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

                    else if (propertyInfo.PropertyType == typeof(Date) || propertyInfo.PropertyType == typeof(Date?))
                        objAsDictionary.Add(propertyInfo.Name, value.ToString());

                    else
                        objAsDictionary.Add(propertyInfo.Name, ChangeType(value, propertyInfo.PropertyType));
                }

                await UpdateAsync<T>(AttributeReader.GetSalesforceObjectName<T>(), objAsDictionary, id.ToString());
            }
            catch(Exception)
            {
                throw;
            }
        }

        private object ChangeType(object value, Type type)
        {
            if (value == null || value == DBNull.Value)
                return null;

            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            return Convert.ChangeType(value, underlyingType);
        }

        public async Task UpdateAsync(string sObject, dynamic obj)
        {
            await UpdateAsync(sObject, obj, null);
        }

        public async Task UpdateAsync(string sObject, dynamic obj, params string[] fields)
        {
            try
            {
                var objAsDictionary = new Dictionary<string, object>();
                var updateableFields = await GetSalesforceObjectFields(sObject, SalesforceObjectFieldAccessibility.Update);
                var id = string.Empty;

                foreach(var property in obj as IDictionary<string, object>)
                {
                    if ((fields != null && fields.Any(x => x == property.Key) && updateableFields.Any(x => x == property.Key)) ||
                        (fields is null && updateableFields.Any(x => x == property.Key)))
                    {
                        if (property.Value != null &&
                            ((property.Value.GetType() == typeof(DateTime) && (DateTime)property.Value == DateTime.MinValue) ||
                            (property.Value.GetType() == typeof(DateTime?) && (DateTime?)property.Value == DateTime.MinValue)))
                            continue;

                        objAsDictionary.Add(property.Key, property.Value);
                    }
                    else if (property.Key.ToLower() == "id")
                        id = property.Value.ToString();
                }
                    
                await UpdateAsync<dynamic>(sObject, objAsDictionary, id.ToString());
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task UpdateAsync<T>(string sObject, Dictionary<string, object> objAsDictionary, string id)
        {
            try
            {
                var authorizationResult = await SalesforceOpenAuthorization.Authorize();

                if (authorizationResult is null)
                    throw new CoreforceException(CoreforceError.AuthorizationError);

                var json = JsonConvert.SerializeObject(objAsDictionary);

                var url = $"{authorizationResult.InstanceUrl}/services/data/{CoreforceConfiguration.ApiVersion}/sobjects/{sObject}/{id}";
                var lockErrorCounter = 0;

                while (true)
                {
                    using (var client = _client ?? new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authorizationResult.AccessToken);

                        using (var message =
                            await client.PatchJsonAsync(
                                url,
                                JsonConvert.SerializeObject(
                                    objAsDictionary,
                                    new JsonSerializerSettings
                                    {
                                        ContractResolver = new CoreforceContractResolver(
                                            objAsDictionary.Select(x => x.Key).ToList()
                                            )
                                    })))
                        {
                            if (message.StatusCode == HttpStatusCode.NoContent)
                                return;

                            var responseString = await message.Content.ReadAsStringAsync();

                            if (responseString.ToLower().Contains(UNABLE_TO_LOCK_ROW.ToLower()))
                                if(lockErrorCounter++ < 5)
                                    continue;

                            List<ApiError> apiErrors = null;

                            try
                            {
                                apiErrors = JsonConvert.DeserializeObject<List<ApiError>>(responseString);
                            }
                            catch (JsonException exception)
                            {
                                throw new CoreforceException(
                                    CoreforceError.JsonDeserializationError,
                                    $"Response: {responseString}",
                                    exception
                                    );
                            }

                            if (apiErrors is null)
                                throw new CoreforceException(CoreforceError.NoUpdateResponse);

                            throw new CoreforceException(CoreforceError.UpdateError, apiErrors);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                throw new CoreforceException(CoreforceError.UpdateError, $"Error while update the sObject {id}. Please check the inner exception.", exception);
            }
        }

        public async Task DeleteAsync<T>(string id)
        {
            await DeleteAsync(AttributeReader.GetSalesforceObjectName<T>(), id);
        }

        public async Task DeleteAsync(string sObject, string id)
        {
            try
            {
                var authorizationResult = await SalesforceOpenAuthorization.Authorize();

                if (authorizationResult is null)
                    throw new CoreforceException(CoreforceError.AuthorizationError);

                var url = $"{authorizationResult.InstanceUrl}/services/data/{CoreforceConfiguration.ApiVersion}/sobjects/{sObject}/{id}";

                using (var client = _client ?? new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authorizationResult.AccessToken);

                    using (var message =
                                await client.DeleteAsync(url))
                    {
                        if (message.StatusCode == HttpStatusCode.NoContent)
                            return;

                        var responseString = await message.Content.ReadAsStringAsync();
                        List<ApiError> apiErrors = null;
                        try
                        {
                            apiErrors = JsonConvert.DeserializeObject<List<ApiError>>(responseString);
                        }
                        catch (JsonException exception)
                        {
                            throw new CoreforceException(
                                CoreforceError.JsonDeserializationError,
                                $"Response: {responseString}",
                                exception
                                );
                        }

                        if (apiErrors is null)
                            throw new CoreforceException(CoreforceError.NoDeleteResponse);

                        throw new CoreforceException(CoreforceError.DeleteError, apiErrors);
                    }
                }
            }
            catch (Exception exception)
            {
                throw new CoreforceException(CoreforceError.DeleteError, $"Error while delete the sObject {id}. Please check the inner exception.", exception);
            }
        }

        private async Task<List<string>> GetSalesforceObjectFields(string sObject, SalesforceObjectFieldAccessibility fieldAccessibility)
        {
            try
            {
                Description sObjectDescription = await GetDescription(sObject);
                var sObjectFields = new List<string>();
                
                foreach(var field in sObjectDescription.fields)
                {
                    if ((fieldAccessibility == SalesforceObjectFieldAccessibility.Insert &&
                        !field.createable) ||
                        (fieldAccessibility == SalesforceObjectFieldAccessibility.Update &&
                        !field.updateable))
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

        private async Task<Description> GetDescription(string sObject)
        {
            try
            {
                Description sObjectDescription = descriptionList.FirstOrDefault(x => x.name.ToLower() == sObject.ToLower());

                if (sObjectDescription is null)
                {
                    var authorizationResult = await SalesforceOpenAuthorization.Authorize();

                    using (var client = _client ?? new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authorizationResult.AccessToken);

                        using (var message =
                                    await client.GetAsync($"{authorizationResult.InstanceUrl}/services/data/{CoreforceConfiguration.ApiVersion}/sobjects/{sObject}/describe"))
                        {
                            var responseString = await message.Content.ReadAsStringAsync();

                            try
                            {
                                sObjectDescription = JsonConvert.DeserializeObject<Description>(responseString);
                            }
                            catch (JsonException exception)
                            {
                                throw new CoreforceException(
                                    CoreforceError.JsonDeserializationError,
                                    $"Response: {responseString}",
                                    exception
                                    );
                            }

                            if (sObjectDescription is null)
                                throw new CoreforceException(CoreforceError.SalesforceObjectNotFound);

                            descriptionList.Add(sObjectDescription);
                        }
                    }
                }

                return sObjectDescription;
            }
            catch (Exception exception)
            {
                throw new CoreforceException(CoreforceError.ProcessingError, exception);
            }
        }
    }
}
