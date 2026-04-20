using EnterpriseChatBot.Mobile.Services;

namespace EnterpriseChatBot.Mobile.Pages;

public partial class RegisterPage : ContentPage
{
    private readonly UserService _userService;

    public RegisterPage(UserService userService)
    {
        InitializeComponent();
        _userService = userService;
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        try
        {
            var name = NameEntry.Text?.Trim() ?? string.Empty;
            var email = EmailEntry.Text?.Trim() ?? string.Empty;
            var password = PasswordEntry.Text ?? string.Empty;

            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Ошибка", "Заполните все поля", "OK");
                return;
            }

            await _userService.RegisterAsync(name, email, password);

            await DisplayAlert("Успех", "Регистрация выполнена", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}