using System.Diagnostics;
using Elementary.Models;
using Microsoft.AspNetCore.Mvc;

namespace Elementary.Controllers
{
    public class HomeController : Controller
    {
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
            return new JsonResult(new { Text = "Vrum Wrum" });
        }

        [HttpPost]
        public JsonResult GetNextQuestion()
        {
            var question = GameValue.GetNextQuestion();

            return new JsonResult(new
            {
                Text = question.Value,
                Options = question.Options,
            });
        }

        [HttpPost]
        public JsonResult SetAnswer(string value)
        {
            bool isCorrect = GameValue.SetAnswer(value);
            return new JsonResult(new
            {
                IsCorrect = isCorrect,
                Explanation = GameValue.GetCurrentQuestion().Explanation,
            });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public class Game()
        {
            public int CurrentQuestionId = -1;

            public List<Question> Questions = QuestionHolder.GetQuestions();
            public List<UserAnswer> Answers = new List<UserAnswer>();

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
                Answers = new List<UserAnswer>();
            }

            internal bool SetAnswer(string value)
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

        public static Game GameValue = new Game();
    }
}
