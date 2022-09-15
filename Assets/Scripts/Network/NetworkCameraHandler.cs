using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkCameraHandler : NetworkBehaviour
{
    AudioListener listener;
    Camera camera;

    void Start()
    {
        camera = GetComponent<Camera>();
        listener = GetComponent<AudioListener>();

        // if netowrking is active, deactivate Audio Listener and Camera for non Owner Objects
        if (IsOwner)
        {
            if (camera) camera.enabled = true;
            if (listener) listener.enabled = true;
        }
        else
        {
            if (camera) camera.enabled = false;
            if (listener) listener.enabled = false;
        }
    }
}
