using System;
using System.Collections.Generic;
using RetsExchange;

// The class to serializer result records into. Either the annotation
// or SearchType & Class in the request is required.
[RetsResource(Class = "RESIDENTIAL", SearchType = "Property")]
public class RetsType
{
    // The field list expected back from the server
    public long UniqueID { get; set; }
    public DateTime ModifiedDateTime { get; set; }
}

public class RetsRunner
{
    public static async void Run()
    {
        var retsClient = new RetsClient("https://rets.realtor.com/login_endpoint", "username", "password");

        // Throws ResponseException if the login fails
        await retsClient.LoginAsync();

        // Fetch all records modified less than 24 hours ago and sold (S) or withdrawn (W)
        var timeFrom = DateTime.Now.AddHours(-24).ToString("s");
        var query = $"ModifiedDateTime={timeFrom}+),(Status=S,W)";
        var retsRequest = new RetsSearchRequest<RetsType>() { Query = query };

        // Run the search, retry twice in case of network failure
        RetsSearchResponse<RetsType> retsResultSet = await retsClient.SearchAsync(retsRequest, retries: 2);

        // Stream the result records
        var recordList = new List<RetsType>();
        foreach (RetsType retsResult in retsResultSet.RecordStream)
            recordList.Add(retsResult);

        // Fetch any remaining records in case the server has a hard result set limit
        var retsRequest2 = new RetsSearchRequest<RetsType>()
        {
            Offset = retsResultSet.Count,
            Query = query
        };

        RetsSearchResponse<RetsType> retsResultSet2 = await retsClient.SearchAsync(retsRequest2, retries: 2);

        foreach (RetsType retsResult in retsResultSet2.RecordStream)
            recordList.Add(retsResult);

        await retsClient.LogoutAsync();
    }
}
