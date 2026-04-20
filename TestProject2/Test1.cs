using EnterpriseChatBot.Desktop.Library.Parsers;
using EnterpriseChatBot.Desktop.Models;
using EnterpriseChatBot.Desktop.Services;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Text;
using System.Text.Json;
namespace TestProject2
{
    [TestClass]
    public sealed class testLibrary
    {
        [TestMethod]
        public async Task TxtFileParser_ParseAsync_ShouldNormalizeLineEndings()
        {
            var parser = new TxtFileParser();
            var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.txt");

            await File.WriteAllTextAsync(filePath, "line1\r\nline2\r\n");

            try
            {
                var result = await parser.ParseAsync(filePath);

                Assert.AreEqual(".txt", result.FileType);
                Assert.AreEqual(Path.GetFileName(filePath), result.FileName);
                Assert.AreEqual("line1\nline2", result.Content);
            }
            finally
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        [TestMethod]
        public async Task JsonFileParser_ParseAsync_ShouldReturnPrettyJson()
        {
            var parser = new JsonFileParser();
            var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");

            await File.WriteAllTextAsync(filePath, "{\"name\":\"test\",\"age\":20}");

            try
            {
                var result = await parser.ParseAsync(filePath);

                Assert.AreEqual(".json", result.FileType);
                Assert.IsTrue(result.Content.Contains("\"name\""));
                Assert.IsTrue(result.Content.Contains("\"age\""));
                Assert.IsTrue(result.Content.Contains(Environment.NewLine) || result.Content.Contains("\n"));
            }
            finally
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        [TestMethod]
        public async Task DocumentParserService_ParseAsync_ShouldThrowForUnsupportedExtension()
        {
            var service = new DocumentParserService();
            var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.xml");

            await File.WriteAllTextAsync(filePath, "<root />");

            try
            {
                await Assert.ThrowsExceptionAsync<NotSupportedException>(() => service.ParseAsync(filePath));
            }
            finally
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }
    }
    [TestClass]
    public class DesktopServicesTests
    {
        [TestMethod]
        public async Task UserService_LoginAsync_ShouldReturnUser_WhenCredentialsAreCorrect()
        {
            var password = "123456";
            var hash = PasswordHasher.HashPassword(password);

            var httpClient = new HttpClient(new FakeHttpMessageHandler(_ =>
            {
                var json = $$"""
            [
              {
                "id":"11111111-1111-1111-1111-111111111111",
                "email":"test@test.com",
                "password_hash":"{{hash}}",
                "full_name":"Test User",
                "role":"user",
                "is_active":true,
                "created_at":"2025-01-01T00:00:00"
              }
            ]
            """;

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            }));

            var service = new UserService(TestHelpers.CreateSupabaseHttpClient(httpClient));

            var user = await service.LoginAsync("test@test.com", password);

            Assert.IsNotNull(user);
            Assert.AreEqual("test@test.com", user.Email);
            Assert.AreEqual("Test User", user.FullName);
        }

        [TestMethod]
        public async Task UserService_LoginAsync_ShouldThrow_WhenPasswordIsWrong()
        {
            var correctHash = PasswordHasher.HashPassword("correct-password");

            var httpClient = new HttpClient(new FakeHttpMessageHandler(_ =>
            {
                var json = $$"""
            [
              {
                "id":"11111111-1111-1111-1111-111111111111",
                "email":"test@test.com",
                "password_hash":"{{correctHash}}",
                "full_name":"Test User",
                "role":"user",
                "is_active":true,
                "created_at":"2025-01-01T00:00:00"
              }
            ]
            """;

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            }));

            var service = new UserService(TestHelpers.CreateSupabaseHttpClient(httpClient));

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => service.LoginAsync("test@test.com", "wrong-password"));
        }

      

        [TestMethod]
        public async Task ChatSessionService_GetChatsByUserIdAsync_ShouldReturnChats()
        {
            var userId = Guid.NewGuid();

            var httpClient = new HttpClient(new FakeHttpMessageHandler(_ =>
            {
                var json = $$"""
            [
              {
                "id":"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
                "user_id":"{{userId}}",
                "title":"Чат 1",
                "model_name":"GigaChat-2-Pro",
                "created_at":"2025-01-01T10:00:00",
                "updated_at":"2025-01-01T10:00:00"
              }
            ]
            """;

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            }));

            var service = new ChatSessionService(TestHelpers.CreateSupabaseHttpClient(httpClient));

            var chats = await service.GetChatsByUserIdAsync(userId);

            Assert.AreEqual(1, chats.Count);
            Assert.AreEqual("Чат 1", chats[0].Title);
        }

        [TestMethod]
        public async Task ChatMessageService_SaveMessageAsync_ShouldThrow_WhenApiReturnsError()
        {
            var httpClient = new HttpClient(new FakeHttpMessageHandler(_ =>
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("{\"error\":\"save failed\"}", Encoding.UTF8, "application/json")
                };
            }));

            var service = new ChatMessageService(TestHelpers.CreateSupabaseHttpClient(httpClient));

            var message = new ChatMessageDto
            {
                Id = Guid.NewGuid(),
                ChatId = Guid.NewGuid(),
                SenderType = "user",
                Content = "Привет",
                CreatedAt = DateTime.UtcNow
            };

            await Assert.ThrowsExceptionAsync<HttpRequestException>(
                () => service.SaveMessageAsync(message));
        }
    }
}
