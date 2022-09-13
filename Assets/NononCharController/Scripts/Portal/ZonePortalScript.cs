using UnityEngine;
using UnityEngine.SceneManagement;

namespace zone.nonon
{

    public class ZonePortalScript : MonoBehaviour
    {

        public string zoneName;

        // Start is called before the first frame update
        void Start()
        {
            GameEvents.Instance.onCollisionEntered += OnCollisionEntered;
            GameEvents.Instance.onCollisionExited += OnCollisionExited;
        }

        private void OnDestroy()
        {
            GameEvents.Instance.onCollisionEntered -= OnCollisionEntered;
            GameEvents.Instance.onCollisionExited -= OnCollisionExited;
        }

        private void OnCollisionEntered(Transform source, Transform origin)
        {
            if (origin.Equals(transform) && NononZoneObject.isOneOfTypes(source, INononZoneObject.NononZoneObjType.PLAYER))
            {
                // save the state of the player before leaving the zone
                NononZonePersister.Instance.WriteTransform2Json(source, source.GetComponent<NononZoneObject>().GetNononZoneObjectName());
                SceneManager.LoadScene(zoneName);
            }
        }

        private void OnCollisionExited(Transform source, Transform origin)
        {
        }

    }
}