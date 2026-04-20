using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using EnterpriseChatBot.Desktop.Services;

namespace TestProject2;

internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

    public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
    {
        _handler = handler;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(_handler(request));
    }
}

internal static class TestHelpers
{
    public static SupabaseHttpClient CreateSupabaseHttpClient(HttpClient httpClient)
    {
        if (httpClient.BaseAddress == null)
        {
            httpClient.BaseAddress = new Uri("https://test.local/rest/v1/");
        }

        var instance = (SupabaseHttpClient)FormatterServices
            .GetUninitializedObject(typeof(SupabaseHttpClient));

        var backingField = typeof(SupabaseHttpClient)
            .GetField("<HttpClient>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);

        if (backingField == null)
            throw new InvalidOperationException("Не удалось найти backing field для HttpClient.");

        backingField.SetValue(instance, httpClient);
        return instance;
    }
}