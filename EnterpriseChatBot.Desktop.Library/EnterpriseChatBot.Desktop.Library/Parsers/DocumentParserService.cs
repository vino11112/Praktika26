using EnterpriseChatBot.Desktop.Library.Interfaces;
using EnterpriseChatBot.Desktop.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseChatBot.Desktop.Library.Parsers
{
    public class DocumentParserService
    {
        private readonly List<IFileParser> _parsers;

        public DocumentParserService()
        {
            _parsers = new List<IFileParser>
        {
            new TxtFileParser(),
            new JsonFileParser()
        };
        }

        public async Task<ParsedDocument> ParseAsync(string filePath, CancellationToken cancellationToken = default)
        {
            var extension = Path.GetExtension(filePath);

            var parser = _parsers.FirstOrDefault(x => x.CanParse(extension));

            if (parser is null)
                throw new NotSupportedException($"Формат {extension} не поддерживается.");

            return await parser.ParseAsync(filePath, cancellationToken);
        }
    }
}
