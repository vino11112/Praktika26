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
    /// Логика взаимодействия для RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        private readonly UserService _userService;

        public RegisterWindow()
        {
            InitializeComponent();

            var supabaseClient = new SupabaseHttpClient();
            _userService = new UserService(supabaseClient);
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusTextBlock.Foreground = System.Windows.Media.Brushes.Green;
                StatusTextBlock.Text = string.Empty;

                var fullName = FullNameTextBox.Text.Trim();
                var email = EmailTextBox.Text.Trim();
                var password = PasswordBox.Password;

                if (string.IsNullOrWhiteSpace(fullName) ||
                    string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(password))
                {
                    throw new InvalidOperationException("Заполни все поля.");
                }

                await _userService.RegisterAsync(fullName, email, password);

                MessageBox.Show("Успех");
                this.Close();
            }
            catch (Exception ex)
            {
                StatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                StatusTextBlock.Text = ex.Message;
            }
        }
    }
}
