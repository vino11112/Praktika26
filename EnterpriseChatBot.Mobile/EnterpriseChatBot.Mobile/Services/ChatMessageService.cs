using System.Text;
using System.Text.Json;
using EnterpriseChatBot.Mobile.Models;

namespace EnterpriseChatBot.Mobile.Services;

public class ChatMessageService
{
    private readonly SupabaseHttpClient _supabaseHttpClient;
    private readonly HttpClient _httpClient;

    public ChatMessageService(SupabaseHttpClient client)
    {
        _supabaseHttpClient = client;
        _httpClient = client.HttpClient;
    }

    public async Task<List<ChatMessageItem>> GetMessagesByChatIdAsync(Guid chatId)
    {
        _supabaseHttpClient.EnsureConfigured();

        var response = await _httpClient.GetAsync(
            $"chat_messages?chat_id=eq.{chatId}&order=created_at.asc");

        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Ошибка загрузки сообщений: {json}");

        var result = JsonSerializer.Deserialize<List<ChatMessageItem>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result ?? new List<ChatMessageItem>();
    }

    public async Task<ChatMessageItem?> GetLastMessageByChatIdAsync(Guid chatId)
    {
        _supabaseHttpClient.EnsureConfigured();

        var response = await _httpClient.GetAsync(
            $"chat_messages?chat_id=eq.{chatId}&order=created_at.desc&limit=1");

        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Ошибка загрузки последнего сообщения: {json}");

        var result = JsonSerializer.Deserialize<List<ChatMessageItem>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result?.FirstOrDefault();
    }

    public async Task SaveMessageAsync(ChatMessageItem message)
    {
        _supabaseHttpClient.EnsureConfigured();

        var response = await _httpClient.PostAsync(
            "chat_messages",
            new StringContent(
                JsonSerializer.Serialize(message),
                Encoding.UTF8,
                "application/json"));

        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Ошибка сохранения сообщения: {json}");
    }

    public async Task DeleteMessagesByChatIdAsync(Guid chatId)
    {
        _supabaseHttpClient.EnsureConfigured();

        var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"chat_messages?chat_id=eq.{chatId}");

        var response = await _httpClient.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Ошибка удаления сообщений: {json}");
    }
}