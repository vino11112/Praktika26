using EnterpriseChatBot.Desktop.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseChatBot.Desktop.Library.Interfaces
{
    public interface IGigaChatClient
    {
        Task<string> SendMessageAsync(
            string userMessage,
            IEnumerable<ChatTurn>? history = null,
            CancellationToken cancellationToken = default);
    }
}
