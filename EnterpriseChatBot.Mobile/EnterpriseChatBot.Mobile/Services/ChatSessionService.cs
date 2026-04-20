using System.Text;
using System.Text.Json;
using EnterpriseChatBot.Mobile.Models;

namespace EnterpriseChatBot.Mobile.Services;

public class ChatSessionService
{
    private readonly SupabaseHttpClient _supabaseHttpClient;
    private readonly HttpClient _httpClient;

    public ChatSessionService(SupabaseHttpClient client)
    {
        _supabaseHttpClient = client;
        _httpClient = client.HttpClient;
    }

    public async Task<List<ChatSessionItem>> GetChatsByUserIdAsync(Guid userId)
    {
        _supabaseHttpClient.EnsureConfigured();

        var response = await _httpClient.GetAsync(
            $"chat_sessions?user_id=eq.{userId}&order=updated_at.desc");

        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Ошибка загрузки чатов: {json}");

        var result = JsonSerializer.Deserialize<List<ChatSessionItem>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result ?? new List<ChatSessionItem>();
    }

    public async Task<ChatSessionItem> CreateChatAsync(ChatSessionItem chat)
    {
        _supabaseHttpClient.EnsureConfigured();

        var request = new HttpRequestMessage(HttpMethod.Post, "chat_sessions");
        request.Headers.Add("Prefer", "return=representation");
        request.Content = new StringContent(
            JsonSerializer.Serialize(chat),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Ошибка создания чата: {json}");

        var result = JsonSerializer.Deserialize<List<ChatSessionItem>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result?.FirstOrDefault()
            ?? throw new InvalidOperationException("Supabase не вернул созданный чат.");
    }

    public async Task DeleteChatAsync(Guid chatId)
    {
        _supabaseHttpClient.EnsureConfigured();

        var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"chat_sessions?id=eq.{chatId}");

        var response = await _httpClient.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Ошибка удаления чата: {json}");
    }

    public async Task TouchChatAsync(Guid chatId)
    {
        _supabaseHttpClient.EnsureConfigured();

        var payload = new
        {
            updated_at = DateTime.UtcNow
        };

        var request = new HttpRequestMessage(
            new HttpMethod("PATCH"),
            $"chat_sessions?id=eq.{chatId}");

        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Ошибка обновления чата: {json}");
    }
}