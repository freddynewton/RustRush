using UnityEngine;

namespace zone.nonon
{
    public class EnemySound : MonoBehaviour
    {
        [SerializeField]
        private AudioClip[] aggroClips;
        [SerializeField]
        private AudioClip attackClip;
        [SerializeField]
        private AudioClip dieClip;

        private AudioSource audioSource;

        // Start is called before the first frame update
        void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public void Aggro()
        {
            if (aggroClips.Length > 0) audioSource.PlayOneShot(aggroClips[Random.Range(0, aggroClips.Length)]);
        }


        public void Attack()
        {
            if (attackClip != null) audioSource.PlayOneShot(attackClip);
        }

        public void Die()
        {
            if (dieClip != null) audioSource.PlayOneShot(dieClip);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}