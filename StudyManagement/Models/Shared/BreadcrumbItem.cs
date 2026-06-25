namespace StudyManagement.Models.Shared;

public sealed class BreadcrumbItem
{
    public BreadcrumbItem(string text, string? url = null)
    {
        Text = text;
        Url = url;
    }

    public string Text { get; }
    public string? Url { get; }
}
