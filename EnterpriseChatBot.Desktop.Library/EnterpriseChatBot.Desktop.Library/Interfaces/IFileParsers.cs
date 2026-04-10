using EnterpriseChatBot.Desktop.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseChatBot.Desktop.Library.Interfaces
{
    public interface IFileParser
    {
        bool CanParse(string extension);
        Task<ParsedDocument> ParseAsync(string filePath, CancellationToken cancellationToken = default);
    }
}
