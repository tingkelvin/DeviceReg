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
    public record class Device(
        [property: JsonPropertyName("deviceId")] string deviceId,
        [property: JsonPropertyName("assetId")] string assetId
    );

    public record class Devices(
        [property: JsonPropertyName("devices")] List<Device> devices
    );
    public record class jsonContent(
        List<string> deviceIds = null);

    public static class register
    {
        [FunctionName("register")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            JArray devices = data?.devices;
            List<string> deviceIds = new List<string>{};
            
            foreach (JObject item in devices) // <-- Note that here we used JObject instead of usual JProperty
            {
                string id = item.GetValue("id").ToString();
                deviceIds.Add(id);
            }

            if (deviceIds.Count == 1)
            {
                using HttpClient client = GetHttpClient("yeK7CM/Pj2vA3MFpuBxIFX7QIl1cKFOiviZaOjtVCrTq0VUzKeQjfw==");
                Device ret = await ProcessDeviceAsync(client, deviceIds[0]);
                return new OkObjectResult(ret);
            }

            if (deviceIds.Count > 1)
            {
                using HttpClient client = GetHttpClient("DRefJc8eEDyJzS19qYAKopSyWW8ijoJe8zcFhH5J1lhFtChC56ZOKQ==");
                Devices ret = await ProcessDevicesAsync(client, deviceIds);
                return new OkObjectResult(ret);
    
            }
            return new OkObjectResult("hi");
        }
        static async Task<Device> ProcessDeviceAsync(HttpClient client, string deviceID)
        {
            string getRequestUrl = "http://tech-assessment.vnext.com.au/api/devices/assetId/DVID00000125" + deviceID;
            await using Stream stream =
                await client.GetStreamAsync(getRequestUrl);
            Device device = await System.Text.Json.JsonSerializer.DeserializeAsync<Device>(stream);
            return device;
        }
        static async Task<Devices> ProcessDevicesAsync(HttpClient client, List<string> deviceIds)
        {
            string postRequestUrl = "http://tech-assessment.vnext.com.au/api/devices/assetId/";
            using HttpResponseMessage response = await client.PostAsJsonAsync(
                postRequestUrl, 
                new jsonContent(deviceIds:deviceIds));
            var deserializedObject = JsonConvert.DeserializeObject<Devices>(response.Content.ReadAsStringAsync().Result);
            return deserializedObject;
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
