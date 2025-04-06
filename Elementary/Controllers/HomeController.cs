using Elementary.Business;
using Elementary.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Reflection;

namespace Elementary.Controllers;

public class HomeController : Controller
{
    public static Game GameValue = new();
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public JsonResult SpinWhell()
    {
        GameValue.SpinWhell();
        return new(new
        {
            SectorValue = GameValue.SectorValue,
        });
    }

    [HttpPost]
    public JsonResult Join([FromBody] JoinModel model)
    {
        GameValue.Join(model.PlayerId, model.IsSingle);
        return GetStateInternal(model.PlayerId);
    }

    [HttpPost]
    public JsonResult GetState([FromBody] PlayerIdModel model)
    {
        return GetStateInternal(model.PlayerId);
    }

    private JsonResult GetStateInternal(Guid playerId)
    {
        var player = GameValue.Players.FirstOrDefault(x => x.Id == playerId);
        QuestionModel? questionModel = null;
        if (GameValue.CurrentQuestionId >= 0)
        {
            var question = GameValue.GetCurrentQuestion();
            questionModel = GetQuestionModel(question);
        }
        return new(new
        {
            PlayerName = player?.Name,
            PlayerImage = player?.Image,
            Question = questionModel,
        });
    }

    [HttpPost]
    public JsonResult StartGame()
    {
        GameValue.StartGame();
        return new(new { Text = "Vrum Wrum" });
    }

    [HttpPost]
    public JsonResult GetNextQuestion()
    {
        var question = GameValue.GetNextQuestion();
        return new(GetQuestionModel(question));
    }

    private QuestionModel GetQuestionModel(Question question)
    {
        var options = question.Options.ToArray();
        Random.Shared.Shuffle(options);

        return new QuestionModel
        {
            Text = question.Value,
            Options = options,
            Type = question.Options.Count > 0 ? "multiple" : "text",
        };
    }

    public class QuestionModel
    {
        public string Text { get; set; }
        public string[] Options { get; set; }
        public string Type { get; set; }
    }

    [HttpPost]
    public JsonResult SetAnswer([FromBody] SetAnswerModel model)
    {
        var isCorrect = GameValue.SetAnswer(model.Value);
        var currentQuestion = GameValue.GetCurrentQuestion();

        return new(new
        {
            IsCorrect = isCorrect,
            currentQuestion.Answer,
            currentQuestion.Explanation,
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

public class SetAnswerModel
{
    public string Value { get; set; }
}

public class PlayerIdModel
{
    public Guid PlayerId { get; set; }
}

public class JoinModel : PlayerIdModel
{
    public bool IsSingle { get; set; }
}