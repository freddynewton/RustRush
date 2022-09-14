using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkCameraHandler : NetworkBehaviour
{
    void Start()
    {
        Debug.Log(IsOwner);

        // if netowrking is active, deactivate Audio Listener and Camera for non Owner Objects
        if (IsOwner)
        {
            GetComponent<AudioListener>().enabled = true;
            GetComponent<Camera>().enabled = true;
        }
        else
        {
            GetComponent<AudioListener>().enabled = false;
            GetComponent<Camera>().enabled = false;
        }
    }
}
