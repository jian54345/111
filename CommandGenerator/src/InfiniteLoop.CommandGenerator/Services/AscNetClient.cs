using System.Net;
using System.Net.Http;
using InfiniteLoop.CommandGenerator.Models;

namespace InfiniteLoop.CommandGenerator.Services;

public sealed class AscNetClient : IDisposable
{
    private readonly HttpClient _httpClient = new();

    public async Task<CommandResult> ExecuteAsync(
        AppSettings settings,
        string command,
        CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(settings.ServerUrl.TrimEnd('/') + "/", UriKind.Absolute, out var baseUri))
            return new(false, 0, "", "服务器地址无效。");

        var endpoint = settings.CommandEndpoint.TrimStart('/');
        var encodedCommand = Uri.EscapeDataString(command.Trim());
        endpoint = endpoint.Replace("{command}", encodedCommand, StringComparison.Ordinal);

        var url = new Uri(baseUri, endpoint);

        using var request = new HttpRequestMessage(
            string.Equals(settings.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase)
                ? HttpMethod.Post
                : HttpMethod.Get,
            url);

        if (request.Method == HttpMethod.Post)
        {
            request.Content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(new { command }),
                System.Text.Encoding.UTF8,
                "application/json");
        }

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(settings.RequestTimeoutSeconds, 1, 120)));

            using var response = await _httpClient.SendAsync(request, cts.Token);
            var body = await response.Content.ReadAsStringAsync(cts.Token);

            return new(
                response.IsSuccessStatusCode,
                (int)response.StatusCode,
                body,
                response.IsSuccessStatusCode ? "" :
                    $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}");
        }
        catch (TaskCanceledException)
        {
            return new(false, 0, "", "请求超时。");
        }
        catch (HttpRequestException ex)
        {
            return new(false, 0, "", ex.Message);
        }
        catch (Exception ex)
        {
            return new(false, 0, "", ex.Message);
        }
    }

    public async Task<bool> TestAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(settings.RequestTimeoutSeconds, 1, 120)));

            var url = new Uri(settings.ServerUrl.TrimEnd('/') + "/");
            using var response = await _httpClient.GetAsync(url, cts.Token);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose() => _httpClient.Dispose();
}
