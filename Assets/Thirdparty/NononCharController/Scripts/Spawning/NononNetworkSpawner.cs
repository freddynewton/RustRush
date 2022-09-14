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