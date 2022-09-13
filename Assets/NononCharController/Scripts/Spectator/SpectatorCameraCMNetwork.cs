using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Cinemachine;
using UnityEngine.EventSystems;

namespace zone.nonon
{
    public class SpectatorCameraCMNetwork : NetworkBehaviour
    {
        [Header("PlayerFollow")]
        public CinemachineVirtualCamera thirdPersonVMCamera;
        public CinemachineFreeLook freelookVMCamera;
        public CinemachineVirtualCamera firstPersonVMCamera;
        public Transform lookAtTarget;

        [Header("CameraMovement")]
        public float cameraMovementSensitivityX = 200f;
        public float cameraMovementSensitivityY = 1f;

        bool sticky = true;
        bool overriddenAggroCam = false;
        bool fixedHeight = false;
        bool fpvObjectCamActive = false;

        PlayerInput playerInput;

        Vector2 currentCameraMovementInput;
        Vector3 currentCameraMovement;

        bool isCameraMovePressed;

        float xDeg = 0.0f;
        float yDeg = 0.0f;


        private void Awake()
        {
            playerInput = new PlayerInput();

            playerInput.CharacterControls.MoveCameraMouse.started += OnCameraMouseMovementInput;
            playerInput.CharacterControls.MoveCameraMouse.canceled += OnCameraMouseMovementInput;
            playerInput.CharacterControls.MoveCameraMouse.performed += OnCameraMouseMovementInput;
            playerInput.CharacterControls.MoveCameraStick.started += OnCameraMovementStickInput;
            playerInput.CharacterControls.MoveCameraStick.canceled += OnCameraMovementStickInput;
            playerInput.CharacterControls.MoveCameraStick.performed += OnCameraMovementStickInput;

            playerInput.CharacterControls.EnableMoveCamera.started += OnEnableMoveCameraInput;
            playerInput.CharacterControls.EnableMoveCamera.canceled += OnEnableMoveCameraInput;
            playerInput.CharacterControls.Enable();

        }

        private void Start()
        {
            // if netowrking is active, deactivate Audio Listener and Camera for non Owner Objects
            if (!IsOwner)
            {
                GetComponent<AudioListener>().enabled = false;
                GetComponent<Camera>().enabled = false;
                GetComponent<CinemachineBrain>().enabled = false;
                freelookVMCamera.GetComponent<CinemachineFreeLook>().enabled = false;
                thirdPersonVMCamera.GetComponent<CinemachineVirtualCamera>().enabled = false;
                thirdPersonVMCamera.enabled = false;
                freelookVMCamera.enabled = false;
            }
            else
            {
                freelookVMCamera.GetComponent<CinemachineFreeLook>().enabled = true;
                firstPersonVMCamera.GetComponent<CinemachineVirtualCamera>().enabled = true;
                thirdPersonVMCamera.GetComponent<CinemachineVirtualCamera>().enabled = true;
            }

            Vector3 angles = lookAtTarget.eulerAngles;
            xDeg = angles.x;
            yDeg = angles.y;
        }

        public void SetIsFixedHeight(bool _fixedHeight)
        {
            fixedHeight = _fixedHeight;
        }

        public void SetFpvObjectCamActive(bool _fpsObjectCamActive)
        {
            fpvObjectCamActive = _fpsObjectCamActive;
        }

        public void EnableDisableMovementInput(bool enabled)
        {
            if (enabled)
            {
                playerInput.CharacterControls.MoveCameraMouse.Enable();
                playerInput.CharacterControls.MoveCameraStick.Enable();
                playerInput.CharacterControls.EnableMoveCamera.Enable();
            }
            else
            {
                playerInput.CharacterControls.MoveCameraMouse.Disable();
                playerInput.CharacterControls.MoveCameraStick.Disable();
                playerInput.CharacterControls.EnableMoveCamera.Disable();
            }


        }

        public void SetSticky(bool _sticky)
        {
            sticky = _sticky;
        }

        public bool IsSticky()
        {
            return sticky;
        }

        /// <summary>
        /// Sets the follow and lookat objects on the cameras
        /// </summary>
        /// <param name="followTransform">follow target</param>
        /// <param name="lookatTransform">lookat target</param>
        public void SetFollowAndLookatTarget(Transform followTransform, Transform lookatTransform = null)
        {
            freelookVMCamera.Follow = followTransform;
            freelookVMCamera.LookAt = followTransform;
            thirdPersonVMCamera.Follow = followTransform;
            if (lookatTransform != null)
            {
                thirdPersonVMCamera.LookAt = lookatTransform;
            }
            else
            {
                thirdPersonVMCamera.LookAt = followTransform;
            }
        }

        /// <summary>
        /// Activates the freelook (1st person) camera (orbit cam)
        /// </summary>
        /// <param name="overriddenAggro">if true, the aggro 3rd person view will not be activated on aggro</param>
        public void ActivateFreelookCamera(bool overriddenAggro = false)
        {
            overriddenAggroCam = overriddenAggro;
            if (freelookVMCamera.Priority < 99)
            {
                firstPersonVMCamera.Priority = 10;
                freelookVMCamera.Priority = 99;
                thirdPersonVMCamera.Priority = 20;
            }
        }

        /// <summary>
        /// Activates Third person view on aggro (Chase cam)
        /// </summary>
        public void Activate3rdPersonCamera()
        {
            if (thirdPersonVMCamera.Priority < 99 && !overriddenAggroCam)
            {
                firstPersonVMCamera.Priority = 10;
                freelookVMCamera.Priority = 30;
                thirdPersonVMCamera.Priority = 99;
            }
        }

        /// <summary>
        /// Activates the 1st person view (freelook camera)
        /// </summary>
        public void Activate1stPersonCamera()
        {
            if (firstPersonVMCamera.Priority < 99)
            {
                firstPersonVMCamera.Priority = 99;
                freelookVMCamera.Priority = 30;
                thirdPersonVMCamera.Priority = 20;
            }
        }

        /// <summary>
        /// Mouse input
        /// </summary>
        /// <param name="context">Callback context</param>
        void OnCameraMouseMovementInput(InputAction.CallbackContext context)
        {
            currentCameraMovementInput = context.ReadValue<Vector2>();
            float mSensitivityX = cameraMovementSensitivityX * Time.deltaTime;
            float mSensitivityY = cameraMovementSensitivityY * Time.deltaTime;
            currentCameraMovement.x = currentCameraMovementInput.x * mSensitivityX;
            currentCameraMovement.z = currentCameraMovementInput.y * mSensitivityY;
        }

        /// <summary>
        /// Stick Input for camera movement
        /// </summary>
        /// <param name="context">Callback context</param>
        void OnCameraMovementStickInput(InputAction.CallbackContext context)
        {
            currentCameraMovementInput = context.ReadValue<Vector2>();
            float mSensitivityX = cameraMovementSensitivityX * Time.deltaTime;
            float mSensitivityY = cameraMovementSensitivityY * Time.deltaTime;
            currentCameraMovement.x = currentCameraMovementInput.x * mSensitivityX;
            currentCameraMovement.z = currentCameraMovementInput.y * mSensitivityY;
            isCameraMovePressed = currentCameraMovementInput.y != 0;
        }

        /// <summary>
        /// Check if left button is pressed (activates look around)
        /// </summary>
        /// <param name="context">Callback context</param>
        void OnEnableMoveCameraInput(InputAction.CallbackContext context)
        {
            isCameraMovePressed = context.ReadValueAsButton();
        }

        /// <summary>
        /// Returns the position of the freelook camera
        /// </summary>
        /// <returns>Position of freelook camera</returns>
        public Vector3 GetFreeLookCameraPosition()
        {
            return freelookVMCamera.transform.position;
        }

        private void LateUpdate()
        {
            if (IsClient && IsOwner)
            {
                if (sticky && !fpvObjectCamActive)
                {
                    if (isCameraMovePressed)
                    {
                        ActivateFreelookCamera(true);
                        freelookVMCamera.m_XAxis.Value += currentCameraMovement.x;
                        freelookVMCamera.m_YAxis.Value += currentCameraMovement.z;
                    }
                }
                else
                {

                    xDeg += currentCameraMovement.x;
                    // set camera rotation

                    // just do the y axis when not fixed height
                    if (!fixedHeight)
                    {
                        yDeg -= (currentCameraMovement.z * 100);
                    }
                    else
                    {
                        yDeg = 0;
                    }
                    Quaternion rotation = Quaternion.Euler(yDeg, xDeg, 0);
                    if ((fpvObjectCamActive && isCameraMovePressed) || !fpvObjectCamActive)
                    {
                        lookAtTarget.rotation = rotation;
                    }

                    if (fixedHeight && lookAtTarget.rotation.x != 0)
                    {
                        if ((fpvObjectCamActive && isCameraMovePressed) || !fpvObjectCamActive)
                        {
                            lookAtTarget.rotation = Quaternion.Lerp(lookAtTarget.rotation, new Quaternion(0, lookAtTarget.rotation.y, lookAtTarget.rotation.z, lookAtTarget.rotation.w), Time.deltaTime);

                        }
                    }

                }
            }

        }
    }
}
