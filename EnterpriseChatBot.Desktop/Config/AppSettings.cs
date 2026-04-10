using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseChatBot.Desktop.Config
{
    public static class AppSettings
    {
        public const string GigaChatAuthKey = "MDE5ZDU0MDYtNTZlNC03MWNkLThhZTktMjYzMDQzZTNiMGZiOmQzYTA0NjFiLTU4MTMtNDE1Ny04MzQ3LWQwZmQyN2VlMzc4Nw==";
        public const string GigaChatScope = "GIGACHAT_API_PERS";
        public const string GigaChatModel = "GigaChat-2-Pro";

        public const string SupabaseUrl = "https://ealjebknxhwqhngmdqzp.supabase.co";
        public const string SupabaseApiKey = "sb_publishable_ZR5ZfJL6EUCpYCWVuXqYdg_r9t7nHB4";

        public static readonly Guid TestUserId =
            Guid.Parse("11111111-1111-1111-1111-111111111111");
    }
}
