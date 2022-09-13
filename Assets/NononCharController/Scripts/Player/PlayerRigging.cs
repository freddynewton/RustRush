using UnityEngine;

namespace zone.nonon
{
    public class PlayerRigging : MonoBehaviour
    {
        [Header("Common")]
        public Transform CameraAimTarget;
        public bool manualRigging = false;
        public Transform rightHandIKTarget;
        public Transform rightHandIKHint;
        public Transform leftHandIKTarget;
        public Transform leftHandIKHint;

        public Transform weaponHolder2HRight;
        public Transform weaponHolder2HLeft;
        public Transform weaponHolder1HRight;
        public Transform weaponHolder1HLeft;

        public Transform rightHandPos;
        public Transform leftHandPos;

        public Transform weaponDrawHolder;
        public Transform weaponPoseInRig;
        public Transform weaponAimingPoseInRig;

        public Transform bodyAimRig;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}