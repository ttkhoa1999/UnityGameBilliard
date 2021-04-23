using UnityEngine;
using ThreeDPool.EventHandlers;
using ThreeDPool.Managers;

namespace ThreeDPool.Controllers
{
    class InputController : MonoBehaviour 
    {
        private void Update()
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                EventManager.Notify(typeof(GameInputEvent).Name, this, new GameInputEvent() { State = GameInputEvent.States.Paused });
            }

            if (GameManager.Instance.CurrGameState == GameManager.GameState.GetSet ||
                GameManager.Instance.CurrGameState == GameManager.GameState.Pause)
                return;

            float x = 0.0f;
            float y = 0f;
            if (Input.GetMouseButton(0))
            {
                x = Input.GetAxis("Mouse X") - Input.GetAxis("Horizontal");
                y = Input.GetAxis("Mouse Y");
            }
            else if(Input.GetMouseButtonUp(0))
            {
                EventManager.Notify(typeof(GameInputEvent).Name, this, new GameInputEvent() { State = GameInputEvent.States.Release });
            }
            else
            {

            }

            if (x != 0.0f)
                EventManager.Notify(typeof(GameInputEvent).Name, this, new GameInputEvent() { State = GameInputEvent.States.HorizontalAxisMovement, axisOffset = x });

            if (y != 0.0f)
                EventManager.Notify(typeof(GameInputEvent).Name, this, new GameInputEvent() { State = GameInputEvent.States.VerticalAxisMovement, axisOffset = y });
        }
    }
}
