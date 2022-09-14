using UnityEngine;
using UnityEngine.InputSystem;

namespace zone.nonon
{
    /***
     * Camera Movement. This class is tightly coupled with The AnimationMovementController and the Player Movement 
     * 
     * */
    public class NononPlayerCamera : MonoBehaviour
    {
        Transform target;
        PlayerInput playerInput;

        LayerMask collisionLayers = -1;

        [Header("Position")]
        public float targetHeight = 2.5f;
        public float distance = 5.0f;
        public float offsetFromWall = 0.2f;
        public float maxDistance = 10;
        public float minDistance = 2f;
        public float speedDistance = 2.0f;
        public int zoomRate = 2;
        public int yMinLimit = -40;
        public int yMaxLimit = 80;
        [Header("Rotation")]
        public float zoomDampening = 5.5f;
        public float rotationBehingSpeed = 6.0f;
        public float mouseSensitivity = 2.5f;
        public float mouseSensitivityWhileMouseWalk = 1.5f;
        public float xDeg = 0.0f;
        public float yDeg = 0.0f;

        float currentDistance;
        float desiredDistance;
        float correctedDistance;


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


            playerInput.CharacterControls.Enable();

        }

        void Start()
        {
            Vector3 angles = transform.eulerAngles;
            xDeg = angles.x;
            yDeg = angles.y;

            currentDistance = distance;
            desiredDistance = distance;
            correctedDistance = distance;

            // Make the rigid body not change rotation
            if (this.gameObject.GetComponent<Rigidbody>())
                this.gameObject.GetComponent<Rigidbody>().freezeRotation = true;

            target = transform.parent;
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

        void OnEnableRotationAndMoveCameraInput(InputAction.CallbackContext context)
        {
            isCharacterRotatePressed = context.ReadValueAsButton();
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
                currentzoomMovement.y = 0.2f;
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
                currentzoomMovement.y = -0.2f;
            }
            else
            {
                currentzoomMovement.y = 0.0f;
            }
        }

        void EaseBehindTarget()
        {
            float targetRotationAngle = target.eulerAngles.y;
            float currentRotationAngle = transform.eulerAngles.y;

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

        /**
         * Camera logic on LateUpdate to only update after all character movement logic has been handled.
         */
        void LateUpdate()
        {

            Vector3 vTargetOffset;

            // Don't do anything if target is not defined
            if (!target)
                return;

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
                //EaseBehindTarget();
            }

            // network adjust
            // As long as the character is rotating without moving easy further until easing is finished
            if (easeBehindStarted && !isCameraMovePressed && !isCharacterRotatePressed && !isStriveLeftPressed && !isStriveRightPressed)
            {
                EaseBehindTarget();
                if (xDeg == 0)
                {
                    easeBehindStarted = false;
                }
            } else
            {
                easeBehindStarted = false;
            }
             
            // calculate the desired distance        
            desiredDistance -= currentzoomMovement.y * Time.deltaTime * zoomRate * Mathf.Abs(desiredDistance) * speedDistance;
            desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);

            yDeg = ClampAngle(yDeg, yMinLimit, yMaxLimit);

            // set camera rotation
            Quaternion rotation = Quaternion.Euler(yDeg, xDeg, 0);
            correctedDistance = desiredDistance;

            // calculate desired camera position
            vTargetOffset = new Vector3(0, -targetHeight, 0);
            Vector3 position = target.position - (rotation * Vector3.forward * desiredDistance + vTargetOffset);

            // check for collision using the true target's desired registration point as set by user using height
            RaycastHit collisionHit;
            Vector3 trueTargetPosition = new Vector3(target.position.x, target.position.y, target.position.z) - vTargetOffset;

            // if there was a collision, correct the camera position and calculate the corrected distance
            bool isCorrected = false;
            if (Physics.Linecast(trueTargetPosition, position, out collisionHit, collisionLayers.value))
            {
                // calculate the distance from the original estimated position to the collision location,
                // subtracting out a safety "offset" distance from the object we hit.  The offset will help
                // keep the camera from being right on top of the surface we hit, which usually shows up as
                // the surface geometry getting partially clipped by the camera's front clipping plane.
                correctedDistance = Vector3.Distance(trueTargetPosition, collisionHit.point) - offsetFromWall;
                isCorrected = true;
            }

            // For smoothing, lerp distance only if either distance wasn't corrected, or correctedDistance is more than currentDistance
            currentDistance = !isCorrected || correctedDistance > currentDistance ? Mathf.Lerp(currentDistance, correctedDistance, Time.deltaTime * zoomDampening) : correctedDistance;

            // keep within legal limits
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);

            // recalculate position based on the new currentDistance
            position = target.position - (rotation * Vector3.forward * currentDistance + vTargetOffset);

            transform.position = position;
            transform.rotation = rotation;

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