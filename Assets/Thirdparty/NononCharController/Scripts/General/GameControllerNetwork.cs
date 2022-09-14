using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using InfimaGames.LowPolyShooterPack;

namespace zone.nonon
{
    public class GameControllerNetwork : NetworkBehaviour
    {
        public Transform playerPrefab;
        public Transform spectatorPrefab;
        public Transform playerSpawnPoint;

        /**
         * Returns the RaycastLayerMask for weapon dmg
         **/
        public static int GetRaycastLayerMaskWeaponDmg()
        {
            // Everything except Layer 31 (coin collector)
            int layerMask = 1 << 31;
            return ~layerMask;
        }

        private void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        // Start is called before the first frame update
        void Start()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkOnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkOnClientDisconnected;


            QualitySettings.vSyncCount = 1;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;

            RegisterGameEvents();
            GameObject player = GameObject.FindGameObjectWithTag(NononZoneConstants.Player.PLAYER_TAG);
        }

        private void RegisterGameEvents()
        {
            GameEvents.Instance.onGotLoot += OnGotLoot;
        }

        private void UnRegisterGameEvents()
        {
            GameEvents.Instance.onGotLoot -= OnGotLoot;
        }

        public void OnGotLoot(Transform source, Transform loot)
        {
            if (NononZoneObjectNetwork.isOneOfTypes(source, INononZoneObject.NononZoneObjType.PLAYER))
            {
                if (IsServer)
                {
                    NononZonePersister.Instance.WriteTransform2Json(source, source.GetComponent<NononZoneObjectNetwork>().GetNononZoneObjectName());
                }
            }
        }

        private void NetworkOnClientDisconnected(ulong obj)
        {
            if (IsServer)
            {
                NononLogger.Instance.LogDebug($"{NetworkManager.Singleton.GetComponent<ServerPlayerData>().GetPlayerName(obj)} disconnected...");

                NetworkManager.Singleton.SceneManager.SetClientSynchronizationMode(LoadSceneMode.Additive);

                GameObject[] players = GameObject.FindGameObjectsWithTag(NononZoneConstants.Player.PLAYER_TAG);
                foreach (GameObject player in players)
                {
                    if (GetComponent<NetworkObject>().NetworkObjectId.Equals(obj))
                    {
                        DestroyImmediate(player);
                        break;
                    }
                }
            }
        }

        private void NetworkOnClientConnected(ulong obj)
        {
            if (IsServer)
            {
                SpawnPlayerAndPort(obj);
            }
        }


        private void SpawnPlayerAndPort(ulong clientId)
        {
            if (IsServer)
            {
                NononLogger.Instance.LogDebug($"{NetworkManager.Singleton.GetComponent<ServerPlayerData>().GetPlayerName(clientId)} connected...");

                string playerName = NetworkManager.Singleton.GetComponent<ServerPlayerData>().GetPlayerName(clientId);
                if (playerName.Equals(NononZoneConstants.Starting.MODE_SPECTATOR))
                {
                    Transform spectatorInstance = Instantiate(spectatorPrefab);
                    spectatorInstance.GetComponent<SpectatorControllerNetwork>().SetPlayerPos(GetSpawnPosition(), GetSpawnRotation());
                    spectatorInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
                }
                else
                {
                    Transform playerInstance = Instantiate(playerPrefab);

                    playerInstance.GetComponent<NononZoneObjectNetwork>().SetNononObjectName(playerName);
                    // Restore the values which are persisted for the Player
                    NononZonePersister.Instance.RestoreValuesOnTransformFromJson(playerInstance, playerName);
                    // Spawn the player
                    playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
                    // Add the weapons again to the character on the server
                    //playerInstance.GetComponent<PlayerControllerNetwork>().InitializeWeaponSlotsAndSpawnWeapons();
                    // Initiate the appearing effect
                    playerInstance.GetComponent<NononZoneObjectNetwork>().StartAppearingEffectClientRpc();
                    // set the position on the server
                    playerInstance.GetComponent<Movement>().SetPlayerPos(GetSpawnPosition(), GetSpawnRotation());
                    // set the position on the client
                    playerInstance.GetComponent<Movement>().PortPlayerClientRpc(GetSpawnPosition(), GetSpawnRotation());
                }

            }
        }

        public override void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            UnRegisterGameEvents();
            RestorePlayer();

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= NetworkOnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkOnClientDisconnected;
            }
            base.OnDestroy();
        }

        private void RestorePlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag(NononZoneConstants.Player.PLAYER_TAG);
            if (player != null)
            {
                player.GetComponent<NononZoneObjectNetwork>().StopAppearingEffect();
            }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            NononLogger.Instance.LogDebug("Scene loaded: " + scene.name);
            if (ClientPlayerData.Instance.mode.Equals(NononZoneConstants.Starting.MODE_CLIENT))
            {
                StartingClient(ClientPlayerData.Instance.joinCode);
            }
            if (ClientPlayerData.Instance.mode.Equals(NononZoneConstants.Starting.MODE_SPECTATOR))
            {
                ClientPlayerData.Instance.playerName = NononZoneConstants.Starting.MODE_SPECTATOR;
                StartingClient(ClientPlayerData.Instance.joinCode);
            }

            if (ClientPlayerData.Instance.mode.Equals(NononZoneConstants.Starting.MODE_SERVER))
            {

                NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;

                if (NetworkManager.Singleton.StartServer())
                {
                    NononLogger.Instance.LogDebug("Server started...");
                }
                else
                {
                    NononLogger.Instance.LogDebug("Server could not be started");
                }
            }

            if (ClientPlayerData.Instance.mode.Equals(NononZoneConstants.Starting.MODE_HOST))
            {
                StartingHost(scene.name);
            }
            ResetPlayerPos();
        }

        private async void StartingClient(string joinCode)
        {
            if (RelayManager.Instance.IsRelayEnabled)
            {
                await RelayManager.Instance.JoinRelay(joinCode);
            }

            Encoding enc = Encoding.GetEncoding(Encoding.UTF8.WebName);
            NetworkManager.Singleton.NetworkConfig.ConnectionData = enc.GetBytes(ClientPlayerData.Instance.playerName);
            if (NetworkManager.Singleton.StartClient())
            {
                NononLogger.Instance.LogDebug("Client started...");
            }
            else
            {
                NononLogger.Instance.LogDebug("Client could not be started");

            }

        }

        private async void StartingHost(string sceneName)
        {
            if (RelayManager.Instance.IsRelayEnabled)
            {
                await RelayManager.Instance.SetupRelay();
            }

            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;


            Encoding enc = Encoding.GetEncoding(Encoding.UTF8.WebName);
            NetworkManager.Singleton.NetworkConfig.ConnectionData = enc.GetBytes(ClientPlayerData.Instance.playerName);
            if (NetworkManager.Singleton.StartHost())
            {
                NononLogger.Instance.LogDebug("Host started in " + sceneName);
                if (!RelayManager.Instance.IsRelayEnabled) SpawnPlayerAndPort(NetworkManager.Singleton.LocalClientId);
            }
            else
            {
                NononLogger.Instance.LogDebug("Host could not be started in " + sceneName);

            }
        }

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            // The client identifier to be authenticated
            var clientId = request.ClientNetworkId;

            // Additional connection data defined by user code
            var connectionData = request.Payload;

            // Your approval logic determines the following values
            response.Approved = true;
            response.CreatePlayerObject = false;


            Encoding enc = Encoding.GetEncoding(Encoding.UTF8.WebName);
            string playerName = enc.GetString(connectionData);
            ServerPlayerData ii = NetworkManager.Singleton.GetComponent<ServerPlayerData>();
            ii.AddPlayerInfo(clientId, playerName);

            // The prefab hash value of the NetworkPrefab, if null the default NetworkManager player prefab is used
            response.PlayerPrefabHash = null;

            // Position to spawn the player object (if null it uses default of Vector3.zero)
            response.Position = Vector3.zero;

            // Rotation to spawn the player object (if null it uses the default of Quaternion.identity)
            response.Rotation = Quaternion.identity;

            // If additional approval steps are needed, set this to true until the additional steps are complete
            // once it transitions from true to false the connection approval response will be processed.
            response.Pending = false;
        }

        Vector3 GetSpawnPosition()
        {
            Vector3 position = Vector3.zero;
            if (playerSpawnPoint != null)
            {
                position = playerSpawnPoint.position;
            }
            return position;
        }

        Quaternion GetSpawnRotation()
        {
            Quaternion rotation = Quaternion.identity;
            if (playerSpawnPoint != null)
            {
                rotation = playerSpawnPoint.rotation;
            }
            return rotation;
        }

        public void ResetPlayerPos()
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag(NononZoneConstants.Player.PLAYER_TAG);
            foreach (GameObject player in players)
            {
                if (player.GetComponent<NetworkObject>().IsOwner)
                {
                    player.GetComponent<Movement>().PortPlayer(GetSpawnPosition(), GetSpawnRotation());
                    break;
                }
            }
        }
    }
}