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

    var table = tableClient.GetTableReference(ConfigurationManager.AppSettings["PresenceTable"]); 

    var query = new TableQuery<Presence>()
                    .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, app))
                    .Take(1);

    var potentialPresence = table.ExecuteQuery(query).SingleOrDefault();

    return potentialPresence == null
        ? req.CreateResponse(HttpStatusCode.BadRequest, "Status unknown.")
        : req.CreateResponse(HttpStatusCode.OK, potentialPresence.Status);
}

public class Presence : TableEntity
{
    public Presence() {
    }

    public string Status { get; set; }
}