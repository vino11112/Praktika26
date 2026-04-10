using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EnterpriseChatBot.Desktop.Library.Interfaces;
using EnterpriseChatBot.Desktop.Library.Models;

namespace EnterpriseChatBot.Desktop.Library.Clients;

public class GigaChatClient : IGigaChatClient
{
    private readonly HttpClient _httpClient;
    private readonly GigaChatOptions _options;

    private string? _accessToken;
    private DateTimeOffset _tokenExpiresAtUtc = DateTimeOffset.MinValue;

    public GigaChatClient(HttpClient httpClient, GigaChatOptions options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<string> SendMessageAsync(
        string userMessage,
        IEnumerable<ChatTurn>? history = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
            throw new ArgumentException("Сообщение не может быть пустым.", nameof(userMessage));

        var token = await GetAccessTokenAsync(cancellationToken);

        var messages = new List<object>();

        if (history is not null)
        {
            foreach (var item in history)
            {
                if (string.IsNullOrWhiteSpace(item.Role) || string.IsNullOrWhiteSpace(item.Content))
                    continue;

                messages.Add(new
                {
                    role = item.Role,
                    content = item.Content
                });
            }
        }

        var finalUserMessage = string.IsNullOrWhiteSpace(_options.SystemPrompt)
            ? userMessage
            : $"{_options.SystemPrompt}\n\nЗапрос пользователя:\n{userMessage}";

        messages.Add(new
        {
            role = "user",
            content = finalUserMessage
        });

        var requestBody = new
        {
            model = _options.Model,
            messages = messages,
            stream = false
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.ChatUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var rawResponse = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"Ошибка запроса к GigaChat: {(int)response.StatusCode} {response.ReasonPhrase}. {rawResponse}");

        using var json = JsonDocument.Parse(rawResponse);

        var content = json.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return content ?? string.Empty;
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_accessToken) &&
            _tokenExpiresAtUtc > DateTimeOffset.UtcNow.AddMinutes(1))
        {
            return _accessToken;
        }

        if (string.IsNullOrWhiteSpace(_options.AuthKey))
            throw new InvalidOperationException("Не указан GigaChat Authorization Key.");

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.AuthUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _options.AuthKey);
        request.Headers.Add("RqUID", Guid.NewGuid().ToString());
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["scope"] = _options.Scope
        });

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var rawResponse = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"Ошибка получения токена GigaChat: {(int)response.StatusCode} {response.ReasonPhrase}. {rawResponse}");

        using var json = JsonDocument.Parse(rawResponse);

        _accessToken = json.RootElement.GetProperty("access_token").GetString();

        var expiresAt = json.RootElement.GetProperty("expires_at").GetInt64();
        _tokenExpiresAtUtc = DateTimeOffset.FromUnixTimeMilliseconds(expiresAt);

        if (string.IsNullOrWhiteSpace(_accessToken))
            throw new InvalidOperationException("GigaChat не вернул access_token.");

        return _accessToken;
    }
}