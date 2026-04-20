using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseChatBot.Mobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }
    }
}