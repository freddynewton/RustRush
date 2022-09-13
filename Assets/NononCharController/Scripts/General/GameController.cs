using UnityEngine;
using UnityEngine.SceneManagement;

namespace zone.nonon
{

    public class GameController : MonoBehaviour
    {

        public Transform playerPrefab;
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
            QualitySettings.vSyncCount = 1;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;

            RegisterGameEvents();
            GameObject player = GameObject.FindGameObjectWithTag(NononZoneConstants.Player.PLAYER_TAG);
            if (player == null)
            {
                // do not instantiate here when it is multiplayer
                Transform playerInstance = GameObject.Instantiate(playerPrefab);
                NononZonePersister.Instance.RestoreValuesOnTransformFromJson(playerInstance.transform, playerInstance.GetComponent<NononZoneObject>().GetNononZoneObjectName());
                playerInstance.GetComponent<NononZoneObject>().StartAppearingEffect();
                playerInstance.GetComponent<PlayerController>().PortPlayer(GetSpawnPosition(), GetSpawnRotation());
            }
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
            if (NononZoneObject.isOneOfTypes(source, INononZoneObject.NononZoneObjType.PLAYER))
            {
                NononZonePersister.Instance.WriteTransform2Json(source, source.GetComponent<NononZoneObject>().GetNononZoneObjectName());
            }
        }


        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            UnRegisterGameEvents();
            RestorePlayer();
        }

        private void RestorePlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag(NononZoneConstants.Player.PLAYER_TAG);
            if (player != null)
            {
                player.GetComponent<NononZoneObject>().StopAppearingEffect();
            }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            NononLogger.Instance.LogDebug("Scene loaded: " + scene.name);
            ResetPlayerPos();
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
                NononZonePersister.Instance.RestoreValuesOnTransformFromJson(player.transform, player.GetComponent<NononZoneObject>().GetNononZoneObjectName());
                player.GetComponent<PlayerController>().PortPlayer(GetSpawnPosition(), GetSpawnRotation());
                break;
            }


        }


        // Update is called once per frame
        void Update()
        {

        }
    }
}