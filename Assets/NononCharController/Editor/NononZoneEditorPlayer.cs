using UnityEngine;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine.Animations.Rigging;
using System.Collections.Generic;

namespace zone.nonon
{
    /***
     * NoNoN Zone Component Helper Classes
     * 
     * */
    [ExecuteInEditMode]
    public class NononZoneEditorPlayer : MonoBehaviour
    {

        [MenuItem(NononZoneConstants.CommonEditor.STORE_BONES_MENU)]
        public static void StorePlayerBones()
        {
            if (!NononZoneEditorCommon.CheckSelectionOfPlayerPrefab()) return;
            GameObject playerGameObject = Selection.activeObject as GameObject;

            PlayerRiggingModel model = playerGameObject.GetComponent<PlayerRiggingModel>();
            List<PlayerRiggingModel.RigModelEntry> rigModelEntries = model.rigModelEntries;
            foreach (PlayerRiggingModel.RigModelEntry entry in rigModelEntries)
            {
                entry.rigModelName = entry.rigModelTransform.name;
            }

            EditorUtility.SetDirty(playerGameObject);
        }

        /***
         * Import a prefab or FBX to the Player
         * 
         * */
        [MenuItem(NononZoneConstants.CommonEditor.IMPORT_PLAYER_MENU)]
        public static void ImportPlayerPrefabFBX()
        {
            if (!NononZoneEditorCommon.CheckSelectionOfPlayerPrefab()) return;
            GameObject playerGameObject = Selection.activeObject as GameObject;

            string path = EditorUtility.OpenFilePanel("Import Prefab/FBX", Application.dataPath, "prefab,fbx");
            if (path.Length != 0)
            { 
                if (!path.Contains(Application.dataPath))
                {
                    bool isAssetPath = true;
                    string newAssetPath = Application.dataPath + "/" + NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH_PURE + path.Substring(path.LastIndexOf("/") + 1, path.Length - path.LastIndexOf("/") - 1);
                    if (!File.Exists(newAssetPath))
                    {
                        newAssetPath = NononZoneConstants.CommonEditor.NON_ZONE_PLAYER_PREFAB_PATH_COMPLETE + path.Substring(path.LastIndexOf("/") + 1, path.Length - path.LastIndexOf("/") - 1);
                        isAssetPath = false;
                    }
                    // Copy the Asset into the player asset path path
                    File.Copy(path, newAssetPath);
                    string newRelativeAssetPath = null;
                    if (isAssetPath)
                    {
                        newRelativeAssetPath = newAssetPath.Substring(Application.dataPath.Length + 1, newAssetPath.Length - Application.dataPath.Length - 1);
                        newRelativeAssetPath = NononZoneConstants.CommonEditor.NON_ZONE_CONTROLLER_PATH + newRelativeAssetPath;
                    }
                    else
                    {
                        newRelativeAssetPath = newAssetPath;
                    }
                    AssetDatabase.ImportAsset(newRelativeAssetPath);

                    ModelImporter importer = ModelImporter.GetAtPath(newRelativeAssetPath) as ModelImporter;
                    importer.animationType = ModelImporterAnimationType.Human;
                    ModelImporterClipAnimation[] clipAnimations = new ModelImporterClipAnimation[importer.defaultClipAnimations.Length];
                    for (int i = 0; i < importer.defaultClipAnimations.Length; i++)
                    {
                        clipAnimations[i] = new ModelImporterClipAnimation();
                        clipAnimations[i].cycleOffset = importer.defaultClipAnimations[i].cycleOffset;
                        clipAnimations[i].events = importer.defaultClipAnimations[i].events;
                        clipAnimations[i].heightFromFeet = importer.defaultClipAnimations[i].heightFromFeet;
                        clipAnimations[i].heightOffset = importer.defaultClipAnimations[i].heightOffset;
                        clipAnimations[i].keepOriginalOrientation = importer.defaultClipAnimations[i].keepOriginalOrientation;
                        clipAnimations[i].keepOriginalPositionXZ = importer.defaultClipAnimations[i].keepOriginalPositionXZ;
                        clipAnimations[i].keepOriginalPositionY = importer.defaultClipAnimations[i].keepOriginalPositionY;
                        clipAnimations[i].lockRootHeightY = importer.defaultClipAnimations[i].lockRootHeightY;
                        clipAnimations[i].lockRootPositionXZ = importer.defaultClipAnimations[i].lockRootPositionXZ;
                        clipAnimations[i].lockRootRotation = importer.defaultClipAnimations[i].lockRootRotation;
                        clipAnimations[i].loopPose = importer.defaultClipAnimations[i].loopPose;
                        clipAnimations[i].maskSource = importer.defaultClipAnimations[i].maskSource;
                        clipAnimations[i].maskType = importer.defaultClipAnimations[i].maskType;
                        clipAnimations[i].mirror = importer.defaultClipAnimations[i].mirror;
                        clipAnimations[i].rotationOffset = importer.defaultClipAnimations[i].rotationOffset;
                        clipAnimations[i].takeName = importer.defaultClipAnimations[i].takeName;

                        clipAnimations[i].curves = importer.defaultClipAnimations[i].curves;
                        clipAnimations[i].name = importer.defaultClipAnimations[i].name;
                        clipAnimations[i].firstFrame = importer.defaultClipAnimations[i].firstFrame;
                        clipAnimations[i].lastFrame = importer.defaultClipAnimations[i].lastFrame;
                        clipAnimations[i].loop = true;
                        clipAnimations[i].loopTime = true;
                        clipAnimations[i].wrapMode = WrapMode.Loop;
                    }
                    importer.clipAnimations = clipAnimations;
                    importer.motionNodeName = "Root Transform";
                    importer.SaveAndReimport();


                    path = newAssetPath;

                }
                else
                {
                    path = NononZoneConstants.CommonEditor.NON_ZONE_ASSET_PATH + path.Substring(Application.dataPath.Length + 1, path.Length - Application.dataPath.Length - 1);
                }

                // Look for the player Object in Scene
                // Add the fbx or prefab into the object and remove old one -- the one which is not MainCamera and Hitbox Collider
                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                GameObject instantiatedAsset = PrefabUtility.InstantiatePrefab(asset) as GameObject;
                ModelImporter importer2 = ModelImporter.GetAtPath(path) as ModelImporter;
                if (importer2 != null)
                {
                    importer2.motionNodeName = "<Root Transform>";
                    importer2.SaveAndReimport();
                }

                if (PrefabUtility.IsPartOfModelPrefab(instantiatedAsset))
                {
                    string newPrefabPath = path.Substring(0, path.LastIndexOf(".")) + ".prefab";
                    // if the path is somewhere else than the player prefab, set the newPrefabPath still to the player prefab
                    if (!path.Contains(NononZoneConstants.CommonEditor.NON_ZONE_PLAYER_PREFAB_PATH_COMPLETE))
                    {
                        string filename = newPrefabPath.Substring(newPrefabPath.LastIndexOf("/") + 1, newPrefabPath.Length - newPrefabPath.LastIndexOf("/") - 1);
                        newPrefabPath = NononZoneConstants.CommonEditor.NON_ZONE_PLAYER_PREFAB_PATH_COMPLETE + filename;
                    }
                    asset = PrefabUtility.SaveAsPrefabAsset(instantiatedAsset, newPrefabPath);
                    DestroyImmediate(instantiatedAsset);
                    instantiatedAsset = PrefabUtility.InstantiatePrefab(asset) as GameObject;
                }

                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                instantiatedAsset.transform.SetParent(playerGameObject.transform);
                instantiatedAsset.transform.position = Vector3.zero;

                Avatar avatar = AssetDatabase.LoadAssetAtPath<Avatar>(path);
                if (avatar == null)
                {
                    Animator anim = asset.GetComponent<Animator>();
                    if (anim != null)
                    {
                        avatar = anim.avatar;
                    }
                }
                playerGameObject.GetComponent<Animator>().avatar = avatar;
                if (CheckBoneNames(playerGameObject.transform))
                {
                    AddWeaponRigs(instantiatedAsset.transform);
                }

                PrefabUtility.ApplyPrefabInstance(instantiatedAsset, InteractionMode.UserAction);
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            }
        }

        [MenuItem(NononZoneConstants.CommonEditor.ADD_PLAYER_WEAPON_RIGS_MENU)]
        public static void AddWeaponRigsMenu()
        {
            if (!NononZoneEditorCommon.CheckSelectionOfPlayerPrefab()) return;

            GameObject playerGameObject = Selection.activeObject as GameObject;
            if (!CheckBoneNames(playerGameObject.transform))
            {
                EditorUtility.DisplayDialog("Bone names are not correct set", "Please make sure all bone names are set on the PlayerPrefab in the Player Rigging Model. Nothing has been done to the model.", "OK");
                return;
            }

            PlayerRiggingModel modelNames = playerGameObject.GetComponent<PlayerRiggingModel>();
            Transform prefabWithArmature = FindModelPartsByName(modelNames.GetNameFromEntries(PlayerRiggingModel.RigModelPart.Armature), playerGameObject.transform);
            if (prefabWithArmature.parent.GetComponent<PlayerRigging>() == null)
            {
                EditorUtility.DisplayDialog("Armature is not below PlayerPrefab", "Set the armature element on the PlayerPrefab in the Player Rigging Model to the element of the model right below the PlayerPrefab. Nothing has been done to the model.", "OK");
                return;
            }
            {

            }
            AddWeaponRigs(prefabWithArmature);
            EditorUtility.SetDirty(playerGameObject);
        }

        public static bool CheckBoneNames(Transform playerObject)
        {
            bool foundAllBones = true;
            PlayerRiggingModel modelNames = playerObject.GetComponent<PlayerRiggingModel>();
            Transform foundObj = FindModelPartsByName(modelNames.GetNameFromEntries(PlayerRiggingModel.RigModelPart.Armature), playerObject);
            if (foundObj == null)
            {
                foundAllBones = false;
                Debug.LogError("No bone named \"" + modelNames.GetNameFromEntries(PlayerRiggingModel.RigModelPart.Armature) + "\" found.");
            }
            foundObj = FindModelPartsByName(modelNames.GetNameFromEntries(PlayerRiggingModel.RigModelPart.Spine), playerObject);
            if (foundObj == null)
            {
                foundAllBones = false;
                Debug.LogError("No bone named \"" + modelNames.GetNameFromEntries(PlayerRiggingModel.RigModelPart.Spine) + "\" found.");
            }
            foundObj = FindModelPartsByName(modelNames.GetNameFromEntries(PlayerRiggingModel.RigModelPart.Head), playerObject);
            if (foundObj == null)
            {
                foundAllBones = false;
                Debug.LogError("No bone named \"" + modelNames.GetNameFromEntries(PlayerRiggingModel.RigModelPart.Head) + "\" found.");
            }
            foundObj = FindModelPartsByName(modelNames.GetNameFromEntries(PlayerRiggingModel.RigModelPart.UpperArmRight), playerObject);
            if (foundObj == null)
            {
                foundAllBones = false;
                Debug.LogError("No bone named \"" + modelNames.GetNameFromEntries(PlayerRiggingModel.RigModelPart.UpperArmRight) + "\" found.");
            }
            foundObj = FindModelPartsByName(modelNames.GetNameFromEntries(PlayerRiggingModel.RigModelPart.HandRight), playerObject);
            if (foundObj == null)
            {
                foundAllBones = false;
                Debug.LogError("No bone named \"" + modelNames.GetNameFromEntries(PlayerRiggingModel.RigModelPart.HandRight) + "\" found.");
            }
            foundObj = FindModelPartsByName(modelNames.GetNameFromEntries(PlayerRiggingModel.RigModelPart.ElbowRight), playerObject);
            if (foundObj == null)
            {
                foundAllBones = false;
                Debug.LogError("No bone named \"" + modelNames.GetNameFromEntries(PlayerRiggingModel.RigModelPart.ElbowRight) + "\" found.");
            }
            foundObj = FindModelPartsByName(modelNames.GetNameFromEntries(PlayerRiggingModel.RigModelPart.ShoulderRight), playerObject);
            if (foundObj == null)
            {
                foundAllBones = false;
                Debug.LogError("No bone named \"" + modelNames.GetNameFromEntries(PlayerRiggingModel.RigModelPart.ShoulderRight) + "\" found.");
            }
            foundObj = FindModelPartsByName(modelNames.GetNameFromEntries(PlayerRiggingModel.RigModelPart.HandLeft), playerObject);
            if (foundObj == null)
            {
                foundAllBones = false;
                Debug.LogError("No bone named \"" + modelNames.GetNameFromEntries(PlayerRiggingModel.RigModelPart.HandLeft) + "\" found.");
            }
            foundObj = FindModelPartsByName(modelNames.GetNameFromEntries(PlayerRiggingModel.RigModelPart.ElbowLeft), playerObject);
            if (foundObj == null)
            {
                foundAllBones = false;
                Debug.LogError("No bone named \"" + modelNames.GetNameFromEntries(PlayerRiggingModel.RigModelPart.ElbowLeft) + "\" found.");
            }
            foundObj = FindModelPartsByName(modelNames.GetNameFromEntries(PlayerRiggingModel.RigModelPart.ShoulderLeft), playerObject);
            if (foundObj == null)
            {
                foundAllBones = false;
                Debug.LogError("No bone named \"" + modelNames.GetNameFromEntries(PlayerRiggingModel.RigModelPart.ShoulderLeft) + "\" found.");
            }

            if (!foundAllBones)
            {
                Debug.LogError("Not all Bone names could not be found. Select Player and in \"Player Rigging Model\" Component register the corresponding names of the bone transforms.");
            }

            return foundAllBones;

        }

        public static void AddWeaponRigs(Transform prefabInstance)
        {
            AddAimTarget(prefabInstance);
            AddWeaponDrawHolder(prefabInstance);
            AddWeaponPoseRig(prefabInstance);
            AddBodyPoseAimRig(prefabInstance);
            AddWeaponAimRig(prefabInstance);
            AddPlayerCharRig(prefabInstance);

            AddRigLayers(prefabInstance);
        }

        static void AddRigLayers(Transform prefabInstance)
        {
            // Clear all rig layers
            prefabInstance.parent.GetComponent<RigBuilder>().layers.Clear();

            PlayerRigging player = prefabInstance.parent.GetComponent<PlayerRigging>();

            // Add Player Char Rig
            Rig characterRig = player.rightHandIKHint.parent.parent.GetComponent<Rig>();
            RigLayer characterRigLayer = new RigLayer(characterRig);
            prefabInstance.parent.GetComponent<RigBuilder>().layers.Insert(0, characterRigLayer);

            // Add weaponAimRig
            Rig weaponAimRig = player.weaponAimingPoseInRig.parent.GetComponent<Rig>();
            RigLayer weaponAimRigLayer = new RigLayer(weaponAimRig);
            prefabInstance.parent.GetComponent<RigBuilder>().layers.Insert(0, weaponAimRigLayer);

            // Add BodyPoseAimRig
            Rig bodyPoseAimRig = player.bodyAimRig.GetComponent<Rig>();
            RigLayer bodyPoseAimRigLayer = new RigLayer(bodyPoseAimRig);
            prefabInstance.parent.GetComponent<RigBuilder>().layers.Insert(0, bodyPoseAimRigLayer);

            // Add WeaponPoseRig            
            Rig weaponPoseRig = player.weaponPoseInRig.parent.GetComponent<Rig>();
            RigLayer weaponPoseRigLayer = new RigLayer(weaponPoseRig);
            prefabInstance.parent.GetComponent<RigBuilder>().layers.Insert(0, weaponPoseRigLayer);

        }



        static void AddAimTarget(Transform prefabInstance)
        {
            PlayerRigging player = prefabInstance.parent.GetComponent<PlayerRigging>();

            // Add the AIM_TARGET_RIG
            GameObject aimTargetHolder = new GameObject(NononZoneConstants.WeaponPlaceHolder.AIM_TARGET_HOLDER_LABEL);
            aimTargetHolder.transform.parent = prefabInstance;
            aimTargetHolder.AddComponent<RigTransform>();
            GameObject aimTarget = new GameObject(NononZoneConstants.WeaponPlaceHolder.AIM_TARGET_LABEL);
            aimTarget.transform.parent = aimTargetHolder.transform;
            player.CameraAimTarget = aimTarget.transform;
        }

        static void AddWeaponDrawHolder(Transform prefabInstance)
        {
            PlayerRigging player = prefabInstance.parent.GetComponent<PlayerRigging>();

            // Add WeaponDrawHolder
            GameObject wpnDrawHolder = new GameObject(NononZoneConstants.WeaponPlaceHolder.WPN_DRAW_HOLDER_LABEL);
            wpnDrawHolder.transform.parent = prefabInstance;
            wpnDrawHolder.AddComponent<RigTransform>();
            player.weaponDrawHolder = wpnDrawHolder.transform;
        }

        static void AddBodyPoseAimRig(Transform prefabInstance)
        {
            PlayerRigging player = prefabInstance.parent.GetComponent<PlayerRigging>();
            PlayerRiggingModel modelNames = player.GetComponent<PlayerRiggingModel>();

            GameObject bodyPoseAimRig = new GameObject(NononZoneConstants.WeaponPlaceHolder.BODY_POSE_AIM_LABEL);
            bodyPoseAimRig.AddComponent<Rig>();
            bodyPoseAimRig.GetComponent<Rig>().weight = 0;
            bodyPoseAimRig.transform.parent = prefabInstance;
            player.bodyAimRig = bodyPoseAimRig.transform;

            // Add the spine
            GameObject spine = new GameObject(NononZoneConstants.WeaponPlaceHolder.BODY_POSE_AIM_SPINE_LABEL);
            spine.transform.parent = bodyPoseAimRig.transform;
            spine.AddComponent<MultiAimConstraint>();
            MultiAimConstraint maConstraint = spine.GetComponent<MultiAimConstraint>();
            maConstraint.weight = 0.2f;
            maConstraint.data.aimAxis = MultiAimConstraintData.Axis.Z;
            maConstraint.data.upAxis = MultiAimConstraintData.Axis.Y;
            WeightedTransformArray sources = new WeightedTransformArray();
            sources.Add(new WeightedTransform(player.CameraAimTarget, 1f));
            maConstraint.data.sourceObjects = sources;

            Transform foundSpine = FindModelPartsByName(modelNames.GetNameFromEntries(PlayerRiggingModel.RigModelPart.Spine), prefabInstance);
            maConstraint.data.constrainedObject = foundSpine.transform;

            // Add the head
            GameObject head = new GameObject(NononZoneConstants.WeaponPlaceHolder.BODY_POSE_AIM_HEAD_LABEL);
            head.transform.parent = bodyPoseAimRig.transform;
            head.AddComponent<MultiAimConstraint>();
            maConstraint = head.GetComponent<MultiAimConstraint>();
            maConstraint.weight = 0.8f;
            maConstraint.data.aimAxis = MultiAimConstraintData.Axis.Z;
            maConstraint.data.upAxis = MultiAimConstraintData.Axis.Y;
            sources = new WeightedTransformArray();
            sources.Add(new WeightedTransform(player.CameraAimTarget, 1f));
            maConstraint.data.sourceObjects = sources;

            Transform foundHead = FindModelPartsByName(modelNames.GetNameFromEntries(PlayerRiggingModel.RigModelPart.Head), prefabInstance);
            maConstraint.data.constrainedObject = foundHead.transform;

        }

        static void AddWeaponAimRig(Transform prefabInstance)
        {
            PlayerRigging player = prefabInstance.parent.GetComponent<PlayerRigging>();
            PlayerRiggingModel modelNames = player.GetComponent<PlayerRiggingModel>();

            GameObject weaponAimRig = new GameObject(NononZoneConstants.WeaponPlaceHolder.WEAPON_AIM_RIG_LABEL);
            weaponAimRig.AddComponent<Rig>();
            weaponAimRig.GetComponent<Rig>().weight = 0;
            weaponAimRig.transform.parent = prefabInstance;


            GameObject weaponAim = new GameObject(NononZoneConstants.WeaponPlaceHolder.WEAPON_AIM_POSE_LABEL);
            weaponAim.transform.parent = weaponAimRig.transform;
            weaponAim.transform.localPosition = new Vector3(0.05656991f, 1.356307f, 0.07923327f);
            weaponAim.transform.localRotation = Quaternion.Euler(-97.837f, 176.443f, -181.118f);
            player.weaponAimingPoseInRig = weaponAim.transform;

            weaponAim.AddComponent<MultiPositionConstraint>();
            MultiPositionConstraint mpConstraint = weaponAim.GetComponent<MultiPositionConstraint>();
            mpConstraint.data.constrainedObject = weaponAim.transform;
            mpConstraint.data.offset = new Vector3(-0.06f, 0.08f, 0.11f);

            Transform foundRightArm = FindModelPartsByName(modelNames.GetNameFromEntries(PlayerRiggingModel.RigModelPart.UpperArmRight), prefabInstance);
            WeightedTransformArray wta = new WeightedTransformArray();
            wta.Add(new WeightedTransform(foundRightArm, 1f));
            mpConstraint.data.sourceObjects = wta;

            weaponAim.AddComponent<MultiAimConstraint>();
            MultiAimConstraint maConstraint = weaponAim.GetComponent<MultiAimConstraint>();
            maConstraint.weight = 1.0f;
            maConstraint.data.constrainedObject = weaponAim.transform;
            maConstraint.data.aimAxis = MultiAimConstraintData.Axis.Z;
            maConstraint.data.upAxis = MultiAimConstraintData.Axis.Y;
            WeightedTransformArray sources = new WeightedTransformArray();
            sources.Add(new WeightedTransform(player.CameraAimTarget, 1f));
            maConstraint.data.sourceObjects = sources;

            weaponAim.AddComponent<MultiParentConstraint>();
            MultiParentConstraint mpc = weaponAim.GetComponent<MultiParentConstraint>();
            mpc.data.constrainedObject = player.weaponDrawHolder;

            sources = new WeightedTransformArray();
            sources.Add(new WeightedTransform(weaponAim.transform, 1f));
            mpc.data.sourceObjects = sources;

        }


        static void AddWeaponPoseRig(Transform prefabInstance)
        {
            PlayerRigging player = prefabInstance.parent.GetComponent<PlayerRigging>();
            PlayerRiggingModel modelNames = player.GetComponent<PlayerRiggingModel>();

            GameObject weaponPoseRig = new GameObject(NononZoneConstants.WeaponPlaceHolder.WEAPON_POSE_RIG_LABEL);
            weaponPoseRig.AddComponent<Rig>();
            weaponPoseRig.transform.parent = prefabInstance;
            GameObject weaponPose = new GameObject(NononZoneConstants.WeaponPlaceHolder.WEAPON_POSE_LABEL);
            weaponPose.AddComponent<MultiPositionConstraint>();
            MultiPositionConstraint mposc = weaponPose.GetComponent<MultiPositionConstraint>();
            mposc.data.constrainedObject = weaponPose.transform;
            mposc.data.offset = new Vector3(-0.07f, 0.32f, 0.15f);
            WeightedTransformArray sources = new WeightedTransformArray();
            Transform foundRightArm = FindModelPartsByName(modelNames.GetNameFromEntries(PlayerRiggingModel.RigModelPart.UpperArmRight), prefabInstance);
            sources.Add(new WeightedTransform(foundRightArm.transform, 1f));
            mposc.data.sourceObjects = sources;

            weaponPose.AddComponent<MultiParentConstraint>();
            weaponPose.transform.parent = weaponPoseRig.transform;
            weaponPose.transform.localPosition = new Vector3(0.137f, 1.495f, 0.12f);
            weaponPose.transform.localRotation = Quaternion.Euler(42.399f, -70.441f, 29.956f);
            player.weaponPoseInRig = weaponPose.transform;

            MultiParentConstraint mpc = weaponPose.GetComponent<MultiParentConstraint>();
            mpc.data.constrainedObject = player.weaponDrawHolder;

            sources = new WeightedTransformArray();
            sources.Add(new WeightedTransform(weaponPose.transform, 1f));
            mpc.data.sourceObjects = sources;

            Transform foundSpine = FindModelPartsByName(modelNames.GetNameFromEntries(PlayerRiggingModel.RigModelPart.Spine), prefabInstance);
            player.weaponDrawHolder.position = foundSpine.position;

            // Adding the weapon holders
            GameObject weaponHolder2HRight = new GameObject(NononZoneConstants.WeaponPlaceHolder.WPN_HOLER_2HRIGHT_LABEL);
            weaponHolder2HRight.transform.parent = foundSpine;
            weaponHolder2HRight.transform.localPosition = new Vector3(0.011f, 0.313f, -0.105f);
            weaponHolder2HRight.transform.localRotation = Quaternion.Euler(35.141f, -97.071f, 3.645f);
            player.weaponHolder2HRight = weaponHolder2HRight.transform;


            GameObject weaponHolder2HLeft = new GameObject(NononZoneConstants.WeaponPlaceHolder.WPN_HOLER_2HLEFT_LABEL);
            weaponHolder2HLeft.transform.parent = foundSpine;
            weaponHolder2HLeft.transform.localPosition = new Vector3(-0.093f, 0.302f, -0.122f);
            weaponHolder2HLeft.transform.localRotation = Quaternion.Euler(35.125f, 96.968f, 9.371f);
            player.weaponHolder2HLeft = weaponHolder2HLeft.transform;


            GameObject weaponHolder1HRight = new GameObject(NononZoneConstants.WeaponPlaceHolder.WPN_HOLER_1HRIGHT_LABEL);
            weaponHolder1HRight.transform.parent = foundSpine;
            weaponHolder1HRight.transform.localPosition = new Vector3(0.166f, 0f, 0f);
            weaponHolder1HRight.transform.localRotation = Quaternion.Euler(30, 0, 0f);
            player.weaponHolder1HRight = weaponHolder1HRight.transform;

            GameObject weaponHolder1HLeft = new GameObject(NononZoneConstants.WeaponPlaceHolder.WPN_HOLER_1HLEFT_LABEL);
            weaponHolder1HLeft.transform.parent = foundSpine;
            weaponHolder1HLeft.transform.localPosition = new Vector3(-0.189f, -0.014f, 0f);
            weaponHolder1HLeft.transform.localRotation = Quaternion.Euler(30, 0, 0f);
            player.weaponHolder1HLeft = weaponHolder1HLeft.transform;

        }

        static void AddPlayerCharRig(Transform prefabInstance)
        {
            PlayerRigging player = prefabInstance.parent.GetComponent<PlayerRigging>();
            PlayerRiggingModel modelNames = player.GetComponent<PlayerRiggingModel>();

            GameObject characterRig = new GameObject(NononZoneConstants.WeaponPlaceHolder.PLAYER_CHAR_RIG_LABEL);
            characterRig.AddComponent<Rig>();
            characterRig.transform.parent = prefabInstance;

            Transform foundHand = FindModelPartsByName(modelNames.GetNameFromEntries(PlayerRiggingModel.RigModelPart.HandRight), prefabInstance);

            // Add a weapon to the hand
            GameObject weaponPosRH = new GameObject(NononZoneConstants.WeaponPlaceHolder.RH_POS_LABEL);
            weaponPosRH.transform.parent = foundHand;
            weaponPosRH.transform.localPosition = Vector3.zero;
            weaponPosRH.transform.localRotation = Quaternion.Euler(180, 180, 0);
            player.rightHandPos = weaponPosRH.transform;

            // Adding the two bone construct                
            GameObject rightArmIK = new GameObject(NononZoneConstants.WeaponPlaceHolder.RIGHT_ARM_IK_LABEL);
            rightArmIK.transform.parent = characterRig.transform;
            rightArmIK.AddComponent<TwoBoneIKConstraint>();
            GameObject rhIKTarget = new GameObject(NononZoneConstants.WeaponPlaceHolder.RH_IK_TARGET_LABEL);
            rhIKTarget.transform.parent = rightArmIK.transform;
            rhIKTarget.transform.position = foundHand.position;
            rhIKTarget.transform.rotation = foundHand.rotation;
            player.rightHandIKTarget = rhIKTarget.transform;
            GameObject rhIKHint = new GameObject(NononZoneConstants.WeaponPlaceHolder.RH_IK_HINT_LABEL);
            rhIKHint.transform.parent = rightArmIK.transform;
            player.rightHandIKHint = rhIKHint.transform;

            Transform elbow = FindModelPartsByName(modelNames.GetNameFromEntries(PlayerRiggingModel.RigModelPart.ElbowRight), prefabInstance);
            Transform shoulder = FindModelPartsByName(modelNames.GetNameFromEntries(PlayerRiggingModel.RigModelPart.ShoulderRight), prefabInstance);

            rhIKHint.transform.position = elbow.transform.position;
            rhIKHint.transform.rotation = elbow.transform.rotation;

            // setting the parts on the Two Bone IK
            TwoBoneIKConstraint rArmIKConstraint = rightArmIK.GetComponent<TwoBoneIKConstraint>();
            rArmIKConstraint.data.root = shoulder;
            rArmIKConstraint.data.mid = elbow;
            rArmIKConstraint.data.tip = foundHand;
            rArmIKConstraint.data.target = rhIKTarget.transform;
            rArmIKConstraint.data.hint = rhIKHint.transform;


            foundHand = FindModelPartsByName(modelNames.GetNameFromEntries(PlayerRiggingModel.RigModelPart.HandLeft), prefabInstance);

            // Add a weapon to the hand
            GameObject weaponPosLH = new GameObject(NononZoneConstants.WeaponPlaceHolder.LH_POS_LABEL);
            weaponPosLH.transform.parent = foundHand;
            weaponPosLH.transform.localPosition = Vector3.zero;
            weaponPosLH.transform.localRotation = Quaternion.Euler(180, 180, 0);
            player.leftHandPos = weaponPosLH.transform;

            // Adding the two bone construct
            GameObject leftArmIK = new GameObject(NononZoneConstants.WeaponPlaceHolder.LEFT_ARM_IK_LABEL);
            leftArmIK.transform.parent = characterRig.transform;
            leftArmIK.AddComponent<TwoBoneIKConstraint>();
            GameObject lhIKTarget = new GameObject(NononZoneConstants.WeaponPlaceHolder.LH_IK_TARGET_LABEL);
            lhIKTarget.transform.parent = leftArmIK.transform;
            lhIKTarget.transform.position = foundHand.position;
            lhIKTarget.transform.rotation = foundHand.rotation;
            player.leftHandIKTarget = lhIKTarget.transform;
            GameObject lhIKHint = new GameObject(NononZoneConstants.WeaponPlaceHolder.LH_IK_HINT_LABEL);
            lhIKHint.transform.parent = leftArmIK.transform;
            player.leftHandIKHint = lhIKHint.transform;

            elbow = FindModelPartsByName(modelNames.GetNameFromEntries(PlayerRiggingModel.RigModelPart.ElbowLeft), prefabInstance);
            shoulder = FindModelPartsByName(modelNames.GetNameFromEntries(PlayerRiggingModel.RigModelPart.ShoulderLeft), prefabInstance);

            lhIKHint.transform.position = elbow.transform.position;
            lhIKHint.transform.rotation = elbow.transform.rotation;

            // setting the parts on the Two Bone IK
            TwoBoneIKConstraint lArmIKConstraint = leftArmIK.GetComponent<TwoBoneIKConstraint>();
            lArmIKConstraint.data.root = shoulder;
            lArmIKConstraint.data.mid = elbow;
            lArmIKConstraint.data.tip = foundHand;
            lArmIKConstraint.data.target = lhIKTarget.transform;
            lArmIKConstraint.data.hint = lhIKHint.transform;

        }

        static Transform FindModelPartsByName(string partName, Transform rootNode)
        {
            if (rootNode.name.Equals(partName))
            {
                return rootNode;
            }
            for (int i = 0; i < rootNode.childCount; i++)
            {
                Transform child = rootNode.GetChild(i);
                if (child.name.Equals(partName))
                {
                    return child;
                }
                else
                {
                    Transform foundSubChild = FindModelPartsByName(partName, child);
                    if (foundSubChild != null)
                    {
                        return foundSubChild;
                    }
                }
            }
            return null;
        }

        static Transform FindSimilarArmaturePart(string partName, Transform rootNode)
        {

            int distance = ComputeLevenshteinDistance(partName, rootNode.name);
            //Debug.Log("Comparing " + partName + " with " + rootNode.name + " Distance=" + distance);
            if (ComputeLevenshteinDistance(partName, rootNode.name) <= 5)
            {
                return rootNode;
            }
            for (int i = 0; i < rootNode.childCount; i++)
            {
                Transform child = rootNode.GetChild(i);
                distance = ComputeLevenshteinDistance(partName, child.name);
                //Debug.Log("Comparing " + partName + " with " + rootNode.name + " Distance=" + distance);
                if (ComputeLevenshteinDistance(partName, child.name) <= 5)
                {
                    return child;
                }
                else
                {
                    Transform foundSubChild = FindSimilarArmaturePart(partName, child);
                    if (foundSubChild != null)
                    {
                        return foundSubChild;
                    }
                }
            }
            return null;

        }

        /***
         * Helper Method to find the similarity in strings
         * 
         * */
        static int ComputeLevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = System.Math.Min(
                        System.Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }
    }
}