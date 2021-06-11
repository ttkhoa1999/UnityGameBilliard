
namespace ThreeDPool.EventHandlers
{
    public struct GameInputEvent : IGameEvent
    {
        public enum States{
            Default,
            HorizontalAxisMovement,
            VerticalAxisMovement,
            Release,
            Paused,
            SpinEffectChoice,
            HorizontalPointMovement,
            VerticalPointMovement
        }

        public float axisOffset;

        public States State;
    }
}
