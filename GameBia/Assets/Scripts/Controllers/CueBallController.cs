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

        private GameObject _point;

        private Vector3 _defaultPointPosition;

        private CueBallActionEvent.States _currState;

        private Vector3 _initialPos;

        public bool IsPocketedInPrevTurn;

        public CueBallType BallType { get { return _ballType; } }

        private bool _spinEffect = false;

        private Vector3 cueBallVelocity;

        private void Start()
        {
            _initialPos = transform.position;
            _point = GameObject.Find("Point");
            _defaultPointPosition = _point.transform.localPosition;

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

            if(_ballType == CueBallType.White && _spinEffect == true)
            {
                GameObject cueHead = GameObject.Find("CueHead");
                Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();
                var spinEffectController = _point.GetComponent<SpinEffectController>();
                var cueController = GameObject.Find("CueParent").GetComponent<CueController>();

                float spinForce = spinEffectController.GetSpinForceBySpinRatioAndForceGathered(cueController.ForceGatheredToHit);
                var spinType = spinEffectController.GetSpinType();

                if (collision.gameObject.tag == "Border" || collision.gameObject.tag == "ColorBall")
                {
                    switch (spinType)
                    {
                        case SpinEffectController.SpinType.BackSpin:
                            rigidBody.AddTorque(-cueHead.transform.forward * spinForce);
                            rigidBody.AddForceAtPosition(-cueHead.transform.forward * spinForce, transform.position, ForceMode.Acceleration);
                            break;

                        case SpinEffectController.SpinType.TopSpin:
                            rigidBody.AddTorque(cueHead.transform.forward * spinForce);
                            rigidBody.AddForceAtPosition(cueHead.transform.forward * spinForce, transform.position, ForceMode.Acceleration);
                            break;
                    }
                }

                _spinEffect = false;
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

                if(_point.transform.localPosition != _defaultPointPosition)
                {
                    _point.transform.localPosition = _defaultPointPosition;
                }
            }
            else
            {

            }

            var v = rigidbody.velocity.normalized;
            if (rigidbody.velocity.magnitude > 0.7f)
            {
                rigidbody.angularVelocity = new Vector3(v.z, 0, -v.x) * rigidbody.velocity.magnitude;
            }
        }

        private void OnStriked(float forceGathered)
        {
            if (_ballType == CueBallType.White)
            {
                Vector3 pos = _point.transform.localPosition;
                if (pos.x != 0 && pos.y != 0)
                {
                    _spinEffect = true;
                }

                GameManager.Instance.NumOfBallsStriked++;

                Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();

                StopBall(gameObject);
                var cueHead = GameObject.Find("CueHead");

                rigidBody.AddForce(cueHead.transform.forward * _force * forceGathered, ForceMode.Force);
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

            transform.position = new Vector3(_initialPos.x, _initialPos.y + 0.01f, _initialPos.z);

            IsPocketedInPrevTurn = false;

            _currState = CueBallActionEvent.States.Placing;
            GameManager.Instance.NumOfBallsStriked = 0;
        }

        public void StopBall(GameObject ball)
        {
            Rigidbody rigidbody = ball.GetComponent<Rigidbody>();

            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }
    }
}
