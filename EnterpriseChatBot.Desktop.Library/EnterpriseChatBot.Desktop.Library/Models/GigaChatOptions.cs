using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseChatBot.Desktop.Library.Models
{
    public class GigaChatOptions
    {
        public string AuthKey { get; set; } = string.Empty;
        public string Scope { get; set; } = "GIGACHAT_API_PERS";
        public string AuthUrl { get; set; } = "https://ngw.devices.sberbank.ru:9443/api/v2/oauth";
        public string ChatUrl { get; set; } = "https://gigachat.devices.sberbank.ru/api/v1/chat/completions";
        public string Model { get; set; } = "GigaChat-2-Pro";
        public string SystemPrompt { get; set; } =
            "Ты полезный корпоративный чат-бот. Отвечай кратко и по делу.";
    }
}
