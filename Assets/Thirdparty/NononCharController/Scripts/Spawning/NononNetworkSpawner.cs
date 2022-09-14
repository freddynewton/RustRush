using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

namespace zone.nonon
{
    public class NononNetworkSpawner : NononSingletonNetwork<NononNetworkSpawner>
    {

        public enum NetworkObjectType { LOOT_BUBBLE, NW_OBJECT };

        [Serializable]
        public class NetworkObjectPrefab
        {
            [SerializeField]
            public int prefabDirectoryIndex;
            [SerializeField]
            public Transform spawnPositon;
            [SerializeField]
            public int respawnTime;
            [SerializeField]
            public NetworkObjectType nwObjectType;
        }
        public Transform lootBubblePrefab;
        public List<NetworkObjectPrefab> networkObjectPrefabs = new List<NetworkObjectPrefab>();

        NetworkVariable<bool> lootSpawned = new NetworkVariable<bool>(false);

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                if (!lootSpawned.Value)
                {
                    int i = 0;
                    foreach (NetworkObjectPrefab networkObjectPrefab in networkObjectPrefabs)
                    {
                        if (networkObjectPrefab.nwObjectType.Equals(NetworkObjectType.LOOT_BUBBLE))
                        {
                            Transform lootBubble = Instantiate(lootBubblePrefab, networkObjectPrefab.spawnPositon.position, networkObjectPrefab.spawnPositon.rotation, null);
                            lootBubble.GetComponent<NetworkObject>().Spawn();
                            lootBubble.GetComponent<LootNetwork>().lootSpawned.Value = true;
                            lootBubble.GetComponent<LootNetwork>().spawnedLootPrefabNr.Value = i;
                            Transform lootTransform = Instantiate(PrefabDirectory.Instance.prefabs[networkObjectPrefab.prefabDirectoryIndex].prefab);
                            lootTransform.parent = lootBubble;
                            lootTransform.localPosition = Vector3.zero;
                            lootTransform.localRotation = Quaternion.identity;
                            Weapon weapon = lootTransform.GetComponent<Weapon>();
                            if (weapon != null)
                            {
                                weapon.SetPrefabDirectoryPrefabNr(i);
                            }

                        }
                        else
                        {
                            Transform lootBubble = Instantiate(PrefabDirectory.Instance.prefabs[networkObjectPrefab.prefabDirectoryIndex].prefab, networkObjectPrefab.spawnPositon.position, networkObjectPrefab.spawnPositon.rotation, null);
                            lootBubble.GetComponent<NetworkObject>().Spawn();
                        }

                        i++;
                    }
                }
            }
            base.OnNetworkSpawn();
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