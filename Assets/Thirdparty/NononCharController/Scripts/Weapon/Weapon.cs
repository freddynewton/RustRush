using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace zone.nonon
{
    public class Weapon : MonoBehaviour
    {
        public class Bullet
        {
            [SerializeField, HideInInspector]
            public float time;
            [SerializeField, HideInInspector]
            public Vector3 initalPosition;
            [SerializeField, HideInInspector]
            public Vector3 initialVelocity;
            [SerializeField, HideInInspector]
            public TrailRenderer tracer;
        }


        [SerializeField]
        public enum WeaponTypes { MELEE_1H, MELEE_2H, RANGE_2H, RANGE_1H };

        [Header("Weapon")]
        public WeaponTypes weaponType;
        [SerializeField]
        public float distance = NononZoneConstants.Weapon.WEAPON_STANDARD_DISTANCE;
        [SerializeField]
        public int fireRate = 25;
        [SerializeField]
        public float bulletSpeed = 1000.0f;
        [SerializeField]
        public float bulletDrop = 300.0f;
        [SerializeField]
        public float maxLifeTime = 3.0f;
        public Transform rightHandWeaponHandle;
        public Transform leftHandWeaponHandle;
        public Transform pivotPoint;
        [Header("Effecs")]
        public Transform muzzleFlashEffect;
        public Transform lazerImpactEffect;
        public TrailRenderer lazerTrailEffect;

        public int prefabDirectoryPrefabNr = -1;

        [SerializeField]
        public Transform raycastOrigin;
        [SerializeField, HideInInspector]
        ParticleSystem lazerImpactPS;

        bool isFiring = false;
        ParticleSystem muzzleFlashPS;
        Ray ray;
        RaycastHit hitInfo;
        float accumulatedTime;

        List<Bullet> bullets = new List<Bullet>();

        // Start is called before the first frame update
        void Start()
        {
            if (muzzleFlashEffect != null)
            {
                muzzleFlashPS = muzzleFlashEffect.GetComponent<ParticleSystem>();
            }
            if (lazerImpactEffect != null)
            {
                lazerImpactPS = lazerImpactEffect.GetComponentInChildren<ParticleSystem>();
            }
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;

        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {

        }

        public void SetPrefabDirectoryPrefabNr(int number)
        {
            prefabDirectoryPrefabNr = number;
        }

        public int GetPrefabDirectoryPrefabNr()
        {
            return prefabDirectoryPrefabNr;
        }

        private void OnSceneUnloaded(Scene current)
        {
            bullets.Clear();
        }

        private void OnDestroy()
        {
            bullets.Clear();
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        Vector3 GetPosition(Bullet bullet)
        {
            // p + v*t + 0.5*g*t*t
            Vector3 gravity = Vector3.down * bulletDrop;
            return (bullet.initalPosition) + (bullet.initialVelocity * bullet.time) + (0.5f * gravity * bullet.time * bullet.time);
        }

        Bullet CreateBullet(Vector3 position, Vector3 velocity)
        {
            Bullet bullet = new Bullet();
            bullet.initalPosition = position;
            bullet.initialVelocity = velocity;
            bullet.time = 0.0f;

            bullet.tracer = Instantiate(lazerTrailEffect, position, Quaternion.identity);
            bullet.tracer.AddPosition(position);
            return bullet;
        }

        public bool IsFiring()
        {
            return isFiring;
        }

        public void UpdateFiring(float deltaTime, Vector3 destinationPos)
        {
            accumulatedTime += deltaTime;
            float fireInterval = 1.0f / fireRate;
            while (accumulatedTime >= 0.0f)
            {
                FireBullet(destinationPos);

                accumulatedTime -= fireInterval;
            }
        }

        public void StartFiring(Vector3 destinationPos)
        {
            if (raycastOrigin == null)
            {
                Debug.LogError("You want to shoot but there is no RayCastOrigin set to the weapon. Set both on the Weapon");
                return;
            }
            if (!isFiring)
            {
                isFiring = true;
                accumulatedTime = 0f;
                FireBullet(destinationPos);
            }
        }

        public void UpdateBullets(float deltaTime)
        {
            SimulateBullets(deltaTime);
            DestroyBullets();

        }

        public void StopFiring()
        {
            if (isFiring)
            {
                isFiring = false;
            }

        }

        private void FireBullet(Vector3 destinationPos)
        {
            FireBulletEffect();
            FireBulletObject(destinationPos);
        }


        private void FireBulletEffect()
        {
            GetComponent<WeaponSound>().Shoot();

            if (muzzleFlashPS != null)
            {
                muzzleFlashPS.Emit(1);

            }
        }

        private void FireBulletObject(Vector3 destinationPos)
        {
            Vector3 velocity = (destinationPos - raycastOrigin.position).normalized * bulletSpeed;
            Bullet bullet = CreateBullet(raycastOrigin.position, velocity);
            bullets.Add(bullet);
        }

        void SimulateBullets(float deltaTime)
        {
            bullets.ForEach(bullet =>
            {
                Vector3 p0 = GetPosition(bullet);
                bullet.time += deltaTime;
                Vector3 p1 = GetPosition(bullet);
                if (Vector3.Distance(bullet.initalPosition, p1) > distance)
                {
                    bullet.time = maxLifeTime + 1;
                }
                else
                {
                    RaycastSegment(p0, p1, bullet);
                }

            });
        }

        void DestroyBullets()
        {
            bullets.ForEach(bullet =>
            {
                if (bullet.time > maxLifeTime)
                {
                    DestroyImmediate(bullet.tracer.gameObject);
                }
            });
            bullets.RemoveAll(bullet => (bullet.time > maxLifeTime));
        }


        void RaycastSegment(Vector3 start, Vector3 end, Bullet bullet)
        {
            Vector3 direction = end - start;
            float distance = direction.magnitude;
            ray.origin = start;
            ray.direction = direction;
            if (Physics.Raycast(ray, out hitInfo, distance, GameController.GetRaycastLayerMaskWeaponDmg()))
            {
                GameEvents.Instance.ObjectHit(GetComponentInParent<PlayerRigging>().transform, hitInfo.transform, transform);

                lazerImpactEffect.position = hitInfo.point;
                lazerImpactEffect.forward = hitInfo.normal;
                lazerImpactPS.Emit(1);
                bullet.tracer.transform.position = hitInfo.point;
                bullet.time = maxLifeTime + 1;
            }
            else
            {
                bullet.tracer.transform.position = end;
            }
        }


    }
}