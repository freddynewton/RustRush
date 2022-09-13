using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

namespace zone.nonon
{
    public class NononZoneObjectNetwork : NetworkBehaviour, INononZoneObject
    {
        public List<INononZoneObject.NononZoneObjType> objectTypes = new List<INononZoneObject.NononZoneObjType>();

        // values to be set in editor
        public int max_health = 100;
        public int attackPower = 200;
        public int defence = 500;
        public int regenerationAmountPerTick = 10;
        public float regenerationRateInSec = 1;
        public int bounty = 200;

        // dynmic binding
        public string nononZoneObjectName = "";

        private NetworkVariable<NetworkString> networkObjectName = new NetworkVariable<NetworkString>();

        NetworkVariable<int> max_health_nw = new NetworkVariable<int>(100);
        NetworkVariable<int> attackPower_nw = new NetworkVariable<int>(200);
        NetworkVariable<int> defence_nw = new NetworkVariable<int>(500);
        NetworkVariable<int> regenerationAmountPerTick_nw = new NetworkVariable<int>(10);
        NetworkVariable<float> regenerationRateInSec_nw = new NetworkVariable<float>(1);
        NetworkVariable<int> bounty_nw = new NetworkVariable<int>(200);

        NetworkVariable<int> current_health_nw = new NetworkVariable<int>(0);
        bool regenStarted = false;

        List<Transform> aggroFromTable = new List<Transform>();
        NetworkVariable<bool> hasAggro_nw = new NetworkVariable<bool>(false);
        NetworkVariable<ulong> aggroNetworkId = new NetworkVariable<ulong>();

        private void Start()
        {
            if (IsServer) current_health_nw.Value = max_health;

            RegisterGameEvents();
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            // take over the name set in the editor - can be overridden later on
            if (IsServer) SetNononObjectName(nononZoneObjectName);
        }

        public override void OnDestroy()
        {
            UnRegisterGameEvents();
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            base.OnDestroy();
        }

        public int GetBounty()
        {
            return GetBountyRecursive(transform);
        }

        private int GetBountyRecursive(Transform parent)
        {
            int accumulatedBounty = 0;
            NononZoneObjectNetwork obj = parent.GetComponent<NononZoneObjectNetwork>();
            if (obj != null)
            {
                accumulatedBounty += bounty_nw.Value;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                accumulatedBounty += GetBountyRecursive(child);
            }
            return accumulatedBounty;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                max_health_nw.Value = max_health;
                attackPower_nw.Value = attackPower;
                defence_nw.Value = defence;
                regenerationAmountPerTick_nw.Value = regenerationAmountPerTick;
                regenerationRateInSec_nw.Value = regenerationRateInSec;
                bounty_nw.Value = bounty;
                current_health_nw.Value = max_health_nw.Value;
            }
        }

        public void SetNononObjectName(string name)
        {
            nononZoneObjectName = name;
            networkObjectName.Value = name;
        }

        public string GetNononZoneObjectName()
        {
            return networkObjectName.Value;
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
            if (IsOwner)
            {
                ResetHealthServerRpc();
            }
        }

        IEnumerator RegenerateHealth()
        {
            regenStarted = true;

            // wait for 3 Seconds with regen health
            yield return new WaitForSeconds(3);


            while (current_health_nw.Value < max_health && current_health_nw.Value > 0)
            {
                if (!HasAggro())
                {
                    if (regenerationRateInSec != 0 && regenerationAmountPerTick != 0) IncreaseHealth(regenerationAmountPerTick);
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
                if (IsServer)
                {
                    hasAggro_nw.Value = true;
                    aggroNetworkId.Value = o.GetComponent<NetworkObject>().NetworkObjectId;
                }
            }
        }

        public Transform GetAggroTransform()
        {
            if (HasAggro())
            {
                NetworkObject networkObject;
                NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(aggroNetworkId.Value, out networkObject);
                if (networkObject != null) return networkObject.transform;
            }
            return null;
        }

        public bool HasAggro()
        {
            return hasAggro_nw.Value;
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
                if (aggroFromTable.Count == 0)
                {
                    if (IsServer) hasAggro_nw.Value = false;
                }
            }
        }

        public int GetCurrentHealth()
        {
            return current_health_nw.Value;
        }

        public void SetCurrentHealth(int _currentHealth)
        {
            current_health_nw.Value = _currentHealth;
        }

        public void IncreaseHealth(int amount)
        {
            if (IsServer)
            {
                if (current_health_nw.Value > 0)
                {
                    current_health_nw.Value += amount;
                    if (current_health_nw.Value > max_health_nw.Value)
                    {
                        current_health_nw.Value = max_health_nw.Value;
                    }
                    // Let all know that the health has been reduced
                    GameEvents.Instance.GainHealth(transform, amount, max_health_nw.Value);
                    SendIncreaseHealthEventClientRpc(amount);
                }
            }

        }

        public void ReduceHealth(int amount)
        {
            if (IsServer)
            {
                if (current_health_nw.Value > 0 && current_health_nw.Value - amount <= 0)
                {
                    current_health_nw.Value = 0;
                    GameEvents.Instance.TakeDmg(transform, amount, max_health_nw.Value);
                    SendTakeDmgEventClientRpc(amount);
                    GameEvents.Instance.Die(transform);
                    SendDieEventClientRpc();
                }
                else
                {
                    current_health_nw.Value -= amount;
                    // Let all know that the health has been reduced
                    GameEvents.Instance.TakeDmg(transform, amount, max_health_nw.Value);
                    SendTakeDmgEventClientRpc(amount);
                    if (!regenStarted)
                    {
                        StartCoroutine(RegenerateHealth());
                    }
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

        [ServerRpc]
        private void ResetHealthServerRpc()
        {
            current_health_nw.Value = max_health_nw.Value;
        }

        [ClientRpc]
        private void SendIncreaseHealthEventClientRpc(int amount)
        {
            GameEvents.Instance.GainHealth(transform, amount, max_health_nw.Value);
        }

        [ClientRpc]
        private void SendTakeDmgEventClientRpc(int amount)
        {
            if (!IsHost) GameEvents.Instance.TakeDmg(transform, amount, max_health_nw.Value);
        }

        [ClientRpc]
        private void SendDieEventClientRpc()
        {
            GameEvents.Instance.Die(transform);
        }

        [ServerRpc]
        public void ReduceHealthServerRpc(int amount)
        {
            ReduceHealth(amount);
        }

        [ServerRpc]
        public void SetHasAggroServerRpc(bool hasAggro)
        {
            hasAggro_nw.Value = hasAggro;
        }

        [ClientRpc]
        public void StartAppearingEffectClientRpc()
        {
            StartAppearingEffect();
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