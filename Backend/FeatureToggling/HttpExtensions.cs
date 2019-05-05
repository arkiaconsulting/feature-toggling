using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace FeatureToggling
{
    public static class HttpExtensions
    {
        public static async Task<T> GetAs<T>(this HttpClient httpClient, string requestUri)
        {
            var json = await httpClient.GetStringAsync(requestUri);

            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
