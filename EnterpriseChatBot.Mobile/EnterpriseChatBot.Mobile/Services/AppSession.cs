namespace EnterpriseChatBot.Mobile.Services;

public static class AppSession
{
    public static Guid? CurrentUserId { get; set; }
    public static string CurrentUserName { get; set; } = string.Empty;
    public static string CurrentUserEmail { get; set; } = string.Empty;

    public static bool IsAuthorized => CurrentUserId.HasValue;

    public static void Clear()
    {
        CurrentUserId = null;
        CurrentUserName = string.Empty;
        CurrentUserEmail = string.Empty;
    }
}