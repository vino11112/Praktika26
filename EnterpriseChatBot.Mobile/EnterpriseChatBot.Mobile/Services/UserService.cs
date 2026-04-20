using System.Text;
using System.Text.Json;
using EnterpriseChatBot.Mobile.Models;

namespace EnterpriseChatBot.Mobile.Services;

public class UserService
{
    private readonly HttpClient _httpClient;

    public UserService(SupabaseHttpClient client)
    {
        _httpClient = client.HttpClient;
    }

    public async Task<AppUserDto?> GetByEmailAsync(string email)
    {
        var response = await _httpClient.GetAsync($"app_users?email=eq.{Uri.EscapeDataString(email)}");
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Ошибка загрузки пользователя: {json}");

        var result = JsonSerializer.Deserialize<List<AppUserDto>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result?.FirstOrDefault();
    }

    public async Task<AppUserDto> RegisterAsync(string fullName, string email, string password)
    {
        var existing = await GetByEmailAsync(email);
        if (existing is not null)
            throw new InvalidOperationException("Пользователь с таким email уже существует.");

        var user = new AppUserDto
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            Email = email,
            PasswordHash = PasswordHasher.HashPassword(password),
            Role = "user",
            IsActive = true
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "app_users");
        request.Headers.Add("Prefer", "return=representation");
        request.Content = new StringContent(
            JsonSerializer.Serialize(user),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Ошибка регистрации: {json}");

        var result = JsonSerializer.Deserialize<List<AppUserDto>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result?.FirstOrDefault()
               ?? throw new InvalidOperationException("Пользователь не был создан.");
    }

    public async Task<AppUserDto> LoginAsync(string email, string password)
    {
        var user = await GetByEmailAsync(email);

        if (user is null)
            throw new InvalidOperationException("Пользователь не найден.");

        if (!user.IsActive)
            throw new InvalidOperationException("Пользователь заблокирован.");

        var isValid = PasswordHasher.VerifyPassword(password, user.PasswordHash);
        if (!isValid)
            throw new InvalidOperationException("Неверный пароль.");

        return user;
    }
}