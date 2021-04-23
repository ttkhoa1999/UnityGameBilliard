using UnityEngine;
using ThreeDPool.EventHandlers;
using ThreeDPool.Managers;

namespace ThreeDPool.Controllers
{
    public class CueBallController : MonoBehaviour
    {
        public enum CueBallType
        {
            White = 0,
            Yellow,
            Blue,
            Red,
            Purple,
            Orange,
            Green,
            Burgandy,
            Black,
            Striped_Yellow,
            Striped_Blue,
            Striped_Red,
            Striped_Purple,
            Striped_Orange,
            Striped_Green,
            Striped_Burgandy,
        }

        [SerializeField]
        float _force = 30f;

        [SerializeField]
        CueBallType _ballType = CueBallType.White;

        private CueBallActionEvent.States _currState;

        private Vector3 _initialPos;

        public bool IsPocketedInPrevTurn;

        public CueBallType BallType { get { return _ballType; } }


        private void Start()
        {
            _initialPos = transform.position;

            EventManager.Subscribe(typeof(CueBallActionEvent).Name, OnCueBallEvent);
            EventManager.Subscribe(typeof(GameStateEvent).Name, OnGameStateEvent);
        }

        private void OnDestroy()
        {
            EventManager.Unsubscribe(typeof(CueBallActionEvent).Name, OnCueBallEvent);
            EventManager.Unsubscribe(typeof(GameStateEvent).Name, OnGameStateEvent);
        }

        private void OnCueBallEvent(object sender, IGameEvent gameEvent)
        {
            CueBallActionEvent actionEvent = (CueBallActionEvent)gameEvent;
            switch(actionEvent.State)
            {
                case CueBallActionEvent.States.Stationary:
                    {
                        _currState = CueBallActionEvent.States.Default;
                    }
                    break;
            }
        }

        private void OnGameStateEvent(object sender, IGameEvent gameEvent)
        {
            GameStateEvent gameStateEvent = (GameStateEvent)gameEvent;
            switch (gameStateEvent.GameState)
            {
                case GameStateEvent.State.Play:
                    {
                        PlaceBallInInitialPos();
                    }
                    break;
            }
        }

        private void OnTriggerEnter(Collider collider)
        {
            CueController cueController = collider.gameObject.transform.parent.GetComponent<CueController>();

            if (cueController != null)
            {
                if (_ballType == CueBallType.White)
                {
                    EventManager.Notify(typeof(CueBallActionEvent).Name, this, new CueBallActionEvent() { State = CueBallActionEvent.States.Striked });

                    _currState = CueBallActionEvent.States.Striked;

                    float forceGatheredToHit = cueController.ForceGatheredToHit;

                    OnStriked(forceGatheredToHit);
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Floor"))
            {
                Debug.Log("Oncollision" + collision.gameObject.name);

                GameManager.Instance.AddToBallHitOutList(this);
            }
        }

        private void FixedUpdate()
        {
            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
            if ((_currState == CueBallActionEvent.States.Placing) && rigidbody.IsSleeping())
            {
                _currState = CueBallActionEvent.States.Default;
            }
            else if ((_currState == CueBallActionEvent.States.Default) && (!rigidbody.IsSleeping()))
            {
                if (GameManager.Instance.CurrGameState == GameManager.GameState.Play)
                    GameManager.Instance.NumOfBallsStriked++;

                _currState = CueBallActionEvent.States.Striked;
            }
            else if ((_currState == CueBallActionEvent.States.Striked) && (!rigidbody.IsSleeping()))
            {
                _currState = CueBallActionEvent.States.InMotion;
            }
            else if ((_currState == CueBallActionEvent.States.Striked) && (rigidbody.IsSleeping()))
            {
                _currState = CueBallActionEvent.States.InMotion;
            }
            else if ((_currState == CueBallActionEvent.States.InMotion) && rigidbody.IsSleeping())
            {
                GameManager.Instance.ReadyForNextRound();

                _currState = CueBallActionEvent.States.Stationary;
            }
            else
            {

            }
        }

        private void OnStriked(float forceGathered)
        {
            if (_ballType == CueBallType.White)
            {
                GameManager.Instance.NumOfBallsStriked++;

                Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();
                rigidBody.AddForce(Camera.main.transform.forward * _force * forceGathered, ForceMode.Force);
            }
        }

        public void BallPocketed()
        {
            GameManager.Instance.AddToBallPocketedList(this);
        }

        public void PlaceBallInPosWhilePractise()
        {
            PlaceBallInInitialPos();

            EventManager.Notify(typeof(CueBallActionEvent).Name, this, new CueBallActionEvent() { State = CueBallActionEvent.States.Stationary });
        }

        public void PlaceBallInInitialPos()
        {

            transform.position = new Vector3(_initialPos.x, _initialPos.y + 0.2f, _initialPos.z);

            IsPocketedInPrevTurn = false;

            _currState = CueBallActionEvent.States.Placing;
            GameManager.Instance.NumOfBallsStriked = 0;
        }
    }
}
