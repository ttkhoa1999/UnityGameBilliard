using System.Collections;
using ThreeDPool.Managers;
using ThreeDPool.EventHandlers;
using UnityEngine;

namespace ThreeDPool.Controllers
{
    class CueController : MonoBehaviour
    {
        public GameObject gameObj = null;

        [SerializeField]
        private Transform _cueBall = null;

        [SerializeField]
        private Transform _cueHead = null;

        private float _defaultDistFromCueBall;

        private float _maxClampDist = 9;

        private float _forceGathered = 0.0f;

        private float _forceThreshold = 0.5f;

        private float _speed = 10.0f;
        private bool _cueReleasedToStrike = false;

        private LineRenderer _line = null;

        private Vector3 _initialPos;
        private Vector3 _initialDir;

        private Vector3 _posToRot = Vector3.one;

        public AudioSource audioStriked;

        public float ForceGatheredToHit { get { return (_forceGathered - _defaultDistFromCueBall) / _maxClampDist;  } }

        private void Start()
        {
            _initialPos = transform.position;
            _initialDir = transform.forward;

            _defaultDistFromCueBall = Vector3.Distance(_cueBall.position, transform.position);

            EventManager.Subscribe(typeof(GameInputEvent).Name, OnGameInputEvent);
            EventManager.Subscribe(typeof(CueBallActionEvent).Name, OnCueBallEvent);
            EventManager.Subscribe(typeof(GameStateEvent).Name, OnGameStateEvent);

            _line = GetComponent<LineRenderer>();
        }

        private void OnDestroy()
        {
            EventManager.Unsubscribe(typeof(GameInputEvent).Name, OnGameInputEvent);
            EventManager.Unsubscribe(typeof(CueBallActionEvent).Name, OnCueBallEvent);
            EventManager.Unsubscribe(typeof(GameStateEvent).Name, OnGameStateEvent);
        }

        private void Update()
        {
            if(Input.GetKey(KeyCode.Mouse0))
            {
                _line.enabled = true;
            }
            else
            {
                _line.enabled = false;
                return;
            }

            float distance;
            Vector3 head = _cueHead.position;
            head.y = _cueBall.position.y;

            Vector3 direction = _cueBall.position - head;
            direction.Normalize();

            RaycastHit hit;
            var layerMask = (1 << LayerMask.NameToLayer("Ball") | 1 << LayerMask.NameToLayer("Border"));

            if (Physics.Raycast(_cueBall.position, direction, out hit, Mathf.Infinity, layerMask))
            {
                distance = Vector3.Distance(_cueBall.position, hit.point);
            }
            else
            {
                distance = 100f;
            }

            Vector3 forward = _cueHead.forward * distance;
            forward.y = 0;

            Vector3[] vertexPos = new Vector3[2] { _cueBall.position, _cueBall.position + forward };

            _line.positionCount = 2;
            _line.SetPositions(vertexPos);
        }

        private void OnGameInputEvent(object sender, IGameEvent gameEvent)
        {
            GameInputEvent gameInputEvent = (GameInputEvent)gameEvent;
            switch (gameInputEvent.State)
            {
                case GameInputEvent.States.HorizontalAxisMovement:
                    {
                        float rotateSpeed = 20f;
                        if(Input.GetKey(KeyCode.LeftShift))
                        {
                            rotateSpeed = 100f;
                        }

                        if (_posToRot == Vector3.one)
                            transform.RotateAround(_cueBall.position, Vector3.up, rotateSpeed * gameInputEvent.axisOffset * Time.deltaTime);
                        else
                            transform.RotateAround(_posToRot, Vector3.up, rotateSpeed * gameInputEvent.axisOffset * Time.deltaTime);
                    }
                    break;
                case GameInputEvent.States.VerticalAxisMovement:
                    {
                        if (_posToRot != Vector3.one)
                            return;

                        var newPosition = transform.position + transform.forward * gameInputEvent.axisOffset;

                        _forceGathered = Vector3.Distance(_cueBall.position, newPosition);

                        if ((_forceGathered < _defaultDistFromCueBall + _maxClampDist) &&
                            _forceGathered > _defaultDistFromCueBall)
                        {
                            transform.position = newPosition;
                            EventManager.Notify(typeof(CueActionEvent).ToString(), this, new CueActionEvent() { ForceGathered = _forceGathered });
                        }
                        else
                        {
                        }

                    }
                    break;
                case GameInputEvent.States.Release:
                    {
                        if (_posToRot != Vector3.one)
                            return;

                        if (_forceGathered > _defaultDistFromCueBall + _forceThreshold)
                            _cueReleasedToStrike = true;
                    }
                    break;
            }
        }

        private void OnCueBallEvent(object sender, IGameEvent gameEvent)
        {
            CueBallActionEvent cueBallActionEvent = (CueBallActionEvent)gameEvent;

            switch (cueBallActionEvent.State)
            {
                case CueBallActionEvent.States.Stationary:
                case CueBallActionEvent.States.Default:
                    {
                        _forceGathered = 0f;

                        transform.position = _cueBall.transform.position - transform.forward * _defaultDistFromCueBall;
                        transform.LookAt(_cueBall);

                        _posToRot = Vector3.one;
                    }
                    break;
                case CueBallActionEvent.States.Striked:
                    {
                        _cueReleasedToStrike = false;

                        if (GameManager.Instance.CurrGameState == GameManager.GameState.Play)
                        {
                            StartCoroutine(MoveCueAfterStrike(transform.position, _cueBall.transform.position - transform.forward * _defaultDistFromCueBall * 1.5f, 1.0f));
                        }

                        transform.LookAt(_cueBall);

                        _posToRot = _cueBall.transform.position;
                    }
                    break;
            }
        }

        private void OnGameStateEvent(object sender, IGameEvent gameEvent)
        {
            GameStateEvent gameStateEvent = (GameStateEvent)gameEvent;
            switch(gameStateEvent.GameState)
            {
                case GameStateEvent.State.Play:
                    {
                        PlaceInInitialPosAndRot();
                    }
                    break;
            }
        }

        IEnumerator MoveCueAfterStrike(Vector3 source, Vector3 target, float overTime)
        {
            float startTime = Time.time;
            while (Time.time < startTime + overTime)
            {
                transform.position = Vector3.Lerp(source, target, (Time.time - startTime) / overTime);
                yield return null;
            }
            transform.position = target;
        }

        private void FixedUpdate()
        {
            if(_cueReleasedToStrike)
            {
                float step = _speed * Time.deltaTime * (_forceGathered/_speed);
                transform.position = Vector3.MoveTowards(transform.position, _cueBall.transform.position, step);

                audioStriked.Play();
            }
        }

        private void PlaceInInitialPosAndRot()
        {
            _forceGathered = 0f;
            _cueReleasedToStrike = false;
            _posToRot = Vector3.one;

            transform.position = _initialPos;
            transform.forward = _initialDir;
        }
    }
}
