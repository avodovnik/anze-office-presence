#r "Microsoft.WindowsAzure.Storage"

using System.Net;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    // parse query parameter
    string app = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "app", true) == 0)
        .Value;

    // Get request body
    dynamic data = await req.Content.ReadAsAsync<object>();

    // Set name to query string or body data
    app = app ?? data?.app;

    return app == null
        ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
        : req.CreateResponse(HttpStatusCode.OK, "Hello " + app);
}

public class Presence 
{
    public string RowKey { get; set; }
    public string PartitionKey { get; set; }
    public string Status { get; set; }
}