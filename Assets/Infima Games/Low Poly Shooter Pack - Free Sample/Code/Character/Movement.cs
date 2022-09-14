﻿// Copyright 2021, Infima Games. All Rights Reserved.

using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class Movement : MovementBehaviour
    {
        #region FIELDS SERIALIZED

        [Header("Audio Clips")]
        
        [Tooltip("The audio clip that is played while walking.")]
        [SerializeField]
        private AudioClip audioClipWalking;

        [Tooltip("The audio clip that is played while running.")]
        [SerializeField]
        private AudioClip audioClipRunning;

        [Header("Speeds")]

        [SerializeField]
        private float speedWalking = 5.0f;

        [Tooltip("How fast the player moves while running."), SerializeField]
        private float speedRunning = 9.0f;

        /// <summary>
        /// Attached Rigidbody.
        /// </summary>
        [SerializeField] private Rigidbody rigidBody;
        #endregion

        #region PROPERTIES

        //Velocity.
        private Vector3 Velocity
        {
            //Getter.
            get => rigidBody.velocity;
            //Setter.
            set => rigidBody.velocity = value;
        }

        #endregion

        #region FIELDS

        /// <summary>
        /// Attached CapsuleCollider.
        /// </summary>
        private CapsuleCollider capsule;
        /// <summary>
        /// Attached AudioSource.
        /// </summary>
        private AudioSource audioSource;
        
        /// <summary>
        /// True if the character is currently grounded.
        /// </summary>
        private bool grounded;

        /// <summary>
        /// Player Character.
        /// </summary>
        private CharacterBehaviour playerCharacter;
        /// <summary>
        /// The player character's equipped weapon.
        /// </summary>
        private WeaponBehaviour equippedWeapon;

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

        /// <summary>
        /// Array of RaycastHits used for ground checking.
        /// </summary>
        private readonly RaycastHit[] groundHits = new RaycastHit[8];

        #endregion

        #region UNITY FUNCTIONS

        /// <summary>
        /// Awake.
        /// </summary>
        protected override void Awake()
        {
            //Get Player Character.
            playerCharacter = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
        }

        /// Initializes the FpsController on start.
        protected override  void Start()
        {
            //Rigidbody Setup.
            rigidBody = GetComponent<Rigidbody>();
            rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
            //Cache the CapsuleCollider.
            capsule = GetComponent<CapsuleCollider>();

            //Audio Source Setup.
            audioSource = GetComponent<AudioSource>();
            audioSource.clip = audioClipWalking;
            audioSource.loop = true;
        }

        /// Checks if the character is on the ground.
        private void OnCollisionStay()
        {
            //Bounds.
            Bounds bounds = capsule.bounds;
            //Extents.
            Vector3 extents = bounds.extents;
            //Radius.
            float radius = extents.x - 0.01f;
            
            //Cast. This checks whether there is indeed ground, or not.
            Physics.SphereCastNonAlloc(bounds.center, radius, Vector3.down,
                groundHits, extents.y - radius * 0.5f, ~0, QueryTriggerInteraction.Ignore);
            
            //We can ignore the rest if we don't have any proper hits.
            if (!groundHits.Any(hit => hit.collider != null && hit.collider != capsule)) 
                return;
            
            //Store RaycastHits.
            for (var i = 0; i < groundHits.Length; i++)
                groundHits[i] = new RaycastHit();

            //Set grounded. Now we know for sure that we're grounded.
            grounded = true;
        }
			
        protected override void FixedUpdate()
        {
            //Move.
            MoveCharacter();
            
            //Unground.
            grounded = false;
        }

        /// Moves the camera to the character, processes jumping and plays sounds every frame.
        protected override  void Update()
        {
            //Get the equipped weapon!
            equippedWeapon = playerCharacter.GetInventory().GetEquipped();
            
            //Play Sounds!
            PlayFootstepSounds();
        }

        #endregion

        #region METHODS

        private void MoveCharacter()
        {
            #region Calculate Movement Velocity

            //Get Movement Input!
            Vector2 frameInput = playerCharacter.GetInputMovement();

            //Calculate local-space direction by using the player's input.
            var movement = new Vector3(frameInput.x, 0.0f, frameInput.y);

            //Running speed calculation.
            if (playerCharacter.IsRunning())
            {
                movement *= speedRunning;
            }
            else
            {
                //Multiply by the normal walking speed.
                movement *= speedWalking;
            }

            //World space velocity calculation. This allows us to add it to the rigidbody's velocity properly.
            movement = transform.TransformDirection(movement);

            #endregion
            
            //Update Velocity.
            Velocity = new Vector3(movement.x, 0.0f, movement.z);

            if (playerCharacter.IsRunning())
            {
                currentRunMovement_nw.Value = Velocity;
            }
            else
            {
                currentWalkMovement_nw.Value = Velocity;
            }

            if (IsClient && IsOwner)
            {
                OnMovementInputServerRpc(Velocity, Velocity, movement != Vector3.zero);
            }
        }

        /// <summary>
        /// Plays Footstep Sounds. This code is slightly old, so may not be great, but it functions alright-y!
        /// </summary>
        private void PlayFootstepSounds()
        {
            //Check if we're moving on the ground. We don't need footsteps in the air.
            if (grounded && rigidBody.velocity.sqrMagnitude > 0.1f)
            {
                //Select the correct audio clip to play.
                audioSource.clip = playerCharacter.IsRunning() ? audioClipRunning : audioClipWalking;
                //Play it!
                if (!audioSource.isPlaying)
                    audioSource.Play();
            }
            //Pause it if we're doing something like flying, or not moving!
            else if (audioSource.isPlaying)
                audioSource.Pause();
        }

        /// <summary>
        /// Ports the player to a new position including sync with server.
        /// </summary>
        /// <param name="newLocation">location to port</param>
        /// <param name="newRotation">rotation for the character</param>
        public void PortPlayer(Vector3 newLocation, Quaternion newRotation)
        {
            SetPlayerPos(newLocation, newRotation);
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


        #endregion

        #region Networking Code
        /// <summary>
        /// Server Method for the On Run Input Action. Sync Method for isRunPressed
        /// </summary>
        /// <param name="_isRunPressed">is run pressed</param>
        [ServerRpc]
        void OnRunServerRpc(bool _isRunPressed)
        {
           // SetIsRunPressed(_isRunPressed);
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

        #endregion
    }
}