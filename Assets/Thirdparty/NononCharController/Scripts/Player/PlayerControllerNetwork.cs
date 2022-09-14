using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System;

namespace zone.nonon
{

    public class PlayerControllerNetwork : NetworkBehaviour
    {

        // Weapons
        class WeaponSlot
        {
            public int id;
            public Weapon.WeaponTypes weaponType;
            public int prefabNr;
            public Weapon weapon;
            public Transform weaponHolder;
            public bool oneHand;
        }
        WeaponSlot OneHandLeftSlot = new WeaponSlot();
        NetworkVariable<int> OneHandLeftSlotPrefab = new NetworkVariable<int>(-1);
        WeaponSlot OneHandRightSlot = new WeaponSlot();
        NetworkVariable<int> OneHandRightSlotPrefab = new NetworkVariable<int>(-1);
        WeaponSlot TwoHandLeftSlot = new WeaponSlot();
        NetworkVariable<int> TwoHandLeftSlotPrefab = new NetworkVariable<int>(-1);
        WeaponSlot TwoHandRightSlot = new WeaponSlot();
        NetworkVariable<int> TwoHandRightSlotPrefab = new NetworkVariable<int>(-1);

        [Serializable]
        public class SerializableWeapon : INetworkSerializable
        {
            [SerializeField]
            public Weapon.WeaponTypes weaponType;
            [SerializeField]
            public int prefabNumber;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref weaponType);
                serializer.SerializeValue(ref prefabNumber);
            }
        }

        NetworkVariable<int> weaponDrawn_nw = new NetworkVariable<int>(-1);


        public ParticleSystem dustParticles;
        public ParticleSystem waterParticles;
        public ParticleSystem thrusterParticles;

        PlayerInput playerInput;

        // Animator variables
        Animator animator;
        int isWalkingHash;
        int isRunningHash;
        int speedPercentHash;
        int isJumpingHash;
        int isFallingHash;
        int isSwimmingHash;
        int isFlyingHash;
        int isDeadHash;
        int draw2HWeaponHash;

        // Actions
        List<InputAction> regularActions;
        bool regularActionsEnabled;
        List<InputAction> dialogActions;
        bool dialogActionsEnabled;

        // Flying Devices
        NetworkVariable<int> currentMount_nw = new NetworkVariable<int>(0);
        NetworkVariable<int> amountOfMounts_nw = new NetworkVariable<int>(0);

        // Lists are not working correctly spawning and initialization
        //NetworkList<int> mountsInBag_nw;

        enum Mount { NONE, THRUSTER, FLY_MOUNT, VEHICLE };
        GameObject instantiatedThruster;

        PlayerRigging playerRigging;
        RigBuilder rigBuilder;

        NetworkVariable<bool> isAimingPressed_nw = new NetworkVariable<bool>(false);
        NetworkVariable<bool> isAiming_nw = new NetworkVariable<bool>(false);
        NetworkVariable<float> timeStartAiming_nw = new NetworkVariable<float>(0f);
        [Header("Shooting")]
        public float aimDuration = 0.3f;
        public Transform cameraAimTarget;
        public float holdAimingDuration = 2f;

        // Movement variables
        NetworkVariable<Vector3> currentWalkMovement_nw = new NetworkVariable<Vector3>(Vector3.zero);
        NetworkVariable<Vector3> currentRunMovement_nw = new NetworkVariable<Vector3>(Vector3.zero);
        NetworkVariable<Vector3> currentStriveMovement_nw = new NetworkVariable<Vector3>(Vector3.zero);
        NetworkVariable<bool> isStriveLeftPressed_nw = new NetworkVariable<bool>(false);
        NetworkVariable<bool> isStriveRightPressed_nw = new NetworkVariable<bool>(false);
        NetworkVariable<bool> isStriveReleased_nw = new NetworkVariable<bool>(false);
        NetworkVariable<bool> isMovementPressed_nw = new NetworkVariable<bool>(false);
        NetworkVariable<bool> isRunPressed_nw = new NetworkVariable<bool>(true);
        NetworkVariable<bool> isCameraMovePressed_nw = new NetworkVariable<bool>(false);
        NetworkVariable<bool> isCharacterRotatePressed_nw = new NetworkVariable<bool>(false);
        NetworkVariable<bool> isJumpPressed_nw = new NetworkVariable<bool>(false);
        NetworkVariable<bool> isJumping_nw = new NetworkVariable<bool>(false);
        NetworkVariable<bool> isJumpReleasedAfterJump_nw = new NetworkVariable<bool>(true);
        NetworkVariable<bool> isJumpAnimating_nw = new NetworkVariable<bool>(false);
        NetworkVariable<bool> isSlidingDown_nw = new NetworkVariable<bool>(false);
        NetworkVariable<bool> isDivePressed_nw = new NetworkVariable<bool>(false);
        NetworkVariable<float> currentSlopeAngle_nw = new NetworkVariable<float>(0f);
        NetworkVariable<float> forwardForceOnJump_nw = new NetworkVariable<float>(0f);
        NetworkVariable<bool> hasFallingStarted_nw = new NetworkVariable<bool>(false);

        // Water
        NetworkVariable<bool> isInWater_nw = new NetworkVariable<bool>(false);
        NetworkVariable<bool> isSwimming_nw = new NetworkVariable<bool>(false);
        NetworkVariable<bool> isFlying_nw = new NetworkVariable<bool>(false);

        [Header("Swimming")]
        [Tooltip("Multiplier for ascending and descending")]
        public float diveAndRaise_Water_Multiplier = 3f;
        [Tooltip("Height under water when character can jump")]
        public float jumpOutOfWaterHeight = 0.2f;
        [Tooltip("Color of the fog")]
        public Color normalColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        [Tooltip("Color of the fog when underwater")]
        public Color underwaterColor = new Color(0.22f, 0.65f, 0.77f, 0.5f);
        [Tooltip("Multiplier of movement in water like running speed")]
        public float swimRunMultiplier = 6.0f;
        [Tooltip("Multiplier of movement in water like walking speed")]
        public float swimWalkMultiplier = 2.0f;

        // Bounds is no longer serialize. I split it into two vector3
        NetworkVariable<Vector3> waterColliderBounds_nw_center = new NetworkVariable<Vector3>(Vector3.zero);
        NetworkVariable<Vector3> waterColliderBounds_nw_extents = new NetworkVariable<Vector3>(Vector3.zero);

        NetworkVariable<bool> isCameraUnderwater_nw = new NetworkVariable<bool>(false);


        // Flying        
        [Header("Flying")]
        [Tooltip("Multiplier for ascending and descending")]
        public float diveAndRaise_Air_Multiplier = 5f;
        [Tooltip("Multiplier of movement flying like running speed")]
        public float flyRunMultiplier = 15.0f;
        [Tooltip("Multiplier of movement flying like walking speed")]
        public float flyWalkMultiplier = 5.0f;
        NetworkVariable<bool> flyingEnabled_nw = new NetworkVariable<bool>(false);

        // Collision variables
        Transform collidedTransform;

        [Header("Movement")]
        // Movement Settings
        [Tooltip("Speed for moving with two mouse buttons")]
        public float forwardMouseSpeed = 1.0f;
        [Tooltip("Multiplier of movement walking speed")]
        public float walkMultiplier = 3.0f;
        [Tooltip("Multiplier of movement running speed")]
        public float runMultiplier = 8.0f;
        [Tooltip("Degrees of strive movement")]
        public float strivedegrees = 20f;
        [Tooltip("Multiplier for rotating character by keys")]
        public float rotationMultiplier = 1.3f;
        [Tooltip("Multiplier for rotating character by mouse or stick")]
        public float rotationMouseDumper = 1.0f;
        [Tooltip("Raylength to check slope angle")]
        public float slopeForceRayLength = 20f;
        [Tooltip("Limit slope angle before character slides down")]
        public int slopeLimit = 50;
        public Transform groundDirection;
        public Transform fallDirection;

        [Header("Steps")]
        [Tooltip("Maximum Step Height")]
        public float maxStepHeight = 0.4f;
        [Tooltip("Overshoot into the direction of potential step")]
        public float stepSearchMargin = 0.01f;
        private List<ContactPoint> contactPoints = new List<ContactPoint>();
        private Vector3 lastVelocity;

        // gravity variables
        [Header("Gravity")]
        [Tooltip("Treshhold gravity before falling")]
        public float fallingGravityTreshhold = -10.0f;
        [Tooltip("Maximum acceleration while falling down")]
        public float maxFallingGravity = -20.0f;
        float gravity = -9.81f;


        // Jumping variables
        [Header("Jumping")]
        [Tooltip("Falling down multiplier")]
        public float fallMultiplier = 1.5f;
        [Tooltip("Force for jumping")]
        public float jumpForce = 275f;
        [Tooltip("Force when jump button pressed")]
        public float addingForceJumpPressed = 3f;
        [Tooltip("Jump multiplier when reaching surface")]
        public float swimmingJumpMultiplier = 2f;
        [Tooltip("Jump multiplier when standing on objects that is moving down")]
        public float collidedTransformJumpMultiplier = 1.5f;
        [Tooltip("Treshhold when player wanted to jump, but is stuck")]
        public float stickingAirTimeTreshhold = 0.2f;
        [Tooltip("Distance when player has jumped successfully")]
        public float outOfHitboxDistance = 0.2f;

        NetworkVariable<bool> hasLeftHitBoxAfterJump_nw = new NetworkVariable<bool>(false);

        // Camera
        [Header("Player Rotation")]
        [Tooltip("Rotation sensitivity when walking with mouse buttons")]
        public float mouseSensitivityWhileMouseWalk = 1.5f;
        [Tooltip("Rotation sensitivity")]
        public float mouseSensitivity = 2.5f;
        NononPlayerCameraCMNetwork playerCam;

        [Header("FallingDmg")]
        [Tooltip("Tresshold in seconds wheres no fall damage")]
        public float nonDmgFallTime = 0.1f;
        [Tooltip("Damage for each second falling")]
        public float damageForFallingSeconds = 500f;
        [Tooltip("Tresshold in seconds wheres no slide down damage")]
        public float nonDmgSlidingTime = 0.6f;
        [Tooltip("Damage for each second sliding down")]
        public float damageForSlidingSeconds = 500f;
        [Tooltip("Tresshold in seconds wheres jumping damage")]
        public float nonDmgJumpingTime = 1f;
        [Tooltip("Damage for each second jumping")]
        public float damageForJumpingSeconds = 500f;

        NetworkVariable<float> airTimeFalling_nw = new NetworkVariable<float>(0f);
        NetworkVariable<float> airTimeJumping_nw = new NetworkVariable<float>(0f);
        NetworkVariable<float> airTimeSlidingDown_nw = new NetworkVariable<float>(0f);
        float dustLastTimePlayed;
        float dustPlayInterval = 0.3f;

        [Header("Sliding")]
        [Tooltip("Force to pull down player")]
        public float slidingDownForce = 300f;

        [SerializeField, HideInInspector]
        Rigidbody rigidBody;

        NetworkVariable<Vector3> moveDirection = new NetworkVariable<Vector3>(Vector3.zero);
        NetworkVariable<Vector3> jumpForceDirection = new NetworkVariable<Vector3>(Vector3.zero);
        NetworkVariable<Vector3> slideDownForceDirection = new NetworkVariable<Vector3>(Vector3.zero);
        NetworkVariable<Vector3> fallingDownForceDircetion = new NetworkVariable<Vector3>(Vector3.zero);

        NetworkVariable<Vector3> cameraEulerAngles_nw = new NetworkVariable<Vector3>(Vector3.zero);
        NetworkVariable<Vector3> cameraAimPointPosition_nw = new NetworkVariable<Vector3>(Vector3.zero);
        NetworkVariable<Quaternion> cameraAimPointRotation_nw = new NetworkVariable<Quaternion>(Quaternion.identity);
        NetworkVariable<Vector3> cameraPosition_nw = new NetworkVariable<Vector3>(Vector3.zero);

        List<Transform> collectedLoot = new List<Transform>();

        private void Awake()
        {
            isJumpPressed_nw.Value = false;
            isJumping_nw.Value = false;
            isJumpReleasedAfterJump_nw.Value = true;


            playerInput = new PlayerInput();
            regularActions = new List<InputAction>();
            dialogActions = new List<InputAction>();

            playerInput.CharacterControls.Move.started += OnMovementInput;
            playerInput.CharacterControls.Move.canceled += OnMovementInput;
            playerInput.CharacterControls.Move.performed += OnMovementInput;
            regularActions.Add(playerInput.CharacterControls.Move);

            playerInput.CharacterControls.Run.started += OnRun;
            regularActions.Add(playerInput.CharacterControls.Run);

            playerInput.CharacterControls.StriveLeft.started += OnStriveLeft;
            playerInput.CharacterControls.StriveLeft.canceled += OnStriveLeft;
            regularActions.Add(playerInput.CharacterControls.StriveLeft);

            playerInput.CharacterControls.StriveRight.started += OnStriveRight;
            playerInput.CharacterControls.StriveRight.canceled += OnStriveRight;
            regularActions.Add(playerInput.CharacterControls.StriveRight);

            playerInput.CharacterControls.EnableMoveCamera.started += OnEnableMoveCameraInput;
            playerInput.CharacterControls.EnableMoveCamera.canceled += OnEnableMoveCameraInput;
            regularActions.Add(playerInput.CharacterControls.EnableMoveCamera);

            playerInput.CharacterControls.EnableRotateAndMoveCamera.started += OnEnableRotationAndMoveCameraInput;
            playerInput.CharacterControls.EnableRotateAndMoveCamera.canceled += OnEnableRotationAndMoveCameraInput;
            regularActions.Add(playerInput.CharacterControls.EnableRotateAndMoveCamera);

            playerInput.CharacterControls.Jump.started += OnJump;
            playerInput.CharacterControls.Jump.canceled += OnJump;
            regularActions.Add(playerInput.CharacterControls.Jump);

            playerInput.CharacterControls.Dive.started += OnDecend;
            playerInput.CharacterControls.Dive.canceled += OnDecend;
            regularActions.Add(playerInput.CharacterControls.Dive);

            playerInput.CharacterControls.MountUp.started += OnEnableDisableFlying;
            regularActions.Add(playerInput.CharacterControls.MountUp);

            playerInput.CharacterControls.EquipNextWeapon.started += OnEquipNextWeapon;
            regularActions.Add(playerInput.CharacterControls.EquipNextWeapon);

            playerInput.CharacterControls.AimAndShoot.started += OnAimAndShoot;
            playerInput.CharacterControls.AimAndShoot.canceled += OnAimAndShoot;
            regularActions.Add(playerInput.CharacterControls.AimAndShoot);

            EnableInputActions(NononZoneConstants.InputActions.REGULAR_INPUT_ACTION);
            dustLastTimePlayed = Time.time;

            animator = GetComponent<Animator>();

            isWalkingHash = Animator.StringToHash(NononZoneConstants.Player.IS_WALKING_ANIM_PARAM);
            isRunningHash = Animator.StringToHash(NononZoneConstants.Player.IS_RUNNING_ANIM_PARAM);
            speedPercentHash = Animator.StringToHash(NononZoneConstants.Player.IS_SPEEDPERCENT_ANIM_PARAM);
            isJumpingHash = Animator.StringToHash(NononZoneConstants.Player.IS_JUMPING_ANIM_PARAM);
            isFallingHash = Animator.StringToHash(NononZoneConstants.Player.IS_FALLING_ANIM_PARAM);
            isSwimmingHash = Animator.StringToHash(NononZoneConstants.Player.IS_SWIMMING_ANIM_PARAM);
            isFlyingHash = Animator.StringToHash(NononZoneConstants.Player.IS_FLYING_ANIM_PARAM);
            isDeadHash = Animator.StringToHash(NononZoneConstants.Player.IS_DEAD_ANIM_PARAM);
            draw2HWeaponHash = Animator.StringToHash(NononZoneConstants.Player.DRAW_2HR_WEAPON_ANIM_TRIGGER);

            SetIsRunPressed(true);

            playerRigging = transform.GetComponent<PlayerRigging>();
            SetupWeaponSlots();

            rigBuilder = GetComponent<RigBuilder>();
            DisableEnableRigBuilder(false);
            rigBuilder.enabled = false;

            rigidBody = GetComponent<Rigidbody>();
            SetupMounts();

            if (Camera.main != null)
            {
                playerCam = Camera.main.GetComponent<NononPlayerCameraCMNetwork>();
            }


        }

        void SetupMounts()
        {
            if (IsServer) currentMount_nw.Value = 0;
        }

        int GetAmountOfMountsInBag()
        {
            return amountOfMounts_nw.Value;
        }

        void AddMount(Mount _mount)
        {
            if (IsServer)
            {
                amountOfMounts_nw.Value = amountOfMounts_nw.Value + 1;
            }
        }

        /// <summary>
        /// Part of the Awake. Initialize the weapon Slot variables
        /// </summary>
        void SetupWeaponSlots()
        {
            // Setup Weapon Slogs            
            OneHandLeftSlot.weaponHolder = playerRigging.weaponHolder1HLeft;
            OneHandLeftSlot.oneHand = true;
            OneHandLeftSlot.prefabNr = -1;
            OneHandLeftSlot.id = 0;

            OneHandRightSlot.weaponHolder = playerRigging.weaponHolder1HRight;
            OneHandRightSlot.oneHand = true;
            OneHandRightSlot.prefabNr = -1;
            OneHandLeftSlot.id = 1;

            TwoHandLeftSlot.weaponHolder = playerRigging.weaponHolder2HLeft;
            TwoHandLeftSlot.oneHand = false;
            TwoHandLeftSlot.prefabNr = -1;
            OneHandLeftSlot.id = 2;

            TwoHandRightSlot.weaponHolder = playerRigging.weaponHolder2HRight;
            TwoHandRightSlot.oneHand = false;
            TwoHandRightSlot.prefabNr = -1;
            OneHandLeftSlot.id = 3;

            if (IsServer)
            {
                OneHandLeftSlotPrefab.Value = -1;
                OneHandRightSlotPrefab.Value = -1;
                TwoHandLeftSlotPrefab.Value = -1;
                TwoHandRightSlotPrefab.Value = -1;
            }

        }

        /// <summary>
        /// Method to enable certain input actions. This method can be used to disable/enable some actions
        /// if a UI Interface is there, so that the character is not moving for example
        /// </summary>
        /// <param name="inputAction">Input Action set to enable</param>
        void EnableInputActions(string inputAction)
        {
            List<InputAction> actionSet = null;
            if (inputAction.Equals(NononZoneConstants.InputActions.REGULAR_INPUT_ACTION))
            {
                if (regularActionsEnabled) return;
                actionSet = regularActions;
                regularActionsEnabled = true;
                DisableInputActions(NononZoneConstants.InputActions.DIALOG_INPUT_ACTION);

            }
            else if (inputAction.Equals(NononZoneConstants.InputActions.DIALOG_INPUT_ACTION))
            {
                if (dialogActionsEnabled) return;
                actionSet = dialogActions;
                dialogActionsEnabled = true;
                DisableInputActions(NononZoneConstants.InputActions.REGULAR_INPUT_ACTION);
            }
            foreach (InputAction action in actionSet)
            {
                if (!action.enabled)
                {
                    action.Enable();
                }
            }
        }

        /// <summary>
        /// Method to disable certain input actions. This method can be used to disable/enable some actions
        /// if a UI Interface is there, so that the character is not moving for example
        /// </summary>
        /// <param name="inputAction">Input Action set to disable</param>
        void DisableInputActions(string inputAction)
        {
            List<InputAction> actionSet = null;
            if (inputAction.Equals(NononZoneConstants.InputActions.REGULAR_INPUT_ACTION))
            {
                if (!regularActionsEnabled) return;
                actionSet = regularActions;
                regularActionsEnabled = false;

            }
            else if (inputAction.Equals(NononZoneConstants.InputActions.DIALOG_INPUT_ACTION))
            {
                if (!dialogActionsEnabled) return;
                actionSet = dialogActions;
                dialogActionsEnabled = false;
            }
            foreach (InputAction action in actionSet)
            {
                if (action.enabled)
                {
                    action.Disable();
                }
            }
        }

        /// <summary>
        /// Returns the currently equipped and drawn weapon
        /// </summary>
        /// <returns>currently equipped and drawn weapon</returns>
        public int GetCurrentWeapon()
        {
            return weaponDrawn_nw.Value;
        }

        /// <summary>
        /// Sets the current equipped weapon. This method is used to setuo the player
        /// with a weapon. It must be added previously to the equipped weapons
        /// If you just call this, this will not equip the weapon.
        /// Use instead <seealso cref="DrawNextWeaponServerOrSinglePlayer"/>
        /// </summary>
        /// <param name="_currentWeapon">Current Weapon equipped and drawn</param>
        public void SetCurrentWeapon(int _currentWeapon)
        {
            if (IsServer) weaponDrawn_nw.Value = _currentWeapon;
        }

        public void SetWeapons(List<SerializableWeapon> weapons)
        {
            OneHandLeftSlot.weaponType = weapons[0].weaponType;
            OneHandLeftSlot.prefabNr = weapons[0].prefabNumber;
            OneHandLeftSlotPrefab.Value = weapons[0].prefabNumber;

            OneHandRightSlot.weaponType = weapons[1].weaponType;
            OneHandRightSlot.prefabNr = weapons[1].prefabNumber;
            OneHandRightSlotPrefab.Value = weapons[1].prefabNumber;

            TwoHandLeftSlot.weaponType = weapons[2].weaponType;
            TwoHandLeftSlot.prefabNr = weapons[2].prefabNumber;
            TwoHandLeftSlotPrefab.Value = weapons[2].prefabNumber;

            TwoHandRightSlot.weaponType = weapons[3].weaponType;
            TwoHandRightSlot.prefabNr = weapons[3].prefabNumber;
            TwoHandRightSlotPrefab.Value = weapons[3].prefabNumber;
        }

        public List<SerializableWeapon> GetWeaponsSlots()
        {
            List<SerializableWeapon> weaponList = new List<SerializableWeapon>();

            weaponList.Add(Convert2SerializeWeapon(OneHandLeftSlot));
            weaponList.Add(Convert2SerializeWeapon(OneHandRightSlot));
            weaponList.Add(Convert2SerializeWeapon(TwoHandLeftSlot));
            weaponList.Add(Convert2SerializeWeapon(TwoHandRightSlot));
            return weaponList;
        }

        private SerializableWeapon Convert2SerializeWeapon(WeaponSlot slot)
        {
            SerializableWeapon wpn = new SerializableWeapon();
            wpn.weaponType = slot.weaponType;
            wpn.prefabNumber = slot.prefabNr;
            return wpn;
        }

        /// <summary>
        /// Sets all mounts. This method is used to setup the player mounts from a persisted state
        /// </summary>
        /// <param name="_mounts">MountsInBag</param>
        public void SetMounts(List<int> _mounts)
        {
            amountOfMounts_nw.Value = _mounts.Count;
        }

        /// <summary>
        /// Method to retrieve a list of mounts
        /// </summary>
        /// <returns>List of mounts in bag</returns>
        public List<int> GetMountsInBag()
        {
            List<int> _mountsinBag = new List<int>();
            for (int i = 0; i < amountOfMounts_nw.Value; i++)
            {
                _mountsinBag.Add(i + 1);

            }
            return _mountsinBag;
        }

        public void SetCurrentMount(int _mnt)
        {
            if (IsServer) currentMount_nw.Value = _mnt;
        }

        public int GetCurrentMountIndex()
        {
            return currentMount_nw.Value;
        }

        Mount GetCurrentMount()
        {
            return (Mount)currentMount_nw.Value;
        }

        bool IsRunPressed()
        {
            return isRunPressed_nw.Value;
        }

        void SetIsRunPressed(bool _isRunPressed)
        {
            if (IsServer) isRunPressed_nw.Value = _isRunPressed;
        }

        /// <summary>
        /// InputAction method. This is called when run button is pressed or released
        /// </summary>
        /// <param name="context">Callback context</param>
        void OnRun(InputAction.CallbackContext context)
        {
            bool _isRunPressed = !IsRunPressed();

            if (IsClient && IsOwner)
            {
                OnRunServerRpc(_isRunPressed);
            }
        }

        bool HasLeftHitBoxAfterJump()
        {
            return hasLeftHitBoxAfterJump_nw.Value;
        }

        void SetHasLeftHitBoxAfterJump(bool _hasLeftHitBoxAfterJump)
        {
            if (IsServer) hasLeftHitBoxAfterJump_nw.Value = _hasLeftHitBoxAfterJump;
        }

        /// <summary>
        /// Server Method for the On Run Input Action. Sync Method for isRunPressed
        /// </summary>
        /// <param name="_isRunPressed">is run pressed</param>
        [ServerRpc]
        void OnRunServerRpc(bool _isRunPressed)
        {
            SetIsRunPressed(_isRunPressed);
        }

        /// <summary>
        /// Callback method when enabling/disabling mounting up is called
        /// </summary>
        /// <param name="context">Callback context</param>
        void OnEnableDisableFlying(InputAction.CallbackContext context)
        {
            if (IsClient && IsOwner)
            {
                OnEnableDisableFlyingServerRpc();
            }
        }

        /// <summary>
        /// Server Method for the mount up. Sync Method
        /// </summary>
        [ServerRpc]
        void OnEnableDisableFlyingServerRpc()
        {
            SetNextMountActive();
        }

        /// <summary>
        /// Sets the next mount in the mount list as active and therefore is mouting up.
        /// If current mount is 0, no mount is choosen. This is setup with adding the <seealso cref="Mount.NONE"/> in <seealso cref="Awake"/>
        /// </summary>
        void SetNextMountActive()
        {
            SetCurrentMount(GetCurrentMountIndex() + 1);
            if (GetCurrentMountIndex() > GetAmountOfMountsInBag())
            {
                SetCurrentMount(0);
            }
        }

        /// <summary>
        /// Callback Method when equip next weapon is pressed. 
        /// Remember: This methoid is not directly synchronized to the server as a variable. 
        /// The server part is called in here <seealso cref="DrawWeapon(WeaponSlot, int)"/>
        /// </summary>
        /// <param name="context">Callback context</param>
        void OnEquipNextWeapon(InputAction.CallbackContext context)
        {
            if (IsClient && IsOwner) DrawNextWeaponServerRpc();
        }

        [ClientRpc]
        private void DrawNextWeaponClientRpc(int lastWeapon, int nextWeapon)
        {
            if (!IsHost) DrawNextWeaponClient(lastWeapon, nextWeapon);
        }

        [ServerRpc]
        private void DrawNextWeaponServerRpc()
        {
            int lastWeapon = DrawNextWeaponServerOrSinglePlayer();
            if (lastWeapon > -1 || GetCurrentWeapon() > -1)
            {
                DrawNextWeaponClientRpc(lastWeapon, GetCurrentWeapon());
            }
        }

        /// <summary>
        /// Callback method when jump button is pressed
        /// </summary>
        /// <param name="context">Callback context</param>
        void OnJump(InputAction.CallbackContext context)
        {
            bool _isJumpPressed = context.ReadValueAsButton();

            if (IsClient && IsOwner)
            {
                OnJumpServerRpc(_isJumpPressed);
            }
        }

        /// <summary>
        /// Server method for jumping. Sync method
        /// </summary>
        /// <param name="_isJumpPressed">player is jumping</param>
        [ServerRpc]
        void OnJumpServerRpc(bool _isJumpPressed)
        {
            isJumpPressed_nw.Value = _isJumpPressed;
        }

        bool IsJumpPressed()
        {
            return isJumpPressed_nw.Value;
        }

        /// <summary>
        /// Callback method if while swimming or flying, the player is decending
        /// </summary>
        /// <param name="context">Callback context</param>
        void OnDecend(InputAction.CallbackContext context)
        {
            bool _isDivePressed = context.ReadValueAsButton();

            if (IsClient && IsOwner)
            {
                OnDecendServerRpc(_isDivePressed);
            }
        }

        bool IsDivePressed()
        {
            return isDivePressed_nw.Value;
        }

        void SetIsDivePressed(bool _isDivePressed)
        {
            if (IsServer) isDivePressed_nw.Value = _isDivePressed;
        }

        /// <summary>
        /// Server method for decending. Sync method
        /// </summary>
        /// <param name="_isDivePressed">if the player is decending</param>
        [ServerRpc]
        void OnDecendServerRpc(bool _isDivePressed)
        {
            SetIsDivePressed(_isDivePressed);
        }

        /// <summary>
        /// Callback method when aiming button is pressed
        /// Remember: This method is not synced to the server. The aiming method is called here <seealso cref="HandleAimingAndShooting"/>
        /// </summary>
        /// <param name="context">Callback context</param>
        void OnAimAndShoot(InputAction.CallbackContext context)
        {
            bool _isAimingPressed = context.ReadValueAsButton();

            if (IsClient && IsOwner)
            {
                OnAimPressedServerRpc(_isAimingPressed);
            }
        }

        [ServerRpc]
        void OnAimPressedServerRpc(bool _isAimingPressed)
        {
            SetIsAimingPressed(_isAimingPressed);
        }

        bool IsAimingPressed()
        {
            return isAimingPressed_nw.Value;
        }

        void SetIsAimingPressed(bool _isAimingPressed)
        {
            if (IsServer) isAimingPressed_nw.Value = _isAimingPressed;
        }

        bool IsAiming()
        {
            return isAiming_nw.Value;
        }

        void SetIsAiming(bool _isAiming)
        {
            if (IsServer) isAiming_nw.Value = _isAiming;
        }

        /// <summary>
        /// Callback method id strive left is pressed
        /// </summary>
        /// <param name="context">Callback context</param>
        void OnStriveLeft(InputAction.CallbackContext context)
        {
            bool _isStriveLeftPressed = context.ReadValueAsButton();
            Vector3 _currentStriveMovement = GetCurrentStriveMovement();
            bool _isStriveReleased = IsStriveReleased();

            if (!_isStriveLeftPressed)
            {
                _isStriveReleased = true;
                _currentStriveMovement = Vector3.zero;
            }

            if (IsClient && IsOwner)
            {
                OnStriveLeftServerRpc(_currentStriveMovement, _isStriveReleased, _isStriveLeftPressed);
            }
        }

        /// <summary>
        /// Server method for striving left. Sync method
        /// </summary>
        /// <param name="_currentStriveMovement">the movement vector</param>
        /// <param name="_isStriveReleased">if strive has been released</param>
        /// <param name="_isStriveLeftPressed">if strive is pressed or not</param>
        [ServerRpc]
        void OnStriveLeftServerRpc(Vector3 _currentStriveMovement, bool _isStriveReleased, bool _isStriveLeftPressed)
        {
            SetCurrentStriveMovement(_currentStriveMovement);
            SetIsStriveReleased(_isStriveReleased);
            isStriveLeftPressed_nw.Value = _isStriveLeftPressed;
        }

        void SetCurrentStriveMovement(Vector3 _currentStriveMovement)
        {
            if (IsServer) currentStriveMovement_nw.Value = _currentStriveMovement;
        }

        bool IsStriveReleased()
        {
            return isStriveReleased_nw.Value;
        }

        void SetIsStriveReleased(bool _isStriveReleased)
        {
            if (IsServer) isStriveReleased_nw.Value = _isStriveReleased;
        }

        bool IsStriveLeftPressed()
        {
            return isStriveLeftPressed_nw.Value;
        }

        private Vector3 GetCurrentStriveMovement()
        {
            return currentStriveMovement_nw.Value;
        }

        /// <summary>
        /// Callback method id strive right is pressed
        /// </summary>
        /// <param name="context">Callback context</param>
        void OnStriveRight(InputAction.CallbackContext context)
        {
            bool _isStriveRightPressed = context.ReadValueAsButton();
            Vector3 _currentStriveMovement = GetCurrentStriveMovement();
            bool _isStriveReleased = IsStriveReleased();

            if (!_isStriveRightPressed)
            {
                _isStriveReleased = true;
                _currentStriveMovement = Vector3.zero;
            }
            if (IsClient && IsOwner)
            {
                OnStriveRightServerRpc(_currentStriveMovement, _isStriveReleased, _isStriveRightPressed);
            }
        }

        /// <summary>
        /// Server method for striving right. Sync method
        /// </summary>
        /// <param name="_currentStriveMovement">current movement vector</param>
        /// <param name="_isStriveReleased">if strive has been released</param>
        /// <param name="_isStriveRightPressed">if strive is pressed or not</param>
        [ServerRpc]
        void OnStriveRightServerRpc(Vector3 _currentStriveMovement, bool _isStriveReleased, bool _isStriveRightPressed)
        {
            SetCurrentStriveMovement(_currentStriveMovement);
            SetIsStriveReleased(_isStriveReleased);
            isStriveRightPressed_nw.Value = _isStriveRightPressed;
        }

        bool IsStriveRightPressed()
        {
            return isStriveRightPressed_nw.Value;
        }

        /// <summary>
        /// Callback Method when the camera mvoe is enabled (e.g. left mouse btn)
        /// </summary>
        /// <param name="context">Callback context</param>
        void OnEnableMoveCameraInput(InputAction.CallbackContext context)
        {
            bool _isCameraMovePressed = context.ReadValueAsButton();

            if (IsClient && IsOwner)
            {
                OnEnableMoveCameraInputServerRpc(_isCameraMovePressed);
            }
        }

        /// <summary>
        /// Server method for sync of camera move state (e.g. left mouse btn)
        /// </summary>
        /// <param name="_isCameraMovePressed">if camera move is enabled</param>
        [ServerRpc]
        void OnEnableMoveCameraInputServerRpc(bool _isCameraMovePressed)
        {
            SetIsCameraMovePressed(_isCameraMovePressed);
        }

        bool IsCameraMovePressed()
        {
            return isCameraMovePressed_nw.Value;
        }

        void SetIsCameraMovePressed(bool _isCameraMovePressed)
        {
            if (IsServer) isCameraMovePressed_nw.Value = _isCameraMovePressed;
        }

        /// <summary>
        /// Callback method if rotation if the player is enabled
        /// </summary>
        /// <param name="context">Callback context</param>
        void OnEnableRotationAndMoveCameraInput(InputAction.CallbackContext context)
        {
            bool _isCharacterRotatePressed = context.ReadValueAsButton();

            if (IsClient && IsOwner)
            {
                OnEnableRotationAndMoveCameraInputServerRpc(_isCharacterRotatePressed);
            }
        }

        /// <summary>
        /// Server method for sync of player rotatoion enabled or not (e.g. right mouse btn)
        /// </summary>
        /// <param name="_isCharacterRotatePressed">player rotation enabled or not</param>
        [ServerRpc]
        void OnEnableRotationAndMoveCameraInputServerRpc(bool _isCharacterRotatePressed)
        {
            SetIsCharacterRotatePressed(_isCharacterRotatePressed);
        }

        bool IsCharacterRotatePressed()
        {
            return isCharacterRotatePressed_nw.Value;
        }

        void SetIsCharacterRotatePressed(bool _isCharacterRotatePressed)
        {
            if (IsServer) isCharacterRotatePressed_nw.Value = _isCharacterRotatePressed;
        }


        /// <summary>
        /// Callback Method if movement btns are pressed (e.g. wsad)
        /// </summary>
        /// <param name="context">Callback Context</param>
        void OnMovementInput(InputAction.CallbackContext context)
        {
            Vector3 currentMovementInput = context.ReadValue<Vector2>();
            Vector3 _currentWalkMovement = Vector3.zero;
            Vector3 _currentRunMovement = Vector3.zero;
            bool _isMovementPressed = false;
            _currentWalkMovement.x = currentMovementInput.x * GetWalkMultiplier();
            _currentWalkMovement.z = currentMovementInput.y * GetWalkMultiplier();
            _currentRunMovement.x = currentMovementInput.x * GetRunMultiplier();
            _currentRunMovement.z = currentMovementInput.y * GetRunMultiplier();
            _isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;

            if (IsClient && IsOwner)
            {
                OnMovementInputServerRpc(_currentWalkMovement, _currentRunMovement, _isMovementPressed);
            }
        }

        /// <summary>
        /// Server Method when movement buttons are pressed. Sync method
        /// </summary>
        /// <param name="_currentWalkMovement">Current walk movement vector</param>
        /// <param name="_currentRunMovement">Current run movement vector</param>
        /// <param name="_isMovementPressed">is a movement btn pressed</param>
        [ServerRpc]
        void OnMovementInputServerRpc(Vector3 _currentWalkMovement, Vector3 _currentRunMovement, bool _isMovementPressed)
        {
            SetCurrentWalkMovement(_currentWalkMovement);
            SetCurrentRunMovement(_currentRunMovement);
            SetIsMovementPressed(_isMovementPressed);
        }

        Vector3 GetCurrentWalkMovement()
        {
            return currentWalkMovement_nw.Value;
        }

        void SetIsMovementPressed(bool _isMovementPressed)
        {
            if (IsServer) isMovementPressed_nw.Value = _isMovementPressed;
        }

        void SetCurrentWalkMovement(Vector3 _currentWalkMovement)
        {
            currentWalkMovement_nw.Value = _currentWalkMovement;
        }

        void SetCurrentRunMovement(Vector3 _currentRunMovement)
        {
            if (IsServer) currentRunMovement_nw.Value = _currentRunMovement;
        }

        Vector3 GetCurrentRunMovement()
        {
            return currentRunMovement_nw.Value;
        }

        /// <summary>
        /// Instantiates dust particles where the player has been standing. If there is no particle system, nothing is done
        /// </summary>
        void CreateDust()
        {
            if (IsServer) CreateDustClientRpc();
        }

        [ClientRpc]
        void CreateDustClientRpc()
        {
            CreateDustClient();
        }

        private void CreateDustClient()
        {
            // don't show it the whole time, that does not look good
            if (Time.time - dustLastTimePlayed > dustPlayInterval)
            {
                GetComponent<PlayerSound>().Jump();
                dustLastTimePlayed = Time.time;

                if (dustParticles != null)
                {
                    GameObject go = Instantiate(dustParticles.gameObject, transform.position, Quaternion.identity);
                    Destroy(go, 1f);
                }
            }
        }

        Bounds GetWaterColliderBounds()
        {
            return new Bounds(waterColliderBounds_nw_center.Value, waterColliderBounds_nw_extents.Value);
        }

        void SetWaterColliderBounds(Bounds bounds)
        {
            if (IsServer)
            {
                waterColliderBounds_nw_center.Value = bounds.center;
                waterColliderBounds_nw_extents.Value = bounds.extents;
            }
        }

        /// <summary>
        /// Instantiates water splash particles where the player is at the top of the water collider. If there is no particle system or water collider, nothing is done
        /// </summary>
        void CreateWaterSplash()
        {
            if (IsServer) CreateWaterSplashClientRpc();
        }

        [ClientRpc]
        void CreateWaterSplashClientRpc()
        {
            CreateWaterSplashClient();
        }

        private void CreateWaterSplashClient()
        {
            if (waterParticles != null && GetWaterColliderBounds() != null)
            {
                GetComponent<PlayerSound>().Splash();
                float y = GetWaterColliderBounds().center.y + GetWaterColliderBounds().extents.y + 0.5f;
                GameObject go = Instantiate(waterParticles.gameObject, new Vector3(transform.position.x, GetWaterColliderBounds().center.y + GetWaterColliderBounds().extents.y, transform.position.z), Quaternion.identity);
                Destroy(go, 1.5f);
            }
        }

        public bool IsFlying()
        {
            return isFlying_nw.Value;
        }

        void SetIsFlying(bool _isFlying)
        {
            if (IsServer) isFlying_nw.Value = _isFlying;
        }

        public bool IsSwimming()
        {
            return isSwimming_nw.Value;
        }

        void SetIsSwimming(bool _isSwimming)
        {
            if (IsServer) isSwimming_nw.Value = _isSwimming;
        }

        /// <summary>
        /// Gets back the "walk" multiplier for the walking/swimming/flying
        /// </summary>
        /// <returns>walking multiplier</returns>
        float GetWalkMultiplier()
        {
            if (IsSwimming())
            {
                return swimWalkMultiplier;
            }
            else if (IsFlying())
            {
                return flyWalkMultiplier;
            }
            else
            {
                return walkMultiplier;
            }
        }

        /// <summary>
        /// Gets back the "run" multiplier for the running/swimming/flying
        /// </summary>
        /// <returns>running multiplier</returns>
        float GetRunMultiplier()
        {
            if (IsSwimming())
            {
                return swimRunMultiplier;
            }
            else if (IsFlying())
            {
                return flyRunMultiplier;
            }
            else
            {
                return runMultiplier;
            }
        }

        /// <summary>
        /// Gets back the dive or rise multiplier for flying and swimming
        /// </summary>
        /// <returns>dive or rise multiplier</returns>
        float GetDiveOrRaiseMultiplier()
        {
            if (IsSwimming())
            {
                return diveAndRaise_Water_Multiplier;

            }
            else if (IsFlying())
            {
                return diveAndRaise_Air_Multiplier;

            }
            else
            {
                return diveAndRaise_Air_Multiplier;
            }

        }

        /// <summary>
        /// Check if the character is in a certain distance to the ground. The checking will be done by SphereCast in size of CapsuleCollider Radius.
        /// The layer <seealso cref="NononZoneConstants.Player.IGNORE_ISGROUNDED_LAYER_NAME"/> and the NononZoneObject <seealso cref="NononZoneObjectNetwork.TypeEnum.PLAYER"/>is ignored in this process
        /// </summary>
        /// <param name="m_GroundCheckDistance">checking distance</param>
        /// <returns>true if character is in distance</returns>
        bool IsInDistance(float m_GroundCheckDistance)
        {
            float radius = GetComponent<CapsuleCollider>().radius;
            RaycastHit hit;
            if (Physics.SphereCast(transform.position + (Vector3.up * radius), radius, Vector3.down, out hit, m_GroundCheckDistance, ~LayerMask.GetMask(NononZoneConstants.Player.IGNORE_ISGROUNDED_LAYER_NAME)))
            {
                if (!NononZoneObjectNetwork.isOneOfTypes(hit.transform, INononZoneObject.NononZoneObjType.PLAYER))
                {
                    return true;
                }

            }
            return false;
        }

        /// <summary>
        /// Check if the character is on the ground or is swimming. This is also valid for plattforms etc.
        /// The process is done by SphereCast and with the CapsuleCollider radius.
        /// </summary>
        /// <returns>true if character is on ground</returns>
        bool IsGroundedOrSwimming()
        {
            float m_GroundCheckDistance = GetComponent<CapsuleCollider>().radius;
            ContactPoint groundCP = default(ContactPoint);
            bool grounded = FindGround(out groundCP, contactPoints);
            if (grounded || IsSwimming())
            {
                grounded = true;
            }

            return grounded;
        }

        bool IsInWater()
        {
            return isInWater_nw.Value;
        }

        void SetIsInWater(bool _isInWater)
        {
            if (IsServer) isInWater_nw.Value = _isInWater;
        }

        /// <summary>
        /// Check if the player is falling, meaning reaching a certain gravity.
        /// This happens after a jump or when the player really falls down.
        /// This is steered by <seealso cref="fallingGravityTreshhold"/>
        /// </summary>
        /// <returns>true if the character is falling down</returns>
        bool IsFalling()
        {
            bool isFalling = IsGravityHigherThan(fallingGravityTreshhold) && !IsJumping() && !IsInWater() && !IsFlying();
            return isFalling;
        }

        bool IsJumping()
        {
            return isJumping_nw.Value;
        }

        void SetIsJumping(bool _isJumping)
        {
            if (IsServer) isJumping_nw.Value = _isJumping;
        }

        bool IsJumpReleasedAfterJump()
        {
            return isJumpReleasedAfterJump_nw.Value;
        }

        void SetIsJumpReleasedAfterJump(bool _isJumpReleasedAfterJump)
        {
            if (IsServer) isJumpReleasedAfterJump_nw.Value = _isJumpReleasedAfterJump;
        }

        float GetForwardForceOnJump()
        {
            return forwardForceOnJump_nw.Value;
        }

        void SetForwardForceOnJump(float _forwardForceOnJump)
        {
            if (IsServer) forwardForceOnJump_nw.Value = _forwardForceOnJump;
        }

        /// <summary>
        /// Adding jump force to the player. This is steered by <seealso cref="jumpForce"/> amd <seealso cref="swimmingJumpMultiplier"/> if swimming
        /// </summary>
        void AddJumpForce()
        {
            SetIsJumping(true);
            SetIsJumpReleasedAfterJump(false);

            float mulitplier = 1f;
            if (IsSwimming()) mulitplier *= swimmingJumpMultiplier;
            if (collidedTransform != null)
            {
                // if the collided rigidbody has a downward movement increase jump force
                Rigidbody collidedRigidBody = collidedTransform.GetComponent<Rigidbody>();
                if (collidedRigidBody != null && collidedRigidBody.velocity.y < 0)
                {
                    mulitplier *= collidedTransformJumpMultiplier;
                }
            }
            jumpForceDirection.Value = new Vector3(0, jumpForce * mulitplier, 0);
            SetForwardForceOnJump(GetForwardMovement());
        }

        bool IsFlyingEnabled()
        {
            return flyingEnabled_nw.Value;
        }

        void SetIsFlyingEnabled(bool _isFlyingEnabled)
        {
            if (IsServer) flyingEnabled_nw.Value = _isFlyingEnabled;
        }

        /// <summary>
        /// Handles the mount changing. It is picking up <seealso cref="currentMount"/> and enables the flying.
        /// </summary>
        void HandleMountChange()
        {
            Mount theCurrentMount = GetCurrentMount();

            if (!IsFlyingEnabled() && theCurrentMount.Equals(Mount.THRUSTER) && instantiatedThruster == null)
            {
                ActivateDeactivateThruster(true);
            }
            else if (IsFlyingEnabled() && !theCurrentMount.Equals(Mount.THRUSTER) && instantiatedThruster != null)
            {
                ActivateDeactivateThruster(false);
            }
        }

        private void ActivateDeactivateThruster(bool activate)
        {
            if (activate)
            {
                if (IsClient && IsOwner) SetIsFlyingEnabledServerRpc(true);
                //float characterHalfHeight = characterController.height / 2;
                float characterHalfHeight = GetComponent<Collider>().bounds.size.y / 2;
                instantiatedThruster = Instantiate(thrusterParticles, new Vector3(transform.position.x + 0.012f, transform.position.y + characterHalfHeight, transform.position.z + 0.062f), Quaternion.identity, transform).transform.gameObject;
                GetComponent<PlayerSound>().StartFlySound();
            }
            else
            {
                if (IsClient && IsOwner) SetIsFlyingEnabledServerRpc(false);
                DestroyImmediate(instantiatedThruster);
                GetComponent<PlayerSound>().StopFlySound();
            }

        }

        [ServerRpc]
        void SetIsFlyingEnabledServerRpc(bool _flyingEnabled)
        {
            SetIsFlyingEnabled(_flyingEnabled);
        }

        bool IsSlidingDown()
        {
            return isSlidingDown_nw.Value;
        }

        void SetIsSlidingDown(bool _isSlidingDown)
        {
            if (IsServer) isSlidingDown_nw.Value = _isSlidingDown;
        }

        float GetCurrentSlopeAngle()
        {
            return currentSlopeAngle_nw.Value;
        }

        void SetCurrentSloapAngle(float _currentSlopeAngle)
        {
            if (IsServer) currentSlopeAngle_nw.Value = _currentSlopeAngle;
        }

        /// <summary>
        /// Handles the sliding down slope. This is steered with <seealso cref="slopeForceRayLength"/>, <seealso cref="slopeLimit"/> and <seealso cref="slidingDownForce"/>
        /// </summary>
        void HandleSlope()
        {
            if (IsJumping()) return;

            SetIsSlidingDown(false);

            if (!IsSwimming() && !IsFlying())
            {
                CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();

                groundDirection.LookAt(transform.forward);
                fallDirection.rotation = transform.rotation;
                ContactPoint groundCP = default(ContactPoint);
                if (FindGround(out groundCP, contactPoints))
                {
                    if (groundCP.normal != Vector3.up)
                    {
                        // prevent the slope jumping when running down
                        SetCurrentSloapAngle(Vector3.Angle(transform.up, groundCP.normal));
                        float forwardAngle = Vector3.Angle(transform.forward, groundCP.normal) - 90;
                        if (forwardAngle < 0 && GetCurrentSlopeAngle() <= slopeLimit)
                        {
                            groundDirection.eulerAngles += new Vector3(-forwardAngle, 0, 0);
                        }
                        else if (GetCurrentSlopeAngle() > slopeLimit && GetCurrentSlopeAngle() < 89)
                        {
                            Vector3 groundCross = Vector3.Cross(groundCP.normal, Vector3.up);
                            fallDirection.rotation = Quaternion.FromToRotation(transform.up, Vector3.Cross(groundCross, groundCP.normal));
                            float slideModifier = (slidingDownForce / 90f) * GetCurrentSlopeAngle();
                            if (slideModifier < gravity * -1)
                            {
                                slideModifier = gravity * -1;
                            }
                            slideDownForceDirection.Value = fallDirection.up * -1 * slideModifier;
                            Debug.DrawRay(transform.position, slideDownForceDirection.Value, Color.red, 1f);
                            SetIsSlidingDown(true);
                            CreateDust();
                        }
                    }
                    else
                    {
                        slideDownForceDirection.Value = Vector3.zero;
                        SetCurrentSloapAngle(0);
                        SetIsSlidingDown(false);
                    }
                }
            }
            else
            {
                SetCurrentSloapAngle(0);
            }
        }


        float GetAirTimeJumping()
        {
            return airTimeJumping_nw.Value;
        }

        void SetAirTimeJumping(float _airTimeJumping)
        {
            if (IsServer) airTimeJumping_nw.Value = _airTimeJumping;
        }

        /***
         * Doing the Jump Handling
         * Adding vertical velocity and checking if character is back on ground.
         * The duration of the Jump is handled in HandleGravity()
         */
        /// <summary>
        /// Handles the jumping. This is steered by <seealso cref="jumpForce"/>, <seealso cref="stickingAirTimeTreshhold"/> and <seealso cref="outOfHitboxDistance"/>
        /// </summary>
        void HandleJump()
        {
            // just jump when character is on the ground and he relased the jump button
            if (IsJumpPressed() && IsJumpReleasedAfterJump() &&
                !IsJumping() && IsGroundedOrSwimming() &&
                !IsFalling() && !IsSwimming() && !IsFlying())
            {
                AddJumpForce();
                CreateDust();
            }

            // if the player has not been able to jump, reset the variables if this amount of time has been passed
            if (IsJumping() && !HasLeftHitBoxAfterJump() && GetAirTimeJumping() > stickingAirTimeTreshhold)
            {
                SetHasLeftHitBoxAfterJump(true);
            }

            // Check if player has left hitbox (meaning he is airborne)
            if (IsJumping() && collidedTransform == null && !IsInDistance(outOfHitboxDistance))
            {
                SetHasLeftHitBoxAfterJump(true);
            }

            // Let him fly if flying is enabled
            if (IsJumping() && HasLeftHitBoxAfterJump() && IsFlyingEnabled())
            {
                SetIsJumping(false);
                SetIsFlying(true);
                SetHasLeftHitBoxAfterJump(true);
            }

            // jump button is release
            if (!IsJumpPressed())
            {
                SetIsJumpReleasedAfterJump(true);
            }

            // Add more force when the button is still pressed until a velocity limit
            if (IsJumping() && HasLeftHitBoxAfterJump() &&
                IsJumpPressed() && !IsGroundedOrSwimming() &&
                !IsFalling() && !IsFlying() && !IsSwimming() &&
                rigidBody.velocity.y > 0
                )
            {
                jumpForceDirection.Value += new Vector3(0, addingForceJumpPressed, 0);
            }

            // if the player landed on ground or in water
            if (IsGroundedOrSwimming() && HasLeftHitBoxAfterJump())
            {
                SetIsJumping(false);
                SetIsFlying(false);
                SetHasLeftHitBoxAfterJump(false);

                if (IsSwimming())
                {
                    CreateWaterSplash();
                }
                else
                {
                    CreateDust();
                }
            }


        }

        Vector3 GetCameraEulerAngles()
        {
            return cameraEulerAngles_nw.Value;
        }

        void SetCameraEulerAngles(Vector3 _eulerAngles)
        {
            if (IsServer) cameraEulerAngles_nw.Value = _eulerAngles;
        }

        Vector3 GetCameraAimPointPosition()
        {
            return cameraAimPointPosition_nw.Value;
        }

        void SetCameraAimPointPosition(Vector3 _aimPointPosition)
        {
            if (IsServer) cameraAimPointPosition_nw.Value = _aimPointPosition;
        }

        Quaternion GetCameraAimPointRotation()
        {
            return cameraAimPointRotation_nw.Value;
        }

        void SetCameraAimPointRotation(Quaternion _aimPointRotation)
        {
            if (IsServer) cameraAimPointRotation_nw.Value = _aimPointRotation;
        }

        Vector3 GetCameraPosition()
        {
            return cameraPosition_nw.Value;
        }

        void SetCameraPosition(Vector3 _aimPointPosition)
        {
            if (IsServer) cameraPosition_nw.Value = _aimPointPosition;
        }

        /***
         * Rotates the character when using keys, strive keys/buttons or using mouse
         * 
         * */
        /// <summary>
        /// Handles all the rotation of the character. This is steered by <seealso cref="rotationMultiplier"/>, <seealso cref="rotationMouseDumper"/>
        /// Remember: In Multiplayer, this method is only called on the server.
        /// </summary>
        void HandleRotation()
        {
            // move forward and rotate with camera
            if (IsCharacterRotatePressed() && IsCameraMovePressed() && !IsStriveRightPressed() && !IsStriveLeftPressed() && !IsMovingHorizontal())
            {
                if (IsSwimming() || IsFlying())
                {
                    transform.rotation = Quaternion.Euler(GetCameraEulerAngles().x, GetCameraEulerAngles().y, 0);
                }
                else
                {
                    transform.rotation = Quaternion.Euler(0, GetCameraEulerAngles().y, 0);
                }

            }

            // rotate character
            if (IsCharacterRotatePressed() && !IsCameraMovePressed() && !(IsStriveLeftPressed() || IsStriveRightPressed()) && !IsMovingHorizontal())
            {
                if (IsSwimming() || IsFlying())
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(GetCameraEulerAngles().x, GetCameraEulerAngles().y, 0), rotationMouseDumper);
                }
                else
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, GetCameraEulerAngles().y, 0), rotationMouseDumper);
                }

            }

            if (!IsCameraMovePressed() && !IsCharacterRotatePressed() && !IsStriveLeftPressed() && !IsStriveRightPressed())
            {
                // make Player upright again
                Quaternion q = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
                transform.rotation = Quaternion.Slerp(transform.rotation, q, 1f);

                // rotate the character
                Vector3 rotateDirection;
                rotateDirection = new Vector3(0, GetCurrentWalkMovement().x * rotationMultiplier, 0);
                transform.Rotate(rotateDirection);
            }


            if (IsStriveLeftPressed() || IsStriveRightPressed() || (IsMovingHorizontal() && IsCharacterRotatePressed()))
            {
                float forwardMovement = GetForwardMovement();
                if (forwardMovement >= 0)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, GetCameraEulerAngles().y + GetStriveDirectionDegrees(), 0), rotationMouseDumper);
                }
                else
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, GetCameraEulerAngles().y + GetStriveDirectionDegrees() * -1f, 0), rotationMouseDumper);
                }
            }

            if (IsStriveReleased())
            {
                transform.rotation = Quaternion.Euler(0, GetCameraEulerAngles().y - GetStriveDirectionDegrees(), 0);
                SetIsStriveReleased(false);
            }

            if (IsAimingPressed() && IsCameraMovePressed() ||
                (IsAiming() && IsCameraMovePressed()))
            {
                float maxAngle = 60;
                Vector3 targetDir = GetCameraAimPointPosition() - transform.position;
                Vector3 forward = transform.forward;
                forward.y = 0;
                targetDir.y = 0;
                float anglex = Vector3.SignedAngle(targetDir, forward, Vector3.up);

                if (anglex >= maxAngle)
                {
                    anglex = anglex - maxAngle;
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, GetCameraEulerAngles().y + anglex, 0), 0.1f);

                }
                else if (anglex <= -maxAngle)
                {
                    anglex = anglex + maxAngle;
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, GetCameraEulerAngles().y + anglex, 0), 0.1f);
                }
            }
        }

        /// <summary>
        /// Checks if a movment button horizontal has been pressed
        /// </summary>
        /// <returns>true if the player is moving horizonal</returns>
        bool IsMovingHorizontal()
        {
            bool hasHorizontalMovement = (GetCurrentRunMovement().x < 0 || GetCurrentWalkMovement().x < 0 || GetCurrentRunMovement().x > 0 || GetCurrentWalkMovement().x > 0);
            return hasHorizontalMovement;
        }

        bool IsMovingVertical()
        {
            bool verticalMovement = (GetCurrentRunMovement().z < 0 || GetCurrentWalkMovement().z < 0 || GetCurrentRunMovement().z > 0 || GetCurrentWalkMovement().z > 0);
            return verticalMovement;
        }

        /// <summary>
        /// Returns the current forward movement value
        /// </summary>
        /// <returns>current forward movement value</returns>
        float GetForwardMovement()
        {
            if (GetCurrentWalkMovement().z < 0 || GetCurrentRunMovement().z < 0 || GetCurrentWalkMovement().z > 0 || GetCurrentRunMovement().z > 0)
            {
                if (IsRunPressed())
                {
                    return GetCurrentRunMovement().z;
                }
                else
                {
                    return GetCurrentWalkMovement().z;
                }
            }
            if (IsCharacterRotatePressed() && IsCameraMovePressed())
            {
                if (IsRunPressed())
                {
                    return forwardMouseSpeed * GetRunMultiplier();
                }
                else
                {
                    return forwardMouseSpeed * GetWalkMultiplier();
                }

            }
            return 0.0f;
        }

        /// <summary>
        /// Returns the degrees that the player is currently striving forward or backward.
        /// This is steered by <seealso cref="strivedegrees"/>
        /// </summary>
        /// <returns>forward or backward striving degrees</returns>
        float GetStriveDirectionDegrees()
        {
            float striveDirection = 0f;
            if (GetCurrentWalkMovement().x > 0 || IsStriveRightPressed())
            {
                striveDirection = strivedegrees;
            }
            else if (GetCurrentWalkMovement().x < 0 || IsStriveLeftPressed())
            {
                striveDirection = -strivedegrees;
            }
            return striveDirection;
        }

        bool IsMovementPressed()
        {
            return isMovementPressed_nw.Value;
        }

        /// <summary>
        /// Handles all the animations by state
        /// </summary>
        void HandleAnimation()
        {
            bool movingForward = IsMovementPressed();
            if ((IsCameraMovePressed() && IsCharacterRotatePressed()) || IsStriveLeftPressed() || IsStriveRightPressed())
            {
                movingForward = true;
            }

            if (IsSwimming())
            {
                if (animator.GetBool(isSwimmingHash) == false)
                {
                    animator.SetBool(isSwimmingHash, true);
                }
            }
            else
            {
                if (animator.GetBool(isSwimmingHash) == true)
                {
                    animator.SetBool(isSwimmingHash, false);
                }
            }

            if (IsFlying())
            {
                if (animator.GetBool(isFlyingHash) == false)
                {
                    animator.SetBool(isFlyingHash, true);
                }

            }
            else
            {
                if (animator.GetBool(isFlyingHash) == true)
                {
                    animator.SetBool(isFlyingHash, false);
                }
            }

            if (IsFalling())
            {
                movingForward = false;
                if (animator.GetBool(isFallingHash) == false)
                {
                    animator.SetBool(isFallingHash, true);
                }
            }
            else
            {
                if (animator.GetBool(isFallingHash) == true)
                {
                    animator.SetBool(isFallingHash, false);
                }
            }

            if (IsJumpAnimating() && !IsJumping())
            {
                if (animator.GetBool(isJumpingHash) == true)
                {
                    animator.SetBool(isJumpingHash, false);
                    SetIsJumpAnimating(false);
                }
            }

            if (!IsJumpAnimating() && IsJumping())
            {
                if (animator.GetBool(isJumpingHash) == false)
                {
                    animator.SetBool(isJumpingHash, true);
                    SetIsJumpAnimating(true);
                }
            }

            // start walking if movement speed is true and not already walking
            if ((movingForward && !IsRunPressed())/* && !isWalking*/)
            {
                float speedPercent = 0.5f;
                if (GetCurrentWalkMovement().z < 0)
                {
                    speedPercent = -0.5f;
                }

                animator.SetBool(isWalkingHash, true);
                animator.SetBool(isRunningHash, false);
                animator.SetFloat(speedPercentHash, speedPercent);
            }
            // stop walking if isMovementPressed is false and not already walking
            else if (!movingForward && /*isWalking*/ !IsRunPressed())
            {
                animator.SetBool(isWalkingHash, false);
                animator.SetBool(isRunningHash, false);
                animator.SetFloat(speedPercentHash, 0.0f);
            }

            if ((movingForward && IsRunPressed()) /*&& !isRunning*/)
            {
                float speedPercent = 1.0f;
                if (GetCurrentRunMovement().z < 0)
                {
                    speedPercent = -1.0f;
                }

                animator.SetBool(isRunningHash, true);
                animator.SetBool(isWalkingHash, false);
                animator.SetFloat(speedPercentHash, speedPercent);
            }

            else if ((!movingForward && IsRunPressed()) /*&& isRunning*/)
            {

                animator.SetBool(isRunningHash, false);
                animator.SetBool(isWalkingHash, false);
                animator.SetFloat(speedPercentHash, 0.0f);
            }

        }

        bool IsJumpAnimating()
        {
            return isJumpAnimating_nw.Value;
        }

        void SetIsJumpAnimating(bool _isJumpAnimating)
        {
            if (IsServer) isJumpAnimating_nw.Value = _isJumpAnimating;
        }

        /// <summary>
        /// Checks if the current gravity is higher than a ref gravity. This is used e.g. for falling
        /// </summary>
        /// <param name="refGravity">reference gravitiy to check for</param>
        /// <returns>true if gravity is higher than ref value</returns>
        bool IsGravityHigherThan(float refGravity)
        {
            if (IsRunPressed())
            {
                return rigidBody.velocity.y < refGravity;
            }
            else
            {
                return rigidBody.velocity.y < refGravity;
            }
        }

        bool HasFallingStarted()
        {
            return hasFallingStarted_nw.Value;
        }

        void SetHasFallingStarted(bool _hasFallingStarted)
        {
            if (IsServer) hasFallingStarted_nw.Value = _hasFallingStarted;
        }

        /// <summary>
        /// Handles the gravity. This method is needed to add special gravity for falling down, swimming etc.
        /// </summary>
        void HandleGravity()
        {
            if (!IsSwimming() && !IsFlying())
            {
                // apply proper gravity depending on if the character is grounded or not
                if (IsGroundedOrSwimming() && !IsJumping())
                {
                    // normal gravity
                    if (HasFallingStarted())
                    {
                        if (IsInWater())
                        {
                            CreateWaterSplash();
                        }
                        else
                        {
                            CreateDust();
                        }
                        SetHasFallingStarted(false);
                        fallingDownForceDircetion.Value = Vector3.zero;
                    }
                }
                else if (IsGravityHigherThan(fallingGravityTreshhold) || !IsJumpPressed())
                {

                    if (!IsGroundedOrSwimming())
                    {
                        if (!HasFallingStarted()) SetHasFallingStarted(true);
                        // TODO: Character is already standing on plattform on the edge, he should fall down....
                        // TODO: this place can be used to grap the plattform
                        if (collidedTransform != null)
                        {
                            //Debug.Log("Should be falling down but is standing on top...");                                
                        }

                        // if the character is falling down enable flying, so he stops falling. But not when sliding down the slope
                        if (IsFlyingEnabled()/* && !isSlidingDown*/)
                        {
                            SetIsFlying(true);
                            SetHasLeftHitBoxAfterJump(true);
                        }
                    }
                }

                if (HasFallingStarted())
                {
                    float fallingDownDirectionY = fallingDownForceDircetion.Value.y;
                    float lastDownForce = fallingDownDirectionY += gravity * fallMultiplier * Time.deltaTime;
                    float nextYVelocity = Mathf.Min(lastDownForce, maxFallingGravity);
                    fallingDownForceDircetion.Value = new Vector3(fallingDownForceDircetion.Value.x, nextYVelocity, fallingDownForceDircetion.Value.z);
                }
            }
            else
            {
                if (HasFallingStarted())
                {
                    if (IsInWater())
                    {
                        CreateWaterSplash();
                    }
                    else
                    {
                        CreateDust();
                    }
                    SetHasFallingStarted(false);
                }

                SetCurrentWalkMovement(new Vector3(GetCurrentWalkMovement().x, 0, GetCurrentWalkMovement().z));
                SetCurrentRunMovement(new Vector3(GetCurrentRunMovement().x, 0, GetCurrentRunMovement().z));
            }

        }

        bool IsCameraUnderwater()
        {
            return isCameraUnderwater_nw.Value;
        }

        void SetIsCameraUnderwater(bool _isCameraUnderwater)
        {
            if (IsServer) isCameraUnderwater_nw.Value = _isCameraUnderwater;
        }

        /// <summary>
        /// Enables or disables fog based on if the camera is below a water collider
        /// </summary>
        void HandleFog()
        {
            if (IsClient && IsOwner) UpdateFog();
        }

        private void UpdateFog()
        {
            if (IsCameraUnderwater() && IsInWater())
            {
                RenderSettings.fogColor = underwaterColor;
                RenderSettings.fogDensity = 0.11f;
            }
            else
            {
                RenderSettings.fogColor = normalColor;
                RenderSettings.fogDensity = 0.005f;
            }
        }

        /// <summary>
        /// Handles the aiming point where the character is aiming at. This needs to be synced with the camera.
        /// The reason for this is, that the aiming point is in the rigging. This method is togehter with <seealso cref="HandleCameraAimPoint(Vector3, Quaternion)"/>
        /// </summary>
        void HandleCameraAimPointPositionAndRotation()
        {
            if (Camera.main != null)
            {
                UpdateCameraAimPoint();
                if (IsClient && IsOwner)
                {
                    // move the aiming target with the camera
                    Vector3 cameraAimPosition = cameraAimTarget.transform.position;
                    Quaternion cameraAimRotation = cameraAimTarget.transform.rotation;
                    // set the angle of the camera
                    Vector3 _cameraEulerAngles = Camera.main.transform.eulerAngles;
                    UpdateCameraAimPointRotationAndPositionServerRpc(Camera.main.transform.position, cameraAimPosition, _cameraEulerAngles, cameraAimRotation);
                }
            }

        }

        [ServerRpc]
        private void UpdateCameraAimPointRotationAndPositionServerRpc(Vector3 _cameraPosition, Vector3 _cameraAimPointPosition, Vector3 _cameraEulerAngles, Quaternion _cameraAimPointRotation)
        {
            SetCameraPosition(_cameraPosition);
            SetCameraEulerAngles(_cameraEulerAngles);
            SetCameraAimPointPosition(_cameraAimPointPosition);
            SetCameraAimPointRotation(_cameraAimPointRotation);
        }



        /// <summary>
        /// Handles the aiming in the riging part of the character
        /// </summary>
        void UpdateCameraAimPoint()
        {
            playerRigging.CameraAimTarget.position = GetCameraAimPointPosition();
            playerRigging.CameraAimTarget.rotation = GetCameraAimPointRotation();
        }

        float GetTimeStartAiming()
        {
            return timeStartAiming_nw.Value;
        }

        WeaponSlot GetCurrentWeaponSlot()
        {
            int currWeapon = GetCurrentWeapon();
            return GetWeaponSlot(currWeapon);
        }

        WeaponSlot GetWeaponSlot(int number)
        {
            switch (number)
            {
                case 0:
                    return OneHandLeftSlot;

                case 1:
                    return OneHandRightSlot;

                case 2:
                    return TwoHandLeftSlot;

                case 3:
                    return TwoHandRightSlot;

                default:
                    return null;
            }
        }

        void SetCameraIfWeaponHittingTarget()
        {
            Weapon wpn = GetCurrentWeaponSlot().weapon;
            RaycastHit hit;
            if (Physics.Raycast(wpn.raycastOrigin.position, GetCameraAimPointPosition() - wpn.raycastOrigin.position, out hit, wpn.distance))
            {
                if (NononZoneObjectNetwork.isOneOfTypes(hit.transform, INononZoneObject.NononZoneObjType.DESTROYABLE))
                {
                    playerCam.SetIsWeaponAimingAtTarget(true);
                }
                else
                {
                    playerCam.SetIsWeaponAimingAtTarget(false);
                }

            }
            else
            {
                playerCam.SetIsWeaponAimingAtTarget(false);
            }
        }

        /// <summary>
        /// Handles the aiming and shooting 
        /// </summary>
        void HandleAimingAndShooting()
        {
            // move the aiming target with the camera
            PlayerRigging playerRigging = GetComponent<PlayerRigging>();

            // When manual rigging is enabled we don't want to use the shoot functions
            PlayerRigging playerRiggging = GetComponent<PlayerRigging>();
            if (playerRiggging == null || !playerRigging.manualRigging)
            {
                if (GetCurrentWeapon() != -1)
                {
                    if (IsOwner) playerCam.SetIsWeaponDrawn(true);

                }

                if (!IsSwimming() && !IsFlying() && GetCurrentWeapon() != -1 && IsAimingPressed())
                {
                    Weapon wpn = GetCurrentWeaponSlot().weapon;
                    StartCoroutine(StartAimingAndFiringWithDelay(true, GetCameraAimPointPosition()));
                    if (IsClient && IsOwner)
                    {
                        SetCameraIfWeaponHittingTarget();
                        playerCam.SetIsWeaponDrawn(true);
                        StartAimingServerRpc(true, GetCameraAimPointPosition(), Time.deltaTime, Time.time);
                    }
                }
                else if (!IsSwimming() && !IsFlying() && GetCurrentWeapon() != -1 && !IsAimingPressed())
                {
                    Weapon wpn = GetCurrentWeaponSlot().weapon;
                    // delay the aim/hold cycle
                    if (Time.time - GetTimeStartAiming() > holdAimingDuration)
                    {
                        if (IsClient && IsOwner)
                        {
                            StartCoroutine(StartAimingAndFiringWithDelay(false, GetCameraAimPointPosition()));
                            SetCameraIfWeaponHittingTarget();
                            StartAimingServerRpc(false, GetCameraAimPointPosition(), Time.deltaTime, Time.time);
                        }
                    }
                    else
                    {
                        if (IsClient && IsOwner)
                        {
                            wpn.StopFiring();
                            StopFiringServerRpc();
                        }
                    }
                }
                else if (GetCurrentWeapon() == -1)
                {
                    if (IsOwner) playerCam.SetIsWeaponDrawn(false);

                    playerRigging.weaponAimingPoseInRig.parent.GetComponent<Rig>().weight = 0;
                    playerRigging.bodyAimRig.GetComponent<Rig>().weight = 0;
                    SetIsAiming(false);
                }
            }

        }

        /// <summary>
        /// Async Method to aim after the animation is done.
        /// This Method also starts Firing. This is steered by <seealso cref="aimDuration"/>
        /// </summary>
        /// <param name="aiming">true if aiming is active</param>
        /// <param name="aimingPoint">aiming point to fire to</param>
        /// <returns></returns>
        private IEnumerator StartAimingAndFiringWithDelay(bool aiming, Vector3 aimingPoint)
        {
            PlayerRigging playerRigging = GetComponent<PlayerRigging>();

            if (aiming)
            {
                while (playerRigging.weaponAimingPoseInRig.parent.GetComponent<Rig>().weight < 1)
                {
                    playerRigging.weaponAimingPoseInRig.parent.GetComponent<Rig>().weight += Time.deltaTime / aimDuration;
                    playerRigging.bodyAimRig.GetComponent<Rig>().weight += Time.deltaTime / aimDuration;
                    yield return null;
                }
                if (playerRigging.weaponAimingPoseInRig.parent.GetComponent<Rig>().weight >= 1)
                {
                    SetIsAiming(true);
                    Weapon wpn = GetCurrentWeaponSlot().weapon;
                    wpn.StartFiring(aimingPoint);
                    if (wpn.IsFiring())
                    {
                        wpn.UpdateFiring(Time.deltaTime, aimingPoint);
                    }
                    wpn.UpdateBullets(Time.deltaTime);
                }
            }
            else
            {
                while (playerRigging.weaponAimingPoseInRig.parent.GetComponent<Rig>().weight > 0)
                {
                    playerRigging.weaponAimingPoseInRig.parent.GetComponent<Rig>().weight -= Time.deltaTime / aimDuration;
                    playerRigging.bodyAimRig.GetComponent<Rig>().weight -= Time.deltaTime / aimDuration;
                    yield return null;
                }
                if (playerRigging.weaponAimingPoseInRig.parent.GetComponent<Rig>().weight <= 0)
                {
                    SetIsAiming(false);
                }
            }

        }

        private void StartStopFiringOnServer(bool firing, Vector3 aimingPoint, float deltaTime)
        {
            if (firing)
            {
                SetIsAiming(true);
                Weapon wpn = GetCurrentWeaponSlot().weapon;
                wpn.StartFiring(aimingPoint);
                if (wpn.IsFiring())
                {
                    wpn.UpdateFiring(deltaTime, aimingPoint);
                }
                wpn.UpdateBullets(deltaTime);
            }
            else
            {
                SetIsAiming(false);
            }
        }


        /// <summary>
        /// Handles the swimming of the character
        /// </summary>
        void HandleSwimming()
        {
            if (IsInWater())
            {
                if (GetCameraPosition().y < GetWaterColliderBounds().center.y + GetWaterColliderBounds().extents.y)
                {
                    SetIsCameraUnderwater(true);
                }
                else
                {
                    SetIsCameraUnderwater(false);
                }

                float colliderHeight = GetComponent<Collider>().bounds.size.y;
                float floatingDepht = GetWaterColliderBounds().center.y + GetWaterColliderBounds().extents.y - (colliderHeight / 3 * 2);
                if (transform.position.y < floatingDepht)
                {
                    if (GetCurrentWeapon() != -1)
                    {
                        ShedWeapon(GetCurrentWeaponSlot(), GetCurrentWeapon());
                        SetCurrentWeapon(-1);
                        ShedCurrentWeaponClientRpc();
                    }

                    SetIsSwimming(true);

                    if (IsJumpPressed())
                    {
                        if (transform.position.y > floatingDepht - jumpOutOfWaterHeight && IsJumpReleasedAfterJump())
                        {
                            // jump out of the water if jump is pressed a little below water
                            AddJumpForce();
                            CreateWaterSplash();
                        }
                        else
                        {
                            // else just rise
                            float newYWalk = GetCurrentWalkMovement().y + GetDiveOrRaiseMultiplier();
                            float newYRun = GetCurrentRunMovement().y + GetDiveOrRaiseMultiplier();
                            SetCurrentWalkMovement(new Vector3(GetCurrentWalkMovement().x, newYWalk, GetCurrentWalkMovement().z));
                            SetCurrentRunMovement(new Vector3(GetCurrentRunMovement().x, newYRun, GetCurrentRunMovement().z));

                        }

                    }
                    else if (IsDivePressed())
                    {
                        float newYWalk = GetCurrentWalkMovement().y - GetDiveOrRaiseMultiplier();
                        float newYRun = GetCurrentRunMovement().y - GetDiveOrRaiseMultiplier();
                        SetCurrentWalkMovement(new Vector3(GetCurrentWalkMovement().x, newYWalk, GetCurrentWalkMovement().z));
                        SetCurrentRunMovement(new Vector3(GetCurrentRunMovement().x, newYRun, GetCurrentRunMovement().z));

                    }


                }
                else
                {
                    SetIsSwimming(false);
                }
            }
        }

        float GetAirTimeFalling()
        {
            return airTimeFalling_nw.Value;
        }

        void SetAirTimeFalling(float _airTimeFalling)
        {
            if (IsServer) airTimeFalling_nw.Value = _airTimeFalling;
        }

        float GetAirTimeSlidingDown()
        {
            return airTimeSlidingDown_nw.Value;
        }

        void SetAirTimeSlidingDown(float _airTimeSlidingDown)
        {
            if (IsServer) airTimeSlidingDown_nw.Value = _airTimeSlidingDown;
        }

        /// <summary>
        /// This handles all the fallilng damage that is done by falling, jumping and sliding down
        /// This is steered by 
        /// <seealso cref="nonDmgFallTime"/>, <seealso cref="damageForFallingSeconds"/>
        /// <seealso cref="nonDmgSlidingTime"/>, <seealso cref="damageForSlidingSeconds"/>
        /// <seealso cref="nonDmgJumpingTime"/>, <seealso cref="damageForJumpingSeconds"/>
        /// </summary>
        void HandleFallingDmg()
        {
            if (IsFalling())
            {
                SetAirTimeFalling(GetAirTimeFalling() + Time.deltaTime);
            }
            else if (GetAirTimeFalling() > 0)
            {
                if (IsGroundedOrSwimming())
                {
                    if (GetAirTimeFalling() > 0 && GetAirTimeFalling() > nonDmgFallTime)
                    {
                        int dmg = (int)(damageForFallingSeconds * (GetAirTimeFalling() - nonDmgFallTime));
                        ReduceHealth(dmg);
                    }
                }
                SetAirTimeFalling(0);
            }

            if (IsSlidingDown())
            {
                SetAirTimeSlidingDown(GetAirTimeSlidingDown() + Time.deltaTime);
            }
            else if (GetAirTimeSlidingDown() > 0)
            {
                if (IsGroundedOrSwimming())
                {
                    if (GetAirTimeSlidingDown() > 0 && GetAirTimeSlidingDown() > nonDmgSlidingTime)
                    {
                        int dmg = (int)(damageForSlidingSeconds * (GetAirTimeSlidingDown() - nonDmgSlidingTime));
                        ReduceHealth(dmg);
                    }
                }
                SetAirTimeSlidingDown(0);
            }

            if (IsJumping())
            {
                SetAirTimeJumping(GetAirTimeJumping() + Time.deltaTime);
            }
            else if (GetAirTimeJumping() > 0)
            {
                if (IsGroundedOrSwimming())
                {
                    if (GetAirTimeJumping() > 0 && GetAirTimeJumping() > nonDmgJumpingTime)
                    {
                        int dmg = (int)(damageForJumpingSeconds * (GetAirTimeJumping() - nonDmgJumpingTime));
                        ReduceHealth(dmg);
                    }
                }
                SetAirTimeJumping(0);
            }
        }

        /// <summary>
        /// Reduces the health of the character. 
        /// </summary>
        /// <param name="dmg">dmg by which health is reduced</param>
        void ReduceHealth(int dmg)
        {
            GetComponent<NononZoneObjectNetwork>().ReduceHealth(dmg);
        }


        /// <summary>
        /// Handles all the flying. The weapon is drawn when the character begins to fly
        /// </summary>
        void HandleFlying()
        {
            if (IsFlyingEnabled())
            {
                if (IsFlying())
                {
                    if (IsJumpPressed())
                    {
                        float newYWalk = GetCurrentWalkMovement().y + GetDiveOrRaiseMultiplier();
                        float newYRun = GetCurrentRunMovement().y + GetDiveOrRaiseMultiplier();
                        SetCurrentWalkMovement(new Vector3(GetCurrentWalkMovement().x, newYWalk, GetCurrentWalkMovement().z));
                        SetCurrentRunMovement(new Vector3(GetCurrentRunMovement().x, newYRun, GetCurrentRunMovement().z));
                    }
                    else if (IsDivePressed())
                    {
                        float newYWalk = GetCurrentWalkMovement().y - GetDiveOrRaiseMultiplier();
                        float newYRun = GetCurrentRunMovement().y - GetDiveOrRaiseMultiplier();
                        SetCurrentWalkMovement(new Vector3(GetCurrentWalkMovement().x, newYWalk, GetCurrentWalkMovement().z));
                        SetCurrentRunMovement(new Vector3(GetCurrentRunMovement().x, newYRun, GetCurrentRunMovement().z));
                    }

                    if (GetCurrentWeapon() != -1)
                    {
                        ShedWeapon(GetCurrentWeaponSlot(), GetCurrentWeapon());
                        SetCurrentWeapon(-1);
                        ShedCurrentWeaponClientRpc();
                    }
                }
            }
            else
            {
                SetIsFlying(false);
            }
        }

        /// <summary>
        /// Draws the next weapon in slot
        /// </summary>
        int DrawNextWeaponServerOrSinglePlayer()
        {
            int theCurrentWeapon = -1;
            if (!IsFlying() && !IsSwimming())
            {
                theCurrentWeapon = GetCurrentWeapon();
                // Do it only if animation is not running at the moment
                AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(1);
                if (!currentState.IsName(NononZoneConstants.Player.DRAW_2HR_WEAPON_ANIM_NAME))
                {
                    if (GetCurrentWeapon() == -1)
                    {
                        int nextSlot = GetNextEquippedWeaponSlot();
                        if (nextSlot != -1)
                        {
                            // just draw the weapon
                            DrawWeapon(GetWeaponSlot(nextSlot), nextSlot);
                            SetCurrentWeapon(nextSlot);

                        }

                    }
                    else if (GetCurrentWeapon() == 3)
                    {
                        SetCurrentWeapon(-1);
                        // just shed the weapon
                        ShedWeapon(GetWeaponSlot(3), 3);

                    }
                    else
                    {
                        // shed the current weapon and draw the next
                        ShedWeapon(GetCurrentWeaponSlot(), GetCurrentWeapon());

                        int nextSlot = GetNextEquippedWeaponSlot();
                        if (nextSlot != -1)
                        {
                            DrawWeapon(GetWeaponSlot(nextSlot), nextSlot);
                            SetCurrentWeapon(nextSlot);

                        }
                        else
                        {
                            SetCurrentWeapon(-1);
                        }
                    }
                }
            }
            return theCurrentWeapon;
        }

        /// <summary>
        /// Method for Networing Only! 
        /// </summary>
        /// <param name="lastWeapon"></param>
        /// <param name="nextWeapon"></param>
        void DrawNextWeaponClient(int lastWeapon, int nextWeapon)
        {
            if (!IsFlying() && !IsSwimming())
            {
                // Do it only if animation is not running at the moment
                AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(1);
                if (!currentState.IsName(NononZoneConstants.Player.DRAW_2HR_WEAPON_ANIM_NAME))
                {
                    if (lastWeapon == -1)
                    {
                        // only Draw the weapon
                        DrawWeapon(GetWeaponSlot(nextWeapon), nextWeapon);

                    }
                    else if (lastWeapon != -1 && nextWeapon == -1)
                    {
                        // only shed
                        ShedWeapon(GetWeaponSlot(lastWeapon), lastWeapon);
                    }
                    else
                    {
                        // shed and Draw
                        ShedWeapon(GetWeaponSlot(lastWeapon), lastWeapon);
                        DrawWeapon(GetWeaponSlot(nextWeapon), nextWeapon);
                    }
                }
            }
        }

        /// <summary>
        /// Returns the next weaponm to draw
        /// </summary>
        /// <returns>next weapon to draw</returns>
        int GetNextEquippedWeaponSlot()
        {
            int index = GetCurrentWeapon() + 1;

            if (GetCurrentWeapon() == -1)
            {
                index = 0;
            }
            if (index == 0)
            {
                if (OneHandLeftSlot.prefabNr != -1) return 0;
                if (OneHandRightSlot.prefabNr != -1) return 1;
                if (TwoHandLeftSlot.prefabNr != -1) return 2;
                if (TwoHandRightSlot.prefabNr != -1) return 3;
            }
            else if (index == 1)
            {
                if (OneHandRightSlot.prefabNr != -1) return 1;
                if (TwoHandLeftSlot.prefabNr != -1) return 2;
                if (TwoHandRightSlot.prefabNr != -1) return 3;
            }
            else if (index == 2)
            {
                if (TwoHandLeftSlot.prefabNr != -1) return 2;
                if (TwoHandRightSlot.prefabNr != -1) return 3;
            }
            else if (index == 3)
            {
                if (TwoHandRightSlot.prefabNr != -1) return 3;
                else return -1;
            }
            return -1;
        }

        /// <summary>
        /// Draws the weapon from a slot
        /// </summary>
        /// <param name="fromSlot">weapon slot to draw from</param>
        /// <param name="slotNumber">slot number</param>
        void DrawWeapon(WeaponSlot fromSlot, int slotNumber)
        {
            // no weapon draw at flying and swimming
            if (!IsFlying() && !IsSwimming())
            {
                StartCoroutine(AttachOrShedWeaponInMiddleOfAnimation(fromSlot, true));
                StartCoroutine(EnableDisableRigBuilderAfterAnimation(fromSlot, true));
                animator.SetTrigger(draw2HWeaponHash);
            }
        }

        /// <summary>
        /// Sheds the current weapon
        /// </summary>
        /// <param name="toSlot">the weapon slot to draw to </param>
        /// <param name="slotNumber">slot number to draw to</param>
        void ShedWeapon(WeaponSlot toSlot, int slotNumber)
        {
            StartCoroutine(AttachOrShedWeaponInMiddleOfAnimation(toSlot, false));
            StartCoroutine(EnableDisableRigBuilderAfterAnimation(toSlot, false));
            animator.SetTrigger(draw2HWeaponHash);
        }

        /// <summary>
        /// Animates the character and when in the middle of the animation the weapon gets attached to the hands. or vice versa
        /// </summary>
        /// <param name="fromSlot">slot from which to attach</param>
        /// <param name="attach">attach or detach weapon</param>
        /// <returns></returns>
        private IEnumerator AttachOrShedWeaponInMiddleOfAnimation(WeaponSlot fromSlot, bool attach)
        {
            bool halfAnimationReached = false;
            while (!halfAnimationReached)
            {
                AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(1);
                if (currentState.IsName(NononZoneConstants.Player.DRAW_2HR_WEAPON_ANIM_NAME) && currentState.normalizedTime >= 0.5f)
                {
                    if (attach)
                    {
                        Transform weapon2Draw = fromSlot.weaponHolder.GetChild(0);
                        weapon2Draw.parent = playerRigging.rightHandPos;
                        weapon2Draw.localPosition = Vector3.zero;
                        weapon2Draw.localRotation = Quaternion.identity;
                        GetComponent<PlayerSound>().DrawWeapon(weapon2Draw.GetComponent<Weapon>().weaponType);

                    }
                    else
                    {
                        Transform weapon2Shed = playerRigging.rightHandPos.GetChild(0);
                        weapon2Shed.parent = fromSlot.weaponHolder;
                        weapon2Shed.localPosition = Vector3.zero;
                        weapon2Shed.localRotation = Quaternion.identity;
                        GetComponent<PlayerSound>().ShedWeapon(weapon2Shed.GetComponent<Weapon>().weaponType);
                    }

                    halfAnimationReached = true;
                    yield break;
                }
                yield return null;
            }
        }

        /// <summary>
        /// Enables or disables the rig builder to attach to the weapon
        /// </summary>
        /// <param name="enabled">true if enabled</param>
        private void DisableEnableRigBuilder(bool enabled)
        {
            foreach (RigLayer layer in rigBuilder.layers)
            {
                layer.active = enabled;
            }
        }

        /// <summary>
        /// Sync with attaching and detaching weapon.
        /// </summary>
        /// <param name="fromSlot">slot from attaching or detaching</param>
        /// <param name="attach">true if attaching</param>
        /// <returns></returns>
        private IEnumerator EnableDisableRigBuilderAfterAnimation(WeaponSlot fromSlot, bool attach)
        {
            if (!attach)
            {
                DisableEnableRigBuilder(false);
                rigBuilder.enabled = true;

                TwoBoneIKConstraint rightHandIK = playerRigging.rightHandIKTarget.parent.GetComponent<TwoBoneIKConstraint>();
                rightHandIK.data.target = playerRigging.rightHandIKTarget;
                TwoBoneIKConstraint leftHandIK = playerRigging.leftHandIKTarget.parent.GetComponent<TwoBoneIKConstraint>();
                leftHandIK.data.target = playerRigging.leftHandIKTarget;

                fromSlot.weapon.transform.parent = playerRigging.rightHandPos;
                fromSlot.weapon.transform.localPosition = Vector3.zero;
                fromSlot.weapon.transform.localRotation = Quaternion.identity;
                playerRigging.weaponDrawHolder.localPosition = Vector3.zero;
                playerRigging.weaponDrawHolder.localRotation = Quaternion.identity;
            }

            bool endOfAnimationReached = false;
            while (!endOfAnimationReached)
            {
                AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(1);
                if (currentState.IsName(NononZoneConstants.Player.DRAW_2HR_WEAPON_ANIM_NAME) && currentState.normalizedTime >= 1.0f)
                {
                    if (attach)
                    {
                        AttachWeaponAndActivateRigBuilder(fromSlot);
                    }

                    endOfAnimationReached = true;
                }
                yield return null;
            }
        }

        /// <summary>
        /// Attaches the weeapon to the hand and enables rig builder
        /// </summary>
        /// <param name="fromSlot">slot from which to attach</param>
        private void AttachWeaponAndActivateRigBuilder(WeaponSlot fromSlot)
        {
            TwoBoneIKConstraint rightHandIK = playerRigging.rightHandIKTarget.parent.GetComponent<TwoBoneIKConstraint>();
            rightHandIK.data.target = fromSlot.weapon.rightHandWeaponHandle;
            TwoBoneIKConstraint leftHandIK = playerRigging.leftHandIKTarget.parent.GetComponent<TwoBoneIKConstraint>();
            leftHandIK.data.target = fromSlot.weapon.leftHandWeaponHandle;

            // set the weapon into the constraint position
            fromSlot.weapon.transform.parent = playerRigging.weaponDrawHolder;
            fromSlot.weapon.transform.localPosition = Vector3.zero;
            fromSlot.weapon.transform.localRotation = Quaternion.identity;

            // set the pivot point of the weapon
            Vector3 pivotDistance = fromSlot.weapon.pivotPoint.localPosition - fromSlot.weapon.transform.localPosition;
            fromSlot.weapon.transform.localPosition -= pivotDistance;

            rigBuilder.enabled = true;
            DisableEnableRigBuilder(true);
        }

        /// <summary>
        /// Adds a weapon to one of the slots by its type.
        /// TODO: this needs to be adjusted as soon as a inventory is done.
        /// </summary>
        /// <param name="weapon">weapon to attach</param>
        void AddWeapon(Weapon weapon)
        {
            WeaponSlot theSlot = GetEmptyWeaponSlot(weapon.weaponType);

            theSlot.weapon = weapon;
            weapon.transform.parent = theSlot.weaponHolder;
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;
        }

        private WeaponSlot GetEmptyWeaponSlot(Weapon.WeaponTypes weaponType)
        {
            WeaponSlot theSlot = null;
            if (weaponType == Weapon.WeaponTypes.MELEE_1H || weaponType == Weapon.WeaponTypes.RANGE_1H)
            {
                if (OneHandLeftSlot.weapon == null)
                {
                    theSlot = OneHandLeftSlot;
                }
                else if (OneHandRightSlot.weapon == null)
                {
                    theSlot = OneHandRightSlot;
                }
                else
                {
                    Debug.Log("Don't add weapon. Slots are full...");
                }

            }
            else if (weaponType == Weapon.WeaponTypes.MELEE_2H || weaponType == Weapon.WeaponTypes.RANGE_2H)
            {
                if (TwoHandLeftSlot.weapon == null)
                {
                    theSlot = TwoHandLeftSlot;
                }
                else if (TwoHandRightSlot.weapon == null)
                {
                    theSlot = TwoHandRightSlot;
                }
                else
                {
                    Debug.Log("Don't add weapon. Slots are full...");
                }
            }

            return theSlot;
        }

        /// <summary>
        /// Disable all components for the character. Meaning no movement, no health bar etc.
        /// </summary>
        private void Die()
        {
            GetComponent<PlayerSound>().Die();
            animator.SetBool(isFallingHash, false);
            animator.SetBool(isJumpingHash, false);
            animator.SetBool(isWalkingHash, false);
            animator.SetBool(isRunningHash, false);
            animator.SetBool(isDeadHash, true);
            // Disable the health bar
            GetComponentInChildren<HealthBarScriptNetwork>().transform.GetComponent<Canvas>().enabled = false;
            playerInput.CharacterControls.Disable();
        }

        /// <summary>
        /// Gamevent when Character is Dying. <seealso cref="Die"/> is called and synced with the server.
        /// </summary>
        /// <param name="source"></param>
        public void OnDying(Transform source)
        {
            if (source.transform.Equals(transform))
            {
                if (IsOwner)
                {
                    DieServerRpc();
                }
            }
        }

        [ClientRpc]
        private void DieClientRpc()
        {
            Die();
        }

        [ServerRpc]
        private void DieServerRpc()
        {
            Die();
            DieClientRpc();
        }



        /// <summary>
        /// Resets the player after rebirth. All elements at dying needs to be activated again
        /// </summary>
        /// <param name="rebirthLocation">Location to rebirth</param>
        /// <param name="rebirthRotation">Rotation to rebirth</param>
        void ResetPlayerClientServer(Vector3 rebirthLocation, Quaternion rebirthRotation)
        {
            NononZoneObjectNetwork o = GetComponent<NononZoneObjectNetwork>();
            o.ResetHealth();
            HealthBarScriptNetwork healthBarScript = GetComponentInChildren<HealthBarScriptNetwork>();
            healthBarScript.Reset();
            healthBarScript.GetComponent<Canvas>().enabled = true;
            SetIsCameraUnderwater(false);
            HandleFog();
            playerInput.CharacterControls.Enable();
            animator.SetBool(isDeadHash, false);
        }

        /// <summary>
        /// Resets the players poision and rotation
        /// </summary>
        /// <param name="resetLocation"></param>
        /// <param name="resetRotation"></param>
        public void ResetPlayer(Vector3 resetLocation, Quaternion resetRotation)
        {
            if (IsOwner)
            {
                ResetPlayerServerRpc(resetLocation, resetRotation);
            }
        }

        [ClientRpc]
        private void ResetPlayerClientRpc(Vector3 revokeLocation, Quaternion revokeRotation)
        {
            ResetPlayerClientServer(revokeLocation, revokeRotation);
            SetPlayerPos(revokeLocation, revokeRotation);
        }

        [ServerRpc]
        private void ResetPlayerServerRpc(Vector3 revokeLocation, Quaternion revokeRotation)
        {
            ResetPlayerClientServer(revokeLocation, revokeRotation);
            SetPlayerPos(revokeLocation, revokeRotation);
            ResetPlayerClientRpc(revokeLocation, revokeRotation);
        }


        /// <summary>
        /// Ports the player to a certain location. 
        /// </summary>
        /// <param name="newLocation">location to port</param>
        /// <param name="newRotation">rotation for the character</param>
        public void SetPlayerPos(Vector3 newLocation, Quaternion newRotation)
        {
            rigidBody.position = newLocation;
            rigidBody.rotation = newRotation;
        }

        /// <summary>
        /// Ports the player to a new position including sync with server.
        /// </summary>
        /// <param name="newLocation">location to port</param>
        /// <param name="newRotation">rotation for the character</param>
        public void PortPlayer(Vector3 newLocation, Quaternion newRotation)
        {
            if (IsClient && IsOwner)
            {
                PortPlayerServerRpc(newLocation, newRotation);
            }
        }

        [ClientRpc]
        public void PortPlayerClientRpc(Vector3 newLocation, Quaternion newRotation)
        {
            SetPlayerPos(newLocation, newRotation);
        }

        [ServerRpc]
        private void PortPlayerServerRpc(Vector3 newLocation, Quaternion newRotation)
        {
            SetPlayerPos(newLocation, newRotation);
            PortPlayerClientRpc(newLocation, newRotation);
        }




        void Start()
        {
            // the player cannot travers between the Scenes with this :(
            DontDestroyOnLoad(transform);
            RegisterGameEvents();
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        public override void OnDestroy()
        {
            UnRegisterGameEvents();
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            // Commented Networklist, since pre.9 the Networklist is not synchronized when another player spawns
            // Enable again, when Networklists are synced correctly
            //networkWpnStates.Dispose();
            base.OnDestroy();
        }

        public void RegisterGameEvents()
        {
            GameEvents.Instance.onCollisionEntered += OnCollisionEntered;
            GameEvents.Instance.onCollisionExited += OnCollisionExited;
            GameEvents.Instance.onDying += OnDying;
        }

        public void UnRegisterGameEvents()
        {
            GameEvents.Instance.onCollisionEntered -= OnCollisionEntered;
            GameEvents.Instance.onCollisionExited -= OnCollisionExited;
            GameEvents.Instance.onDying -= OnDying;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RegisterGameEvents();
        }

        private void OnSceneUnloaded(Scene current)
        {
            UnRegisterGameEvents();
        }

        /// <summary>
        /// Handles the strive movements.
        /// </summary>
        void HandleStriveMovement()
        {
            Vector3 _currentStriveMovement = Vector3.zero;

            if (IsStriveLeftPressed())
            {
                if (IsRunPressed())
                {
                    _currentStriveMovement = new Vector3(-1 * GetRunMultiplier(), GetCurrentStriveMovement().y, GetCurrentRunMovement().z);
                }
                else
                {
                    _currentStriveMovement = new Vector3(-1 * GetWalkMultiplier(), GetCurrentStriveMovement().y, GetCurrentWalkMovement().z);
                }

            }

            if (IsStriveRightPressed())
            {
                if (IsRunPressed())
                {
                    _currentStriveMovement = new Vector3(1 * GetRunMultiplier(), GetCurrentStriveMovement().y, GetCurrentRunMovement().z);
                }
                else
                {
                    _currentStriveMovement = new Vector3(1 * GetWalkMultiplier(), GetCurrentStriveMovement().y, GetCurrentWalkMovement().z);
                }
            }

            SetCurrentStriveMovement(_currentStriveMovement);

        }

        private void Update()
        {
            if (IsClient)
            {
                HandleCameraAimPointPositionAndRotation();
                HandleMountChange();
                HandleAimingAndShooting();
                HandleFog();
            }

            if (IsServer)
            {
                HandleRotation();
                HandleAnimation();
                UpdatePlayer();
            }
        }

        // Update is called once per frame
        void UpdatePlayer()
        {
            Vector3 moveDir = Vector3.zero;
            // only don't move is player is really falling. otherwise the player get stuck
            float beginFallingTreshhold = Time.deltaTime;

            HandleStriveMovement();

            if (!IsStriveLeftPressed() && !IsStriveRightPressed() && !(IsCharacterRotatePressed() && IsMovingHorizontal()))
            {
                // normal forward movement to character if strive is not active
                if (IsRunPressed())
                {
                    if (!IsJumping() && (!IsFalling() || GetAirTimeFalling() < beginFallingTreshhold))
                    {
                        moveDir = new Vector3(0, GetCurrentRunMovement().y, GetCurrentRunMovement().z);
                    }
                    else
                    {
                        moveDir = new Vector3(0, GetCurrentRunMovement().y, GetForwardForceOnJump());
                    }

                }
                else
                {
                    if (!IsJumping() && (!IsFalling() || GetAirTimeFalling() < beginFallingTreshhold))
                    {
                        moveDir = new Vector3(0, GetCurrentWalkMovement().y, GetCurrentWalkMovement().z);
                    }
                    else
                    {
                        moveDir = new Vector3(0, GetCurrentWalkMovement().y, GetForwardForceOnJump());
                    }
                }
            }
            else if (IsStriveRightPressed() || IsStriveLeftPressed() || (IsCharacterRotatePressed() && IsMovingHorizontal()))
            {
                float forwardMovement = GetForwardMovement();
                float horizontalMovement = 0f;
                if (IsStriveLeftPressed() || IsStriveRightPressed())
                {
                    horizontalMovement = GetCurrentStriveMovement().x;

                }
                else if (IsMovingHorizontal())
                {
                    if (IsRunPressed())
                    {
                        horizontalMovement = Mathf.Sign(GetCurrentRunMovement().x) * GetRunMultiplier();
                    }
                    else
                    {
                        horizontalMovement = Mathf.Sign(GetCurrentWalkMovement().x) * GetWalkMultiplier();
                    }
                }

                if (forwardMovement > 0)
                {
                    float angle = 90 + strivedegrees;
                    float x = Mathf.Cos(angle * Mathf.Deg2Rad) * horizontalMovement * -1f;
                    float z = Mathf.Sin(angle * Mathf.Deg2Rad) * horizontalMovement * -1f;
                    moveDir = new Vector3(x, GetCurrentRunMovement().y, Mathf.Abs(z));
                }
                else if (forwardMovement < 0)
                {

                    float angle = 90 + strivedegrees;
                    float x = Mathf.Cos(angle * Mathf.Deg2Rad) * horizontalMovement * -1f;
                    float z = Mathf.Sin(angle * Mathf.Deg2Rad) * horizontalMovement;
                    moveDir = new Vector3(x, GetCurrentRunMovement().y, Mathf.Abs(z) * -1f);
                }
                else
                {
                    float angle = 180 + strivedegrees;
                    float x = Mathf.Cos(angle * Mathf.Deg2Rad) * horizontalMovement * -1f;
                    float z = Mathf.Sin(angle * Mathf.Deg2Rad) * horizontalMovement * -1f;
                    moveDir = new Vector3(x, GetCurrentRunMovement().y, Mathf.Abs(z));
                }

            }

            if (IsCameraMovePressed() && IsCharacterRotatePressed() && !IsStriveLeftPressed() && !IsStriveRightPressed())
            {
                if (!(IsCharacterRotatePressed() && IsMovingHorizontal()))
                {
                    if (!IsJumping() && (!IsFalling() || GetAirTimeFalling() < beginFallingTreshhold))
                    {
                        moveDir = new Vector3(GetCurrentRunMovement().x, moveDir.y, GetForwardMovement());
                    }
                    else
                    {
                        moveDir = new Vector3(GetCurrentRunMovement().x, moveDir.y, GetForwardForceOnJump());
                    }
                }
            }

            forwardMouseSpeed = 1.0f;
            moveDir = transform.TransformDirection(moveDir);
            moveDirection.Value = moveDir;

            HandleSlope();
            HandleGravity();
            HandleJump();
            HandleSwimming();
            HandleFlying();
            HandleFallingDmg();
        }

        private void FixedUpdate()
        {
            if (IsServer)
            {
                MoveCharacterAndAddGravity(moveDirection.Value);
                if (!IsSlidingDown()) HandleStepMove();
            }
        }

        /// <summary>
        /// Moves the character up the steps
        /// </summary>
        private void HandleStepMove()
        {
            Vector3 velocity = this.GetComponent<Rigidbody>().velocity;

            ContactPoint groundContactPoint = default(ContactPoint);
            bool grounded = FindGround(out groundContactPoint, contactPoints);

            Vector3 stepUpOffset = default(Vector3);
            bool stepUp = false;
            if (grounded)
                stepUp = FindStepUpPoint(out stepUpOffset, contactPoints, groundContactPoint, velocity);

            //Steps
            if (stepUp)
            {
                rigidBody.position += stepUpOffset;
                rigidBody.velocity = lastVelocity;
            }

            contactPoints.Clear();
            lastVelocity = velocity;
        }

        /// <summary>
        /// Moves the character by moving variables
        /// </summary>
        /// <param name="_moveDirection">direction to move</param>
        private void MoveCharacterAndAddGravity(Vector3 _moveDirection)
        {
            if (!IsSwimming() && !IsFlying())
            {
                rigidBody.velocity = new Vector3(_moveDirection.x, rigidBody.velocity.y, _moveDirection.z);
                Rigidbody body = null;
                if (collidedTransform != null)
                {
                    body = collidedTransform.GetComponent<Rigidbody>();
                    // add the rigidbody movement to the character movement to keep him attached                    
                    if (body != null)
                    {
                        rigidBody.velocity += body.velocity;
                    }
                }
                Vector3 force = Vector3.up * gravity;
                force += jumpForceDirection.Value;
                force += slideDownForceDirection.Value;
                force += fallingDownForceDircetion.Value;

                rigidBody.AddForce(force);
                jumpForceDirection.Value = Vector3.zero;
            }
            else
            {
                rigidBody.velocity = new Vector3(_moveDirection.x, _moveDirection.y, _moveDirection.z);
            }


        }

        void OnEnable()
        {
            playerInput.CharacterControls.Enable();
        }

        private void OnDisable()
        {
            playerInput.CharacterControls.Disable();
        }

        void OnCollisionEnter(Collision col)
        {
            contactPoints.AddRange(col.contacts);
        }

        void OnCollisionStay(Collision col)
        {
            contactPoints.AddRange(col.contacts);
        }

        /// <summary>
        /// Find the flat planes in the contactPoints
        /// </summary>
        /// <param name="groundContactPoint">returning contactPoint with the ground</param>
        /// <param name="_contactPoints">all contactPoints</param>
        /// <returns>if we found ground</returns>
        bool FindGround(out ContactPoint groundContactPoint, List<ContactPoint> _contactPoints)
        {
            groundContactPoint = default(ContactPoint);
            bool found = false;
            foreach (ContactPoint cp in _contactPoints)
            {
                //Pointing with some up direction
                if (cp.normal.y > 0.0001f && (found == false || cp.normal.y > groundContactPoint.normal.y))
                {
                    groundContactPoint = cp;
                    found = true;
                }
            }

            return found;
        }

        /// <summary>
        /// Find the step where a possible step is found
        /// </summary>
        /// <param name="stepUpOffset">Offset for steps</param>
        /// <param name="_contactPoints">All contact points</param>
        /// <param name="groundContactPoint">contact point with ground</param>
        /// <param name="currentVelocity">current velocity</param>
        /// <returns>if a step has been found</returns>
        bool FindStepUpPoint(out Vector3 stepUpOffset, List<ContactPoint> _contactPoints, ContactPoint groundContactPoint, Vector3 currentVelocity)
        {
            stepUpOffset = default(Vector3);

            // no stepping without moving
            Vector2 velocityXZ = new Vector2(currentVelocity.x, currentVelocity.z);
            if (velocityXZ.sqrMagnitude < 0.0001f)
                return false;

            foreach (ContactPoint cp in _contactPoints)
            {
                bool test = GetStepupOffset(out stepUpOffset, cp, groundContactPoint);
                if (test)
                    return test;
            }
            return false;
        }

        bool GetStepupOffset(out Vector3 stepUpOffset, ContactPoint _contactPoint, ContactPoint groundContactPoint)
        {
            stepUpOffset = default(Vector3);
            Collider stepCol = _contactPoint.otherCollider;
            if (stepCol == null)
            {
                return false;
            }

            // Check if the contact point normal matches that of a step (y close to 0)
            if (Mathf.Abs(_contactPoint.normal.y) >= 0.01f)
            {
                return false;
            }

            // Make sure the contact point is low enough to be a step
            if (!(_contactPoint.point.y - groundContactPoint.point.y < maxStepHeight))
            {
                return false;
            }

            // Check to see if there's actually a place to step in front of us
            RaycastHit hitInfo;
            float stepHeight = groundContactPoint.point.y + maxStepHeight + 0.0001f;
            Vector3 stepTestInvDir = new Vector3(-_contactPoint.normal.x, 0, -_contactPoint.normal.z).normalized;
            Vector3 origin = new Vector3(_contactPoint.point.x, stepHeight, _contactPoint.point.z) + (stepTestInvDir * stepSearchMargin);
            Vector3 direction = Vector3.down;
            if (!(stepCol.Raycast(new Ray(origin, direction), out hitInfo, maxStepHeight)))
            {
                return false;
            }

            // calculate the points
            Vector3 stepUpPoint = new Vector3(_contactPoint.point.x, hitInfo.point.y + 0.0001f, _contactPoint.point.z) + (stepTestInvDir * stepSearchMargin);
            Vector3 stepUpPointOffset = stepUpPoint - new Vector3(_contactPoint.point.x, groundContactPoint.point.y, _contactPoint.point.z);

            // Calculate and return the point
            stepUpOffset = stepUpPointOffset;
            return true;
        }

        private void OnCollisionEntered(Transform source, Transform origin)
        {
            // only connect if player is above object with a margin (origin is the player)
            if (origin.Equals(transform) && origin.position.y >= (source.position.y - 0.1f))
            {
                Transform theSource = null;
                theSource = source;
                if (source.tag.Equals(NononZoneConstants.HitBoxCollider.HITBOX_COLLIDED_TAG))
                {
                    theSource = source.parent;
                }

                if (!theSource.name.Equals(transform.name) && !NononZoneObjectNetwork.isOneOfTypes(theSource, INononZoneObject.NononZoneObjType.PLAYER))
                {
                    collidedTransform = theSource;
                }
            }
        }

        private void OnCollisionExited(Transform source, Transform origin)
        {
            if (origin.Equals(transform))
            {
                collidedTransform = null;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            ActionOnTriggerEnter(other);
        }

        [ClientRpc]
        private void CreateAndAddWeaponClientRpc(Weapon.WeaponTypes weaponType, int prefabNumber)
        {
            CreateAndAddWeapon(weaponType, prefabNumber);
        }

        private void ActionOnTriggerEnter(Collider other)
        {
            Transform theCollidedTransform = other.transform;
            if (theCollidedTransform.tag.Equals(NononZoneConstants.HitBoxCollider.HITBOX_COLLIDED_TAG))
            {
                theCollidedTransform = theCollidedTransform.parent.transform;
            }
            if (theCollidedTransform != null && NononZoneObjectNetwork.isOneOfTypes(theCollidedTransform, INononZoneObject.NononZoneObjType.WATER))
            {
                SetWaterColliderBounds(other.bounds);
                SetIsInWater(true);
            }

            if (theCollidedTransform != null && NononZoneObjectNetwork.isOneOfTypes(theCollidedTransform, INononZoneObject.NononZoneObjType.LOOT) && theCollidedTransform.childCount > 0 && !collectedLoot.Contains(other.transform))
            {
                collectedLoot.Add(other.transform);

                Transform loot = theCollidedTransform.GetChild(0);
                if (loot.gameObject != null && !ReferenceEquals(loot.gameObject, null))
                {

                    if (NononZoneObjectNetwork.isOneOfTypes(loot, INononZoneObject.NononZoneObjType.WEAPON))
                    {
                        Weapon wpn = loot.GetComponent<Weapon>();
                        if (IsServer)
                        {
                            CreateAndAddWeapon(wpn.weaponType, wpn.GetPrefabDirectoryPrefabNr());
                            GameEvents.Instance.GotLoot(transform, loot);
                            CreateAndAddWeaponClientRpc(wpn.weaponType, wpn.GetPrefabDirectoryPrefabNr());
                        }
                    }
                    if (NononZoneObjectNetwork.isOneOfTypes(loot, INononZoneObject.NononZoneObjType.MOUNT))
                    {
                        if (IsServer)
                        {
                            AddMount(Mount.THRUSTER);
                            GameEvents.Instance.GotLoot(transform, loot);
                        }
                    }

                    if (IsOwner)
                    {
                        NetworkObject nwObject = theCollidedTransform.GetComponent<NetworkObject>();
                        if (nwObject != null) DestroyLootBubbleServerRpc(nwObject);
                        theCollidedTransform = null;
                    }
                }
            }
        }

        [ServerRpc]
        private void DestroyLootBubbleServerRpc(NetworkObjectReference lootBubble)
        {
            if (lootBubble.TryGet(out NetworkObject noLootBubble))
            {
                Destroy(noLootBubble.gameObject);
            }
        }



        private void OnTriggerExit(Collider other)
        {
            ActionOnTriggerExit(other);
        }

        private void ActionOnTriggerExit(Collider other)
        {
            collectedLoot.Clear();
            Transform collidedTransform = other.transform;
            if (collidedTransform.tag.Equals(NononZoneConstants.HitBoxCollider.HITBOX_COLLIDED_TAG))
            {
                collidedTransform = collidedTransform.parent.transform;
            }

            if (NononZoneObjectNetwork.isOneOfTypes(collidedTransform, INononZoneObject.NononZoneObjType.WATER))
            {
                SetIsInWater(false);
                SetIsSwimming(false);
            }
        }

        [ServerRpc]
        private void UpdateAnimationTriggerServerRpc(int triggerHash)
        {
            animator.SetTrigger(triggerHash);
            UpdateAnimationTriggerClientRpc(triggerHash);
        }

        [ClientRpc]
        private void UpdateAnimationTriggerClientRpc(int triggerHash)
        {
            animator.SetTrigger(triggerHash);
        }

        /// <summary>
        /// Instantiates a weapon and adds it to the weapon slots. This is needed for attaching a weapon to a NetworkTransform player object.
        /// It's a cheat to add a non- network object weapon to a 
        /// </summary>
        /// <param name="weaponType">type of the weapon</param>
        /// <param name="prefabNumber">prefab to instantiate</param>
        private void CreateAndAddWeapon(Weapon.WeaponTypes weaponType, int prefabNumber)
        {
            WeaponSlot slot = GetEmptyWeaponSlot(weaponType);
            if (slot != null)
            {
                PrefabDirectory.PrefabHolder lootHolder = PrefabDirectory.Instance.prefabs[prefabNumber];
                Transform weapon = Instantiate(lootHolder.prefab);
                slot.weapon = weapon.GetComponent<Weapon>();
                slot.weapon.SetPrefabDirectoryPrefabNr(prefabNumber);
                slot.prefabNr = prefabNumber;
                slot.weaponType = weaponType;
                slot.weapon.weaponType = weaponType;
                weapon.transform.parent = slot.weaponHolder;
                weapon.transform.localPosition = Vector3.zero;
                weapon.transform.localRotation = Quaternion.identity;

                if (IsServer)
                {
                    if (slot.id == 0) OneHandLeftSlotPrefab.Value = prefabNumber;
                    if (slot.id == 1) OneHandRightSlotPrefab.Value = prefabNumber;
                    if (slot.id == 2) TwoHandLeftSlotPrefab.Value = prefabNumber;
                    if (slot.id == 3) TwoHandRightSlotPrefab.Value = prefabNumber;
                }
            }
        }

        /// <summary>
        /// Initializes the player on the spawn in the network
        /// </summary>
        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                List<SerializableWeapon> wpns = GetWeaponsSlots();
                SyncWeaponsFromServerOnInitializeClientRpc(wpns[0], wpns[1], wpns[2], wpns[3]);
                SpawnWeapons();
                SpawnWeaponsClientRpc();
                InitializeMounts();
                InitializeMountsClientRpc();
            }
            if (IsClient && !IsOwner)
            {
                // initialize from network variables
                InitializeWeaponsNotOwnerClient();
            }
            base.OnNetworkSpawn();
        }

        private void InitializeWeaponsNotOwnerClient()
        {
            OneHandLeftSlot.prefabNr = OneHandLeftSlotPrefab.Value;
            OneHandRightSlot.prefabNr = OneHandRightSlotPrefab.Value;
            TwoHandLeftSlot.prefabNr = TwoHandLeftSlotPrefab.Value;
            TwoHandRightSlot.prefabNr = TwoHandRightSlotPrefab.Value;

            SpawnWeapons();
            InitializeMounts();
        }

        [ClientRpc]
        private void SyncWeaponsFromServerOnInitializeClientRpc(SerializableWeapon oneHandLeft, SerializableWeapon oneHandRight, SerializableWeapon twoHandLeft, SerializableWeapon twoHandRight)
        {
            if (IsOwner)
            {
                List<SerializableWeapon> wpns = new List<SerializableWeapon>();
                wpns.Add(oneHandLeft);
                wpns.Add(oneHandRight);
                wpns.Add(twoHandLeft);
                wpns.Add(twoHandRight);
                SetWeapons(wpns);
            }
        }

        private void InitializeMounts()
        {
            Mount theCurrentMount = GetCurrentMount();
            if (IsFlyingEnabled() && theCurrentMount.Equals(Mount.THRUSTER) && instantiatedThruster == null)
            {
                ActivateDeactivateThruster(true);
            }
        }

        [ClientRpc]
        private void InitializeMountsClientRpc()
        {
            if (IsOwner)
            {
                InitializeMounts();
            }
        }

        /// <summary>
        /// Initializes the weapon slots. It spawns the weapons and adds it to the player.
        /// </summary>
        public void SpawnWeapons()
        {
            InstantiateWeapon(OneHandLeftSlot);
            InstantiateWeapon(OneHandRightSlot);
            InstantiateWeapon(TwoHandLeftSlot);
            InstantiateWeapon(TwoHandRightSlot);

            if (GetCurrentWeapon() != -1)
            {
                AttachWeaponAndActivateRigBuilder(GetCurrentWeaponSlot());
            }
        }

        [ClientRpc]
        private void SpawnWeaponsClientRpc()
        {
            SpawnWeapons();
        }

        void InstantiateWeapon(WeaponSlot slot)
        {
            if (slot.prefabNr != -1 && slot.weapon == null)
            {
                PrefabDirectory.PrefabHolder lootHolder = PrefabDirectory.Instance.prefabs[slot.prefabNr];
                Transform weapon = Instantiate(lootHolder.prefab);
                slot.weapon = weapon.GetComponent<Weapon>();
                weapon.transform.parent = slot.weaponHolder;
                weapon.transform.localPosition = Vector3.zero;
                weapon.transform.localRotation = Quaternion.identity;
                slot.weapon.SetPrefabDirectoryPrefabNr(slot.prefabNr);
                slot.weapon.weaponType = slot.weaponType;
            }
        }


        [ServerRpc]
        private void StartAimingServerRpc(bool isAiming, Vector3 raycastDestination, float deltaTime, float nowTime)
        {
            timeStartAiming_nw.Value = nowTime;
            StartStopFiringOnServer(isAiming, raycastDestination, deltaTime);
            StartAimingClientRpc(isAiming, raycastDestination);
        }

        [ServerRpc]
        private void StopFiringServerRpc()
        {
            if (GetCurrentWeapon() != -1) GetCurrentWeaponSlot().weapon.StopFiring();
            StopFiringClientRpc();
        }

        [ClientRpc]
        private void StopFiringClientRpc()
        {
            if (!IsOwner)
            {
                if (GetCurrentWeapon() != -1) GetCurrentWeaponSlot().weapon.StopFiring();
            }
        }

        [ClientRpc]
        private void StartAimingClientRpc(bool isAiming, Vector3 raycastDestination)
        {

            if (!IsOwner)
            {
                StartCoroutine(StartAimingAndFiringWithDelay(isAiming, raycastDestination));
            }
        }

        [ClientRpc]
        private void ShedCurrentWeaponClientRpc()
        {
            ShedWeapon(GetCurrentWeaponSlot(), GetCurrentWeapon());
            SetCurrentWeapon(-1);
        }
    }
}