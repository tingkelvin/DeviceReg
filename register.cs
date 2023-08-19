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

using Newtonsoft.Json.Linq;

namespace AzureDevOps
{
    public record class device(
        [property: JsonPropertyName("deviceId")] string deviceId,
        [property: JsonPropertyName("assetId")] string assetId
    );

    public class MyClass
    {
        public List<device> devices { get; set; }
  
    }

public record class d(
    List<string> deviceIds = null);

    public static class register
    {

        public record class Repository(
            [property: JsonPropertyName("name")] string Name);

        [FunctionName("register")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // Newtonsoft.Json.Linq.JArray devices = req["devices"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            JArray devices = data?.devices;
            List<string> deviceIDs = new List<string>{};
            foreach (JObject item in devices) // <-- Note that here we used JObject instead of usual JProperty
            {
                string id = item.GetValue("id").ToString();
                // string url = item.GetValue("url").ToString();
                deviceIDs.Add(id);
                // ...
            }
      
            using HttpClient client = GetHttpClient("DRefJc8eEDyJzS19qYAKopSyWW8ijoJe8zcFhH5J1lhFtChC56ZOKQ==");
            // using HttpClient client = new();
            // client.DefaultRequestHeaders.Accept.Clear();
            // client.DefaultRequestHeaders.Accept.Add(
            //     new MediaTypeWithQualityHeaderValue("application/json"));
            // client.DefaultRequestHeaders.Add("x-functions-key", "yeK7CM/Pj2vA3MFpuBxIFX7QIl1cKFOiviZaOjtVCrTq0VUzKeQjfw==");
            await ProcessDevicesAsync(client, deviceIDs, log);

            // log.LogInformation($"deviceId: {device.deviceId}");
            // log.LogInformation($"assetId: {device.assetId}");
            // foreach (var repo in repositories)
            // {
            //     log.LogInformation($"Name: {repo.Name}");
            //     log.LogInformation($"Homepage: {repo.Homepage}");
            //     log.LogInformation($"GitHub: {repo.GitHubHomeUrl}");
            //     log.LogInformation($"Description: {repo.Description}");
            //     log.LogInformation($"Watchers: {repo.Watchers:#,0}");
            //     log.LogInformation($"{repo.LastPush}");
            //     log.LogInformation();
            // }

            return new OkObjectResult("hi");
        }
        static async Task<device> ProcessDeviceAsync(HttpClient client)
        {
            await using Stream stream =
                await client.GetStreamAsync("http://tech-assessment.vnext.com.au/api/devices/assetId/DVID00000125");
            var device = await System.Text.Json.JsonSerializer.DeserializeAsync<device>(stream);
            return device;
        }
        static async Task<MyClass> ProcessDevicesAsync(HttpClient client, List<string> deviceIDs, ILogger log)
        {
             using HttpResponseMessage response = await client.PostAsJsonAsync(
                "http://tech-assessment.vnext.com.au/api/devices/assetId/", 
                new d(deviceIds:deviceIDs));

            var deserializedObject = JsonConvert.DeserializeObject<MyClass>(response.Content.ReadAsStringAsync().Result);
            log.LogInformation(response.Content.ReadAsStringAsync().Result);
            return deserializedObject;

            // using StringContent jsonContent = new(
            //     System.Text.Json.JsonSerializer.Serialize(new
            //     {
            //         deviceIDs = deviceIDs
            //     }),
            //     Encoding.UTF8,
            //     "application/json");
            // using HttpResponseMessage response =
            //     await client.PostAsync("http://tech-assessment.vnext.com.au/api/devices/assetId/", jsonContent);
            // var device = await System.Text.Json.JsonSerializer.DeserializeAsync<List<device>>(stream);
            // return device;
        }

        public static HttpClient GetHttpClient(string password){
            // "yeK7CM/Pj2vA3MFpuBxIFX7QIl1cKFOiviZaOjtVCrTq0VUzKeQjfw=="
            HttpClient client = new();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("x-functions-key", password);
            return client;
        }
    }
}
