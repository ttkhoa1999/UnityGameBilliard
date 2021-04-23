using System.Collections.Generic;
using UnityEngine;

namespace ThreeDPool.States
{
    public class FSM :  MonoBehaviour
    {
        private List<IState> _states;

        private IState _currentState;

        public void AddState(IState state)
        {
            if (_states.Find(s => s.GetType() == state.GetType()) != null)
                _states.Add(state);
        }

        public void ChangeStateTo(IState newState)
        {
            if (newState == _currentState)
                return;

            if (_currentState != null)
                _currentState.OnExit();

            if (newState != null)
            {
                _currentState = newState;
                _currentState.OnEnter();
            }          
        }

        public void Update()
        {
            if (_currentState != null)
                _currentState.OnUpdate();
        }
    }
}
