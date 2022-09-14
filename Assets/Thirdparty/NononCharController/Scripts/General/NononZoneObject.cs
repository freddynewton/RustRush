using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace zone.nonon
{

    public class NononZoneObject : MonoBehaviour, INononZoneObject
    {
        public List<INononZoneObject.NononZoneObjType> objectTypes = new List<INononZoneObject.NononZoneObjType>();

        public string nononZoneObjectName = "";

        public int max_health = 100;
        public int attackPower = 200;
        public int defence = 500;
        public int regenerationAmountPerTick = 10;
        public float regenerationRateInSec = 1;
        public int bounty = 200;

        int current_health = 0;
        bool regenStarted = false;

        List<Transform> aggroFromTable = new List<Transform>();

        private void Start()
        {
            current_health = max_health;

            RegisterGameEvents();
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnDestroy()
        {
            UnRegisterGameEvents();
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        public int GetBounty()
        {
            return GetBountyRecursive(transform);
        }

        private int GetBountyRecursive(Transform parent)
        {
            int accumulatedBounty = 0;
            NononZoneObject obj = parent.GetComponent<NononZoneObject>();
            if (obj != null)
            {
                accumulatedBounty += bounty;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                accumulatedBounty += GetBountyRecursive(child);
            }
            return accumulatedBounty;
        }

        public void SetNononObjectName(string name)
        {
            nononZoneObjectName = name;
        }

        public string GetNononZoneObjectName()
        {
            return nononZoneObjectName;
        }

        public void RegisterGameEvents()
        {
            GameEvents.Instance.onGotAggro += AggroGained;
            GameEvents.Instance.onLostAggro += AggroLost;
        }

        public void UnRegisterGameEvents()
        {
            GameEvents.Instance.onGotAggro -= AggroGained;
            GameEvents.Instance.onLostAggro -= AggroLost;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RegisterGameEvents();
        }

        private void OnSceneUnloaded(Scene current)
        {
            UnRegisterGameEvents();
        }

        public void ResetHealth()
        {
            current_health = max_health;
        }

        IEnumerator RegenerateHealth()
        {
            regenStarted = true;

            // wait for 3 Seconds with regen health
            yield return new WaitForSeconds(3);

            while (current_health < max_health && current_health > 0)
            {
                if (!HasAggro())
                {
                    IncreaseHealth(regenerationAmountPerTick);
                }
                yield return new WaitForSeconds(regenerationRateInSec);
            }

            regenStarted = false;
        }

        public void AddAggro(Transform o)
        {
            if (!aggroFromTable.Contains(o))
            {
                aggroFromTable.Add(o);
            }
        }

        public bool HasAggro()
        {
            return aggroFromTable.Count > 0;
        }

        public void AggroGained(Transform target, Transform from)
        {
            if (target != null && from != null && target.transform.Equals(transform))
            {
                AddAggro(from);
            }
        }

        public void AggroLost(Transform target, Transform from)
        {
            if (target != null && from != null && target.transform.Equals(transform))
            {
                aggroFromTable.Remove(from);
            }
        }

        public int GetCurrentHealth()
        {
            return current_health;
        }

        public void SetCurrentHealth(int _currentHealth)
        {
            current_health = _currentHealth;
        }

        public void IncreaseHealth(int amount)
        {

            if (current_health > 0)
            {
                current_health += amount;
                if (current_health > max_health)
                {
                    current_health = max_health;
                }
                // Let all know that the health has been reduced
                GameEvents.Instance.GainHealth(transform, amount, max_health);
            }

        }

        public void ReduceHealth(int amount)
        {

            if (current_health > 0 && current_health - amount <= 0)
            {
                current_health = 0;
                GameEvents.Instance.TakeDmg(transform, amount, max_health);
                GameEvents.Instance.Die(transform);
            }
            else
            {
                current_health -= amount;
                // Let all know that the health has been reduced
                GameEvents.Instance.TakeDmg(transform, amount, max_health);
                if (!regenStarted)
                {
                    StartCoroutine(RegenerateHealth());
                }
            }
        }

        public bool IsOneOfTypes(INononZoneObject.NononZoneObjType type)
        {
            if (transform.tag.Equals(NononZoneConstants.HitBoxCollider.HITBOX_COLLIDED_TAG))
            {
                // if it's the hitbox collider lookup the NononZoneObject on parent
                INononZoneObject obj;
                if (transform.parent.TryGetComponent<INononZoneObject>(out obj))
                {
                    return obj.IsOneOfTypes(type);

                }
                else
                {
                    // otherwise just try to get it from here
                    return objectTypes.Contains(type);
                }

            }
            else
            {
                return objectTypes.Contains(type);
            }
        }

        public static bool isOneOfTypes(Transform _transform, INononZoneObject.NononZoneObjType type)
        {
            INononZoneObject obj;
            if (_transform.TryGetComponent<INononZoneObject>(out obj))
            {
                return obj.IsOneOfTypes(type);
            }
            else
            {
                return false;
            }
        }

        public void StartAppearingEffect()
        {
            DissolveEffect dissolveEffect = GetComponent<DissolveEffect>();
            if (dissolveEffect != null)
            {
                dissolveEffect.StartAppear(transform);
            }
        }

        public void StopAppearingEffect()
        {
            DissolveEffect dissolveEffect = GetComponent<DissolveEffect>();
            if (dissolveEffect != null)
            {
                dissolveEffect.StopAppear(transform);
            }
        }

    }
}
