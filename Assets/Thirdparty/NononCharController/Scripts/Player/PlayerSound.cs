using UnityEngine;

namespace zone.nonon
{
    public class PlayerSound : MonoBehaviour
    {
        [SerializeField]
        private AudioClip runClip;
        [SerializeField]
        private AudioClip walkClip;
        [SerializeField]
        private AudioClip jumpClip;
        [SerializeField]
        private AudioClip splashClip;
        [SerializeField]
        private AudioClip swimClip;
        [SerializeField]
        private AudioClip dieClip;
        [SerializeField]
        private AudioClip flyClip;
        [SerializeField]
        private AudioClip draw2HRangeWeaponClip;
        [SerializeField]
        private AudioClip shed2HRangeWeaponClip;        

        private AudioSource audioSource;
        bool flyingSoundStarted = false;

        // Start is called before the first frame update
        void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public void Run()
        {
            if (runClip != null) audioSource.PlayOneShot(runClip);
        }

        public void Walk()
        {
            if (walkClip != null) audioSource.PlayOneShot(walkClip);
        }

        public void Jump()
        {
            if (jumpClip != null) audioSource.PlayOneShot(jumpClip);
        }

        public void Splash()
        {
            if (splashClip != null) audioSource.PlayOneShot(splashClip);
        }

        private void Swim()
        {
            if (swimClip != null) audioSource.PlayOneShot(swimClip);
        }

        public void Die()
        {
            if (dieClip != null) audioSource.PlayOneShot(dieClip);
        }

        public void DrawWeapon(Weapon.WeaponTypes wpnType)
        {
            if (wpnType.Equals(Weapon.WeaponTypes.RANGE_2H))
            {
                if (draw2HRangeWeaponClip != null) audioSource.PlayOneShot(draw2HRangeWeaponClip);
            }
        }

        public void ShedWeapon(Weapon.WeaponTypes wpnType)
        {
            if (wpnType.Equals(Weapon.WeaponTypes.RANGE_2H))
            {
                if (shed2HRangeWeaponClip != null) audioSource.PlayOneShot(shed2HRangeWeaponClip);
            }
        }

        public void StartFlySound()
        {
            if (flyClip != null)
            {

                if (!flyingSoundStarted)
                {
                    flyingSoundStarted = true;
                    audioSource.loop = true;
                    audioSource.PlayOneShot(flyClip);
                    audioSource.loop = false;
                }
            }
        }

        public void StopFlySound()
        {
            if (flyClip != null)
            {

                if (flyingSoundStarted)
                {
                    flyingSoundStarted = false;
                    audioSource.Stop();
                }
            }
        }
    }
}