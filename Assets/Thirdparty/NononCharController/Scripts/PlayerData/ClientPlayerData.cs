using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace zone.nonon
{
    public class ClientPlayerData : NononSingleton<ClientPlayerData>
    {
        public string mode = "";
        public string loadingScene = "";
        public string playerName = "";
        public string joinCode = "";

        private void Start()
        {
            DontDestroyOnLoad(transform);
        }
    }
}