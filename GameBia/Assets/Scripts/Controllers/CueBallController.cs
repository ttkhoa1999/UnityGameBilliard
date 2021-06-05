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

            if(collider.gameObject.tag == "Hole")
            {
                var currPos = gameObject.transform.position;
                gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                gameObject.transform.position = new Vector3(currPos.x, currPos.y, currPos.z);
            }
        }

        private void OnTriggerExit(Collider collider)
        {
            if (BallType != CueBallType.White && collider.gameObject.tag == "Hole")
            {
                gameObject.GetComponent<MeshRenderer>().enabled = false;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Floor"))
            {
                GameManager.Instance.AddToBallHitOutList(this);
            }

            if(collision.gameObject.tag == "ColorBall")
            {
                var vector = gameObject.GetComponent<Rigidbody>().velocity;

                gameObject.GetComponent<Rigidbody>().velocity = new Vector3(vector.x, vector.y, vector.z) * 1.13f;
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

            var v = rigidbody.velocity.normalized;
            if (rigidbody.velocity.magnitude > 1.5f && rigidbody.angularVelocity.magnitude < 1)
            {
                rigidbody.angularVelocity = new Vector3(v.z, 0, -v.x) * rigidbody.velocity.magnitude;
            }
        }

        private void OnStriked(float forceGathered)
        {
            if (_ballType == CueBallType.White)
            {
                GameManager.Instance.NumOfBallsStriked++;

                Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();
                rigidBody.AddForce(Camera.main.transform.forward * _force * forceGathered, ForceMode.Force);

                var rigidbody = gameObject.GetComponent<Rigidbody>();
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
