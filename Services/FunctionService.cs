using Azure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuneLib.Services
{
    internal class FunctionService
    {
        private static readonly string? _code = EnvironmentVariables.FunctionsAuthCode;
        private static readonly string? _baseUrl = EnvironmentVariables.FunctionsBaseUrl;



        private static string GetFunctionUrl(string functionName, string parameters = "")
        {
            if (String.IsNullOrEmpty(_code))
                throw new InvalidOperationException("No environment variable found for Functions auth code.");
            if (String.IsNullOrEmpty(_baseUrl))
                throw new InvalidOperationException("No environment variable found for Functions base url.");

            if (FunctionUrls.TryGetValue(functionName, out string functionPath))
            {
                return $"{_baseUrl}{functionPath}?code={_code}{parameters}";
            }
            throw new InvalidOperationException($"Function URL for {functionName} not found.");
        }
        private static readonly Dictionary<string, string> FunctionUrls = new Dictionary<string, string>
        {
            { nameof (ImageGenToUrlAsync), "ImageGen" },
            { nameof (WebContentFromUrlAsync), "ScrapeWebContent" },
            { nameof (WebSummaryFromUrlAsync), "WebSummary" },
        };

        public static async Task<string?> ImageGenToUrlAsync(string prompt, string? style = null)
        {
            using (HttpClient client = new HttpClient())
            {
                string functionUrl = GetFunctionUrl(nameof(ImageGenToUrlAsync), $"&prompt={prompt}");

                if (style != null)
                    functionUrl = GetFunctionUrl(nameof(ImageGenToUrlAsync), $"&prompt={prompt}&style={style}");

                HttpResponseMessage response = await client.GetAsync(functionUrl);

                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsStringAsync();
                else
                    return null;
            }
        }
        public static async Task<string?> WebContentFromUrlAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(GetFunctionUrl(nameof(WebContentFromUrlAsync), $"&url={url}"));
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsStringAsync();
                else
                    throw new HttpRequestException(response.StatusCode + "\n" + response.RequestMessage);
            }
        }
        public static async Task<string?> WebSummaryFromUrlAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(GetFunctionUrl(nameof(WebSummaryFromUrlAsync), $"&url={url}"));
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsStringAsync();
                else
                    throw new HttpRequestException(response.StatusCode + "\n" + response.RequestMessage);
            }
        }
    }
}

