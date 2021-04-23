using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ThreeDPool.EventHandlers;
using ThreeDPool.Controllers;
using ThreeDPool.UIControllers;

namespace ThreeDPool.Managers
{
    public class GameManager : Singleton<GameManager>
    {
        public enum GameType
        {
            JustCue = 1,
            ThreeBall = 3,
            SixBall = 6,
            SevenBall,
        }

        public enum GameState
        {
            Practise = 1,
            GetSet,
            Play,
            Pause,
            Complete
        }

        [SerializeField]
        private string[] _playerNames;

        [SerializeField]
        private GameType _gameType;

        [SerializeField]
        private Transform _rackTransform;

        [SerializeField]
        private CueBallController _cueBall;

        [SerializeField]
        private GameUIScreen _gameUIScreen;

        private Queue<Player> _players = new Queue<Player>();

        private List<CueBallController> _ballsPocketed;
        private List<CueBallController> _ballsHitOut;
        private GameState _currGameState;
        private GameState _prevGameState;
        private bool _ballsInstantiated;

        public int NumOfBallsStriked;

        public GameState CurrGameState { get { return _currGameState; } }
        public GameState PrevGameState { get { return _prevGameState;  } }

        public Queue<Player> Players { get { return _players;  } }

        public string[] Winners;

        public int NumOfTimesPlayed { private set; get; }

        protected override void Start()
        {
            base.Start();

            ChangeGameState(GameState.Practise);
            NumOfBallsStriked = 0;

            if (_playerNames != null)
            {
                foreach (var playerName in _playerNames)
                {
                    var player = new Player(playerName);

                    _players.Enqueue(player);
                }
            }

            int arraySize = (int)_gameType + 1;
            _ballsPocketed = new List<CueBallController>(arraySize);
            _ballsHitOut = new List<CueBallController>(arraySize);

            _gameUIScreen.CreatePlayerUI();

        }

        private void PlaceBallBasedOnGameType()
        {
            if (_gameType != GameType.JustCue)
            {
                string rackString = "Rack";
                Instantiate((Resources.Load(_gameType.ToString() + rackString, typeof(GameObject)) as GameObject), _rackTransform.position, _rackTransform.rotation);
            }
        }

        private bool IsGameComplete()
        {
            if (_ballsPocketed.Count() == (int)_gameType)
                return true;

            return false;
        }

        private IEnumerator OnGameComplete()
        {
            yield return new WaitForEndOfFrame();

            int winningScore = 0;

            foreach (var player in _players)
            {
                if (player.Score >= winningScore)
                    winningScore = player.Score;
            }

            Winners = _players.Where(p => p.Score == winningScore).Select(p => p.Name).ToArray();

            EventManager.Notify(typeof(GameStateEvent).Name, this, new GameStateEvent() { GameState = GameStateEvent.State.Complete });
        }

        private void SetNewPlayerTurn()
        {
            Player player = _players.Dequeue();
            _players.Enqueue(player);

            Player newPlayer = _players.Peek();
            EventManager.Notify(typeof(GameStateEvent).Name, this, new GameStateEvent() { CurrPlayer = newPlayer.Name });
        }

        private void CalculateThePointAndNextTurn()
        {
            Player currPlayer = _players.Peek();

            if (currPlayer.HasStrikedBall)
            {
                CueBallController whiteBall = _ballsPocketed.FirstOrDefault(b => b.BallType == CueBallController.CueBallType.White);
                if (whiteBall != null)
                {
                    currPlayer.CalculateScore(-1);

                    _ballsPocketed.Remove(whiteBall);

                    _ballsPocketed.ForEach(b => b.IsPocketedInPrevTurn = true);

                    whiteBall.PlaceBallInInitialPos();

                    SetNewPlayerTurn();
                }
                else
                {
                    if (_ballsPocketed.Count() > 0)
                    {
                        var ballsCurrentlyPocketed = _ballsPocketed.Where(b => b.IsPocketedInPrevTurn == false);
                        Debug.Log("Bi đã lọt lỗ" + ballsCurrentlyPocketed.Count());
                        if (ballsCurrentlyPocketed.Count() > 0)
                        {
                            currPlayer.CalculateScore(ballsCurrentlyPocketed.Count());

                            _ballsPocketed.ForEach(b => b.IsPocketedInPrevTurn = true);
                        }
                        else
                        {
                            SetNewPlayerTurn();
                        }
                    }
                    else
                    {
                        SetNewPlayerTurn();
                    }
                }

                foreach (var ballHitOut in _ballsHitOut)
                    ballHitOut.PlaceBallInInitialPos();
            }

            _ballsHitOut.Clear();

            foreach (var player in _players)
            {
                player.SetPlayingState((player == _players.Peek()));
            }

            if (IsGameComplete())
                StartCoroutine(OnGameComplete());
            else
                EventManager.Notify(typeof(CueBallActionEvent).Name, this, new CueBallActionEvent() { State = CueBallActionEvent.States.Stationary });
        }

        public void ChangeGameState(GameState newGameState)
        {
            if (newGameState != _currGameState)
            {
                _prevGameState = _currGameState;
                _currGameState = newGameState;
            }
        }

        public void OnGetSet()
        {
            ChangeGameState(GameState.GetSet);
        }

        public void OnPlay()
        {
            _ballsHitOut.Clear();
            _ballsPocketed.Clear();

            NumOfBallsStriked = 0;

            NumOfTimesPlayed++;

            foreach (var player in _players)
                player.ResetScore();

            ChangeGameState(GameState.Play);

            _cueBall.PlaceBallInInitialPos();

            if (!_ballsInstantiated)
            {
                PlaceBallBasedOnGameType();

                _ballsInstantiated = true;
            }
        }

        public void OnPaused()
        {
            ChangeGameState(GameState.Pause);
        }

        public void OnContinue()
        {
            ChangeGameState(GameState.Play);
        }

        public void ReadyForNextRound()
        {
            if (CurrGameState == GameState.Practise)
            {
                _cueBall.PlaceBallInPosWhilePractise();
            }
            else if(CurrGameState == GameState.Play || CurrGameState == GameState.Pause)
            {
                NumOfBallsStriked--;

                if (NumOfBallsStriked == 0)
                    CalculateThePointAndNextTurn();
            }
            else
            {

            }
        }

        public void AddToBallPocketedList(CueBallController ball)
        {
            if (!_ballsPocketed.Contains(ball))
                _ballsPocketed.Add(ball);
        }

        public void AddToBallHitOutList(CueBallController ball) 
        { 
            if (!_ballsHitOut.Contains(ball) && !_ballsPocketed.Contains(ball))
                _ballsHitOut.Add(ball);
        }
    }
}
