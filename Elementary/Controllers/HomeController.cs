using Elementary.Business;
using Microsoft.AspNetCore.Mvc;

namespace Elementary.Controllers;

public class HomeController : Controller
{
    private static readonly Game GameValue = new();

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public StateResponse Join([FromBody] JoinModel model)
    {
        GameValue.Join(model.PlayerId, model.IsSingle);
        return GetStateInternal(model.PlayerId, false);
    }

    [HttpPost]
    public StateResponse GetState([FromBody] PlayerIdModel model)
    {
        return GetStateInternal(model.PlayerId, model.IsAdmin);
    }

    [HttpPost]
    public IActionResult InitGame()
    {
        GameValue.InitGame();
        return Ok();
    }

    [HttpPost]
    public IActionResult StartGame([FromBody] StartGameModel model)
    {
        GameValue.StartGame(model.Level);
        return Ok();
    }

    [HttpPost]
    public SpinWhellResponse SpinWhell()
    {
        GameValue.SpinWhell();
        return new(GameValue.SectorValue);
    }

    [HttpPost]
    public JsonResult GetNextQuestion()
    {
        var question = GameValue.GetNextQuestion();
        return new(GetQuestionModel(question));
    }

    [HttpPost]
    public SetAnswerResponse SetAnswer([FromBody] SetAnswerModel model)
    {
        var isCorrect = GameValue.SetAnswer(model.PlayerId, model.Value);
        var currentQuestion = GameValue.GetCurrentQuestion();

        return new(isCorrect, currentQuestion?.Answer);
    }

    private StateResponse GetStateInternal(Guid playerId, bool isAdmin)
    {
        var player = GameValue.Players.FirstOrDefault(x => x.Id == playerId);
        QuestionModel? questionModel = null;

        var question = GameValue.GetCurrentQuestion();

        if (GameValue.State == Game.GameState.Started)
        {
            questionModel = GetQuestionModel(question);
        }

        var answer = player?.GetAnswer(GameValue.CurrentQuestionId);

        return new()
        {
            GameState = (int)GameValue.State,
            Level = GameValue.Level,
            Player = player,
            Question = questionModel,
            Answer = answer,
            CorrectAnswer = answer == null
                ? null
                : question?.Answer,
            Players = GameValue.State != Game.GameState.Started || isAdmin
                ? GameValue.Players
                : null,
            SectorValue = GameValue.SectorValue,
        };
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

    public class StateResponse
    {
        public int GameState { get; set; }
        public int Level { get; set; }
        public Player? Player { get; set; }
        public QuestionModel? Question { get; set; }
        public UserAnswer? Answer { get; set; }
        public string? CorrectAnswer { get; set; }
        public List<Player>? Players { get; set; }
        public int? SectorValue { get; set; }
    }

    public record SpinWhellResponse(int? SectorValue);

    public class QuestionModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string[] Options { get; set; }
        public string Type { get; set; }
        public string[]? TargetOptions { get; set; }
    }

    public record SetAnswerResponse(bool IsCorrect, string? Answer);
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
