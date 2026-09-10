namespace InfiniteLoop.CommandGenerator.Models;

public sealed record CommandResult(
    bool Success,
    int StatusCode,
    string Response,
    string ErrorMessage = "");
