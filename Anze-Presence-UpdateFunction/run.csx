using System;
using System.Net;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log,  ICollector<Presence> presenceTable)
{
    string applicationName = System.Configuration.ConfigurationManager.AppSettings["ApplicationName"];
    
    log.Info("Application started as " + applicationName);
    log.Info("C# HTTP trigger function processed a request.");

    // parse query parameter
    string status = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "status", true) == 0)
        .Value;

    // Get request body
    dynamic data = await req.Content.ReadAsAsync<object>();

    // Set name to query string or body data
    status = status ?? data?.status;

    // let's write into the table entity
    var p = new Presence() {
        PartitionKey = applicationName,
        // idea is that we have a key
        RowKey = (DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks).ToString("d19"),
        Status = status
    };

    presenceTable.Add(p);

    return status == null
        ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a status on the query string or in the request body")
        : req.CreateResponse(HttpStatusCode.OK, "Looks like you're " + status + " from " + applicationName);
}

public class Presence 
{
    public string RowKey { get; set; }
    public string PartitionKey { get; set; }
    public string Status { get; set; }
}