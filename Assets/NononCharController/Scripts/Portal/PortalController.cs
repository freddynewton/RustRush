using UnityEngine;

namespace zone.nonon
{

    public class PortalController : MonoBehaviour
    {
        public PortalController targetPortal;
        bool activated = false;

        public void SetPortalActivated()
        {
            activated = true;
        }

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

        // Update is called once per frame
        void Update()
        {

        }

        private void OnCollisionEntered(Transform source, Transform origin)
        {
            if (!activated)
            {
                // TODO: this stuff has to be more generic
                if (origin.Equals(transform))
                {
                    if (NononZoneObject.isOneOfTypes(source, INononZoneObject.NononZoneObjType.PLAYER))
                    {
                        targetPortal.SetPortalActivated();
                        source.GetComponent<PlayerController>().PortPlayer(targetPortal.transform.position, source.rotation);
                    }

                    if (NononZoneObject.isOneOfTypes(source, INononZoneObject.NononZoneObjType.ENEMY))
                    {
                        targetPortal.SetPortalActivated();
                        // Let enemies go through too :)
                        source.GetComponent<Enemy>().PortEnemy(targetPortal.transform.position, source.rotation);

                    }
                }
            }
        }

        private void OnCollisionExited(Transform source, Transform origin)
        {
            // TODO: this stuff has to be more generic
            if (origin.Equals(transform))
            {
                activated = false;
            }
        }
    }
}