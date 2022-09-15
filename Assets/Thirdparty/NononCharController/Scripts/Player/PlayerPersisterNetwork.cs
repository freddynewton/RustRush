using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace zone.nonon
{

    public class PlayerPersisterNetwork : NetworkBehaviour, INononZonePersister
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
            public List<PlayerControllerNetwork.SerializableWeapon> weapons = new List<PlayerControllerNetwork.SerializableWeapon>();
        }

        public void RestoreFromJSON(string serializedJSON)
        {
            Debug.Log("Restore from json....");
            PlayerData playerData = JsonUtility.FromJson<PlayerData>(serializedJSON);
            GetComponent<NononZoneObjectNetwork>().SetCurrentHealth(playerData.currentHealth);
            GetComponent<NononZoneObjectNetwork>().SetNononObjectName(playerData.playerName);
        }

        public string Serialize2JSON()
        {
            Debug.Log("Store 2 json....");
            PlayerData playerData = new PlayerData();
            playerData.currentHealth = GetComponent<NononZoneObjectNetwork>().GetCurrentHealth();
            playerData.playerName = GetComponent<NononZoneObjectNetwork>().GetNononZoneObjectName();
            return JsonUtility.ToJson(playerData);
        }

    }
}