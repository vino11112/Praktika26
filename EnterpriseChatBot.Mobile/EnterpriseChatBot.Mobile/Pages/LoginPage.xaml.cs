using EnterpriseChatBot.Mobile.Services;

namespace EnterpriseChatBot.Mobile.Pages;

public partial class LoginPage : ContentPage
{
    private readonly UserService _userService;

    public LoginPage(UserService userService)
    {
        InitializeComponent();
        _userService = userService;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        try
        {
            var email = EmailEntry.Text?.Trim() ?? string.Empty;
            var password = PasswordEntry.Text ?? string.Empty;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Ошибка", "Введите email и пароль", "OK");
                return;
            }

            var user = await _userService.LoginAsync(email, password);

            AppSession.CurrentUserId = user.Id;
            AppSession.CurrentUserName = user.FullName;
            AppSession.CurrentUserEmail = user.Email;

            await Shell.Current.GoToAsync($"//{nameof(ChatListPage)}");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private async void OnGoToRegisterClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(RegisterPage));
    }
}