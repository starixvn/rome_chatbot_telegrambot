using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Client.Extensions
{
    public static class HttpClientExtensions
    {
        public static async Task<string> PostAsStringAsync(this HttpClient client, string requestUri, object value = null)
        {
            var json = value != null ? JsonConvert.SerializeObject(value) : string.Empty;
            using var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = stringContent
            };
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<T> PostAsJsonAsync<T>(this HttpClient client, string requestUri, object value = null)
        {
            var content = await PostAsStringAsync(client, requestUri, value);
            return JsonConvert.DeserializeObject<T>(content);
        }
    }
}
