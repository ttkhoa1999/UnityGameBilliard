using ThreeDPool.EventHandlers;
using ThreeDPool.Managers;

namespace ThreeDPool
{
    public class Player
    {
        public string Name { private set; get; }
        public int Score { private set; get; }

        public bool HasStrikedBall { private set; get; }

        private bool _isPlaying;

        public Player(string name)
        {
            //Khởi tạo các trường player, điểm 
            Name = name;
            Score = 0;
            //Đăng kí event
            EventManager.Subscribe(typeof(CueBallActionEvent).Name, OnCueBallStriked);
        }
        //Khi bi cái được đánh
        private void OnCueBallStriked(object sender, IGameEvent gameEvent)
        {
            //Khởi tạo event, kiểm tra trạng thái playing = true và trạng thái event đã đánh
            CueBallActionEvent actionEvent = (CueBallActionEvent)gameEvent;
            if (_isPlaying && actionEvent.State == CueBallActionEvent.States.Striked)
                HasStrikedBall = true;
        }

        //Hàm kiểm tra trạng thái playing
        public void SetPlayingState(bool isPlaying)
        {
            _isPlaying = isPlaying;
            HasStrikedBall = false;
        }
        //Hàm tính điểm
        public void CalculateScore(int score)
        {
            Score += score;

            //Điểm sẽ không bao giờ bé hơn 0
            if (Score < 0)
                Score = 0;
            //Đưa ra cảnh báo
            EventManager.Notify(typeof(ScoreUpdateEvent).Name, this, new ScoreUpdateEvent());
        }

        public void ResetScore()
        {
            Score = 0;
        }
    }
}
