using System.Text.Json;

namespace EnterpriseChatBot.Mobile.Services;

public class ChatDocumentStateService
{
    private const string Prefix = "chat_document_state_";

    public Task SaveAsync(Guid chatId, string fileName, string content)
    {
        var dto = new ChatDocumentStateDto
        {
            FileName = fileName,
            Content = content
        };

        var json = JsonSerializer.Serialize(dto);
        Preferences.Default.Set(Prefix + chatId, json);
        return Task.CompletedTask;
    }

    public Task<(string? FileName, string? Content)> GetAsync(Guid chatId)
    {
        var json = Preferences.Default.Get(Prefix + chatId, string.Empty);

        if (string.IsNullOrWhiteSpace(json))
            return Task.FromResult<(string?, string?)>((null, null));

        try
        {
            var dto = JsonSerializer.Deserialize<ChatDocumentStateDto>(json);
            return Task.FromResult<(string?, string?)>((dto?.FileName, dto?.Content));
        }
        catch
        {
            return Task.FromResult<(string?, string?)>((null, null));
        }
    }

    public Task ClearAsync(Guid chatId)
    {
        Preferences.Default.Remove(Prefix + chatId);
        return Task.CompletedTask;
    }

    private sealed class ChatDocumentStateDto
    {
        public string FileName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}