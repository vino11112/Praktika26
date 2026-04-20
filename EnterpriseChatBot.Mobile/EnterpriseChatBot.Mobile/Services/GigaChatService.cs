using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EnterpriseChatBot.Mobile.Models;

namespace EnterpriseChatBot.Mobile.Services;

public class GigaChatService
{
    private const string OAuthUrl = "https://ngw.devices.sberbank.ru:9443/api/v2/oauth";
    private const string ChatCompletionsUrl = "https://gigachat.devices.sberbank.ru/api/v1/chat/completions";

    private readonly HttpClient _httpClient = new();

    private string? _accessToken;
    private DateTimeOffset _accessTokenExpiresAtUtc;

    public async Task<string> SendMessageAsync(
        string userMessage,
        List<ChatRequestMessage> history,
        string? documentContext)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
            return "Сообщение пустое.";

        EnsureConfigured();

        var accessToken = await GetAccessTokenAsync();
        var messages = BuildMessages(userMessage, history, documentContext);

        var requestBody = new ChatCompletionsRequest
        {
            Model = AppConfig.GigaChatModel,
            Messages = messages,
            Temperature = 0.7m,
            TopP = 0.9m,
            Stream = false
        };

        var requestJson = JsonSerializer.Serialize(requestBody, JsonOptions());

        using var request = new HttpRequestMessage(HttpMethod.Post, ChatCompletionsUrl);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request);
        var responseJson = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Ошибка GigaChat: {response.StatusCode}\n{responseJson}");

        var parsed = JsonSerializer.Deserialize<ChatCompletionsResponse>(responseJson, JsonOptions());

        var content = parsed?.Choices?
            .FirstOrDefault()?
            .Message?
            .Content?
            .Trim();

        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("GigaChat вернул пустой ответ.");

        return content;
    }

    public Task<string> ProcessFileAsync(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return Task.FromResult("Имя файла не указано.");

        return Task.FromResult($"Файл {fileName} загружен и его содержимое добавлено в контекст.");
    }

    private async Task<string> GetAccessTokenAsync()
    {
        if (!string.IsNullOrWhiteSpace(_accessToken) &&
            _accessTokenExpiresAtUtc > DateTimeOffset.UtcNow.AddMinutes(1))
        {
            return _accessToken;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, OAuthUrl);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.TryAddWithoutValidation("RqUID", Guid.NewGuid().ToString());
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", AppConfig.GigaChatAuthorizationKey);

        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["scope"] = "GIGACHAT_API_PERS"
        });

        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

        using var response = await _httpClient.SendAsync(request);
        var responseJson = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Ошибка получения токена GigaChat: {response.StatusCode}\n{responseJson}");

        var tokenResponse = JsonSerializer.Deserialize<OAuthTokenResponse>(responseJson, JsonOptions());

        if (tokenResponse is null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            throw new InvalidOperationException("Не удалось получить access token GigaChat.");

        _accessToken = tokenResponse.AccessToken;
        _accessTokenExpiresAtUtc = ParseExpiresAt(tokenResponse.ExpiresAt);

        return _accessToken;
    }

    private static DateTimeOffset ParseExpiresAt(long expiresAt)
    {
        // На случай, если сервер вернет milliseconds вместо seconds.
        // 10^12+ почти наверняка миллисекунды.
        return expiresAt >= 1_000_000_000_000
            ? DateTimeOffset.FromUnixTimeMilliseconds(expiresAt)
            : DateTimeOffset.FromUnixTimeSeconds(expiresAt);
    }

    private static List<GigaChatMessage> BuildMessages(
        string userMessage,
        List<ChatRequestMessage> history,
        string? documentContext)
    {
        var messages = new List<GigaChatMessage>();

        var systemPrompt = BuildSystemPrompt(documentContext);
        messages.Add(new GigaChatMessage
        {
            Role = "system",
            Content = systemPrompt
        });

        if (history.Count > 0)
        {
            foreach (var item in history)
            {
                if (string.IsNullOrWhiteSpace(item.Content))
                    continue;

                messages.Add(new GigaChatMessage
                {
                    Role = NormalizeRole(item.Role),
                    Content = item.Content
                });
            }
        }

        messages.Add(new GigaChatMessage
        {
            Role = "user",
            Content = userMessage
        });

        return messages;
    }

    private static string BuildSystemPrompt(string? documentContext)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Ты полезный ассистент.");
        sb.AppendLine("Отвечай по-русски.");
        sb.AppendLine("Используй историю диалога.");
        sb.AppendLine("Если приложен текст документа, опирайся на него в первую очередь.");
        sb.AppendLine("Если данных недостаточно, честно скажи об этом.");
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(documentContext))
        {
            sb.AppendLine("Контекст документа:");
            sb.AppendLine(documentContext);
        }

        return sb.ToString().Trim();
    }

    private static string NormalizeRole(string role)
    {
        return role switch
        {
            "assistant" => "assistant",
            "system" => "system",
            _ => "user"
        };
    }

    private static JsonSerializerOptions JsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    private static void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(AppConfig.GigaChatAuthorizationKey))
            throw new InvalidOperationException("Заполни GigaChatAuthorizationKey в AppConfig.");
    }

    private sealed class OAuthTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_at")]
        public long ExpiresAt { get; set; }
    }

    private sealed class ChatCompletionsRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("messages")]
        public List<GigaChatMessage> Messages { get; set; } = new();

        [JsonPropertyName("temperature")]
        public decimal Temperature { get; set; }

        [JsonPropertyName("top_p")]
        public decimal TopP { get; set; }

        [JsonPropertyName("stream")]
        public bool Stream { get; set; }
    }

    private sealed class GigaChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    private sealed class ChatCompletionsResponse
    {
        [JsonPropertyName("choices")]
        public List<ChatChoice>? Choices { get; set; }
    }

    private sealed class ChatChoice
    {
        [JsonPropertyName("message")]
        public ChatChoiceMessage? Message { get; set; }
    }

    private sealed class ChatChoiceMessage
    {
        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }
}