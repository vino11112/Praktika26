using EnterpriseChatBot.Desktop.Library.Interfaces;
using EnterpriseChatBot.Desktop.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EnterpriseChatBot.Desktop.Library.Parsers
{
    public class JsonFileParser : IFileParser
    {
        public bool CanParse(string extension)
        {
            return extension.Equals(".json", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<ParsedDocument> ParseAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Файл не найден.", filePath);

            var rawJson = await File.ReadAllTextAsync(filePath, cancellationToken);

            using var doc = JsonDocument.Parse(rawJson);

            var prettyJson = JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            return new ParsedDocument
            {
                FileName = Path.GetFileName(filePath),
                FileType = Path.GetExtension(filePath),
                Content = prettyJson
            };
        }
    }
}
