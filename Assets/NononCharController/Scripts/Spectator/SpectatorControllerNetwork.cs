using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using System;
using UnityEngine.UI;
using TMPro;

namespace zone.nonon
{
    /// <summary>
    /// Controller Component of the SpectatorView
    /// </summary>
    public class SpectatorControllerNetwork : NetworkBehaviour
    {
        /// <summary>
        /// A filter object, which is used to switch between
        /// </summary>
        [Serializable]
        public class ObjectTypeFilter
        {
            public INononZoneObject.NononZoneObjType objType;
            public bool selected;
        }
        /// <summary>
        /// Filter list which will be applied to the list of objects in the filteredObjects List
        /// </summary>
        public List<ObjectTypeFilter> objectTypeFilterList = new List<ObjectTypeFilter>();

        /// <summary>
        /// An Object holder for the objects filterd by the list and search string
        /// </summary>
        public class SpectatorsObject
        {
            public string objectName;
            public Transform objectTransform;
            public ulong networkObjectId;
            public bool favorite = false;
            public bool autoSelect = true;
            public bool isHome = false;
            public bool isOrbitViewDefault = true;
        }

        [HideInInspector]
        public bool spectatorListIsDirty = false;

        /// <summary>
        /// All Objects that are found applied by the objectTypeFilterList
        /// </summary>
        List<SpectatorsObject> spectatorObjectsList = new List<SpectatorsObject>();
        HashSet<ulong> favSpectatorList = new HashSet<ulong>();
        HashSet<ulong> nonAutoViewSpectatorList = new HashSet<ulong>();
        HashSet<ulong> fpvDefaultSpectatorList = new HashSet<ulong>();
        ulong homeNetworkID = 0;

        /// <summary>
        /// Currently selected object
        /// </summary>
        int selectedFilterObject = 0;
        /// <summary>
        /// NetworkObject currently selected
        /// </summary>
        NononZoneObjectNetwork targetObject;
        /// <summary>
        /// Tracker for the objects spawned - will be needed to refresh the list
        /// </summary>
        int amountOfCurrentObjectsSpawned = 0;

        float timerBeginResetThirdPerson;
        float timerBeginAutoSwitch;
        float randomTimeAutoSwitch;
        float idleTimerStart;
        bool hasAggro = false;
        bool autoSwitch = true;

        // strings to display camera state
        string EMPTY_STRING = "";
        string ATTACH_TEXT_STRING = "ATTACH TO OBJECT (B)";
        string ORBIT_CAM_TEXT_STRING = "ORBIT CAM";
        string OBJECT_CAM_TEXT_STRING = "OBJECT FPV CAM";
        string CHASING_CAM_TEXT_STRING = "CHASING CAM";
        string FREEMOVE_CAM_TEXT_STRING = "FREEMOVE CAM";

        [Header("Auto Settings")]
        [Tooltip("Time (s) until chase cam switches back to orbit cam")]
        public float time4ResetThirdPerson = 5;
        [Tooltip("Switch auto reset to orbit cam from chase cam")]
        public bool autoReset3rdPerson = true;
        [Tooltip("Time (s) until cam switches to next object. It is a random number between min and max")]
        public float time4AutoSwitchMin = 4;
        [Tooltip("Time (s) until cam switches to next object. It is a random number between min and max")]
        public float time4AutoSwitchMax = 8;
        [Tooltip("Time when spectator is idle and in freelook cam to switch back to auto switch mode")]
        public float idleTime4AutoSwitchBack = 10f;

        [Header("Ghost Mode")]
        public bool ghostMode = false;
        [Header("Fixed Height")]
        [Tooltip("If true, the spectator moves to a fixed height and stay there.")]
        public bool fixedHeight = false;
        [Tooltip("Fixed height the spectator moves to when in fixed height mode.")]
        public float distanceFixedHeight = 1.7f;
        [Tooltip("Fixed height raycast distance for layer Terrain in downward direction")]
        public float rayCastDistanceFixedHeight = 20f;
        [Tooltip("Time multiplier for moving to the fixed height")]
        public float moveToFixedHeightMultiplier = 20f;

        [Header("Object Cam")]
        [Tooltip("Speed the camera rotates with the target")]
        public float fpvRotationSpeedMultiplier = 10.0f;
        [Tooltip("Position Offset of FPS View")]
        public float fpvPositionOffsetMultiplier = 1.1f;
        [Tooltip("Percentage of the offset from top of collider: 0.25 = 25%")]
        public float ppositionOffsetPercentage = 0.15f;

        bool fpvObjectCamActive = false;
        bool isCameraMovePressed = false;

        [Header("Camera and UI Objects")]
        public SpectatorCameraCMNetwork spectatorCamera;
        public Transform navigationUI;
        public Image crosshair;
        public TextMeshProUGUI attachText;
        public TextMeshProUGUI camText;
        public TextMeshProUGUI targetText;
        public Toggle autoSwitchToggle;
        public bool isUIInitialVisible = true;

        // Input System
        PlayerInput playerInput;

        // movement input
        Vector2 currentMovementInput;
        Vector3 currentMovement;
        Vector3 bounceOffDirection;

        // transform that is in aiming cross at the moment
        Transform possibleAttachTransform = null;


        private void Awake()
        {
            playerInput = new PlayerInput();
            playerInput.CharacterControls.Move.started += OnMovementInput;
            playerInput.CharacterControls.Move.canceled += OnMovementInput;
            playerInput.CharacterControls.Move.performed += OnMovementInput;
            playerInput.CharacterControls.AttachToObject.started += OnAttachInput;
            playerInput.CharacterControls.AttachToNextObject.started += OnAttachToNextObjectInput;
            playerInput.CharacterControls.AttachToPrevObject.started += OnAttachToPrevObjectInput;
            playerInput.CharacterControls.AutoSwitch.started += OnAutoSwitchInput;
            playerInput.CharacterControls.ToggleUI.started += OnToggleUIInput;
            playerInput.CharacterControls.GhostMode.started += OnGhostModeInput;
            playerInput.CharacterControls.FixedHeight.started += OnFixedHeightInput;
            playerInput.CharacterControls.CameraSwitch.started += OnCameraSwitchInput;
            playerInput.CharacterControls.EnableMoveCamera.started += OnEnableMoveCameraInput;
            playerInput.CharacterControls.EnableMoveCamera.canceled += OnEnableMoveCameraInput;

            // just for idle management
            playerInput.CharacterControls.MoveCameraMouse.started += OnMouseOrStickMoved;
            playerInput.CharacterControls.MoveCameraMouse.performed += OnMouseOrStickMoved;
            playerInput.CharacterControls.MoveCameraStick.started += OnMouseOrStickMoved;
            playerInput.CharacterControls.MoveCameraStick.performed += OnMouseOrStickMoved;

            playerInput.CharacterControls.Enable();
            attachText.text = "";

            if (isUIInitialVisible) navigationUI.gameObject.SetActive(true);
            else navigationUI.gameObject.SetActive(false);

            // initially set the values
            SetFixedHeight(fixedHeight);
            if (fixedHeight)
            {
                ghostMode = true;

            }
            else
            {
                SetGhostMode(ghostMode);
            }


        }

        void OnMouseOrStickMoved(InputAction.CallbackContext context)
        {
            idleTimerStart = Time.time;
        }

        void OnEnableMoveCameraInput(InputAction.CallbackContext context)
        {
            idleTimerStart = Time.time;
            isCameraMovePressed = context.ReadValueAsButton();
        }

        public void AddRemoveAsFavSpectator(SpectatorsObject obj, bool add)
        {
            if (add) favSpectatorList.Add(obj.networkObjectId);
            else favSpectatorList.Remove(obj.networkObjectId);
            RefreshFilteredObjects();
        }

        public void SetOrbDefault(SpectatorsObject obj, bool orbDefault)
        {
            foreach (SpectatorsObject o in spectatorObjectsList)
            {
                if (o.networkObjectId.Equals(obj.networkObjectId))
                {
                    o.isOrbitViewDefault = orbDefault;
                    fpvDefaultSpectatorList.Add(o.networkObjectId);
                }
            }
        }

        public void AddRemoveAsNonAutoViewSpectator(SpectatorsObject obj, bool add)
        {
            if (add) nonAutoViewSpectatorList.Add(obj.networkObjectId);
            else nonAutoViewSpectatorList.Remove(obj.networkObjectId);
            RefreshFilteredObjects();
        }

        public void SetHomeNetworkID(ulong _homeNetworkID)
        {
            homeNetworkID = _homeNetworkID;
            RefreshFilteredObjects();
        }

        public List<SpectatorsObject> GetSpectatorObjectList()
        {
            return spectatorObjectsList;
        }

        public bool IsGhostMode()
        {
            return ghostMode;
        }

        public bool IsFixedHeight()
        {
            return fixedHeight;
        }

        public void SetGhostMode(bool _ghostMode)
        {
            ghostMode = _ghostMode;
        }

        public void SetFixedHeight(bool _fixedHeight)
        {
            fixedHeight = _fixedHeight;
            spectatorCamera.SetIsFixedHeight(_fixedHeight);
        }

        /// <summary>
        /// Enables or disables the UI
        /// </summary>
        public void ToggleUIElements()
        {
            if (navigationUI.gameObject.activeSelf)
            {
                navigationUI.gameObject.SetActive(false);
            }
            else
            {
                navigationUI.gameObject.SetActive(true);
            }
        }

        public ulong GetCurrentSelectedNetworkID()
        {
            return targetObject.NetworkObjectId;
        }

        /// <summary>
        /// Refreshes the objects, that can be switched between, by set object types and a search string
        /// </summary>
        public void RefreshFilteredObjects()
        {
            spectatorObjectsList.Clear();
            HashSet<NetworkObject> spawnedObjectsList = NetworkManager.Singleton.SpawnManager.SpawnedObjectsList;
            amountOfCurrentObjectsSpawned = spawnedObjectsList.Count;

            List<SpectatorsObject> favObjectList = new List<SpectatorsObject>();
            List<SpectatorsObject> objectList = new List<SpectatorsObject>();

            foreach (NetworkObject o in spawnedObjectsList)
            {
                if (CheckObjectType(o))
                {
                    NononZoneObjectNetwork nononZoneObjectNetwork = o.GetComponent<NononZoneObjectNetwork>();
                    if (nononZoneObjectNetwork != null)
                    {

                        SpectatorsObject fObj = new SpectatorsObject();
                        fObj.networkObjectId = o.NetworkObjectId;
                        //fObj.objectName = nononZoneObjectNetwork.nononZoneObjectName;
                        fObj.objectName = GetNononZoneObjectName(nononZoneObjectNetwork.transform);
                        fObj.objectTransform = o.transform;

                        ulong favObjectID;
                        if (favSpectatorList.TryGetValue(fObj.networkObjectId, out favObjectID))
                        {
                            fObj.favorite = true;
                        }
                        else
                        {
                            fObj.favorite = false;
                        }

                        if (nonAutoViewSpectatorList.TryGetValue(fObj.networkObjectId, out favObjectID))
                        {
                            fObj.autoSelect = false;
                        }
                        else
                        {
                            fObj.autoSelect = true;
                        }
                        if (fObj.favorite) favObjectList.Add(fObj);
                        else objectList.Add(fObj);

                        if (fObj.networkObjectId == homeNetworkID) fObj.isHome = true;

                        ulong fpvDefaultID;
                        if (fpvDefaultSpectatorList.TryGetValue(fObj.networkObjectId, out fpvDefaultID))
                        {
                            fObj.isOrbitViewDefault = false;
                        }
                        else
                        {
                            fObj.isOrbitViewDefault = true;
                        }
                    }
                }
            }
            // sort the list alphabetically            
            favObjectList.Sort(CompareListByName);
            objectList.Sort(CompareListByName);
            spectatorObjectsList.AddRange(favObjectList);
            spectatorObjectsList.AddRange(objectList);
            // Check favlist if some entries do not exist anymore
            CheckFavList();
            // Check non auto view list if some entries do not exist anymore
            CheckNonAutoViewList();
            // Check non fpv default list if some entries do not exist anymore
            CheckFpvDefaultSList();
            // mark as dirty for the UI to refresh
            spectatorListIsDirty = true;
        }

        void CheckFavList()
        {

        }
        void CheckNonAutoViewList()
        {

        }

        void CheckFpvDefaultSList()
        {

        }


        private static int CompareListByName(SpectatorsObject i1, SpectatorsObject i2)
        {
            return i1.objectName.CompareTo(i2.objectName);
        }

        /// <summary>
        /// Monitors the Spawned Objects and it the amount monitored differs from it it refreshes the list by applying the filters again
        /// </summary>
        void MonitorAndRefreshObjectList()
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjectsList.Count != amountOfCurrentObjectsSpawned)
            {
                RefreshFilteredObjects();
                if (selectedFilterObject >= amountOfCurrentObjectsSpawned ||
                    (selectedFilterObject >= amountOfCurrentObjectsSpawned && spectatorObjectsList[selectedFilterObject].objectTransform == null))
                {
                    SelectNextObject();
                }
            }

        }

        /// <summary>
        /// Method reacting when toggle the auto switch
        /// </summary>
        public void OnAutoToggleChanged()
        {
            autoSwitch = autoSwitchToggle.isOn;
        }

        /// <summary>
        /// Toggles the ui element and the autoswitch bool
        /// </summary>
        public void ToggleAutoToggle()
        {
            autoSwitch = !autoSwitch;
            autoSwitchToggle.isOn = autoSwitch;
        }

        void DisableAutoToggle()
        {
            autoSwitch = false;
            autoSwitchToggle.isOn = false;
        }

        public void EnableDisableMovementInput(bool enabled)
        {
            spectatorCamera.EnableDisableMovementInput(enabled);
            if (enabled)
            {
                playerInput.CharacterControls.Move.Enable();
            }
            else
            {
                playerInput.CharacterControls.Move.Disable();
            }
        }

        public void EnableDisableHotKeys(bool enabled)
        {
            if (enabled)
            {
                playerInput.CharacterControls.AttachToObject.Enable();
                playerInput.CharacterControls.AttachToNextObject.Enable();
                playerInput.CharacterControls.AttachToPrevObject.Enable();
                playerInput.CharacterControls.AutoSwitch.Enable();
                playerInput.CharacterControls.ToggleUI.Enable();
            }
            else
            {
                playerInput.CharacterControls.AttachToObject.Disable();
                playerInput.CharacterControls.AttachToNextObject.Disable();
                playerInput.CharacterControls.AttachToPrevObject.Disable();
                playerInput.CharacterControls.AutoSwitch.Disable();
                playerInput.CharacterControls.ToggleUI.Disable();
            }

        }

        /// <summary>
        /// Checks if the network object is of one of the types in the filter list
        /// </summary>
        /// <param name="o">network object to compare</param>
        /// <returns>true if object is one of the types in filter list</returns>
        public bool CheckObjectType(NetworkObject o)
        {
            HashSet<INononZoneObject.NononZoneObjType> selectedObjectTypes = GetSelectedObjectTypes();

            NononZoneObjectNetwork nononZoneObjectNetwork = o.GetComponent<NononZoneObjectNetwork>();
            if (nononZoneObjectNetwork != null)
            {
                foreach (INononZoneObject.NononZoneObjType oType in nononZoneObjectNetwork.objectTypes)
                {
                    if (selectedObjectTypes.Contains(oType)) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// returns all the filtered object types from the list for which selected is true
        /// </summary>
        /// <returns>list of object types</returns>
        public HashSet<INononZoneObject.NononZoneObjType> GetSelectedObjectTypes()
        {
            HashSet<INononZoneObject.NononZoneObjType> selectedObjTypes = new HashSet<INononZoneObject.NononZoneObjType>();
            foreach (ObjectTypeFilter objTypeFilter in objectTypeFilterList)
            {
                if (objTypeFilter.selected) selectedObjTypes.Add(objTypeFilter.objType);
            }
            return selectedObjTypes;
        }

        /// <summary>
        /// Placeholder - Can be used if you don't want to set the object types on the object itself, but by a settings ui
        /// </summary>
        void SetupObjectTypeFilter()
        {
            objectTypeFilterList.Clear();
            foreach (INononZoneObject.NononZoneObjType oType in Enum.GetValues(typeof(INononZoneObject.NononZoneObjType)))
            {
                ObjectTypeFilter oTypeFilter = new ObjectTypeFilter();
                oTypeFilter.objType = oType;
                if (oType.Equals(INononZoneObject.NononZoneObjType.ENEMY) || oType.Equals(INononZoneObject.NononZoneObjType.PLAYER) || oType.Equals(INononZoneObject.NononZoneObjType.PLATTFORM))
                {
                    oTypeFilter.selected = true;
                }
                else
                {
                    oTypeFilter.selected = false;
                }
                objectTypeFilterList.Add(oTypeFilter);
            }
        }

        /// <summary>
        /// Switches the camera to the next object in the filtered list
        /// </summary>
        public void SelectNextObject()
        {
            spectatorCamera.SetSticky(true);

            if (spectatorObjectsList.Count == 0)
            {
                targetText.text = EMPTY_STRING;
                SetFollowTarget(null);
                return;
            }
            selectedFilterObject++;
            if (selectedFilterObject >= spectatorObjectsList.Count)
            {
                selectedFilterObject = 0;
            }
            selectedFilterObject = GetNextActiveFilterObject();
            if (spectatorObjectsList[selectedFilterObject].isOrbitViewDefault) ActivateFreelookCamera(selectedFilterObject);
            else ActivateFirstPersonCamera(selectedFilterObject);
            // Reset the timer
            timerBeginAutoSwitch = Time.time;
            randomTimeAutoSwitch = GetRandomAutoSwitchTime();
        }

        int GetNextActiveFilterObject()
        {
            int index = selectedFilterObject;
            for (int i = 0; i < spectatorObjectsList.Count; i++)
            {
                if (index >= spectatorObjectsList.Count) index = 0;
                if (spectatorObjectsList[index].autoSelect)
                {
                    return index;
                }
                index++;
            }
            return 0;
        }

        void ActivateFreelookCamera(int specatorObject)
        {
            targetText.text = GetNononZoneObjectName(spectatorObjectsList[specatorObject].objectTransform);
            SetFollowTarget(spectatorObjectsList[specatorObject].objectTransform);
            camText.text = ORBIT_CAM_TEXT_STRING;
            spectatorCamera.ActivateFreelookCamera();
            SetFPVObjectCamActve(false);
        }

        void ActivateFreelookCamera(SpectatorsObject obj)
        {
            targetText.text = GetNononZoneObjectName(obj.objectTransform);
            SetFollowTarget(obj.objectTransform);
            camText.text = ORBIT_CAM_TEXT_STRING;
            spectatorCamera.ActivateFreelookCamera();
            SetFPVObjectCamActve(false);
        }

        void ActivateFirstPersonCamera(int specatorObject)
        {
            if (specatorObject == -1)
            {
                camText.text = FREEMOVE_CAM_TEXT_STRING;
                targetText.text = EMPTY_STRING;
                DisableAutoToggle();
                SetFPVObjectCamActve(false);
                spectatorCamera.SetSticky(false);
                spectatorCamera.Activate1stPersonCamera();
            }
            else
            {
                targetText.text = GetNononZoneObjectName(spectatorObjectsList[specatorObject].objectTransform);
                SetFollowTarget(spectatorObjectsList[specatorObject].objectTransform);
                camText.text = OBJECT_CAM_TEXT_STRING;
                SetFPVObjectCamActve(true);
                spectatorCamera.Activate1stPersonCamera();
            }
        }

        void ActivateFirstPersonCamera(SpectatorsObject obj)
        {
            targetText.text = GetNononZoneObjectName(obj.objectTransform);
            SetFollowTarget(obj.objectTransform);
            camText.text = OBJECT_CAM_TEXT_STRING;
            SetFPVObjectCamActve(true);
            spectatorCamera.Activate1stPersonCamera();
        }

        void SetFPVObjectCamActve(bool _active)
        {
            fpvObjectCamActive = _active;
            spectatorCamera.SetFpvObjectCamActive(_active);
        }

        /// <summary>
        /// Switches the camera to the previous object in the filtered list
        /// </summary>
        public void SelectPrevObject()
        {
            spectatorCamera.SetSticky(true);

            if (spectatorObjectsList.Count == 0)
            {
                targetText.text = EMPTY_STRING;
                SetFollowTarget(null);
                return;
            }

            selectedFilterObject--;
            if (selectedFilterObject < 0)
            {
                selectedFilterObject = spectatorObjectsList.Count - 1;
            }
            selectedFilterObject = GetPrevActiveFilterObject();
            if (spectatorObjectsList[selectedFilterObject].isOrbitViewDefault) ActivateFreelookCamera(selectedFilterObject);
            else ActivateFirstPersonCamera(selectedFilterObject);
            // Reset the timer
            timerBeginAutoSwitch = Time.time;
            randomTimeAutoSwitch = GetRandomAutoSwitchTime();
        }

        int GetPrevActiveFilterObject()
        {
            int index = selectedFilterObject;
            for (int i = 0; i < spectatorObjectsList.Count; i++)
            {
                if (index < 0) index = spectatorObjectsList.Count - 1;
                if (spectatorObjectsList[index].autoSelect)
                {
                    return index;
                }
                index--;
            }
            return 0;
        }

        public void SelectHomeObject()
        {
            spectatorCamera.SetSticky(true);

            SpectatorsObject selectedSpectatorObject = GetSpectatorObjectByNetworkID(homeNetworkID);
            if (homeNetworkID == 0 || selectedSpectatorObject == null)
            {
                // Don't do anything if no home object is choosen
                return;
            }
            selectedFilterObject = GetSpectatorListIndex(selectedSpectatorObject);
            if (spectatorObjectsList[selectedFilterObject].isOrbitViewDefault) ActivateFreelookCamera(selectedSpectatorObject);
            else ActivateFirstPersonCamera(selectedSpectatorObject);
            // Reset the timer
            timerBeginAutoSwitch = Time.time;
            randomTimeAutoSwitch = GetRandomAutoSwitchTime();
        }

        int GetSpectatorListIndex(SpectatorsObject so)
        {
            int i = 0;
            foreach (SpectatorsObject theSO in spectatorObjectsList)
            {
                if (theSO.networkObjectId == so.networkObjectId) return i;
                i++;
            }
            return -1;
        }

        int GetSpectatorListIndex(ulong networkID)
        {
            int i = 0;
            foreach (SpectatorsObject theSO in spectatorObjectsList)
            {
                if (theSO.networkObjectId == networkID) return i;
                i++;
            }
            return -1;
        }

        public void ShowSpecificSpectatorObject(SpectatorsObject so)
        {
            spectatorCamera.SetSticky(true);

            selectedFilterObject = GetSpectatorListIndex(so);
            if (so.isOrbitViewDefault) ActivateFreelookCamera(so);
            else ActivateFirstPersonCamera(so);
            // Reset the timer
            timerBeginAutoSwitch = Time.time;
            randomTimeAutoSwitch = GetRandomAutoSwitchTime();
        }

        SpectatorsObject GetSpectatorObjectByNetworkID(ulong networkID)
        {
            foreach (SpectatorsObject so in spectatorObjectsList)
            {
                if (so.networkObjectId == networkID) return so;
            }
            return null;
        }


        /// <summary>
        /// Sets the follow object on the camera
        /// </summary>
        /// <param name="_target">object to set follow to</param>
        public void SetFollowTarget(Transform _target)
        {
            if (_target != null) targetObject = _target.GetComponent<NononZoneObjectNetwork>();
            else targetObject = null;
            spectatorCamera.SetFollowAndLookatTarget(_target);
        }

        /// <summary>
        /// Sets the LookAt object on the camera
        /// </summary>
        /// <param name="_target">object to set lookAt to</param>
        public void SetLookAtTarget(Transform _target)
        {
            spectatorCamera.SetFollowAndLookatTarget(targetObject.transform, _target);
        }

        /// <summary>
        /// Is called when Spectator player is spawned
        /// </summary>
        public override void OnNetworkSpawn()
        {
            if (IsClient && IsOwner)
            {
                camText.text = ORBIT_CAM_TEXT_STRING;
                //SetupObjectTypeFilter();
                RefreshFilteredObjects();
                if (spectatorObjectsList.Count > 0)
                {
                    targetText.text = GetNononZoneObjectName(spectatorObjectsList[0].objectTransform);
                    SetFollowTarget(spectatorObjectsList[0].objectTransform);
                }
            }
            else
            {
                navigationUI.gameObject.SetActive(false);
            }
            base.OnNetworkSpawn();
        }


        /// <summary>
        /// Ports the player to a certain location. 
        /// </summary>
        /// <param name="newLocation">location to port</param>
        /// <param name="newRotation">rotation for the character</param>
        public void SetPlayerPos(Vector3 newLocation, Quaternion newRotation)
        {
            transform.position = newLocation;
            transform.rotation = newRotation;
        }

        /// <summary>
        /// Callback method on movement (stick or wsad)
        /// </summary>
        /// <param name="context">Callback context</param>
        void OnMovementInput(InputAction.CallbackContext context)
        {
            idleTimerStart = Time.time;

            currentMovementInput = context.ReadValue<Vector2>();
            currentMovement.x = currentMovementInput.x;
            currentMovement.z = currentMovementInput.y;
        }

        /// <summary>
        /// Checks if the current object has aggro. If true, it will switch for a certain time to a third person camera
        /// </summary>
        public void HandleAggroTarget()
        {
            if (targetObject != null && spectatorCamera.IsSticky())
            {
                if (targetObject.HasAggro() && !hasAggro)
                {
                    hasAggro = true;
                    SetLookAtTarget(targetObject.GetAggroTransform());
                    camText.text = CHASING_CAM_TEXT_STRING;
                    spectatorCamera.Activate3rdPersonCamera();
                    if (autoReset3rdPerson)
                    {
                        StartCoroutine(Timer4ThirdpersonReset());
                    }
                }

                if (!targetObject.HasAggro() && hasAggro)
                {
                    camText.text = ORBIT_CAM_TEXT_STRING;
                    hasAggro = false;
                    SetLookAtTarget(null);
                    spectatorCamera.ActivateFreelookCamera();
                }
            }
        }

        /// <summary>
        /// Timer method to switch back to freelook camera
        /// </summary>
        /// <returns></returns>
        private IEnumerator Timer4ThirdpersonReset()
        {
            timerBeginResetThirdPerson = Time.time;
            while (Time.time - timerBeginResetThirdPerson < time4ResetThirdPerson)
            {
                yield return null;
            }
            SetLookAtTarget(null);
            camText.text = ORBIT_CAM_TEXT_STRING;
            spectatorCamera.ActivateFreelookCamera();
        }

        public void HandleFirstPersonMovement()
        {
            if (fpvObjectCamActive && targetObject != null)
            {
                Vector3 targetDestPosition = targetObject.transform.position;
                Collider collider1 = targetObject.GetComponent<Collider>();
                if (collider1 != null)
                {
                    float yHalfExtents = collider1.bounds.extents.y;
                    float yCenter = collider1.bounds.center.y;
                    float yUpper = yCenter + yHalfExtents;
                    float yLower = yCenter - yHalfExtents;
                    float aQuarter = yHalfExtents * 2 * ppositionOffsetPercentage;
                    yUpper = yUpper - aQuarter;
                    targetDestPosition = new Vector3(targetDestPosition.x, yUpper, targetDestPosition.z);
                }
                targetDestPosition = targetDestPosition + targetObject.transform.forward * fpvPositionOffsetMultiplier;
                transform.position = targetDestPosition;
                if (!isCameraMovePressed)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetObject.transform.rotation, Time.deltaTime * fpvRotationSpeedMultiplier);
                }
            }
        }

        /// <summary>
        /// Handles the movement of the freelook camera
        /// </summary>
        public void HandleMovement()
        {

            if (currentMovement.x < 0 || currentMovement.x > 0 ||
                currentMovement.z < 0 || currentMovement.z > 0)
            {
                if (spectatorCamera.IsSticky())
                {
                    idleTimerStart = Time.time;
                    ActivateFirstPersonCamera(-1);
                    if (targetObject != null)
                    {
                        transform.position = spectatorCamera.GetFreeLookCameraPosition();
                        transform.LookAt(targetObject.transform);
                    }
                }

                Vector3 movement = transform.right * currentMovement.x + transform.forward * currentMovement.z;
                //movement.y = 0f;
                //Normalizes the vector3 to ensure that even if 2 inputs is pressed at the same time, the value still is either 1, 0 or -1, before multiplying with
                //deltaTime (framerate issue) and movementSpeed.
                movement.Normalize();
                movement *= Time.deltaTime * 10;
                transform.position = transform.position + movement;
                if (fixedHeight)
                {
                    // Raycast down to get the distance to the 

                    //get the mask to raycast against either the player or enemy layer
                    int layer_mask = LayerMask.GetMask(NononZoneConstants.Spectator.TERRAIN_LAYER_NAME);
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.up) * -1, out hit, rayCastDistanceFixedHeight, layer_mask))
                    {
                        transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, hit.point.y + distanceFixedHeight, transform.position.z), Time.deltaTime * moveToFixedHeightMultiplier);
                    }

                }
            }
        }

        /// <summary>
        /// Checks if there is a collision of the freelook camera and pushes back the object on contact
        /// </summary>
        /// <param name="other">Other collider</param>
        private void OnTriggerEnter(Collider other)
        {
            if (!ghostMode)
            {
                Vector3 movement = transform.right * currentMovement.y + transform.forward * currentMovement.z;
                movement.Normalize();
                movement *= -15;

                bounceOffDirection = movement;

                transform.position = Vector3.Lerp(transform.position, transform.position + movement, Time.deltaTime);
            }
        }

        /// <summary>
        /// Checks if the collision still occurs. If yes the object will be pushed back the same way it was on first contact
        /// </summary>
        /// <param name="other">Other collider</param>
        private void OnTriggerStay(Collider other)
        {
            if (!ghostMode)
            {
                transform.position = Vector3.Lerp(transform.position, transform.position + bounceOffDirection, Time.deltaTime);
            }
        }

        /// <summary>
        /// Handles when the freelook mode is active if a NononZoneNetwork Object is in focus. If yes it displays the message for
        /// a possible attach to the object
        /// </summary>
        public void HandleAttach()
        {
            if (!spectatorCamera.IsSticky() && !crosshair.isActiveAndEnabled)
            {
                crosshair.gameObject.SetActive(true);
            }

            if (spectatorCamera.IsSticky() && crosshair.isActiveAndEnabled)
            {
                crosshair.gameObject.SetActive(false);
            }

            if (!spectatorCamera.IsSticky())
            {
                Ray theRay = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 100));
                RaycastHit hit;
                if (Physics.Raycast(theRay, out hit))
                {

                    if (hit.transform.GetComponent<NononZoneObjectNetwork>() != null)
                    {
                        possibleAttachTransform = hit.transform;
                        crosshair.color = Color.red;
                        attachText.text = ATTACH_TEXT_STRING;
                    }
                    else
                    {
                        possibleAttachTransform = null;
                        crosshair.color = Color.white;
                        attachText.text = "";
                    }
                }
                else
                {
                    possibleAttachTransform = null;
                    crosshair.color = Color.white;
                    attachText.text = "";
                }
            }
            else
            {
                attachText.text = "";
            }
        }

        /// <summary>
        /// Returns the name of the object
        /// </summary>
        /// <param name="transform">transform to get name from</param>
        /// <returns>Name of the object</returns>
        string GetNononZoneObjectName(Transform transform)
        {
            NononZoneObjectNetwork nononZoneObject = transform.GetComponent<NononZoneObjectNetwork>();
            if (nononZoneObject != null) return nononZoneObject.GetNononZoneObjectName();
            else return "";
        }

        /// <summary>
        /// If an object is in focus in freelook mode, this object will be attached to and the
        /// orbit cam will be activated
        /// </summary>
        /// <param name="context"></param>
        void OnAttachInput(InputAction.CallbackContext context)
        {
            bool isAttachButtonPressed = context.ReadValueAsButton();
            if (isAttachButtonPressed && possibleAttachTransform != null)
            {
                camText.text = ORBIT_CAM_TEXT_STRING;
                targetText.text = GetNononZoneObjectName(possibleAttachTransform);
                selectedFilterObject = GetSpectatorListIndex(possibleAttachTransform.GetComponent<NetworkObject>().NetworkObjectId);
                SetFollowTarget(possibleAttachTransform);
                spectatorCamera.ActivateFreelookCamera();
                spectatorCamera.SetSticky(true);
            }
        }

        /// <summary>
        /// Input for attaching to next object in list
        /// </summary>
        /// <param name="context">Callback context</param>
        void OnAttachToNextObjectInput(InputAction.CallbackContext context)
        {
            bool attachToNext = context.ReadValueAsButton();
            if (attachToNext) SelectNextObject();
        }

        /// <summary>
        /// Input for attaching to previous object in list
        /// </summary>
        /// <param name="context">Callback context</param>
        void OnAttachToPrevObjectInput(InputAction.CallbackContext context)
        {
            bool attachToPrev = context.ReadValueAsButton();
            if (attachToPrev) SelectPrevObject();
        }

        /// <summary>
        /// Input for enabling/disabling auto switch
        /// </summary>
        /// <param name="context">Callback context</param>
        void OnAutoSwitchInput(InputAction.CallbackContext context)
        {
            bool autoSwitchPressed = context.ReadValueAsButton();
            if (autoSwitchPressed) ToggleAutoToggle();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        void OnToggleUIInput(InputAction.CallbackContext context)
        {
            bool toggleUIPressed = context.ReadValueAsButton();
            if (toggleUIPressed && IsClient && IsOwner) ToggleUIElements();
        }

        void OnGhostModeInput(InputAction.CallbackContext context)
        {
            bool keyPressed = context.ReadValueAsButton();
            if (keyPressed)
            {
                ghostMode = !ghostMode;
            }
        }

        void OnFixedHeightInput(InputAction.CallbackContext context)
        {
            bool keyPressed = context.ReadValueAsButton();
            if (keyPressed)
            {
                SetFixedHeight(!fixedHeight);
            }
        }

        void OnCameraSwitchInput(InputAction.CallbackContext context)
        {
            bool keyPressed = context.ReadValueAsButton();
            if (keyPressed)
            {
                if (spectatorCamera.IsSticky())
                {
                    if (fpvObjectCamActive)
                    {
                        ActivateFreelookCamera(selectedFilterObject);
                    }
                    else
                    {
                        ActivateFirstPersonCamera(selectedFilterObject);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the auto switch toggle
        /// </summary>
        void HandleAutoSwitch()
        {
            if (autoSwitch)
            {
                if (Time.time - timerBeginAutoSwitch > randomTimeAutoSwitch)
                {
                    SelectNextObject();
                }
            }
        }

        void HandleFreeMoveIdleTimer()
        {
            if (!spectatorCamera.IsSticky() && Time.time - idleTimerStart > idleTime4AutoSwitchBack)
            {
                Debug.Log(Time.time - idleTimerStart + ":" + idleTime4AutoSwitchBack);
                autoSwitch = true;
                autoSwitchToggle.isOn = true;
                SelectNextObject();
            }
        }

        float GetRandomAutoSwitchTime()
        {
            return UnityEngine.Random.Range(time4AutoSwitchMin, time4AutoSwitchMax);
        }


        private void Update()
        {
            if (IsClient && IsOwner)
            {
                MonitorAndRefreshObjectList();
                HandleAggroTarget();
                HandleMovement();
                HandleFirstPersonMovement();
                HandleAttach();
                HandleAutoSwitch();
                HandleFreeMoveIdleTimer();
            }
        }
    }
}