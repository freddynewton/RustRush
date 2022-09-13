using UnityEngine;

namespace zone.nonon
{
    public class LookAtCamera : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Camera.main != null)
            {
                Quaternion lookRotation = Camera.main.transform.rotation;
                transform.rotation = lookRotation;
            }            
        }
    }
}