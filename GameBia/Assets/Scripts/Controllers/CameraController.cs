using UnityEngine;
using ThreeDPool.EventHandlers;
using ThreeDPool.Managers;

namespace ThreeDPool.Controllers
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField]
        private Transform _cueBall = null;

        private float _distFromCueBall;

        private Vector3 _initialPos;
        private Vector3 _initialDir;

        private Vector3 _posToRot = Vector3.one;

        // Use this for initialization
        private void Start()
        {
            _initialPos = transform.position;
            _initialDir = transform.forward;

            _distFromCueBall = Vector3.Distance(_cueBall.position, transform.position);

            EventManager.Subscribe(typeof(GameInputEvent).Name, OnGameInputEvent);
            EventManager.Subscribe(typeof(CueBallActionEvent).Name, OnCueBallEvent);
            EventManager.Subscribe(typeof(GameStateEvent).Name, OnGameStateEvent);
        }

        private void OnDestroy()
        {
            EventManager.Unsubscribe(typeof(GameInputEvent).Name, OnGameInputEvent);
            EventManager.Unsubscribe(typeof(CueBallActionEvent).Name, OnCueBallEvent);
            EventManager.Unsubscribe(typeof(GameStateEvent).Name, OnGameStateEvent);
        }

        private void OnCueBallEvent(object sender, IGameEvent gameEvent)
        {
            CueBallActionEvent cueBallActionEvent = (CueBallActionEvent)gameEvent;

            switch (cueBallActionEvent.State)
            {
                case CueBallActionEvent.States.Stationary:
                case CueBallActionEvent.States.Default:
                    {
                        float yPos = transform.position.y;

                        transform.position = _cueBall.transform.position - transform.forward * _distFromCueBall;
                        transform.position = new Vector3(transform.position.x, yPos, transform.position.z);

                        transform.LookAt(_cueBall);

                        _posToRot = Vector3.one;
                    }
                    break;
                case CueBallActionEvent.States.Striked:
                    {
                        _posToRot = _cueBall.transform.position;
                    }
                    break;
            }
        }

        private void OnGameInputEvent(object sender, IGameEvent gameEvent)
        {
            GameInputEvent gameInputEvent = (GameInputEvent)gameEvent;

            switch (gameInputEvent.State)
            {
                case GameInputEvent.States.HorizontalAxisMovement:
                    {
                        if (_posToRot == Vector3.one)
                        {
                            transform.RotateAround(_cueBall.position, Vector3.up, 20f * gameInputEvent.axisOffset * Time.deltaTime);
                        }
                        else
                        {
                            transform.RotateAround(_posToRot, Vector3.up, 20f * gameInputEvent.axisOffset * Time.deltaTime);
                        }
                    }
                    break;
                case GameInputEvent.States.VerticalAxisMovement:
                    {

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
                        PlaceInInitialPosAndRot();
                    }
                    break;
            }
        }

        private void PlaceInInitialPosAndRot()
        {
            _posToRot = Vector3.one;

            transform.position = _initialPos;
            transform.forward = _initialDir;
        }
    }
}
