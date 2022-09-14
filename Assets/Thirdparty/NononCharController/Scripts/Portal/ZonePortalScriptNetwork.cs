using UnityEngine;
using UnityEngine.SceneManagement;
using zone.nonon;
using Unity.Netcode;

namespace zone.nonon
{
    public class ZonePortalScriptNetwork : NetworkBehaviour
    {
        public string zoneName;

        // Start is called before the first frame update
        void Start()
        {
            GameEvents.Instance.onCollisionEntered += OnCollisionEntered;
            GameEvents.Instance.onCollisionExited += OnCollisionExited;
        }

        public override void OnDestroy()
        {
            GameEvents.Instance.onCollisionEntered -= OnCollisionEntered;
            GameEvents.Instance.onCollisionExited -= OnCollisionExited;
            base.OnDestroy();
        }

        private void OnCollisionEntered(Transform source, Transform origin)
        {
            if (origin.Equals(transform) && NononZoneObjectNetwork.isOneOfTypes(source, INononZoneObject.NononZoneObjType.PLAYER))
            {
                if (IsClient && source.GetComponent<NetworkObject>().IsOwner)
                {
                    Destroy(NetworkManager.Singleton.transform.gameObject);
                    SceneManager.LoadScene(zoneName);
                }
                if (IsServer)
                {
                    // save the state of the player before leaving the zone
                    NononZonePersister.Instance.WriteTransform2Json(source, source.GetComponent<NononZoneObjectNetwork>().GetNononZoneObjectName());
                }
            }
        }

        private void OnCollisionExited(Transform source, Transform origin)
        {
        }

    }
}