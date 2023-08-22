using System.Threading;
using System.Net.Http;
using System.Threading.Tasks;

namespace AzureDevOps
{
    public class RetryHandler : DelegatingHandler
    {
        // Limiting the number of retries to 100
        private const int MaxRetries = 100;
        public RetryHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        { }

        // Override the delegating method to handle bad request 400
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
}