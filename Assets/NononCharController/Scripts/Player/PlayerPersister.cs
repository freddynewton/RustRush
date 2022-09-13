using System;
using System.Collections.Generic;
using UnityEngine;

namespace zone.nonon
{
    public class PlayerPersister : MonoBehaviour, INononZonePersister
    {
        [Serializable]
        public class PlayerData
        {
            [SerializeField]
            public int currentHealth;
            [SerializeField]
            public string playerName;
            [SerializeField]
            public int currentWeapon;
            [SerializeField]
            public int currentMount;
            [SerializeField]
            public List<int> mountsInBag = new List<int>();
            [SerializeField]
            public List<PlayerController.SerializableWeapon> weapons = new List<PlayerController.SerializableWeapon>();
        }

        public void RestoreFromJSON(string serializedJSON)
        {
            Debug.Log("Restore from json....");
            PlayerData playerData = JsonUtility.FromJson<PlayerData>(serializedJSON);
            GetComponent<NononZoneObject>().SetCurrentHealth(playerData.currentHealth);
            GetComponent<NononZoneObject>().SetNononObjectName(playerData.playerName);
            GetComponent<PlayerController>().SetCurrentWeapon(playerData.currentWeapon);
            GetComponent<PlayerController>().SetMounts(playerData.mountsInBag);
            GetComponent<PlayerController>().SetCurrentMount(playerData.currentMount);
            GetComponent<PlayerController>().SetWeapons(playerData.weapons);
        }

        public string Serialize2JSON()
        {
            Debug.Log("Store 2 json....");
            PlayerData playerData = new PlayerData();
            playerData.currentHealth = GetComponent<NononZoneObject>().GetCurrentHealth();
            playerData.playerName = GetComponent<NononZoneObject>().GetNononZoneObjectName();
            playerData.currentWeapon = GetComponent<PlayerController>().GetCurrentWeapon();
            playerData.mountsInBag = GetComponent<PlayerController>().GetMountsInBag();
            playerData.currentMount = GetComponent<PlayerController>().GetCurrentMountIndex();
            playerData.weapons = GetComponent<PlayerController>().GetWeapons();
            return JsonUtility.ToJson(playerData);
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