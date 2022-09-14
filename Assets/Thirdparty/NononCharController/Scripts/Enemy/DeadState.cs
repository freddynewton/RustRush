using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace zone.nonon
{
    public class DeadState : State
    {
        public override void OnStateEntry()
        {
            // nothing to do
        }

        public override void OnStateExit()
        {
            // noting to do
        }

        public override State RunCurrentState()
        {
            return this;
        }
    }
}