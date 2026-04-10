using EnterpriseChatBot.Desktop.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseChatBot.Desktop.Services
{
    public class SupabaseHttpClient
    {
        public HttpClient HttpClient { get; }

        public SupabaseHttpClient()
        {
            HttpClient = new HttpClient();
            HttpClient.BaseAddress = new Uri($"{AppSettings.SupabaseUrl}/rest/v1/");
            HttpClient.DefaultRequestHeaders.Clear();
            HttpClient.DefaultRequestHeaders.Add("apikey", AppSettings.SupabaseApiKey);
            HttpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AppSettings.SupabaseApiKey);
        }
    }
}
