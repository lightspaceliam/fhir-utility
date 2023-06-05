using System;
using System.Net.Http.Headers;
using System.Text;

namespace Common.Extensions
{
	public static class HttpExtensions
	{
        public static HttpClient CreateHttpClient(string baseUrl, string username, string password)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", CreateBasicAuthentication(username, password));

            return httpClient;
        }

        public static string CreateBasicAuthentication(string username, string password)
        {
            var bytes = Encoding.ASCII.GetBytes($"{username}:{password}");
            return Convert.ToBase64String(bytes);
        }

        public static async Task<string> GetRequestAsync(this HttpClient httpClient, string uri)
        {
            using (var httpResponseMessage = await httpClient.GetAsync(uri))
            {
                var content = await httpResponseMessage.Content.ReadAsStringAsync();
                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    throw new Exception($"Something went wrong: {httpResponseMessage.StatusCode}");
                }
                return content;
            }
        }
    }
}

