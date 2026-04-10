using EnterpriseChatBot.Desktop.Commands;
using EnterpriseChatBot.Desktop.Config;
using EnterpriseChatBot.Desktop.Library.Clients;
using EnterpriseChatBot.Desktop.Library.Models;
using EnterpriseChatBot.Desktop.Library.Parsers;
using EnterpriseChatBot.Desktop.Models;
using EnterpriseChatBot.Desktop.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;

namespace EnterpriseChatBot.Desktop.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly GigaChatClient _gigaChatClient;
    private readonly DocumentParserService _documentParserService;
    private readonly ChatSessionService _chatSessionService;
    private readonly ChatMessageService _chatMessageService;
    private readonly ParsedDocumentService _parsedDocumentService;

    private string _userInput = string.Empty;
    private bool _isBusy;
    private ChatItem? _selectedChat;
    private string? _loadedDocumentContent;

    public ObservableCollection<ChatItem> Chats { get; } = new();
    public ObservableCollection<string> Messages { get; } = new();
    public ObservableCollection<ChatTurn> History { get; } = new();

    public RelayCommand SendMessageCommand { get; }
    public RelayCommand NewChatCommand { get; }
    public RelayCommand LoadFileCommand { get; }

    public string UserInput
    {
        get => _userInput;
        set
        {
            _userInput = value;
            OnPropertyChanged();
            SendMessageCommand.RaiseCanExecuteChanged();
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged();
            SendMessageCommand.RaiseCanExecuteChanged();
        }
    }

    public ChatItem? SelectedChat
    {
        get => _selectedChat;
        set
        {
            _selectedChat = value;
            OnPropertyChanged();
            _ = LoadSelectedChatMessagesAsync();
        }
    }

    public MainViewModel()
    {
        var gigaHttpClient = new HttpClient();
        var gigaOptions = new GigaChatOptions
        {
            AuthKey = AppSettings.GigaChatAuthKey,
            Scope = AppSettings.GigaChatScope,
            Model = AppSettings.GigaChatModel
        };

        _gigaChatClient = new GigaChatClient(gigaHttpClient, gigaOptions);
        _documentParserService = new DocumentParserService();

        var supabaseClient = new SupabaseHttpClient();
        _chatSessionService = new ChatSessionService(supabaseClient);
        _chatMessageService = new ChatMessageService(supabaseClient);
        _parsedDocumentService = new ParsedDocumentService(supabaseClient);

        SendMessageCommand = new RelayCommand(async () => await SendMessageAsync(), CanSendMessage);
        NewChatCommand = new RelayCommand(async () => await CreateNewChatAsync());
        LoadFileCommand = new RelayCommand(async () => await LoadFileAsync());

        _ = LoadChatsAsync();
    }

    private bool CanSendMessage()
    {
        return !IsBusy &&
               !string.IsNullOrWhiteSpace(UserInput) &&
               SelectedChat is not null;
    }

    private async Task LoadChatsAsync()
    {
        try
        {
            Chats.Clear();

            var chats = await _chatSessionService.GetChatsByUserIdAsync(AppSession.CurrentUser!.Id);

            foreach (var chat in chats)
            {
                Chats.Add(new ChatItem
                {
                    Id = chat.Id,
                    Title = chat.Title
                });
            }

            if (Chats.Count == 0)
            {
                await CreateNewChatAsync();
            }
            else
            {
                SelectedChat = Chats[0];
            }
        }
        catch (Exception ex)
        {
            Messages.Clear();
            Messages.Add($"Ошибка загрузки чатов: {ex.Message}");
        }
    }

    private async Task CreateNewChatAsync()
    {
        try
        {
            var newChat = new ChatSessionDto
            {
                Id = Guid.NewGuid(),
                UserId = AppSession.CurrentUser!.Id,
                Title = $"Чат {Chats.Count + 1}",
                ModelName = AppSettings.GigaChatModel
            };

            var created = await _chatSessionService.CreateChatAsync(newChat);

            var item = new ChatItem
            {
                Id = created.Id,
                Title = created.Title
            };

            Chats.Add(item);
            SelectedChat = item;

            Messages.Clear();
            History.Clear();
            Messages.Add("Бот: Новый чат создан.");
        }
        catch (Exception ex)
        {
            Messages.Add($"Ошибка создания чата: {ex.Message}");
        }
    }

    private async Task LoadSelectedChatMessagesAsync()
    {
        if (SelectedChat is null)
            return;

        try
        {
            Messages.Clear();
            History.Clear();

            var messages = await _chatMessageService.GetMessagesByChatIdAsync(SelectedChat.Id);

            foreach (var message in messages)
            {
                if (message.SenderType == "user")
                {
                    Messages.Add($"Вы: {message.Content}");
                    History.Add(new ChatTurn
                    {
                        Role = "user",
                        Content = message.Content
                    });
                }
                else
                {
                    Messages.Add($"Бот: {message.Content}");
                    History.Add(new ChatTurn
                    {
                        Role = "assistant",
                        Content = message.Content
                    });
                }
            }

            if (messages.Count == 0)
            {
                Messages.Add("Бот: Новый чат открыт.");
            }
        }
        catch (Exception ex)
        {
            Messages.Add($"Ошибка загрузки сообщений: {ex.Message}");
        }
    }

    private async Task LoadFileAsync()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Text and JSON files|*.txt;*.json|All files|*.*"
        };

        if (dialog.ShowDialog() != true)
            return;

        try
        {
            var parsed = await _documentParserService.ParseAsync(dialog.FileName);
            _loadedDocumentContent = parsed.Content;

            Messages.Add($"Система: Загружен файл {parsed.FileName}");

            var preview = parsed.Content.Length > 300
                ? parsed.Content.Substring(0, 300)
                : parsed.Content;

            var dto = new ParsedDocumentDto
            {
                Id = Guid.NewGuid(),
                UserId = AppSession.CurrentUser!.Id,
                FileName = parsed.FileName,
                FileType = parsed.FileType,
                ContentPreview = preview
            };

            await _parsedDocumentService.SaveDocumentAsync(dto);
        }
        catch (Exception ex)
        {
            Messages.Add($"Ошибка загрузки файла: {ex.Message}");
        }
    }

    private async Task SendMessageAsync()
    {
        if (SelectedChat is null || string.IsNullOrWhiteSpace(UserInput))
            return;

        try
        {
            IsBusy = true;

            var text = UserInput.Trim();
            UserInput = string.Empty;

            Messages.Add($"Вы: {text}");
            History.Add(new ChatTurn
            {
                Role = "user",
                Content = text
            });

            await _chatMessageService.SaveMessageAsync(new ChatMessageDto
            {
                Id = Guid.NewGuid(),
                ChatId = SelectedChat.Id,
                SenderType = "user",
                Content = text
            });

            var prompt = string.IsNullOrWhiteSpace(_loadedDocumentContent)
                ? text
                : $"Контекст документа:\n{_loadedDocumentContent}\n\nВопрос пользователя:\n{text}";

            var answer = await _gigaChatClient.SendMessageAsync(prompt, History);

            Messages.Add($"Бот: {answer}");
            History.Add(new ChatTurn
            {
                Role = "assistant",
                Content = answer
            });

            await _chatMessageService.SaveMessageAsync(new ChatMessageDto
            {
                Id = Guid.NewGuid(),
                ChatId = SelectedChat.Id,
                SenderType = "assistant",
                Content = answer
            });
        }
        catch (Exception ex)
        {
            Messages.Add($"Ошибка: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}