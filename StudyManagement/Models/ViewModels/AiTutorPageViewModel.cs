namespace StudyManagement.Models.ViewModels;

public sealed class AiTutorPageViewModel
{
    public string Question { get; set; } = "";
    public string Answer { get; set; } = "";
    public string AnswerHtml { get; set; } = "";
    public string? ErrorMessage { get; set; }
}
