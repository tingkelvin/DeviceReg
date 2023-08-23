using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace AzureDevOps
{
    class Client
    {
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
        static public async Task<List<Asset>> ProcessDeviceAsync(Dictionary<string, Device> devicesHashMap, string API_GET, string API_POST, string API_HOST)
        {
            if (devicesHashMap.Count == 1)
            {
                using HttpClient getClient = GetHttpClient(API_GET);

                // get the first element of the hashmap
                var enumerator = devicesHashMap.GetEnumerator();
                enumerator.MoveNext();
                string anElement = enumerator.Current.Key;
                
                // create get request
                string getRequestUrl = API_HOST + anElement;
                using HttpResponseMessage getResponse = await getClient.GetAsync(getRequestUrl);
                Asset asset = JsonConvert.DeserializeObject<Asset>(getResponse.Content.ReadAsStringAsync().Result);
                return new List<Asset>{asset};
            }

            using HttpClient postClient = GetHttpClient(API_POST);
            string postRequestUrl = API_HOST;

            // create post request
            using HttpResponseMessage postResponse = await postClient.PostAsJsonAsync(
                postRequestUrl, 
                new jsonContent(deviceIds:devicesHashMap.Keys));
            Assets assets = JsonConvert.DeserializeObject<Assets>(postResponse.Content.ReadAsStringAsync().Result);
            return assets.devices;
        }
    }
}