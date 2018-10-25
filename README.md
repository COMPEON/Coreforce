# Welcome to Coreforce
[![NuGet version (Compos.Coreforce)](https://img.shields.io/nuget/v/Compos.Coreforce.svg?style=flat-square)](https://www.nuget.org/packages/Compos.Coreforce/)

Coreforce is a .NET Core wrapper library for Salesforce REST API.

---

# Benefits and Features

- **Easy to use:**
  - **one method configuration** for the whole repository.
  - **get, insert, update and delete** salesforce objects **without writing soql queries**.
  - create soql **where clause without writing soql query**.
  - **select the fields** you want to get or update **via linq expressions**.
- **Speed up your application:**
  - get, insert, update or delete multiple salesforce objects **parallel or sequential**.
  - **get** a salesforce object **by id**.

---

# Getting started

## Setup

You can get Coreforce via nuget:

```cs
PM> Install-Package Compos.Coreforce
```

## Usage

### sObjects as C# classes

Coreforce is an object based repository and needs sObjects as C# classes.
Note the following:
- each sObject requires a C# class
- there's a direct correlation between the C# class name and the sObject (don't forget: a custom object ends with __c)
- each salesforce field requires a property (property name has to be similar to the API name)
- mark read-only fields as read-only

Example: sObject **Account**

| Id | Name          | Description             | AnnualRevenue |
|----|---------------|-------------------------|---------------|
| 1  | Test1 Company | Automotive manufacturer | 12.000.000 €  |
| 2  | Test2 Company |                         | 54.000.000 €  |
| 3  | Test3 Company |                         | 49.000.000 €  |
| 4  | Test4 Company |                         |    900.000 €  |

C# class:

```cs
using Compos.Coreforce.Attributes;
...

public class Account
{
	// Mark readonly properties
	[Readonly]
	public string Id { get; set; }
	
	public string Name { get; set; }
	public string Description { get; set; }
	public decimal AnnualRevenue__c { get; set; }
}
```

### Configuration

Add the Coreforce repository configuration

```cs
public static void ConfigureServices(IServiceCollection services)
{
	...
	services.AddCoreforce(
	new OpenAuthorizationCredentials()
	{
		AuthorizationUrl = "",
		ClientId = "",
		ClientSecret = "",
		GrantType = "",
		Password = "",
		Username = ""
	}, 
	"v40.0", 
	10
	);
	...
}
```

The configuration extension method ***AddCoreforce*** needs the following:
- OpenAuthorizationCredentials:
  - AuthorizationUrl:
    - Testing: https://test.salesforce.com/services/oauth2/token
    - Production: https://login.salesforce.com/services/oauth2/token
  - ClientId and ClientSecret from your salesforce environment.
  - Your salesforce password and username.
  - GrantType: password
- API version
- max. number of parallel running threads

The repository will always take the configured values.
Add the Salesforce Repository via dependency injection:

```cs
using Compos.Coreforce;
...

public class TestClass 
{
	public ISalesforceRepository<Account> SalesforceRepository { get; set; }

	public TestClass(
		ISalesforceRepository<Account> salesforceRepository
		)
	{
		SalesforceRepository = salesforceRepository;
	}
	
	...
}
```

### Get

Get all objects:

```cs
var accounts = await SalesforceRepository.GetAsync();
```

Get all objects and field selection:

```cs
var accounts = await SalesforceRepository.GetAsync(x => x.Id, x => x.Description);
```

Get a specific object by id:

```cs
var account = await SalesforceRepository.GetByIdAsync("ID");
```

#### Use filter 

Instead of getting all the rows you can filter them:

```cs
var accounts = await SalesforceRepository.GetAsync(
new FilterItemCollection<Account>(
	new FilterItem<Account>(x => x.Name, "Test", FilterOperator.NotEquals, FilterConcatination.And),
	new FilterItemIn<Account>(x => x.Id, new List<object>() { "ID_1", "ID_2" })
));
```

The following filter items are applicable:
- FilterItemCollection: brackets the inner FilterItems (can be used multiple times)
- FilterItem: creates a standard soql query (objectA = 'Test')
- FilterItemIn: creates a soql IN query (objectA IN ('A', 'B'))

When you use multiple FilterItems you have to use the FilterConcatination And / Or.

You can also combine filter and field selection:

```cs
var accounts = await SalesforceRepository.GetAsync(
new FilterItemCollection<Account>(
	new FilterItem<Account>(x => x.Name, "Test", FilterOperator.NotEquals, FilterConcatination.And),
	new FilterItemIn<Account>(x => x.Id, new List<object>() { "ID_1", "ID_2" })
), x => x.Id, x => x.Name);
```

### Update

You can update single or multiple sObjects and you are able to select the fields that should be updatable.

Update single sObject:

```cs
var updateResult = await SalesforceRepository.UpdateAsync(account, x => x.Name, x => x.Description);

if(updateResult != null)
    // an error occured
```

Update multiple sObjects:

```cs
var accounts = new List<Account>();
accounts.Add(account);

await SalesforceRepository.UpdateAsync(
    accounts, 
    x => x.Name, x => x.Description
);
```

### Delete

Delete single sObject:

```cs
await SalesforceRepository.DeleteAsync(account);
```

Delete multiple sObjects:

```cs
var accounts = new List<Account>();
accounts.Add(account);

await SalesforceRepository.DeleteAsync(accounts);
```

### Query

You can write your own query with the Query-Method:

```cs
var accounts = await SalesforceRepository.Query("select id, name from account where name = 'test'");
```

At present, only sObjects for a SalesforceRepository<sObject> (e.g. SalesforceRepository<Account> Query method will return accounts) is available.
