using System.Text.Json.Serialization;

namespace EnterpriseChatBot.Mobile.Models;

public class ChatSessionItem
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("user_id")]
    public Guid UserId { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("model_name")]
    public string ModelName { get; set; } = "GigaChat-2-Pro";

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonIgnore]
    public string LastMessage { get; set; } = string.Empty;

    [JsonIgnore]
    public string CreatedAtText => CreatedAt == default
        ? ""
        : CreatedAt.ToString("dd.MM.yyyy HH:mm");
}