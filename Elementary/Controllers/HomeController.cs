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
    public JsonResult Join([FromBody] JoinModel model)
    {
        GameValue.Join(model.PlayerId, model.IsSingle);
        if(false)
        {
            for(int i = 0;i < 11; i++)
            {
                GameValue.Join(Guid.NewGuid(), model.IsSingle);
            }
        }
        
        return GetStateInternal(model.PlayerId, false);
    }

    [HttpPost]
    public JsonResult GetState([FromBody] PlayerIdModel model)
    {
        return GetStateInternal(model.PlayerId, model.IsAdmin);
    }

    private JsonResult GetStateInternal(Guid playerId, bool isAdmin)
    {
        var player = GameValue.Players.FirstOrDefault(x => x.Id == playerId);
        QuestionModel? questionModel = null;

        var question = GameValue.GetCurrentQuestion();
        if (GameValue.State == Game.GameState.Started)
        {
            questionModel = GetQuestionModel(question);
        }

        var answer = player?.GetAnswer(GameValue.CurrentQuestionId);

        return new(new
        {
            GameState = (int)GameValue.State,
            Level = GameValue.Level,
            Player = player,
            Question = questionModel,
            Answer = answer,
            CorrectAnswer = answer == null ? null : question?.Answer,

            Players = GameValue.State != Game.GameState.Started || isAdmin ? GameValue.Players : null,
            SectorValue = GameValue.SectorValue,
        });
    }

    [HttpPost]
    public JsonResult InitGame()
    {
        GameValue.InitGame();
        return new(new { Text = "Vrum Wrum" });
    }

    [HttpPost]
    public JsonResult StartGame([FromBody] StartGameModel model)
    {
        GameValue.StartGame(model.Level);
        return new(new { Text = "Vrum Wrum" });
    }

    [HttpPost]
    public JsonResult SpinWhell()
    {
        GameValue.SpinWhell();

        return new(new
        {
            GameValue.SectorValue,
        });
    }

    [HttpPost]
    public JsonResult GetNextQuestion()
    {
        var question = GameValue.GetNextQuestion();
        return new(GetQuestionModel(question));
    }

    private QuestionModel? GetQuestionModel(Question? question)
    {
        if (question == null)
        {
            return null;
        }

        var options = question.Options.ToArray();
        if (question.Type == "multiple")
        {
            // дорого реализовывать передачу ответа
            Random.Shared.Shuffle(options);
        }

        return new()
        {
            Id = GameValue.CurrentQuestionId,
            Text = question.Value,
            Options = options,
            TargetOptions = question.TargetOptions?.ToArray(),
            Type = question.Type,
        };
    }

    public class QuestionModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string[] Options { get; set; }
        public string Type { get; set; }
        public string[]? TargetOptions { get; set; }
    }

    [HttpPost]
    public JsonResult SetAnswer([FromBody] SetAnswerModel model)
    {
        var isCorrect = GameValue.SetAnswer(model.PlayerId, model.Value);
        var currentQuestion = GameValue.GetCurrentQuestion();

        return new(new
        {
            IsCorrect = isCorrect,
            currentQuestion.Answer,
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

public class SetAnswerModel : PlayerIdModel
{
    public string Value { get; set; }
}

public class PlayerIdModel
{
    public Guid PlayerId { get; set; }
    public bool IsAdmin { get; set; }
}

public class StartGameModel
{
    public int Level { get; set; }
}

public class JoinModel : PlayerIdModel
{
    public bool IsSingle { get; set; }
    public bool IsAdmin { get; set; }
}
