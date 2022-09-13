using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace zone.nonon
{
    public class StateManager : MonoBehaviour
    {
        public State currentState;

        // Update is called once per frame
        void Update()
        {
            RunStateMachine();
        }

        private void RunStateMachine()
        {
            State nextState = currentState?.RunCurrentState();

            if (nextState != null)
            {
                // Switch to the next state
                SwitchToTheNextState(nextState);
            }
        }

        private void SwitchToTheNextState(State nextState)
        {
            currentState.OnStateExit();
            nextState.OnStateEntry();
            currentState = nextState;

        }
    }
}