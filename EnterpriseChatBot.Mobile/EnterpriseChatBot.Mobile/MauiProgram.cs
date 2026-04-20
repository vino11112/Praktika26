using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

using EnterpriseChatBot.Mobile.Pages;
using EnterpriseChatBot.Mobile.Services;

namespace EnterpriseChatBot.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<SupabaseHttpClient>();
        builder.Services.AddSingleton<UserService>();
        builder.Services.AddSingleton<ChatSessionService>();
        builder.Services.AddSingleton<ChatMessageService>();
        builder.Services.AddSingleton<GigaChatService>();
        builder.Services.AddSingleton<DocumentPickerService>();
        builder.Services.AddSingleton<ParsedDocumentService>();
        builder.Services.AddSingleton<ChatDocumentStateService>();

        builder.Services.AddSingleton<LoginPage>();
        builder.Services.AddSingleton<RegisterPage>();
        builder.Services.AddSingleton<ChatListPage>();
        builder.Services.AddSingleton<ChatPage>();

        return builder.Build();
    }
}