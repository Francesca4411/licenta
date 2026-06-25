using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyManagement.Models.ViewModels;
using StudyManagement.Services.Ai;
using System.Net;
using System.Text.RegularExpressions;

namespace StudyManagement.Controllers;

[Authorize]
public class AiTutorController : Controller
{
    private readonly IAiTutorService _aiTutor;

    public AiTutorController(IAiTutorService aiTutor)
    {
        _aiTutor = aiTutor;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new AiTutorPageViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(AiTutorPageViewModel model, CancellationToken cancellationToken)
    {
        model.Question = (model.Question ?? "").Trim();

        if (string.IsNullOrWhiteSpace(model.Question))
        {
            model.ErrorMessage = "Please type a question first.";
            return View(model);
        }

        var result = await _aiTutor.AskAsync(model.Question, cancellationToken);
        if (!result.IsSuccess)
        {
            model.ErrorMessage = result.ErrorMessage ?? "Could not generate a response.";
            return View(model);
        }

        model.Answer = result.Answer;
        model.AnswerHtml = ToFriendlyHtml(result.Answer);
        return View(model);
    }

    private static string ToFriendlyHtml(string text)
    {
        var encoded = WebUtility.HtmlEncode(text ?? string.Empty);
        encoded = Regex.Replace(encoded, @"\*\*(.+?)\*\*", "<strong>$1</strong>");
        encoded = Regex.Replace(encoded, @"\*(.+?)\*", "<em>$1</em>");
        encoded = encoded.Replace("\r\n", "\n").Replace("\n", "<br />");
        return encoded;
    }
}
