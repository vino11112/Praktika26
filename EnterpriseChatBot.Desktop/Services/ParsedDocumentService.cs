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
    public class ParsedDocumentService
    {
        private readonly HttpClient _httpClient;

        public ParsedDocumentService(SupabaseHttpClient client)
        {
            _httpClient = client.HttpClient;
        }

        public async Task SaveDocumentAsync(ParsedDocumentDto document)
        {
            var response = await _httpClient.PostAsync(
                "parsed_documents",
                new StringContent(
                    JsonSerializer.Serialize(document),
                    Encoding.UTF8,
                    "application/json"));

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Ошибка сохранения документа: {json}");
        }
    }
}
