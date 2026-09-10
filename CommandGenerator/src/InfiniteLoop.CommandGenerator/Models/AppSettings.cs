namespace InfiniteLoop.CommandGenerator.Models;

public sealed class AppSettings
{
    public string ServerUrl { get; set; } = "http://127.0.0.1:8080";
    public string CommandEndpoint { get; set; } = "/api/AscNet/command/{command}";
    public string HttpMethod { get; set; } = "GET";
    public bool AutoCopy { get; set; } = false;
    public bool AutoSaveAfterBatch { get; set; } = false;
    public int RequestTimeoutSeconds { get; set; } = 10;
    public int HistoryLimit { get; set; } = 50;
}
