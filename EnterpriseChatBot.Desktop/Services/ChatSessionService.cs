using EnterpriseChatBot.Desktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EnterpriseChatBot.Desktop.Services
{
    public class ChatSessionService
    {
        private readonly HttpClient _httpClient;

        public ChatSessionService(SupabaseHttpClient client)
        {
            _httpClient = client.HttpClient;
        }

        public async Task<List<ChatSessionDto>> GetChatsByUserIdAsync(Guid userId)
        {
            var response = await _httpClient.GetAsync($"chat_sessions?user_id=eq.{userId}&order=created_at.asc");
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Ошибка загрузки чатов: {json}");

            var result = JsonSerializer.Deserialize<List<ChatSessionDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result ?? new List<ChatSessionDto>();
        }

        public async Task<ChatSessionDto> CreateChatAsync(ChatSessionDto chat)
        {
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

            var result = JsonSerializer.Deserialize<List<ChatSessionDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.FirstOrDefault()
                   ?? throw new InvalidOperationException("Supabase не вернул созданный чат.");
        }
    }
}
