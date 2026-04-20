using System.Text.Json.Serialization;

namespace EnterpriseChatBot.Mobile.Models;

public class ChatMessageItem
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("chat_id")]
    public Guid ChatId { get; set; }

    [JsonPropertyName("sender_type")]
    public string SenderType { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonIgnore]
    public bool IsUser => SenderType == "user";

    [JsonIgnore]
    public string Text => Content;

    [JsonIgnore]
    public string TimeText => CreatedAt == default ? "" : CreatedAt.ToString("HH:mm");

    [JsonIgnore]
    public LayoutOptions HorizontalAlignment => IsUser ? LayoutOptions.End : LayoutOptions.Start;

    [JsonIgnore]
    public Color BubbleColor => IsUser ? Color.FromArgb("#5B3DF5") : Colors.White;

    [JsonIgnore]
    public Color TextColor => IsUser ? Colors.White : Color.FromArgb("#1F2937");
}