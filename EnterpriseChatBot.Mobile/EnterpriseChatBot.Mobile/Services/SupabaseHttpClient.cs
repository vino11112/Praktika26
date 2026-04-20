using System.Net.Http.Headers;
using EnterpriseChatBot.Mobile.Models;

namespace EnterpriseChatBot.Mobile.Services;

public class SupabaseHttpClient
{
    public HttpClient HttpClient { get; }

    public SupabaseHttpClient()
    {
        HttpClient = new HttpClient();

        if (!string.IsNullOrWhiteSpace(AppConfig.SupabaseUrl) &&
            !string.IsNullOrWhiteSpace(AppConfig.SupabaseKey))
        {
            ConfigureClient();
        }
    }

    public void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(AppConfig.SupabaseUrl) ||
            string.IsNullOrWhiteSpace(AppConfig.SupabaseKey))
        {
            throw new InvalidOperationException("Заполни SupabaseUrl и SupabaseKey в AppConfig.");
        }

        if (HttpClient.BaseAddress == null)
        {
            ConfigureClient();
        }
    }

    private void ConfigureClient()
    {
        HttpClient.BaseAddress = new Uri($"{AppConfig.SupabaseUrl}/rest/v1/");
        HttpClient.DefaultRequestHeaders.Clear();
        HttpClient.DefaultRequestHeaders.Add("apikey", AppConfig.SupabaseKey);
        HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", AppConfig.SupabaseKey);
    }
}