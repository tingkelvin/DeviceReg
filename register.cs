using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text;
using Microsoft.Data.SqlClient;

using Newtonsoft.Json.Linq;
using System.Threading;
namespace AzureDevOps
{
        public class RetryHandler : DelegatingHandler
    {
        // Strongly consider limiting the number of retries - "retry forever" is
        // probably not the most user friendly way you could respond to "the
        // network cable got pulled out."
        private const int MaxRetries = 20;
        public RetryHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        { }
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;
            for (int i = 0; i < MaxRetries; i++)
            {
                response = await base.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode) {
                    return response;
                }
            }
            return response;
        }
    }
    public static class register
    {
        [FunctionName("register")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log,
            [Sql(commandText: "dbo.Devices", connectionStringSetting: "SqlConnectionString")] IAsyncCollector<Device> deviceTable)
        {
            log.LogInformation("C# HTTP trigger processed a request.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            JArray devices = data?.devices;
        
            Dictionary<string, Device> devicesHashMap = new Dictionary<string, Device>();

            foreach (JObject item in devices) // <-- Note that here we used JObject instead of usual JProperty
            {
                string id = item.GetValue("id").ToString();
                string name = item.GetValue("Name").ToString();
                string type = item.GetValue("type").ToString();
                string location = item.GetValue("location").ToString();
                devicesHashMap.Add(id, new Device{
                    DeviceId = id,
                    Name = name,
                    Type = type,
                    Location = location
                });
            }

            List<Asset> assets = await ProcessDeviceAsync(devicesHashMap);

            foreach(Asset asset in assets)
            {
                devicesHashMap[asset.deviceId].AssetId = asset.assetId;
            }

            foreach (var device in devicesHashMap){
                // log.LogInformation($"{d.Key}: {d.Value.assetId}");
                await deviceTable.AddAsync(device.Value);
            }
            await deviceTable.FlushAsync();
            return new OkObjectResult(assets);
        }
        static async Task<List<Asset>> ProcessDeviceAsync(Dictionary<string, Device> devicesHashMap)
        {
            if (devicesHashMap.Count == 1)
            {
                using HttpClient getClient = GetHttpClient(Environment.GetEnvironmentVariable("GET_API_KEY"));

                var enumerator = devicesHashMap.GetEnumerator();
                enumerator.MoveNext();
                string anElement = enumerator.Current.Key;
                
                string getRequestUrl = Environment.GetEnvironmentVariable("API_HOST") + anElement;
                using HttpResponseMessage getResponse = await getClient.GetAsync(getRequestUrl);
                // getResponse.EnsureSuccessStatusCode();
                Asset asset = JsonConvert.DeserializeObject<Asset>(getResponse.Content.ReadAsStringAsync().Result);
                
                return new List<Asset>{asset};
            }

            using HttpClient postClient = GetHttpClient(Environment.GetEnvironmentVariable("POST_API_KEY"));
            string postRequestUrl = Environment.GetEnvironmentVariable("API_HOST");
            using HttpResponseMessage postResponse = await postClient.PostAsJsonAsync(
                postRequestUrl, 
                new jsonContent(deviceIds:devicesHashMap.Keys));
            Assets assets = JsonConvert.DeserializeObject<Assets>(postResponse.Content.ReadAsStringAsync().Result);
            return assets.devices;
        }
        public static HttpClient GetHttpClient(string password){
            HttpClient client = new HttpClient(new RetryHandler(new HttpClientHandler()));
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("x-functions-key", password);
            return client;
        }
        
    }
}
