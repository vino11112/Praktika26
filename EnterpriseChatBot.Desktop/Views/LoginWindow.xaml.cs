using EnterpriseChatBot.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EnterpriseChatBot.Desktop.Views
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly UserService _userService;

        public LoginWindow()
        {
            InitializeComponent();

            var supabaseClient = new SupabaseHttpClient();
            _userService = new UserService(supabaseClient);
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusTextBlock.Text = string.Empty;

                var email = EmailTextBox.Text.Trim();
                var password = PasswordBox.Password;

                var user = await _userService.LoginAsync(email, password);

                AppSession.CurrentUser = user;

                var mainWindow = new MainWindow();
                mainWindow.Show();

                Close();
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = ex.Message;
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.ShowDialog();
        }
    }
}
