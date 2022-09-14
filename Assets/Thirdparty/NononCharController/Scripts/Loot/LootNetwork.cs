using UnityEngine;
using Unity.Netcode;

namespace zone.nonon
{

    public class LootNetwork : NetworkBehaviour
    {
        [HideInInspector]
        public NetworkVariable<bool> lootSpawned = new NetworkVariable<bool>(false);
        [HideInInspector]
        public NetworkVariable<int> spawnedLootPrefabNr = new NetworkVariable<int>(-1);


        public override void OnNetworkSpawn()
        {

            if (IsClient)
            {
                if (lootSpawned.Value && spawnedLootPrefabNr.Value != -1)
                {
                    Transform wpnPrefab = PrefabDirectory.Instance.prefabs[spawnedLootPrefabNr.Value].prefab;
                    Transform wpnTransform = Instantiate(wpnPrefab, Vector3.zero, Quaternion.identity, transform);
                    wpnTransform.localPosition = Vector3.zero;
                    wpnTransform.localRotation = Quaternion.identity;
                    Weapon weapon = wpnTransform.GetComponent<Weapon>();
                    if (weapon != null)
                    {
                        weapon.SetPrefabDirectoryPrefabNr(spawnedLootPrefabNr.Value);
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