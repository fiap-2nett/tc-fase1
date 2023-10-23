using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace TechChallenge.Api.IntegrationTests.Extensions
{
    internal static class HttpResponseMessageExtensions
    {
        #region Extension Methods

        public static async Task<TResult> ReadJsonContentAsAsync<TResult>(this HttpResponseMessage responseMessage, JsonSerializerSettings settings = null)
        {
            var content = await responseMessage.Content.ReadAsStringAsync();

            if (typeof(TResult) == typeof(string))            
                content = $"\"{content}\"";

            return JsonConvert.DeserializeObject<TResult>(content, settings);
        }

        #endregion
    }
}
