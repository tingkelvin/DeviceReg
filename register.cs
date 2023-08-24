using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
            try 
            {
                log.LogInformation("C# HTTP trigger processed a request.");
                // parsing request body
        
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                JArray devices = data?.devices;

                if (devices.Count == 0)
                {
                    return new BadRequestObjectResult("There are no devices.");
                }
            
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
                List<Asset> assets = await Client.ProcessDeviceAsync(devicesHashMap, 
                                                                    Environment.GetEnvironmentVariable("GET_API_KEY"), 
                                                                    Environment.GetEnvironmentVariable("POST_API_KEY"),
                                                                    Environment.GetEnvironmentVariable("API_HOST"));

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
                return new OkObjectResult("Register succesfully");
            }
            catch (JsonReaderException e)
            {
                log.LogInformation(e.ToString());
                return new BadRequestObjectResult("Invalid body, cannot register");
            }
            catch (NullReferenceException e)
            {
                log.LogInformation(e.ToString());
                return new BadRequestObjectResult("Invalid data, cannot register");
            }
            catch (Exception e)
            {
                log.LogInformation(e.ToString());
                return new BadRequestObjectResult("Error, cannot register");
            }
        }
    }
}