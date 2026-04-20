using System.Text;
using DocumentFormat.OpenXml.Packaging;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using EnterpriseChatBot.Mobile.Models;

namespace EnterpriseChatBot.Mobile.Services;

public class ParsedDocumentService
{
    public async Task<ParsedDocumentDto> ParseAsync(FileResult file)
    {
        var result = new ParsedDocumentDto
        {
            FileName = file.FileName,
            FileType = Path.GetExtension(file.FileName).ToLowerInvariant()
        };

        try
        {
            result.ExtractedText = result.FileType switch
            {
                ".txt" or ".json" or ".md" or ".log" or ".csv" => await ReadPlainTextAsync(file),
                ".docx" => await ReadDocxAsync(file),
                ".pdf" => await ReadPdfAsync(file),
                _ => string.Empty
            };

            result.IsSuccess = !string.IsNullOrWhiteSpace(result.ExtractedText);

            if (!result.IsSuccess)
            {
                result.ErrorMessage = IsSupported(result.FileType)
                    ? "Текст не найден или файл пустой."
                    : $"Формат {result.FileType} пока не поддерживается.";
            }

            return result;
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    private static bool IsSupported(string extension)
    {
        return extension is ".txt" or ".json" or ".md" or ".log" or ".csv" or ".docx" or ".pdf";
    }

    private static async Task<string> ReadPlainTextAsync(FileResult file)
    {
        await using var stream = await file.OpenReadAsync();
        using var reader = new StreamReader(stream, Encoding.UTF8, true);
        return await reader.ReadToEndAsync();
    }

    private static async Task<string> ReadDocxAsync(FileResult file)
    {
        await using var sourceStream = await file.OpenReadAsync();
        using var memoryStream = new MemoryStream();
        await sourceStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        using var document = WordprocessingDocument.Open(memoryStream, false);
        var body = document.MainDocumentPart?.Document?.Body;

        if (body is null)
            return string.Empty;

        var text = body.InnerText ?? string.Empty;
        return NormalizeText(text);
    }

    private static async Task<string> ReadPdfAsync(FileResult file)
    {
        await using var sourceStream = await file.OpenReadAsync();
        using var memoryStream = new MemoryStream();
        await sourceStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        using var document = PdfDocument.Open(memoryStream);

        var sb = new StringBuilder();

        foreach (var page in document.GetPages())
        {
            var text = ContentOrderTextExtractor.GetText(page);
            if (!string.IsNullOrWhiteSpace(text))
            {
                sb.AppendLine(text);
            }
        }

        return NormalizeText(sb.ToString());
    }

    private static string NormalizeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        return text
            .Replace("\r\n", "\n")
            .Replace("\r", "\n")
            .Trim();
    }
}