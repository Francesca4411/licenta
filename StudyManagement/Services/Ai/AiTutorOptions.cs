namespace StudyManagement.Services.Ai;

public sealed class AiTutorOptions
{
    public string ApiUrl { get; set; } = "https://api.anthropic.com/v1/messages";
    public string ApiKey { get; set; } = "";
    public string Model { get; set; } = "claude-3-5-sonnet-latest";
    public int MaxTokens { get; set; } = 350;
    public double Temperature { get; set; } = 0.4;
    public string AnthropicVersion { get; set; } = "2023-06-01";
}
