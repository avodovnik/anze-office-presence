#r "Microsoft.WindowsAzure.Storage"

using System.Configuration;
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

    if(string.IsNullOrEmpty(app)) { 
        return req.CreateResponse(HttpStatusCode.BadRequest, "No partition given.");
    }

    var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["anzepresence_STORAGE"]);

    // Create the table client.
    var tableClient = storageAccount.CreateCloudTableClient();

    var table = tableClient.GetTableReference("anzepresence"); // TODO: move to config

    var query = new TableQuery<Presence>()
                    .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, app));

    var potentialPresence = table.ExecuteQuery(query).Take(1);

    return app == null
        ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
        : req.CreateResponse(HttpStatusCode.OK, "Hello " + app + ", looks like you're " + potentialPresence.Status);
}

public class Presence : TableEntity
{
    public Presence() {
    }

    public string Status { get; set; }
}