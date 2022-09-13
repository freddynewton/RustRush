using UnityEngine;

namespace zone.nonon
{

    public class WeaponSound : MonoBehaviour
    {
        [SerializeField]
        private AudioClip shootClip;

        private AudioSource audioSource;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Shoot()
        {
            if (shootClip != null) audioSource.PlayOneShot(shootClip);
        }
    }
}