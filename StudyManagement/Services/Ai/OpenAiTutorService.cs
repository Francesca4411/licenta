using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace StudyManagement.Services.Ai;

public sealed class ClaudeTutorService : IAiTutorService
{
    private readonly HttpClient _http;
    private readonly AiTutorOptions _options;
    private readonly ILogger<ClaudeTutorService> _logger;

    public ClaudeTutorService(HttpClient http, IOptions<AiTutorOptions> options, ILogger<ClaudeTutorService> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AiTutorReply> AskAsync(string question, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return new AiTutorReply
            {
                IsSuccess = false,
                ErrorMessage = "AI is not configured yet. Set AiTutor__ApiKey as an environment variable."
            };
        }

        var payload = new
        {
            model = _options.Model,
            temperature = _options.Temperature,
            max_tokens = _options.MaxTokens,
            system = "You are a concise and friendly study assistant. Detect the language of the user's latest message and answer in that same language. Keep explanations clear and practical. You may use a light amount of emojis and simple markdown emphasis (like **important terms** or *short emphasis*) when it helps readability, but do not overdo it.",
            messages = new object[]
            {
                new
                {
                    role = "user",
                    content = question.Trim()
                }
            }
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, _options.ApiUrl);
        req.Headers.Add("x-api-key", _options.ApiKey);
        req.Headers.Add("anthropic-version", _options.AnthropicVersion);
        req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        try
        {
            using var resp = await _http.SendAsync(req, cancellationToken);
            var body = await resp.Content.ReadAsStringAsync(cancellationToken);

            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("AI provider returned status {StatusCode}. Body: {Body}", (int)resp.StatusCode, body);
                string providerMessage = "Claude request failed. Check AiTutor__ApiKey and AiTutor__Model.";
                try
                {
                    using var errDoc = JsonDocument.Parse(body);
                    if (errDoc.RootElement.TryGetProperty("error", out var errEl) &&
                        errEl.TryGetProperty("message", out var msgEl))
                    {
                        var raw = msgEl.GetString();
                        if (!string.IsNullOrWhiteSpace(raw))
                            providerMessage = $"Claude error: {raw}";
                    }
                }
                catch
                {
                    // Keep fallback message.
                }

                return new AiTutorReply
                {
                    IsSuccess = false,
                    ErrorMessage = providerMessage
                };
            }

            using var doc = JsonDocument.Parse(body);
            string? answer = null;
            if (doc.RootElement.TryGetProperty("content", out var contentArray) && contentArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var block in contentArray.EnumerateArray())
                {
                    if (!block.TryGetProperty("type", out var typeEl) || typeEl.GetString() != "text")
                        continue;

                    if (!block.TryGetProperty("text", out var textEl))
                        continue;

                    var candidate = textEl.GetString();
                    if (!string.IsNullOrWhiteSpace(candidate))
                    {
                        answer = candidate;
                        break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(answer))
            {
                return new AiTutorReply
                {
                    IsSuccess = false,
                    ErrorMessage = "Claude returned an empty answer."
                };
            }

            return new AiTutorReply
            {
                IsSuccess = true,
                Answer = answer.Trim()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while calling Claude provider.");
            return new AiTutorReply
            {
                IsSuccess = false,
                ErrorMessage = "Could not contact Claude right now. Try again."
            };
        }
    }
}
