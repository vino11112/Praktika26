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
    public class ChatMessageService
    {
        private readonly HttpClient _httpClient;

        public ChatMessageService(SupabaseHttpClient client)
        {
            _httpClient = client.HttpClient;
        }

        public async Task<List<ChatMessageDto>> GetMessagesByChatIdAsync(Guid chatId)
        {
            var response = await _httpClient.GetAsync($"chat_messages?chat_id=eq.{chatId}&order=created_at.asc");
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Ошибка загрузки сообщений: {json}");

            var result = JsonSerializer.Deserialize<List<ChatMessageDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result ?? new List<ChatMessageDto>();
        }

        public async Task SaveMessageAsync(ChatMessageDto message)
        {
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
    }
}
