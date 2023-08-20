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

namespace AzureDevOps
{
    public record class Asset(
        [property: JsonPropertyName("deviceId")] string deviceId,
        [property: JsonPropertyName("assetId")] string assetId
    );

    public record class Assets(
        [property: JsonPropertyName("devices")] List<Asset> devices
    );

    public class Device
    {
        public string DeviceId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Type { get; set; }
        public string AssetId { get; set; }
    }
    public record class jsonContent(
        Dictionary<string, Device>.KeyCollection deviceIds = null);

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
                await using Stream stream =
                    await getClient.GetStreamAsync(getRequestUrl);
                Asset asset = await System.Text.Json.JsonSerializer.DeserializeAsync<Asset>(stream);
                return new List<Asset>{asset};
            }

            using HttpClient postClient = GetHttpClient(Environment.GetEnvironmentVariable("POST_API_KEY"));
            string postRequestUrl = Environment.GetEnvironmentVariable("API_HOST");
            using HttpResponseMessage response = await postClient.PostAsJsonAsync(
                postRequestUrl, 
                new jsonContent(deviceIds:devicesHashMap.Keys));
            var deserializedObject = JsonConvert.DeserializeObject<Assets>(response.Content.ReadAsStringAsync().Result);
            return deserializedObject.devices;
        }
        public static HttpClient GetHttpClient(string password){
            HttpClient client = new();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("x-functions-key", password);
            return client;
        }
        
    }
}
