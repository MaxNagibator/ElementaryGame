using Elementary.Business;
using Elementary.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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
    public JsonResult StartGame()
    {
        GameValue.StartGame();
        return new(new { Text = "Vrum Wrum" });
    }

    [HttpPost]
    public JsonResult GetNextQuestion()
    {
        var question = GameValue.GetNextQuestion();

        return new(new
        {
            Text = question.Value,
            question.Options,
        });
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

public class Game
{
    public int CurrentQuestionId = -1;

    public List<Question> Questions = QuestionHolder.GetQuestions();
    public List<UserAnswer> Answers = [];

    public Question GetNextQuestion()
    {
        CurrentQuestionId++;
        return Questions[CurrentQuestionId];
    }

    public Question GetCurrentQuestion()
    {
        return Questions[CurrentQuestionId];
    }

    public void StartGame()
    {
        CurrentQuestionId = -1;
        Answers = [];
    }

    public bool SetAnswer(string value)
    {
        var answer = new UserAnswer
        {
            IsCorrect = string.Equals(Questions[CurrentQuestionId].Answer, value, StringComparison.InvariantCultureIgnoreCase),
            Value = value,
        };

        Answers.Add(answer);
        return answer.IsCorrect;
    }
}
