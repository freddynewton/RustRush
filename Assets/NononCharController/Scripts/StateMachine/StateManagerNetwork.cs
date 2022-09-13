using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace zone.nonon
{

    public class StateManagerNetwork : NetworkBehaviour
    {
        public State currentState;

        // Update is called once per frame
        void Update()
        {
            if (IsServer)
            {
                RunStateMachine();
            }
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