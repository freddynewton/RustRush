using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace zone.nonon
{
    public abstract class State : MonoBehaviour
    {
        public Transform mainTransform;

        public abstract State RunCurrentState();
        public abstract void OnStateEntry();
        public abstract void OnStateExit();
    }
}