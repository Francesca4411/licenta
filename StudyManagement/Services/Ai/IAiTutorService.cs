namespace StudyManagement.Services.Ai;

public interface IAiTutorService
{
    Task<AiTutorReply> AskAsync(string question, CancellationToken cancellationToken = default);
}

public sealed class AiTutorReply
{
    public bool IsSuccess { get; init; }
    public string Answer { get; init; } = "";
    public string? ErrorMessage { get; init; }
}
