using System.Collections.ObjectModel;
using EnterpriseChatBot.Mobile.Models;
using EnterpriseChatBot.Mobile.Services;

namespace EnterpriseChatBot.Mobile.Pages;

[QueryProperty(nameof(ChatId), "chatId")]
[QueryProperty(nameof(ChatTitle), "chatTitle")]
public partial class ChatPage : ContentPage
{
    private readonly ObservableCollection<ChatMessageItem> _messages = new();
    private readonly DocumentPickerService _documentPickerService;
    private readonly ParsedDocumentService _parsedDocumentService;
    private readonly ChatMessageService _chatMessageService;
    private readonly ChatSessionService _chatSessionService;
    private readonly ChatDocumentStateService _chatDocumentStateService;
    private readonly GigaChatService _gigaChatService;

    private Guid _chatId;
    private string _chatTitle = "Чат";
    private string? _selectedDocumentContent;
    private string? _selectedFileName;

    public string ChatId
    {
        get => _chatId.ToString();
        set
        {
            if (Guid.TryParse(Uri.UnescapeDataString(value ?? string.Empty), out var id))
                _chatId = id;
        }
    }

    public string ChatTitle
    {
        get => _chatTitle;
        set => _chatTitle = Uri.UnescapeDataString(value ?? "Чат");
    }

    public ChatPage(
        DocumentPickerService documentPickerService,
        ParsedDocumentService parsedDocumentService,
        ChatMessageService chatMessageService,
        ChatSessionService chatSessionService,
        ChatDocumentStateService chatDocumentStateService,
        GigaChatService gigaChatService)
    {
        InitializeComponent();

        _documentPickerService = documentPickerService;
        _parsedDocumentService = parsedDocumentService;
        _chatMessageService = chatMessageService;
        _chatSessionService = chatSessionService;
        _chatDocumentStateService = chatDocumentStateService;
        _gigaChatService = gigaChatService;

        MessagesCollection.ItemsSource = _messages;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        ChatTitleLabel.Text = string.IsNullOrWhiteSpace(ChatTitle) ? "Чат" : ChatTitle;

        if (_chatId == Guid.Empty)
            return;

        await RestoreDocumentStateAsync();
        await LoadMessagesAsync();
    }

    private async Task RestoreDocumentStateAsync()
    {
        var state = await _chatDocumentStateService.GetAsync(_chatId);
        _selectedFileName = state.FileName;
        _selectedDocumentContent = state.Content;

        SelectedFileLabel.Text = string.IsNullOrWhiteSpace(_selectedFileName)
            ? "Файл не выбран"
            : $"Файл: {_selectedFileName}";
    }

    private async Task LoadMessagesAsync()
    {
        try
        {
            _messages.Clear();

            var items = await _chatMessageService.GetMessagesByChatIdAsync(_chatId);
            foreach (var item in items)
                _messages.Add(item);

            ScrollToLastMessage();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private List<ChatRequestMessage> BuildHistoryWithoutCurrentUserText()
    {
        return _messages
            .Select(x => new ChatRequestMessage
            {
                Role = x.IsUser ? "user" : "assistant",
                Content = x.Content
            })
            .ToList();
    }

    private async void OnSendClicked(object sender, EventArgs e)
    {
        var text = MessageEntry.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(text) || _chatId == Guid.Empty)
            return;

        try
        {
            MessageEntry.IsEnabled = false;
            SetLoading(true);

            var history = BuildHistoryWithoutCurrentUserText();

            var userMessage = new ChatMessageItem
            {
                Id = Guid.NewGuid(),
                ChatId = _chatId,
                SenderType = "user",
                Content = text,
                CreatedAt = DateTime.Now
            };

            await _chatMessageService.SaveMessageAsync(userMessage);
            _messages.Add(userMessage);

            MessageEntry.Text = string.Empty;
            ScrollToLastMessage();

            var botAnswer = await _gigaChatService.SendMessageAsync(
                text,
                history,
                _selectedDocumentContent);

            var botMessage = new ChatMessageItem
            {
                Id = Guid.NewGuid(),
                ChatId = _chatId,
                SenderType = "assistant",
                Content = botAnswer,
                CreatedAt = DateTime.Now
            };

            await _chatMessageService.SaveMessageAsync(botMessage);
            _messages.Add(botMessage);

            await _chatSessionService.TouchChatAsync(_chatId);
            ScrollToLastMessage();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
        finally
        {
            SetLoading(false);
            MessageEntry.IsEnabled = true;
            MessageEntry.Focus();
        }
    }

    private async void OnPickFileClicked(object sender, EventArgs e)
    {
        if (_chatId == Guid.Empty)
            return;

        try
        {
            SetLoading(true);

            var file = await _documentPickerService.PickFileAsync();
            if (file == null)
                return;

            var parsed = await _parsedDocumentService.ParseAsync(file);

            _selectedFileName = parsed.FileName;
            _selectedDocumentContent = parsed.IsSuccess ? parsed.ExtractedText : null;

            if (parsed.IsSuccess && !string.IsNullOrWhiteSpace(_selectedDocumentContent))
            {
                await _chatDocumentStateService.SaveAsync(
                    _chatId,
                    parsed.FileName,
                    _selectedDocumentContent);
            }

            SelectedFileLabel.Text = parsed.IsSuccess
                ? $"Файл: {parsed.FileName}"
                : $"Файл: {parsed.FileName} (ошибка)";

            var userMessage = new ChatMessageItem
            {
                Id = Guid.NewGuid(),
                ChatId = _chatId,
                SenderType = "user",
                Content = $"Выбран файл: {parsed.FileName}",
                CreatedAt = DateTime.Now
            };

            await _chatMessageService.SaveMessageAsync(userMessage);
            _messages.Add(userMessage);
            ScrollToLastMessage();

            string botAnswer = !parsed.IsSuccess
                ? $"Не удалось обработать файл: {parsed.ErrorMessage}"
                : $"Файл {parsed.FileName} подключён к этому чату.";

            var botMessage = new ChatMessageItem
            {
                Id = Guid.NewGuid(),
                ChatId = _chatId,
                SenderType = "assistant",
                Content = botAnswer,
                CreatedAt = DateTime.Now
            };

            await _chatMessageService.SaveMessageAsync(botMessage);
            _messages.Add(botMessage);

            await _chatSessionService.TouchChatAsync(_chatId);
            ScrollToLastMessage();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private async void OnClearDocumentClicked(object sender, EventArgs e)
    {
        if (_chatId == Guid.Empty)
            return;

        _selectedFileName = null;
        _selectedDocumentContent = null;
        SelectedFileLabel.Text = "Файл не выбран";

        await _chatDocumentStateService.ClearAsync(_chatId);
        await DisplayAlert("Готово", "Документ отвязан от чата", "OK");
    }

    private void SetLoading(bool isLoading)
    {
        LoadingIndicator.IsRunning = isLoading;
        LoadingIndicator.IsVisible = isLoading;
    }

    private void ScrollToLastMessage()
    {
        if (_messages.Count == 0)
            return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            MessagesCollection.ScrollTo(_messages[^1], position: ScrollToPosition.End, animate: true);
        });
    }
}