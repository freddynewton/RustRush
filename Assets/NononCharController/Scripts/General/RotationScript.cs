using UnityEngine;
using Unity.Netcode;

namespace zone.nonon
{

    public class RotationScript : NetworkBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            transform.Rotate(Vector3.up * Time.deltaTime);
        }
    }
}