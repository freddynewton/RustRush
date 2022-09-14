using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEditor;
using Unity.Netcode;

namespace zone.nonon
{
    [ExecuteInEditMode]
    public class NononZoneEditorMultiplayerElements : MonoBehaviour
    {

        [MenuItem(NononZoneConstants.EditorMultiplayer.ADD_HITBOX_COLLIDER_MENU)]
        public static void AddHitBoxCollider()
        {
            NononZoneEditorCommon.AddHitBoxCollider(NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH, NononZoneConstants.EditorMultiplayer.HITBOX_PREFAB_NAME);
        }

        [MenuItem(NononZoneConstants.EditorMultiplayer.ADD_HEALTH_BAR_MENU)]
        public static void AddHealthBar()
        {
            NononZoneEditorCommon.AddHealthBar(NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH, NononZoneConstants.EditorMultiplayer.HEALTH_BAR_PREFAB_NAME);
        }

        [MenuItem(NononZoneConstants.EditorMultiplayer.ADD_DMG_HEALTH_TEXT_MENU)]
        public static void AddDmgHealthText()
        {
            AddDmgHealthText(NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH,
                NononZoneConstants.EditorMultiplayer.FLOATING_TEXT_CONTAINER_PREFAB_NAME, NononZoneConstants.EditorMultiplayer.DMG_HEALTH_TEXT_SPAWN_POS);
        }

        public static void AddDmgHealthText(string nononZoneControllerPath, string floatingTextContainerPrefabName, string dmgHealthTextSpawnPos)
        {
            GameObject selectedGameObject = Selection.activeObject as GameObject;
            if (selectedGameObject == null)
            {
                EditorUtility.DisplayDialog("Select the Gameobject", "Select the GameObject you want to place the Dmg Health Text on.", "OK");
                return;
            }

            string[] foldersToSearch = { nononZoneControllerPath };
            GameObject foundObject = NononZoneEditorCommon.FindPrefab(floatingTextContainerPrefabName, foldersToSearch);
            if (foundObject != null)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                GameObject dmgTextSpawnPos = new GameObject(dmgHealthTextSpawnPos);
                dmgTextSpawnPos.transform.parent = selectedGameObject.transform;
                dmgTextSpawnPos.transform.localPosition = Vector3.zero;
                dmgTextSpawnPos.transform.localRotation = Quaternion.identity;
                DmgHealthTextControllerNetwork controller = selectedGameObject.AddComponent<DmgHealthTextControllerNetwork>();
                controller.textSpawnPosition = dmgTextSpawnPos.transform;
                controller.floatingTextPrefab = foundObject.GetComponent<FloatingText>();
            }
            else
            {
                Debug.LogError("Prefab " + floatingTextContainerPrefabName + " not found in " + nononZoneControllerPath + ". Did you move or delete it?");
            }
        }

        [MenuItem(NononZoneConstants.EditorMultiplayer.ADD_WATER_MENU)]
        public static void AddWater()
        {
            NononZoneEditorCommon.AddWater(NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH, NononZoneConstants.EditorMultiplayer.WATER_BASIC_DAYTIME_PREFAB_NAME);
        }

        [MenuItem(NononZoneConstants.EditorMultiplayer.ADD_PORTAL_PAIR_MENU)]
        public static void AddPortalPair()
        {
            NononZoneEditorCommon.AddPortalPair(NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH, NononZoneConstants.EditorMultiplayer.PORTAL_PAIR_PREFAB_NAME);
        }

        [MenuItem(NononZoneConstants.EditorMultiplayer.ADD_LOOT_BUBBLE_MENU)]
        public static void AddLootBubble()
        {
            NononZoneEditorCommon.AddLootBubble(NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH, NononZoneConstants.EditorMultiplayer.LOOT_BUBBLE_PREFAB_NAME);
        }

        [MenuItem(NononZoneConstants.EditorMultiplayer.ADD_SPAWN_POINT_MENU)]
        public static void AddSpawnPoint()
        {
            NononZoneEditorCommon.AddSpawnPoint(NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH, NononZoneConstants.EditorMultiplayer.SPAWN_POINT_PREFAB_NAME);
        }

        [MenuItem(NononZoneConstants.EditorMultiplayer.ADD_CUSTOMER_INFO_MENU)]
        public static void AddCustomerInfo()
        {
            NononZoneEditorCommon.AddCustomerInfo(NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH, NononZoneConstants.EditorMultiplayer.CUSTOMER_INFO_PREFAB_NAME);
        }

        [MenuItem(NononZoneConstants.EditorMultiplayer.ADD_ENEMY_MENU)]
        public static void AddEnemy()
        {
            NononZoneEditorCommon.AddEnemy(NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH, NononZoneConstants.EditorMultiplayer.ENEMY_PREFAB_NAME);
        }

        [MenuItem(NononZoneConstants.EditorMultiplayer.ADD_LANDING_PLATTFORM_MENU)]
        public static void AddLandingPlattform()
        {
            NononZoneEditorCommon.AddLandingPlattform(NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH, NononZoneConstants.EditorMultiplayer.LANDING_PLATTFORM_PREFAB_NAME);
        }

        [MenuItem(NononZoneConstants.EditorMultiplayer.ADD_SMALL_PLATTFORM_MENU)]
        public static void AddSmallPlattform()
        {
            NononZoneEditorCommon.AddSmallPlattform(NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH, NononZoneConstants.EditorMultiplayer.SMALL_PLATTFORM_PREFAB_NAME);
        }

        [MenuItem(NononZoneConstants.EditorMultiplayer.ADD_BIG_PLATTFORM_MENU)]
        public static void AddBigPlattform()
        {
            NononZoneEditorCommon.AddBigPlattform(NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH, NononZoneConstants.EditorMultiplayer.BIG_PLATTFORM_PREFAB_NAME);
        }

        [MenuItem(NononZoneConstants.EditorMultiplayer.SETUP_ZONE_MENU)]
        public static void SetupZone()
        {
            if (Camera.main != null)
            {
                DestroyImmediate(Camera.main.gameObject);
            }

            SceneView view = SceneView.lastActiveSceneView;
            view.pivot = Vector3.zero;
            view.Repaint();

            AddTerrain();
            AddMultiPlayer();
            AddZoneControllerUI();
            AddZonePortal();
            AddNononNetworkManager();
            AddNononMultiplayerDevUI();
            AddPrefabDirectory();
            DestroyImmediate(GameObject.FindGameObjectWithTag(NononZoneConstants.Player.PLAYER_TAG));
        }


        public static void AddTerrain()
        {
            NononZoneEditorCommon.AddTerrain(NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH, NononZoneConstants.EditorMultiplayer.TERRAIN_PREFAB_NAME);
        }

        [MenuItem(NononZoneConstants.EditorMultiplayer.ADD_PLAYER_MENU)]
        public static void AddMultiPlayer()
        {
            AddPlayer(NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH,
                NononZoneConstants.EditorMultiplayer.PLAYER_PREFAB_NAME, NononZoneConstants.EditorMultiplayer.GAME_CONTROLLER_PREFAB_NAME,
                NononZoneConstants.EditorMultiplayer.SPAWN_POINT_PREFAB_NAME, NononZoneConstants.EditorMultiplayer.PLAYER_SPAWN_POINT_NAME);
        }

        public static void AddPlayer(string nononZoneControllerPath, string playerPrefabName, string gameControllerPrefabName, string spawnPointPrefabName, string playerSpawnPointName)
        {
            string[] foldersToSearch = { nononZoneControllerPath };
            GameObject playerPrefab = NononZoneEditorCommon.FindPrefab(playerPrefabName, foldersToSearch);
            GameObject gameControllerPrefab = NononZoneEditorCommon.FindPrefab(gameControllerPrefabName, foldersToSearch);
            GameObject spawnPointPrefab = NononZoneEditorCommon.FindPrefab(spawnPointPrefabName, foldersToSearch);
            if (playerPrefab != null && gameControllerPrefab != null)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                GameObject instanciatedPrefab = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
                instanciatedPrefab.transform.position = NononZoneEditorCommon.GetCurrentPositioningVector();
                Selection.activeGameObject = instanciatedPrefab;

                GameObject instanciatedGameController = PrefabUtility.InstantiatePrefab(gameControllerPrefab) as GameObject;
                instanciatedGameController.transform.position = NononZoneEditorCommon.GetCurrentPositioningVector();

                GameObject playerSpawnPoint = PrefabUtility.InstantiatePrefab(spawnPointPrefab) as GameObject;
                playerSpawnPoint.name = playerSpawnPointName;
                playerSpawnPoint.transform.position = NononZoneEditorCommon.GetCurrentPositioningVector();
                GameControllerNetwork gameController = instanciatedGameController.GetComponent<GameControllerNetwork>();
                gameController.playerPrefab = playerPrefab.transform;
                gameController.playerSpawnPoint = playerSpawnPoint.transform;
            }
            else
            {
                if (playerPrefab == null)
                {
                    Debug.LogError("Prefab " + playerPrefabName + " not found in " + nononZoneControllerPath + ". Did you move or delete it?");
                }
                if (gameControllerPrefab == null)
                {
                    Debug.LogError("Prefab " + gameControllerPrefabName + " not found in " + nononZoneControllerPath + ". Did you move or delete it?");
                }
            }
        }

        [MenuItem(NononZoneConstants.EditorMultiplayer.ADD_ZONE_CONTROLLER_UI_MENU)]
        public static void AddZoneControllerUI()
        {
            AddZoneControllerUI(NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH, NononZoneConstants.EditorMultiplayer.ZONE_CONTROLLE_UI_PREFAB_NAME,
                NononZoneConstants.EditorMultiplayer.SPAWN_POINT_PREFAB_NAME, NononZoneConstants.EditorMultiplayer.PLAYER_REBIRTH_POINT_NAME);
        }

        public static void AddZoneControllerUI(string nononZoneControllerPath, string zoneControllerUIPrefabName, string spawnPointPrefabName, string playerRebirthPointName)
        {
            string[] foldersToSearch = { nononZoneControllerPath };
            GameObject foundObject = NononZoneEditorCommon.FindPrefab(zoneControllerUIPrefabName, foldersToSearch);
            GameObject spawnPointPrefab = NononZoneEditorCommon.FindPrefab(spawnPointPrefabName, foldersToSearch);
            if (foundObject != null)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                GameObject playerRebirthGameObject = PrefabUtility.InstantiatePrefab(spawnPointPrefab) as GameObject;
                playerRebirthGameObject.name = playerRebirthPointName;
                playerRebirthGameObject.transform.position = NononZoneEditorCommon.GetCurrentPositioningVector();
                GameObject instanciatedPrefab = PrefabUtility.InstantiatePrefab(foundObject) as GameObject;
                instanciatedPrefab.transform.position = NononZoneEditorCommon.GetCurrentPositioningVector();
                NononZoneControllerNetwork noNoNZoneController = instanciatedPrefab.GetComponent<NononZoneControllerNetwork>();
                noNoNZoneController.playerRespawnPoint = playerRebirthGameObject.transform;
                Selection.activeGameObject = instanciatedPrefab;
            }
            else
            {
                Debug.LogError("Prefab " + zoneControllerUIPrefabName + " not found in " + nononZoneControllerPath + ". Did you move or delete it?");
            }

        }

        public static void AddPrefabDirectory()
        {
            NononZoneEditorCommon.AddPrefabDirectory(NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH, NononZoneConstants.EditorMultiplayer.PREFAB_DIR_PREFAB_NAME);
        }

        [MenuItem(NononZoneConstants.EditorMultiplayer.ADD_ZONE_PORTAL_MENU)]
        public static void AddZonePortal()
        {
            NononZoneEditorCommon.AddZonePortal(NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH, NononZoneConstants.EditorMultiplayer.ZONE_PORTAL_PREFAB_NAME);
        }

        [MenuItem(NononZoneConstants.EditorMultiplayer.ADD_NW_MANAGER_MENU)]
        public static void AddNononNetworkManager()
        {
            string[] foldersToSearch = { NononZoneConstants.EditorMultiplayer.NON_NW_ZONE_CONTROLLER_PATH };
            string[] guids = AssetDatabase.FindAssets("t:prefab", foldersToSearch);
            GameObject foundObject = null;
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (assetPath.Contains(NononZoneConstants.EditorMultiplayer.NETWORK_MANAGER_PREFAB_NAME))
                {
                    foundObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                }

            }
            if (foundObject != null)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                GameObject nwManager = PrefabUtility.InstantiatePrefab(foundObject) as GameObject;
                nwManager.transform.position = NononZoneEditorCommon.GetCurrentPositioningVector();
                Selection.activeGameObject = nwManager;
            }
            else
            {
                Debug.LogError("Prefab " + NononZoneConstants.EditorMultiplayer.NETWORK_MANAGER_PREFAB_NAME + " not found in " + NononZoneConstants.EditorMultiplayer.NON_NW_ZONE_CONTROLLER_PATH + ". Did you move or delete it?");
            }
        }

        [MenuItem(NononZoneConstants.EditorMultiplayer.ADD_MULTIPLAYER_DEV_UI_MENU)]
        public static void AddNononMultiplayerDevUI()
        {
            string[] foldersToSearch = { NononZoneConstants.EditorMultiplayer.NON_NW_ZONE_CONTROLLER_PATH };
            string[] guids = AssetDatabase.FindAssets("t:prefab", foldersToSearch);
            GameObject foundObject = null;
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (assetPath.Contains(NononZoneConstants.EditorMultiplayer.MULTIPLAYER_DEV_UI_PREFAB_NAME))
                {
                    foundObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                }

            }
            if (foundObject != null)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                GameObject nwManager = PrefabUtility.InstantiatePrefab(foundObject) as GameObject;
                nwManager.transform.position = NononZoneEditorCommon.GetCurrentPositioningVector();
                Selection.activeGameObject = nwManager;
            }
            else
            {
                Debug.LogError("Prefab " + NononZoneConstants.EditorMultiplayer.MULTIPLAYER_DEV_UI_PREFAB_NAME + " not found in " + NononZoneConstants.EditorMultiplayer.NON_NW_ZONE_CONTROLLER_PATH + ". Did you move or delete it?");
            }
        }

        [MenuItem(NononZoneConstants.EditorMultiplayer.ADD_NW_SPAWNER_MENU)]
        public static void AddNononNetworkSpawner()
        {
            string[] foldersToSearch = { NononZoneConstants.EditorMultiplayer.NON_NW_ZONE_CONTROLLER_PATH };
            string[] guids = AssetDatabase.FindAssets("t:prefab", foldersToSearch);
            GameObject foundObject = null;

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (assetPath.Contains(NononZoneConstants.EditorMultiplayer.NETWORK_SPAWNER_PREFAB_NAME))
                {
                    foundObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                }

            }

            string[] foldersToSearch2 = { NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH };
            string[] guids2 = AssetDatabase.FindAssets("t:prefab", foldersToSearch2);
            GameObject spawnPointObject = null;
            foreach (string guid in guids2)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (assetPath.Contains(NononZoneConstants.EditorMultiplayer.SPAWN_POINT_PREFAB_NAME))
                {
                    spawnPointObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                }

            }

            if (foundObject != null && spawnPointObject != null)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                GameObject instantiatedPrefab = PrefabUtility.InstantiatePrefab(foundObject) as GameObject;
                instantiatedPrefab.transform.position = NononZoneEditorCommon.GetCurrentPositioningVector();
                NononNetworkSpawner spawner = instantiatedPrefab.GetComponent<NononNetworkSpawner>();
                GameObject instantiateSpawnPointObj = null;
                for (int i = 0; i < 4; i++)
                {
                    instantiateSpawnPointObj = PrefabUtility.InstantiatePrefab(spawnPointObject) as GameObject;
                    instantiateSpawnPointObj.transform.position = NononZoneEditorCommon.GetCurrentPositioningVector();
                    instantiateSpawnPointObj.transform.name = NononZoneConstants.EditorMultiplayer.LOOT_SPAWN_POINT_GO_NAME + i;
                    spawner.networkObjectPrefabs[i].spawnPositon = instantiateSpawnPointObj.transform;
                }

                Selection.activeGameObject = instantiateSpawnPointObj;
            }
            else
            {
                Debug.LogError("Prefab " + NononZoneConstants.EditorMultiplayer.NETWORK_SPAWNER_PREFAB_NAME + " or " + NononZoneConstants.EditorMultiplayer.SPAWN_POINT_PREFAB_NAME + "not found in " + NononZoneConstants.EditorMultiplayer.NON_NW_ZONE_CONTROLLER_PATH + ". Did you move or delete it?");
            }
        }

    }
}