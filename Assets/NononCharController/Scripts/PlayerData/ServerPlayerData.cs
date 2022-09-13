using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace zone.nonon
{
    public class ServerPlayerData : MonoBehaviour
    {
        public class PlayerInfo
        {
            public ulong clientId;
            public string playerName;

            public PlayerInfo()
            {

            }

            public PlayerInfo(ulong _clientId, string _playerName)
            {
                clientId = _clientId;
                playerName = _playerName;
            }
        }

        public Dictionary<ulong, PlayerInfo> playerInfos = new Dictionary<ulong, PlayerInfo>();

        public void AddPlayerInfo(ulong _clientId, string _playername)
        {
            playerInfos.Add(_clientId, new PlayerInfo(_clientId, _playername));
        }

        public string GetPlayerName(ulong _clientId)
        {
            PlayerInfo info;
            if (playerInfos.TryGetValue(_clientId, out info))
            {
                return info.playerName;
            }
            return "PlayerNotFound";
        }
    }
}