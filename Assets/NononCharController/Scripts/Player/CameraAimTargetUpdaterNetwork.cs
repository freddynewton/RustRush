using Cinemachine;
using Unity.Netcode;
using UnityEngine;

namespace zone.nonon
{
    public class CameraAimTargetUpdaterNetwork : NetworkBehaviour
    {
        CinemachineBrain brain;

        // Start is called before the first frame update
        void Start()
        {
            if (IsClient && IsOwner)
            {
                if (Camera.main != null)
                {
                    brain = Camera.main.GetComponent<CinemachineBrain>();
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (brain != null && brain.ActiveVirtualCamera != null)
            {
                Vector3 referenceLookAt = brain.ActiveVirtualCamera.State.ReferenceLookAt;
                if (referenceLookAt != null) transform.position = referenceLookAt;
                transform.rotation = new Quaternion(transform.rotation.x, transform.rotation.y, 0, transform.rotation.w);
            }
        }
    }
}