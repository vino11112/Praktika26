using EnterpriseChatBot.Mobile.Models;
using EnterpriseChatBot.Mobile.Services;

namespace EnterpriseChatBot.Mobile.Pages;

public partial class ChatListPage : ContentPage
{
    private readonly ChatSessionService _chatSessionService;
    private readonly ChatMessageService _chatMessageService;
    private readonly ChatDocumentStateService _chatDocumentStateService;

    public ChatListPage(
        ChatSessionService chatSessionService,
        ChatMessageService chatMessageService,
        ChatDocumentStateService chatDocumentStateService)
    {
        InitializeComponent();
        _chatSessionService = chatSessionService;
        _chatMessageService = chatMessageService;
        _chatDocumentStateService = chatDocumentStateService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadChatsAsync();
    }

    private async Task LoadChatsAsync()
    {
        try
        {
            if (!AppSession.CurrentUserId.HasValue)
            {
                await Shell.Current.GoToAsync("//LoginPage");
                return;
            }

            WelcomeLabel.Text = string.IsNullOrWhiteSpace(AppSession.CurrentUserName)
                ? "Список чатов"
                : $"Привет, {AppSession.CurrentUserName}";

            var chats = await _chatSessionService.GetChatsByUserIdAsync(AppSession.CurrentUserId.Value);

            foreach (var chat in chats)
            {
                var lastMessage = await _chatMessageService.GetLastMessageByChatIdAsync(chat.Id);
                chat.LastMessage = lastMessage?.Content ?? "Нет сообщений";
            }

            ChatsCollection.ItemsSource = chats
                .OrderByDescending(x => x.UpdatedAt == default ? x.CreatedAt : x.UpdatedAt)
                .ToList();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private async void OnCreateChatClicked(object sender, EventArgs e)
    {
        try
        {
            if (!AppSession.CurrentUserId.HasValue)
            {
                await DisplayAlert("Ошибка", "Пользователь не авторизован", "OK");
                return;
            }

            string result = await DisplayPromptAsync(
                "Новый чат",
                "Введите название чата:",
                initialValue: $"Чат {DateTime.Now:HHmmss}");

            if (string.IsNullOrWhiteSpace(result))
                return;

            var now = DateTime.UtcNow;

            var created = await _chatSessionService.CreateChatAsync(new ChatSessionItem
            {
                Id = Guid.NewGuid(),
                UserId = AppSession.CurrentUserId.Value,
                Title = result,
                ModelName = "GigaChat-2-Pro",
                CreatedAt = now,
                UpdatedAt = now
            });

            await LoadChatsAsync();

            await Shell.Current.GoToAsync(
                $"{nameof(ChatPage)}?chatId={created.Id}&chatTitle={Uri.EscapeDataString(created.Title)}");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private async void OnChatSelected(object sender, SelectionChangedEventArgs e)
    {
        var selectedChat = e.CurrentSelection.FirstOrDefault() as ChatSessionItem;
        if (selectedChat == null)
            return;

        ((CollectionView)sender).SelectedItem = null;

        await Shell.Current.GoToAsync(
            $"{nameof(ChatPage)}?chatId={selectedChat.Id}&chatTitle={Uri.EscapeDataString(selectedChat.Title)}");
    }

    private async void OnDeleteChatInvoked(object sender, EventArgs e)
    {
        try
        {
            if (sender is not SwipeItem swipeItem)
                return;

            if (swipeItem.CommandParameter is not Guid chatId)
                return;

            var confirm = await DisplayAlert(
                "Удалить чат",
                "Удалить чат и все его сообщения?",
                "Да",
                "Нет");

            if (!confirm)
                return;

            await _chatMessageService.DeleteMessagesByChatIdAsync(chatId);
            await _chatSessionService.DeleteChatAsync(chatId);
            await _chatDocumentStateService.ClearAsync(chatId);

            await LoadChatsAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        AppSession.Clear();
        await Shell.Current.GoToAsync("//LoginPage");
    }
}