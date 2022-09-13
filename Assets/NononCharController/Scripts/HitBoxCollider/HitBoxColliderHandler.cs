using UnityEngine;

namespace zone.nonon
{

    /***
     * Standard Helper Class on HitBoxCollider
     * This class needs to have a Collider with isTrigger enabled
     * 
     * The HitBox Collider registrates the following collisions:
     * - Rigidbody Collider
     * - Kinematic Rigidbody Collider
     * - Rigidbody Trigger Collider
     * - Kinematic Trigger Collider
     * https://docs.unity3d.com/Manual/CollidersOverview.html
     * 
     * */

    public class HitBoxColliderHandler : MonoBehaviour
    {

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        /***
         * HitCollider is colliding with another object
         * 
         * */
        void OnTriggerEnter(Collider other)
        {
            

            Transform collidedTransform = other.transform;
            if (other.tag.Equals(NononZoneConstants.HitBoxCollider.HITBOX_COLLIDED_TAG))
            {
                collidedTransform = other.transform.parent;
            }
            GameEvents.Instance.CollisionEntered(collidedTransform, transform.parent);
        }

        /***
         * The other object is leaving the hitbox collider
         * 
         * */
        void OnTriggerExit(Collider other)
        {
            Transform collidedTransform = other.transform;
            if (other.tag.Equals(NononZoneConstants.HitBoxCollider.HITBOX_COLLIDED_TAG))
            {
                collidedTransform = other.transform.parent;
            }
            GameEvents.Instance.CollisionExited(collidedTransform, transform.parent);
        }

        

    }
}