namespace Elementary.Business
{
    public class Game
    {
        public Game()
        {
            InitGame();
        }

        public enum GameState
        {
            Welcome = 0,
            WhellRun = 1,
            Started = 2,
            Finish = 3,
        }

        public GameState State { get; set; }
        public int Level { get; private set; }

        public int CurrentQuestionId = -1;

        public List<Question> Questions { get; set; }
        public List<Player> Players { get; set; } = new List<Player>();
        public List<int> FreePlaces { get; set; }
        public int? SectorValue { get; private set; }

        public Question? GetNextQuestion()
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].Answers.Count == CurrentQuestionId)
                {
                    Players[i].Answers.Add(new UserAnswer { IsCorrect = false, Value = null });
                }
            }

            CurrentQuestionId++;

            if (CurrentQuestionId >= Questions.Count)
            {
                State = GameState.Finish;
                CurrentQuestionId = -1;
                return null;
            }

            return Questions[CurrentQuestionId];
        }

        public Question? GetCurrentQuestion()
        {
            return CurrentQuestionId == -1 || CurrentQuestionId > Questions.Count ? null :  Questions[CurrentQuestionId];
        }

        public void InitGame()
        {
            CurrentQuestionId = -1;
            Players = new List<Player>();
            State = GameState.Welcome;
            SectorValue = null;

            var freePlaces = new int[12];

            for (var i = 0; i < 12; i++)
            {
                freePlaces[i] = i;
            }

            Random.Shared.Shuffle(freePlaces);
            FreePlaces = freePlaces.ToList();
            Level = 0;
        }

        public void StartGame(int level)
        {
            if (Players.Count == 0)
            {
                throw new BusinessException("нет игроков");
            }
            if (Players[0].Name == null)
            {
                throw new BusinessException("крути рулетку");
            }
            if (level == 2 && State != GameState.Finish)
            {
                throw new BusinessException("сначала уровень простой сыграйте");
            }

            if (Level == 1 && level == 1)
            {
                throw new BusinessException("уже начато");
            }

            CurrentQuestionId = 0;
            Questions = QuestionHolder.GetQuestions(level);
            State = GameState.Started;
            Level = level;
            if (level == 2)
            {
                for (int i = 0; i < Players.Count; i++)
                {
                    Players[i].Answers1 = Players[i].Answers;
                    Players[i].Answers = new List<UserAnswer>();
                }
            }
        }

        public bool SetAnswer(Guid playerId, string value)
        {
            var answer = new UserAnswer
            {
                IsCorrect = string.Equals(Questions[CurrentQuestionId].Answer, value, StringComparison.InvariantCultureIgnoreCase),
                Value = value,
            };

            var player = Players.First(x => x.Id == playerId);

            if (player.Answers.Count > CurrentQuestionId)
            {
                throw new BusinessException("вы уже ответили");
            }

            player.Answers.Add(answer);
            return answer.IsCorrect;
        }

        public void Join(Guid playerId, bool isSingle)
        {
            if (State != GameState.Welcome)
            {
                throw new BusinessException("Ожидайте, пока администатор начнёт сбор желающих поиграть");
            }

            if (Players.Any(x => x.Id == playerId))
            {
                return;
            }

            var number = FreePlaces[Players.Count];

            var player = new Player
            {
                Id = playerId,
                PlaceNumber = number,
                IsSingle = isSingle,
                TeamNumber = Players.Count + 1
            };

            Players.Add(player);
        }

        public void SpinWhell()
        {
            State = GameState.WhellRun;
            var sectorValue = Random.Shared.Next(0, 12);
            SectorValue = sectorValue;

            for (var i = 0; i < Players.Count; i++)
            {
                var myNumber = 12 - Players[i].PlaceNumber;

                myNumber = myNumber + sectorValue;

                if (myNumber >= 12)
                {
                    // todo волшебная 12 в константы
                    myNumber = myNumber - 12;
                }

                if (Players[i].IsSingle)
                {
                    Players[i].Name = PlayerConsts.SingleNames[myNumber];
                }
                else
                {
                    Players[i].Name = PlayerConsts.TeamNames[myNumber];
                }

                Players[i].Descriptionn = PlayerConsts.Descriptions[myNumber];
                Players[i].Image = (myNumber + 1) + ".png";
            }
        }
    }

    public class Player
    {
        public Guid Id { get; set; }
        public int PlaceNumber { get; set; }
        public int TeamNumber { get; set; }
        public string Name { get; set; }
        public string Descriptionn { get; set; }
        public string Image { get; set; }
        public bool IsSingle { get; set; }

        public List<UserAnswer> Answers { get; set; } = [];
        public List<UserAnswer> Answers1 { get; set; }

        public UserAnswer? GetAnswer(int questionId)
        {
            if (Answers.Count > 0 && questionId == Answers.Count - 1)
            {
                return Answers[questionId];
            }

            return null;
        }
    }
}
