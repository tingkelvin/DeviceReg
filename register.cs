using System;
using System.IO;
using System.Threading.Tasks;


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
    }
}
