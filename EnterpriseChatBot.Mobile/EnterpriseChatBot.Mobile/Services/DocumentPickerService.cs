using System.Text;

namespace EnterpriseChatBot.Mobile.Services;

public class DocumentPickerService
{
    public async Task<FileResult?> PickFileAsync()
    {
        try
        {
            return await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Выберите файл"
            });
        }
        catch
        {
            return null;
        }
    }

    public async Task<string> ReadTextAsync(FileResult file)
    {
        try
        {
            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();

            await using var stream = await file.OpenReadAsync();
            using var reader = new StreamReader(stream, Encoding.UTF8, true);

            var content = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(content))
                return string.Empty;

            if (extension == ".json")
                return content;

            if (extension == ".txt" || extension == ".md" || extension == ".log" || extension == ".csv")
                return content;

            return content;
        }
        catch
        {
            return string.Empty;
        }
    }
}