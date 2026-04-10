using EnterpriseChatBot.Desktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseChatBot.Desktop.Services
{
    public static class AppSession
    {
        public static AppUserDto? CurrentUser { get; set; }

        public static bool IsAuthenticated => CurrentUser is not null;
    }
}
