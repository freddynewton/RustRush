using UnityEngine;

namespace zone.nonon
{
    public class SpawnPoint : MonoBehaviour
    {
        public Color gizmoColor = new Color(1, 0, 0, 0.2f);

        void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawCube(transform.position, new Vector3(0.5f, 0.5f, 0.5f));
        }

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