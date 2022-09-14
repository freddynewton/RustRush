using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Cinemachine;
using UnityEngine.UI;

namespace zone.nonon
{
    public class NononPlayerCameraCMNetwork : NetworkBehaviour
    {
        [Header("PlayerFollow")]
        public Transform playerFollow;
        public CinemachineVirtualCamera firstPersonVMCamera;
        public CinemachineVirtualCamera thirdPersonVMCamera;
        public float lookAtMaxDistance = 200f;
        public bool thirdPersonCameraDefault = true;

        CinemachineBrain brain;
        int activeCameraValue = 99;
        int firstPersonVMCameraInactiveValue = 20;
        int thirdPersonVMCameraInactiveValue = 30;

        PlayerInput playerInput;

        [Header("Position")]
        public float distance = 5.0f;
        public float maxDistance = 10;
        public float minDistance = 1f;
        public float zoomRate = 0.1f;
        public int yMinLimit = -40;
        public int yMaxLimit = 80;

        [Header("Rotation")]
        public float zoomDampening = 5.5f;
        public float mouseSensitivity = 2.5f;
        public float mouseSensitivityWhileMouseWalk = 1.5f;
        public float rotationBehingSpeed = 6.0f;

        [Header("AimAndShoot")]
        public Image crossHairWhite;
        public Image crossHairRed;
        public float sideMovementWhenDrawn = 0.5f;


        float xDeg = 0.0f;
        float yDeg = 0.0f;

        Vector2 currentMovementInput;
        Vector3 currentMovement;

        Vector2 currentCameraMovementInput;
        Vector3 currentCameraMovement;
        Vector2 currentzoomMovementInput;
        Vector3 currentzoomMovement;

        bool isCameraMovePressed;
        bool isCharacterRotatePressed;
        bool isStriveLeftPressed;
        bool isStriveRightPressed;
        bool isStriveReleased = false;

        bool easeBehindStarted = false;

        bool isWeaponDrawn = false;
        bool isWeaponAiming = false;

        Transform lockOnTarget = null;

        private void Awake()
        {
            playerInput = new PlayerInput();

            playerInput.CharacterControls.Move.started += OnMovementInput;
            playerInput.CharacterControls.Move.canceled += OnMovementInput;
            playerInput.CharacterControls.Move.performed += OnMovementInput;

            playerInput.CharacterControls.MoveCameraMouse.started += OnCameraMouseMovementInput;
            playerInput.CharacterControls.MoveCameraMouse.canceled += OnCameraMouseMovementInput;
            playerInput.CharacterControls.MoveCameraMouse.performed += OnCameraMouseMovementInput;
            playerInput.CharacterControls.MoveCameraStick.started += OnCameraMovementStickInput;
            playerInput.CharacterControls.MoveCameraStick.canceled += OnCameraMovementStickInput;
            playerInput.CharacterControls.MoveCameraStick.performed += OnCameraMovementStickInput;

            playerInput.CharacterControls.StriveLeft.started += OnStriveLeft;
            playerInput.CharacterControls.StriveLeft.canceled += OnStriveLeft;
            playerInput.CharacterControls.StriveRight.started += OnStriveRight;
            playerInput.CharacterControls.StriveRight.canceled += OnStriveRight;


            playerInput.CharacterControls.EnableMoveCamera.started += OnEnableMoveCameraInput;
            playerInput.CharacterControls.EnableMoveCamera.canceled += OnEnableMoveCameraInput;
            playerInput.CharacterControls.EnableRotateAndMoveCamera.started += OnEnableRotationAndMoveCameraInput;
            playerInput.CharacterControls.EnableRotateAndMoveCamera.canceled += OnEnableRotationAndMoveCameraInput;

            playerInput.CharacterControls.Zoom.started += OnZoomInput;
            playerInput.CharacterControls.Zoom.canceled += OnZoomInput;
            playerInput.CharacterControls.Zoom.performed += OnZoomInput;

            playerInput.CharacterControls.ZoomOut.started += OnZoomOutInput;
            playerInput.CharacterControls.ZoomOut.canceled += OnZoomOutInput;

            playerInput.CharacterControls.ZoomIn.started += OnZoomInInput;
            playerInput.CharacterControls.ZoomIn.canceled += OnZoomInInput;

            playerInput.CharacterControls.CameraSwitch.started += OnCameraSwitchInput;
            //playerInput.CharacterControls.LockCameraOntoTarget.started += OnIsLockAtTargetInput; // make a binding if you want to have a lock mechanism

            playerInput.CharacterControls.Enable();

            if (Camera.main != null)
            {
                brain = Camera.main.GetComponent<CinemachineBrain>();
            }

        }

        public void SetIsWeaponDrawn(bool _isWeaponDrawn)
        {
            isWeaponDrawn = _isWeaponDrawn;
        }

        public void SetIsWeaponAimingAtTarget(bool _isWeaponAiming)
        {
            isWeaponAiming = _isWeaponAiming;
        }

        void Start()
        {
            // if netowrking is active, deactivate Audio Listener and Camera for non Owner Objects
            if (!IsOwner)
            {
                GetComponent<AudioListener>().enabled = false;
                GetComponent<Camera>().enabled = false;
                GetComponent<CinemachineBrain>().enabled = false;
                thirdPersonVMCamera.GetComponent<CinemachineVirtualCamera>().enabled = false;
                firstPersonVMCamera.GetComponent<CinemachineVirtualCamera>().enabled = false;
                thirdPersonVMCamera.GetComponent<Cinemachine3rdPersonAim>().enabled = false;
                firstPersonVMCamera.GetComponent<Cinemachine3rdPersonAim>().enabled = false;
                firstPersonVMCamera.enabled = false;
                thirdPersonVMCamera.enabled = false;
            }
            else
            {
                if (thirdPersonCameraDefault)
                {
                    thirdPersonVMCamera.Priority = activeCameraValue;
                    firstPersonVMCamera.Priority = firstPersonVMCameraInactiveValue;
                }
                else
                {
                    firstPersonVMCamera.Priority = activeCameraValue;
                    thirdPersonVMCamera.Priority = thirdPersonVMCameraInactiveValue;
                }
            }

            Vector3 angles = playerFollow.eulerAngles;
            xDeg = angles.x;
            yDeg = angles.y;

            GetThirdPersonCameraBody().CameraDistance = distance;
        }

        private Cinemachine3rdPersonFollow GetThirdPersonCameraBody()
        {
            CinemachineComponentBase componentBase = thirdPersonVMCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
            if (componentBase is Cinemachine3rdPersonFollow)
            {
                return (componentBase as Cinemachine3rdPersonFollow);
            }
            else
            {
                return null;
            }
        }

        public void SetMouseSensitivity(float value)
        {
            mouseSensitivity = value;
        }

        void OnStriveLeft(InputAction.CallbackContext context)
        {
            isStriveLeftPressed = context.ReadValueAsButton();
            if (!isStriveLeftPressed)
            {
                isStriveReleased = true;
            }
        }


        void OnStriveRight(InputAction.CallbackContext context)
        {
            isStriveRightPressed = context.ReadValueAsButton();
            if (!isStriveRightPressed)
            {
                isStriveReleased = true;
            }
        }

        void OnMovementInput(InputAction.CallbackContext context)
        {
            currentMovementInput = context.ReadValue<Vector2>();
            currentMovement.x = currentMovementInput.x;
            currentMovement.z = currentMovementInput.y;
        }

        void OnCameraMouseMovementInput(InputAction.CallbackContext context)
        {
            currentCameraMovementInput = context.ReadValue<Vector2>();
            float mSensitivity = mouseSensitivity;
            if (isCameraMovePressed && isCharacterRotatePressed)
            {
                mSensitivity = mouseSensitivityWhileMouseWalk;
            }
            mSensitivity *= Time.deltaTime * 80;
            currentCameraMovement.x = currentCameraMovementInput.x * mSensitivity;
            currentCameraMovement.z = currentCameraMovementInput.y * mSensitivity * 0.5f;
        }

        void OnCameraMovementStickInput(InputAction.CallbackContext context)
        {
            currentCameraMovementInput = context.ReadValue<Vector2>();
            float mSensitivity = mouseSensitivity;
            mSensitivity *= Time.deltaTime * 80;
            currentCameraMovement.x = currentCameraMovementInput.x * mouseSensitivity;
            currentCameraMovement.z = currentCameraMovementInput.y * mouseSensitivity * 0.5f;
            isCameraMovePressed = currentCameraMovementInput.y != 0;
        }


        void OnEnableMoveCameraInput(InputAction.CallbackContext context)
        {
            isCameraMovePressed = context.ReadValueAsButton();
        }

        void OnCameraSwitchInput(InputAction.CallbackContext context)
        {
            if (IsOwner)
            {
                DoCameraSwitch();
            }
        }

        private void DoCameraSwitch()
        {
            if (thirdPersonVMCamera.GetComponent<CinemachineVirtualCamera>().Priority == activeCameraValue)
            {
                firstPersonVMCamera.GetComponent<CinemachineVirtualCamera>().Priority = activeCameraValue;
                thirdPersonVMCamera.GetComponent<CinemachineVirtualCamera>().Priority = thirdPersonVMCameraInactiveValue;
            }
            else
            {
                thirdPersonVMCamera.GetComponent<CinemachineVirtualCamera>().Priority = activeCameraValue;
                firstPersonVMCamera.GetComponent<CinemachineVirtualCamera>().Priority = firstPersonVMCameraInactiveValue;
            }
        }

        void OnEnableRotationAndMoveCameraInput(InputAction.CallbackContext context)
        {
            isCharacterRotatePressed = context.ReadValueAsButton();
        }

        void OnIsLockAtTargetInput(InputAction.CallbackContext context)
        {
            if (IsOwner) SetLockOnTarget();
        }

        void SetLockOnTarget()
        {
            Vector3 referenceLookAt = brain.ActiveVirtualCamera.State.ReferenceLookAt;
            RaycastHit hit;
            if (Physics.Raycast(referenceLookAt, referenceLookAt.normalized, out hit, 20))
            {
                if (NononZoneObjectNetwork.isOneOfTypes(hit.transform, INononZoneObject.NononZoneObjType.DESTROYABLE))
                {
                    lockOnTarget = hit.transform;
                }
                else
                {
                    lockOnTarget = null;
                }
            }
            else
            {
                lockOnTarget = null;
            }
        }

        void OnZoomInput(InputAction.CallbackContext context)
        {
            currentzoomMovementInput = context.ReadValue<Vector2>();
            currentzoomMovement.y = currentzoomMovementInput.y;
        }

        void OnZoomOutInput(InputAction.CallbackContext context)
        {
            bool isBtnPressed = context.ReadValueAsButton();
            if (isBtnPressed)
            {
                currentzoomMovement.y = zoomRate;
            }
            else
            {
                currentzoomMovement.y = 0.0f;
            }
        }

        void OnZoomInInput(InputAction.CallbackContext context)
        {
            bool isBtnPressed = context.ReadValueAsButton();
            if (isBtnPressed)
            {
                currentzoomMovement.y = zoomRate * -1f;
            }
            else
            {
                currentzoomMovement.y = 0.0f;
            }

        }

        void ClampZoomMovement()
        {
            if (GetThirdPersonCameraBody().CameraDistance > maxDistance)
            {
                GetThirdPersonCameraBody().CameraDistance = maxDistance;
            }
            if (GetThirdPersonCameraBody().CameraDistance < minDistance)
            {
                GetThirdPersonCameraBody().CameraDistance = minDistance;
            }
        }

        void EaseBehindTarget()
        {
            float targetRotationAngle = transform.parent.eulerAngles.y;
            float currentRotationAngle = playerFollow.eulerAngles.y;

            if (isStriveReleased)
            {
                xDeg = targetRotationAngle;
                isStriveReleased = false;
            }
            else
            {
                xDeg = Mathf.LerpAngle(currentRotationAngle, targetRotationAngle, rotationBehingSpeed * Time.deltaTime);
            }
        }

        void HandleAiming()
        {
            if (isWeaponDrawn)
            {
                GetThirdPersonCameraBody().CameraSide = 0.5f + sideMovementWhenDrawn;
            }
            else
            {
                GetThirdPersonCameraBody().CameraSide = 0.5f;
                crossHairWhite.gameObject.SetActive(false);
                crossHairRed.gameObject.SetActive(false);
            }

            if (isWeaponAiming && isWeaponDrawn)
            {
                crossHairWhite.gameObject.SetActive(false);
                crossHairRed.gameObject.SetActive(true);

            }
            else if (isWeaponDrawn)
            {
                crossHairWhite.gameObject.SetActive(true);
                crossHairRed.gameObject.SetActive(false);
            }

        }

        void HandleLockOnTarget()
        {
            if (lockOnTarget != null)
            {
                playerFollow.rotation = Quaternion.LookRotation(lockOnTarget.position - playerFollow.position);
            }
        }

        /**
         * Camera logic on LateUpdate to only update after all character movement logic has been handled.
         */
        void LateUpdate()
        {


            GetThirdPersonCameraBody().CameraDistance += currentzoomMovement.y;
            ClampZoomMovement();

            if (isCameraMovePressed || isCharacterRotatePressed)
            {
                xDeg += currentCameraMovement.x;
                yDeg -= currentCameraMovement.z;
                easeBehindStarted = false;
            }
            // otherwise, ease behind the target if any of the directional keys are pressed WHEN the char begins to move
            else if ((currentMovement.x > 0 || currentMovement.x < 0 || currentMovement.z > 0 || currentMovement.z < 0)
                && !isCameraMovePressed && !isCharacterRotatePressed && !isStriveLeftPressed && !isStriveRightPressed)
            {
                easeBehindStarted = true;
            }

            //HandleLockOnTarget(); enable this when you want to have a lock mechanism
            HandleAiming();

            // network adjust
            // As long as the character is rotating without moving easy further until easing is finished
            if (easeBehindStarted && !isCameraMovePressed && !isCharacterRotatePressed && !isStriveLeftPressed && !isStriveRightPressed)
            {
                EaseBehindTarget();
            }
            else
            {
                easeBehindStarted = false;
            }

            yDeg = ClampAngle(yDeg, yMinLimit, yMaxLimit);

            // set camera rotation
            Quaternion rotation = Quaternion.Euler(yDeg, xDeg, 0);
            /*if (lockOnTarget == null)
            {*/
            playerFollow.rotation = rotation;
            //}

        }

        private static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360)
                angle += 360;
            if (angle > 360)
                angle -= 360;
            return Mathf.Clamp(angle, min, max);
        }
    }
}