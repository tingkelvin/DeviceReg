using System;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AzureDevOps
{
    public static class register
    {
        [FunctionName("register")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log,
            [Sql(commandText: "dbo.Devices", connectionStringSetting: "SqlConnectionString")] IAsyncCollector<Device> deviceTable)
        {
            // parsing request body
            log.LogInformation("C# HTTP trigger processed a request.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            JArray devices = data?.devices;
        
            Dictionary<string, Device> devicesHashMap = new Dictionary<string, Device>();

            // build the hashmap to store the devices
            foreach (JObject item in devices) 
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

            // api call to retrive assetid
            List<Asset> assets = await ProcessDeviceAsync(devicesHashMap);

            // write the asset id to the hashmap
            foreach(Asset asset in assets)
            {
                devicesHashMap[asset.deviceId].AssetId = asset.assetId;
            }

            // write to sql database
            foreach (var device in devicesHashMap){
                await deviceTable.AddAsync(device.Value);
            }
            await deviceTable.FlushAsync();
            return new OkObjectResult("Succesfully register.");
        }
        public static HttpClient GetHttpClient(string password)
        {
            // create http request client
            HttpClient client = new HttpClient(new RetryHandler(new HttpClientHandler()));
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("x-functions-key", password);
            return client;
        }
        static async Task<List<Asset>> ProcessDeviceAsync(Dictionary<string, Device> devicesHashMap)
        {
            if (devicesHashMap.Count == 1)
            {
                using HttpClient getClient = GetHttpClient(Environment.GetEnvironmentVariable("GET_API_KEY"));

                // get the first element of the hashmap
                var enumerator = devicesHashMap.GetEnumerator();
                enumerator.MoveNext();
                string anElement = enumerator.Current.Key;
                
                // create get request
                string getRequestUrl = Environment.GetEnvironmentVariable("API_HOST") + anElement;
                using HttpResponseMessage getResponse = await getClient.GetAsync(getRequestUrl);
                Asset asset = JsonConvert.DeserializeObject<Asset>(getResponse.Content.ReadAsStringAsync().Result);
                return new List<Asset>{asset};
            }

            using HttpClient postClient = GetHttpClient(Environment.GetEnvironmentVariable("POST_API_KEY"));
            string postRequestUrl = Environment.GetEnvironmentVariable("API_HOST");

            // create post request
            using HttpResponseMessage postResponse = await postClient.PostAsJsonAsync(
                postRequestUrl, 
                new jsonContent(deviceIds:devicesHashMap.Keys));
            Assets assets = JsonConvert.DeserializeObject<Assets>(postResponse.Content.ReadAsStringAsync().Result);
            return assets.devices;
        }
    }
}