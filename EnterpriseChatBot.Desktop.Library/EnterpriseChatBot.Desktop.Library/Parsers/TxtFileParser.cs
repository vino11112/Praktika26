using EnterpriseChatBot.Desktop.Library.Interfaces;
using EnterpriseChatBot.Desktop.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseChatBot.Desktop.Library.Parsers
{
    public class TxtFileParser : IFileParser
    {
        public bool CanParse(string extension)
        {
            return extension.Equals(".txt", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<ParsedDocument> ParseAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Файл не найден.", filePath);

            var content = await File.ReadAllTextAsync(filePath, cancellationToken);

            return new ParsedDocument
            {
                FileName = Path.GetFileName(filePath),
                FileType = Path.GetExtension(filePath),
                Content = Normalize(content)
            };
        }

        private static string Normalize(string text)
        {
            return text.Replace("\r\n", "\n").Trim();
        }
    }
}
