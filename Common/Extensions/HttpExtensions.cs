using System;
using System.Net;
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
                    if(httpResponseMessage.StatusCode == System.Net.HttpStatusCode.NotFound) 
                    {
                        return "";
                    }

                    throw new Exception($"Something went wrong: {httpResponseMessage.StatusCode}");
                }
                return content;
            }
        }

        public static async Task<string> PostRequestAsync(HttpClient httpClient, string uri, StringContent stringContent)
        {
            using (var httpResponseMessage = await httpClient.PostAsync(uri, stringContent))
            {
                var content = await httpResponseMessage.Content.ReadAsStringAsync();
                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return "";
                    }

                    throw new Exception($"Something went wrong: {httpResponseMessage.StatusCode}");
                }

                return content;
            }
        }

        public static async Task<string> PutRequestAsync(HttpClient httpClient, string uri, StringContent stringContent, AuthenticationHeaderValue authenticationHeaderValue)
        {
            httpClient.DefaultRequestHeaders.Authorization = authenticationHeaderValue;

            using (var httpResponseMessage = await httpClient.PutAsync(uri, stringContent))
            {
                var content = await httpResponseMessage.Content.ReadAsStringAsync();
                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return "";
                    }

                    throw new Exception($"Something went wrong: {httpResponseMessage.StatusCode}");
                }

                return content;
            }
        }

        public static async Task<string> DeleteAsync(HttpClient httpClient, string uri, AuthenticationHeaderValue authenticationHeaderValue)
        {
            httpClient.DefaultRequestHeaders.Authorization = authenticationHeaderValue;

            using (var httpResponseMessage = await httpClient.DeleteAsync(uri))
            {
                var content = await httpResponseMessage.Content.ReadAsStringAsync();
                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return "";
                    }

                    throw new Exception($"Something went wrong: {httpResponseMessage.StatusCode}");
                }

                return content;
            }
        }
    }
}

