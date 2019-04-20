# RetsExchange
A pure .NET RETS client

Quick start:
```csharp
[RetsResource(Class = "RESIDENTIAL", SearchType = "Property")]
public class RetsType {
    public long UniqueID {get; set;}
    public DateTime ModifiedDateTime {get; set;}
}

// ...

var retsClient = new RetsClient(retsEndpoint, username, password);
await retsClient.LoginAsync();

// Fetch all records modified less than 24 hours ago
var timeFrom = DateTime.Now.AddHours(-24).ToString("s");
var retsRequest = new RetsSearchRequest<RetsType>()
{
    Query = $"ModifiedDateTime={timeFrom}+)"
};
var retsResultSet = await retsClient.SearchAsync(retsRequest);

// Stream the results
var recordList = new List<RetsType>();
foreach (var retsResult in retsResultSet.RecordStream)
{
    recordList.Add(retsResult);
}

await retsClient.LogoutAsync();
```

See further examples [here](examples/)
