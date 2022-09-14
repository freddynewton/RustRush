using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEditor;

namespace zone.nonon
{

    public class NononZoneEditorCommon
    {
        public static Vector3 GetCurrentPositioningVector()
        {
            Vector3 objectInCameraView = SceneView.lastActiveSceneView.camera.transform.TransformPoint(Vector3.forward * NononZoneConstants.EditorSinglePlayer.OBJECT_PLACING_DISTANCE);
            return objectInCameraView;
        }

        public static void AddHitBoxCollider(string nononZoneControllerPath, string hitBoxPrefabName)
        {
            GameObject selectedGameObject = Selection.activeObject as GameObject;
            if (selectedGameObject == null)
            {
                EditorUtility.DisplayDialog("Select the Gameobject", "Select the GameObject you want to place the Hitbox on.", "OK");
                return;
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            string[] foldersToSearch = { nononZoneControllerPath };
            GameObject foundObject = FindPrefab(hitBoxPrefabName, foldersToSearch);
            if (foundObject != null)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                GameObject instanciatedPrefab = PrefabUtility.InstantiatePrefab(foundObject, selectedGameObject.transform) as GameObject;
                instanciatedPrefab.transform.rotation = selectedGameObject.transform.rotation;
                instanciatedPrefab.transform.localPosition = Vector3.zero;
                instanciatedPrefab.transform.localScale = Vector3.one;
                BoxCollider collider = instanciatedPrefab.GetComponent<BoxCollider>();
                // make the collider slightly bigger otherwise trigger will not fire
                collider.size = Vector3.one;
                collider.center = Vector3.zero;

                Rigidbody body;
                if (!selectedGameObject.TryGetComponent<Rigidbody>(out body))
                {
                    Rigidbody newBody = selectedGameObject.AddComponent<Rigidbody>();
                    newBody.isKinematic = true;
                    newBody.useGravity = false;
                }

            }
            else
            {
                Debug.LogError("Prefab " + hitBoxPrefabName + " not found in " + nononZoneControllerPath + ". Did you move or delete it?");
            }
        }

        /// <summary>
        /// Looks up a prefab in the given paths and returns the found object or null
        /// </summary>
        /// <param name="prefabName">name of the prefab</param>
        /// <param name="foldersToSearch">path elements to look in</param>
        /// <returns>returns the found object or null</returns>
        public static GameObject FindPrefab(string prefabName, string[] foldersToSearch)
        {
            string[] guids = AssetDatabase.FindAssets("t:prefab", foldersToSearch);
            GameObject foundObject = null;
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (assetPath.Contains(prefabName))
                {
                    int index = assetPath.IndexOf(prefabName);
                    string matching = assetPath.Substring(assetPath.IndexOf(prefabName), assetPath.Length - index);
                    if (matching.Equals(prefabName))
                    {
                        foundObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    }
                }

            }

            return foundObject;
        }

        public static void AddHealthBar(string nononZoneControllerPath, string healthBarPrefabName)
        {
            GameObject selectedGameObject = Selection.activeObject as GameObject;
            if (selectedGameObject == null)
            {
                EditorUtility.DisplayDialog("Select the Gameobject", "Select the GameObject you want to place the Healthbar on.", "OK");
                return;
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            string[] foldersToSearch = { nononZoneControllerPath };
            GameObject foundObject = FindPrefab(healthBarPrefabName, foldersToSearch);
            if (foundObject != null)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                GameObject instanciatedPrefab = PrefabUtility.InstantiatePrefab(foundObject, selectedGameObject.transform) as GameObject;
            }
            else
            {
                Debug.LogError("Prefab " + healthBarPrefabName + " not found in " + nononZoneControllerPath + ". Did you move or delete it?");
            }
        }

        public static void AddWater(string nononZoneControllerPath, string waterPrefabName)
        {
            string[] foldersToSearch = { nononZoneControllerPath };
            GameObject foundObject = FindPrefab(waterPrefabName, foldersToSearch);
            if (foundObject != null)
            {
                InstantiateSingleObjectAndPosition(foundObject);
            }
            else
            {
                Debug.LogError("Prefab " + waterPrefabName + " not found in " + nononZoneControllerPath + ". Did you move or delete it?");
            }
        }

        /// <summary>
        /// Instantiates the Object and positions it at the current view vector
        /// </summary>
        /// <param name="foundObject">object to instantiate and position</param>
        private static void InstantiateSingleObjectAndPosition(GameObject foundObject)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            GameObject instanciatedPrefab = PrefabUtility.InstantiatePrefab(foundObject) as GameObject;
            instanciatedPrefab.transform.position = NononZoneEditorCommon.GetCurrentPositioningVector();
            Selection.activeGameObject = instanciatedPrefab;
        }

        public static void AddPortalPair(string nononZoneControllerPath, string portalPairPrefabName)
        {
            string[] foldersToSearch = { nononZoneControllerPath };
            GameObject foundObject = FindPrefab(portalPairPrefabName, foldersToSearch);
            if (foundObject != null)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                GameObject portal1 = PrefabUtility.InstantiatePrefab(foundObject) as GameObject;
                portal1.name = "Portal1";
                portal1.transform.position = NononZoneEditorCommon.GetCurrentPositioningVector();
                GameObject portal2 = PrefabUtility.InstantiatePrefab(foundObject) as GameObject;
                portal2.name = "Portal2";
                portal2.transform.position = NononZoneEditorCommon.GetCurrentPositioningVector() + new Vector3(0, 0, 15);

                PortalController portalController2 = portal2.GetComponent<PortalController>();
                PortalController portalController1 = portal1.GetComponent<PortalController>();
                portalController1.targetPortal = portalController2;
                portalController2.targetPortal = portalController1;
                Selection.activeGameObject = portal1;
            }
            else
            {
                Debug.LogError("Prefab " + portalPairPrefabName + " not found in " + nononZoneControllerPath + ". Did you move or delete it?");
            }
        }

        public static void AddZonePortal(string nononZoneControllerPath, string zonePortalPrefabName)
        {
            string[] foldersToSearch = { nononZoneControllerPath };
            GameObject foundObject = FindPrefab(zonePortalPrefabName, foldersToSearch);
            if (foundObject != null)
            {
                InstantiateSingleObjectAndPosition(foundObject);
            }
            else
            {
                Debug.LogError("Prefab " + zonePortalPrefabName + " not found in " + nononZoneControllerPath + ". Did you move or delete it?");
            }
        }

        public static void AddLootBubble(string nononZoneControllerPath, string lootBubblePrefabName)
        {
            string[] foldersToSearch = { nononZoneControllerPath };
            GameObject foundObject = FindPrefab(lootBubblePrefabName, foldersToSearch);
            if (foundObject != null)
            {
                InstantiateSingleObjectAndPosition(foundObject);
            }
            else
            {
                Debug.LogError("Prefab " + lootBubblePrefabName + " not found in " + nononZoneControllerPath + ". Did you move or delete it?");
            }
        }

        public static void AddSpawnPoint(string nononZoneControllerPath, string spawnPointPrefabName)
        {
            string[] foldersToSearch = { nononZoneControllerPath };
            GameObject foundObject = FindPrefab(spawnPointPrefabName, foldersToSearch);
            if (foundObject != null)
            {
                InstantiateSingleObjectAndPosition(foundObject);
            }
            else
            {
                Debug.LogError("Prefab " + spawnPointPrefabName + " not found in " + nononZoneControllerPath + ". Did you move or delete it?");
            }
        }

        public static void AddEnemy(string nononZoneControllerPath, string enemyPrefabName)
        {
            string[] foldersToSearch = { nononZoneControllerPath };
            GameObject foundObject = FindPrefab(enemyPrefabName, foldersToSearch);
            if (foundObject != null)
            {
                InstantiateSingleObjectAndPosition(foundObject);
            }
            else
            {
                Debug.LogError("Prefab " + enemyPrefabName + " not found in " + nononZoneControllerPath + ". Did you move or delete it?");
            }
        }

        public static void AddCustomerInfo(string nononZoneControllerPath, string customerInfoPrefabName)
        {
            string[] foldersToSearch = { nononZoneControllerPath };
            GameObject foundObject = FindPrefab(customerInfoPrefabName, foldersToSearch);
            if (foundObject != null)
            {
                InstantiateSingleObjectAndPosition(foundObject);
            }
            else
            {
                Debug.LogError("Prefab " + customerInfoPrefabName + " not found in " + nononZoneControllerPath + ". Did you move or delete it?");
            }
        }

        public static void AddLandingPlattform(string nononZoneControllerPath, string landingPlattformPrefabName)
        {
            string[] foldersToSearch = { nononZoneControllerPath };
            GameObject foundObject = FindPrefab(landingPlattformPrefabName, foldersToSearch);
            if (foundObject != null)
            {
                InstantiateSingleObjectAndPosition(foundObject);
            }
            else
            {
                Debug.LogError("Prefab " + landingPlattformPrefabName + " not found in " + nononZoneControllerPath + ". Did you move or delete it?");
            }
        }

        public static void AddSmallPlattform(string nononZoneControllerPath, string smallPlattformPrefabName)
        {
            string[] foldersToSearch = { nononZoneControllerPath };
            GameObject foundObject = FindPrefab(smallPlattformPrefabName, foldersToSearch);
            if (foundObject != null)
            {
                InstantiateSingleObjectAndPosition(foundObject);
            }
            else
            {
                Debug.LogError("Prefab " + smallPlattformPrefabName + " not found in " + nononZoneControllerPath + ". Did you move or delete it?");
            }
        }

        public static void AddBigPlattform(string nononZoneControllerPath, string bigPlattformPrefabName)
        {
            string[] foldersToSearch = { nononZoneControllerPath };
            GameObject foundObject = FindPrefab(bigPlattformPrefabName, foldersToSearch);
            if (foundObject != null)
            {
                InstantiateSingleObjectAndPosition(foundObject);
            }
            else
            {
                Debug.LogError("Prefab " + bigPlattformPrefabName + " not found in " + nononZoneControllerPath + ". Did you move or delete it?");
            }
        }

        public static bool CheckSelectionOfPlayerPrefab()
        {
            GameObject playerGameObject = Selection.activeObject as GameObject;
            if (playerGameObject == null || !NononZoneObject.isOneOfTypes(playerGameObject.transform, INononZoneObject.NononZoneObjType.PLAYER))
            {
                EditorUtility.DisplayDialog("Select Player Prefab", "Please make sure you have imported the \"Player\" Prefab into the Scene. Select \"Player\" Prefab node with Tag Player. Best in Prefab itself.", "OK");
                return false;
            }
            return true;
        }

        public static void AddPrefabDirectory(string nononZoneControllerPath, string prefabDirPrefabName)
        {

            string[] foldersToSearch = { nononZoneControllerPath };
            GameObject foundObject = FindPrefab(prefabDirPrefabName, foldersToSearch);
            if (foundObject != null)
            {
                InstantiateSingleObjectAndPosition(foundObject);
            }
            else
            {
                Debug.LogError("Prefab " + prefabDirPrefabName + " not found in " + nononZoneControllerPath + ". Did you move or delete it?");
            }
        }

        public static void AddTerrain(string nononZoneControllerPath, string terrainPrefabName)
        {
            string[] foldersToSearch = { nononZoneControllerPath };
            GameObject foundObject = FindPrefab(terrainPrefabName, foldersToSearch);
            if (foundObject != null)
            {
                InstantiateSingleObjectAndPosition(foundObject);
            }
            else
            {
                Debug.LogError("Prefab " + terrainPrefabName + " not found in " + nononZoneControllerPath + ". Did you move or delete it?");
            }
        }

    }
}