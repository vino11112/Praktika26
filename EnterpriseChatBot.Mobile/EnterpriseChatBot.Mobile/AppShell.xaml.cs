namespace EnterpriseChatBot.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(Pages.RegisterPage), typeof(Pages.RegisterPage));
        Routing.RegisterRoute(nameof(Pages.ChatPage), typeof(Pages.ChatPage));
    }
}