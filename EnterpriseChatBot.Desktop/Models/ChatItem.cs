using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseChatBot.Desktop.Models
{
    public class ChatItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;

        public override string ToString()
        {
            return Title;
        }
    }
}
