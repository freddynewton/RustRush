using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEditor;

namespace zone.nonon
{
    /***
     * NoNoN Zone Component Helper Classes
     * 
     * */
    [ExecuteInEditMode]
    public class NononZoneEditorSinglePlayerElements : MonoBehaviour
    {
        [MenuItem(NononZoneConstants.EditorSinglePlayer.SETUP_ZONE_MENU)]
        public static void SetupZoneSinglePlayer()
        {
            if (Camera.main != null)
            {
                DestroyImmediate(Camera.main.gameObject);
            }

            SceneView view = SceneView.lastActiveSceneView;
            view.pivot = Vector3.zero;
            view.Repaint();

            AddTerrain();
            AddSinglePlayer();
            AddPrefabDirectory();
            AddZoneControllerUI();
            AddZonePortal();
            DestroyImmediate(GameObject.FindGameObjectWithTag(NononZoneConstants.Player.PLAYER_TAG));
        }

        public static void AddTerrain()
        {
            NononZoneEditorCommon.AddTerrain(NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH, NononZoneConstants.EditorSinglePlayer.TERRAIN_PREFAB_NAME);
        }

        [MenuItem(NononZoneConstants.EditorSinglePlayer.ADD_PREFAB_DIR_MENU)]
        public static void AddPrefabDirectory()
        {
            NononZoneEditorCommon.AddPrefabDirectory(NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH, NononZoneConstants.EditorSinglePlayer.PREFAB_DIR_PREFAB_NAME);
        }


        [MenuItem(NononZoneConstants.EditorSinglePlayer.ADD_PLAYER_MENU)]
        public static void AddSinglePlayer()
        {
            AddPlayer(NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH,
                NononZoneConstants.EditorSinglePlayer.PLAYER_PREFAB_NAME, NononZoneConstants.EditorSinglePlayer.GAME_CONTROLLER_PREFAB_NAME,
                NononZoneConstants.EditorSinglePlayer.SPAWN_POINT_PREFAB_NAME, NononZoneConstants.EditorSinglePlayer.PLAYER_SPAWN_POINT_NAME);
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
                GameController gameController = instanciatedGameController.GetComponent<GameController>();
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

        [MenuItem(NononZoneConstants.EditorSinglePlayer.ADD_ZONE_CONTROLLER_UI_MENU)]
        public static void AddZoneControllerUI()
        {
            AddZoneControllerUI(NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH, NononZoneConstants.EditorSinglePlayer.ZONE_CONTROLLE_UI_PREFAB_NAME,
                NononZoneConstants.EditorSinglePlayer.SPAWN_POINT_PREFAB_NAME, NononZoneConstants.EditorSinglePlayer.PLAYER_REBIRTH_POINT_NAME);
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
                NononZoneController noNoNZoneController = instanciatedPrefab.GetComponent<NononZoneController>();
                noNoNZoneController.playerRespawnPoint = playerRebirthGameObject.transform;
                Selection.activeGameObject = instanciatedPrefab;
            }
            else
            {
                Debug.LogError("Prefab " + zoneControllerUIPrefabName + " not found in " + nononZoneControllerPath + ". Did you move or delete it?");
            }

        }

        [MenuItem(NononZoneConstants.EditorSinglePlayer.ADD_HITBOX_COLLIDER_MENU)]
        public static void AddHitBoxCollider()
        {
            NononZoneEditorCommon.AddHitBoxCollider(NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH, NononZoneConstants.EditorSinglePlayer.HITBOX_PREFAB_NAME);
        }

        [MenuItem(NononZoneConstants.EditorSinglePlayer.ADD_HEALTH_BAR_MENU)]
        public static void AddHealthBar()
        {
            NononZoneEditorCommon.AddHealthBar(NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH, NononZoneConstants.EditorSinglePlayer.HEALTH_BAR_PREFAB_NAME);
        }

        [MenuItem(NononZoneConstants.EditorSinglePlayer.ADD_DMG_HEALTH_TEXT_MENU)]
        public static void AddDmgHealthText()
        {
            AddDmgHealthText(NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH,
                NononZoneConstants.EditorSinglePlayer.FLOATING_TEXT_CONTAINER_PREFAB_NAME, NononZoneConstants.EditorSinglePlayer.DMG_HEALTH_TEXT_SPAWN_POS);
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
                DmgHealthTextController controller = selectedGameObject.AddComponent<DmgHealthTextController>();
                controller.textSpawnPosition = dmgTextSpawnPos.transform;
                controller.floatingTextPrefab = foundObject.GetComponent<FloatingText>();
            }
            else
            {
                Debug.LogError("Prefab " + floatingTextContainerPrefabName + " not found in " + nononZoneControllerPath + ". Did you move or delete it?");
            }
        }

        [MenuItem(NononZoneConstants.EditorSinglePlayer.ADD_WATER_MENU)]
        public static void AddWater()
        {
            NononZoneEditorCommon.AddWater(NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH, NononZoneConstants.EditorSinglePlayer.WATER_BASIC_DAYTIME_PREFAB_NAME);
        }

        [MenuItem(NononZoneConstants.EditorSinglePlayer.ADD_PORTAL_PAIR_MENU)]
        public static void AddPortalPair()
        {
            NononZoneEditorCommon.AddPortalPair(NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH, NononZoneConstants.EditorSinglePlayer.PORTAL_PAIR_PREFAB_NAME);
        }


        [MenuItem(NononZoneConstants.EditorSinglePlayer.ADD_ZONE_PORTAL_MENU)]
        public static void AddZonePortal()
        {
            NononZoneEditorCommon.AddZonePortal(NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH, NononZoneConstants.EditorSinglePlayer.ZONE_PORTAL_PREFAB_NAME);
        }

        [MenuItem(NononZoneConstants.EditorSinglePlayer.ADD_LOOT_BUBBLE_MENU)]
        public static void AddLootBubble()
        {
            NononZoneEditorCommon.AddLootBubble(NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH, NononZoneConstants.EditorSinglePlayer.LOOT_BUBBLE_PREFAB_NAME);
        }

        [MenuItem(NononZoneConstants.EditorSinglePlayer.ADD_SPAWN_POINT_MENU)]
        public static void AddSpawnPoint()
        {
            NononZoneEditorCommon.AddSpawnPoint(NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH, NononZoneConstants.EditorSinglePlayer.SPAWN_POINT_PREFAB_NAME);
        }

        [MenuItem(NononZoneConstants.EditorSinglePlayer.ADD_ENEMY_MENU)]
        public static void AddEnemy()
        {
            NononZoneEditorCommon.AddEnemy(NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH, NononZoneConstants.EditorSinglePlayer.ENEMY_PREFAB_NAME);
        }

        [MenuItem(NononZoneConstants.EditorSinglePlayer.ADD_CUSTOMER_INFO_MENU)]
        public static void AddCustomerInfo()
        {
            NononZoneEditorCommon.AddCustomerInfo(NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH, NononZoneConstants.EditorSinglePlayer.CUSTOMER_INFO_PREFAB_NAME);
        }

        [MenuItem(NononZoneConstants.EditorSinglePlayer.ADD_LANDING_PLATTFORM_MENU)]
        public static void AddLandingPlattform()
        {
            NononZoneEditorCommon.AddLandingPlattform(NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH, NononZoneConstants.EditorSinglePlayer.LANDING_PLATTFORM_PREFAB_NAME);
        }

        [MenuItem(NononZoneConstants.EditorSinglePlayer.ADD_SMALL_PLATTFORM_MENU)]
        public static void AddSmallPlattform()
        {
            NononZoneEditorCommon.AddSmallPlattform(NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH, NononZoneConstants.EditorSinglePlayer.SMALL_PLATTFORM_PREFAB_NAME);
        }

        [MenuItem(NononZoneConstants.EditorSinglePlayer.ADD_BIG_PLATTFORM_MENU)]
        public static void AddBigPlattform()
        {
            NononZoneEditorCommon.AddBigPlattform(NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH, NononZoneConstants.EditorSinglePlayer.BIG_PLATTFORM_PREFAB_NAME);
        }
    }
}