namespace Elementary.Business
{
    public class Game
    {
        public Game()
        {
            var freePlaces = new int[12];
            for (var i = 0; i < 12; i++)
            {
                freePlaces[i] = i;
            }

            Random.Shared.Shuffle(freePlaces);
            FreePlaces = freePlaces.ToList();
            State = GameState.Welcome;
        }

        public enum GameState
        {
            Welcome = 0,
            WhellRun = 1,
            Started = 2,
        }

        public GameState State { get; set; }

        public int CurrentQuestionId = -1;

        public List<Question> Questions = QuestionHolder.GetQuestions();
        public List<UserAnswer> Answers = [];
        public List<Player> Players { get; set; } = new List<Player>();
        public List<int> FreePlaces { get; set; }
        public int SectorValue { get; private set; }

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
            State = GameState.Started;
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

        public void Join(Guid playerId, bool isSingle)
        {
            if (State != GameState.Welcome)
            {
                throw new Exception("need welcome state");
            }

            if (Players.Any(x => x.Id == playerId))
            {
                return;
            }

            var number = FreePlaces[Players.Count];
            Players.Add(new Player { Id = playerId, PlaceNumber = number, IsSingle = isSingle });
        }

        internal void SpinWhell()
        {
            State = GameState.WhellRun;
            var sectorValue = Random.Shared.Next(0, 12);
            SectorValue = sectorValue;
            for (var i = 0; i < Players.Count; i++)
            {
                var myNumber = Players[i].PlaceNumber + sectorValue;
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
                Players[i].Descriptionn = PlayerConsts.TeamNames[myNumber];
            }
        }
    }

    public class Player
    {
        public Guid Id { get; set; }
        public int PlaceNumber { get; set; }
        public string Name { get; set; }
        public string Descriptionn { get; set; }
        public string Image { get; set; }
        public bool IsSingle { get; set; }
    }
}
