using UnityEngine;


// Script taken and evolved from: https://www.youtube.com/watch?v=KkYco_7-ULA&t=438s.

namespace CameraController
{
    [RequireComponent(typeof(CameraMovementInputManager))]
    public class CameraKeyboardInput : MonoBehaviour
    {
        public CameraMovementInputManager cm;

        [Header("Pan")]
        public float amountToPan = 0.3f;            // how far camera will go on WASD press
        public float panLerpSpeed = 10;             // the speed (frames) at which it will take to get there
        private Vector3 targetPanPosition;
        private bool panTowardsNewPosition;

        [Header("Rotation")]
        public float rotationSpeed = 0.3f;
        public bool smoothRotation = true;
        public bool invertHorizontalRotation;
        public bool invertVerticalRotation;
        private Vector3 pointToOrbitRotation;
        private Quaternion targetRotation;
        private bool rotateToNewPosition;
        [SerializeField] private float rotationX;
        [SerializeField] private float rotationY;

        [Header("Zoom")]
        public float amountToZoom = 8;
        public bool smoothZoom = true;
        private float targetZoomDistance;
        private bool zoomToNewPosition;

        private Vector3 mousePositionLastFrame;

        public Vector3 cameraPositionLastFrame;

        private void Awake()
        {
            if (GetComponent<Camera>() == null)
                cm.mainCamera = Camera.main;

            cm.PrepareGroundPlane();

            pointToOrbitRotation = cm.GetOrbitPointForRotation();
            targetZoomDistance = Vector3.Distance(transform.position, pointToOrbitRotation);
            targetRotation = transform.rotation;
        }

        private void Start()
        {
            // So that variables don't start at 0. Otherwise camera will jump to a really low rotation (almost flat on ground) on button press.
            cm.SyncVariablesWithCamerasCurrentRotation(ref rotationX, ref rotationY);

            // If player zooms at start of game before ever rotating, zoom needs this point to be defined
            pointToOrbitRotation = cm.GetOrbitPointForRotation();

            // So that it's not null for first frame
            cameraPositionLastFrame = transform.position;
        }

        private void Update()
        {
            // if (PauseButton.GamePaused)
            //     return;

            // Touches count as mouse clicks.So if this script is enabled and touch script is enabled, Unity will detect two clicks per frame which throws off calculations
#if (UNITY_EDITOR)
            if (Input.touchCount != 0)
                return;

            // Prevents rotationX and rotationY from changing when holding alt to rotate around a game object in the Unity editor.
            if (Input.GetKeyDown(KeyCode.LeftAlt) && MousePointerInGameWindow() == false)
                return;
#endif

            GetMouseInput();
            GetKeyboardInput();

            MoveCameraToTargets();
        }


        private void GetMouseInput()
        {
            if (Input.GetMouseButtonDown(2) || Input.GetAxis("Mouse ScrollWheel") != 0f)
            {
                targetPanPosition = transform.position;
                panTowardsNewPosition = false;
            }

            // *** Zoom ***
            if (Input.GetAxis("Mouse ScrollWheel") != 0f)
            {
                if (MousePointerInGameWindow() == false)
                    return;

                if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                {
                    targetZoomDistance -= amountToZoom;
                    targetZoomDistance = ClampZoomLevel();
                }

                else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
                {
                    targetZoomDistance += amountToZoom;
                    targetZoomDistance = ClampZoomLevel();
                }

                zoomToNewPosition = true;
            }

            // *** Pan ***
            if (cm.AllowDragToPan || (Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftCommand)))
            {
                if (Input.GetMouseButton(0) || (Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftCommand)))
                {
                    rotateToNewPosition = false;

                    if (Input.GetMouseButtonDown(0))
                    {
                        mousePositionLastFrame = Input.mousePosition;
                        panTowardsNewPosition = false;
                        targetPanPosition = transform.position;
                    }

                    Vector3 lastMousePosition = cm.ScreenToPlanePosition(mousePositionLastFrame);
                    Vector3 currentMousePosition = cm.ScreenToPlanePosition(Input.mousePosition);
                    Vector3 newCameraPosition = lastMousePosition - currentMousePosition;

                    transform.Translate(newCameraPosition, Space.World);
                }

                if (Input.GetMouseButtonUp(0))
                {
                    // Flicks camera in position it was going after player has let go of screen (or mouse button)
                    Vector3 lastMousePosition = cm.ScreenToPlanePosition(mousePositionLastFrame);
                    Vector3 currentMousePosition = cm.ScreenToPlanePosition(Input.mousePosition);
                    float cameraCurrentSpeed = (currentMousePosition - lastMousePosition).magnitude;
                    //Debug.Log(cameraCurrentSpeed);
                    //if (cameraCurrentSpeed > 100)
                    //    cameraCurrentSpeed = 100;
                    Vector3 cameraDirection = lastMousePosition - currentMousePosition;
                    cameraDirection.Normalize();
                    targetPanPosition = transform.position + cameraDirection * cameraCurrentSpeed;
                    panTowardsNewPosition = true;
                }
            }

            // *** Rotate ***
            if (Input.GetMouseButton(2) || (Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.LeftCommand)))
            {
                if (Input.GetMouseButtonDown(2) || Input.GetKeyDown(KeyCode.LeftAlt))
                    mousePositionLastFrame = Input.mousePosition;

                Vector3 currentMousePosition = Input.mousePosition;
                Vector3 deltaMousePositon = currentMousePosition - mousePositionLastFrame;

                if (invertHorizontalRotation)
                    rotationX -= deltaMousePositon.x * rotationSpeed;
                else
                    rotationX += deltaMousePositon.x * rotationSpeed;

                if (invertVerticalRotation)
                    rotationY += deltaMousePositon.y * rotationSpeed;
                else
                    rotationY -= deltaMousePositon.y * rotationSpeed;

                SetCameraRotation(ref rotationX, ref rotationY);
            }

            mousePositionLastFrame = Input.mousePosition;
            cameraPositionLastFrame = transform.position;
        }

        // Returns true if mouse pointer is over game window in Unity. False if not.
        private bool MousePointerInGameWindow()
        {
            var mousePosition = GetComponent<Camera>().ScreenToViewportPoint(Input.mousePosition);
            if (mousePosition.x < 0 || mousePosition.x > 1 || mousePosition.y < 0 || mousePosition.y > 1)
                return false;

            return true;
        }




        private float ClampZoomLevel()
        {
            if (targetZoomDistance > cm.maxAllowedZoom)
                targetZoomDistance = cm.maxAllowedZoom;

            else if (targetZoomDistance < cm.minAllowedZoom)
                targetZoomDistance = cm.minAllowedZoom;

            //Debug.Log(targetZoomDistance);
            return targetZoomDistance;
        }



        // WASD keys to pan camera around.
        private void GetKeyboardInput()
        {
            // If no movement keys are pressed, exit function
            if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D))
                return;


            Vector3 cameraForwardDir = GetAdjustedForwardDirection();
            float panAmount = GetKeyboardPanAmount(amountToPan);

            if (Input.GetKey(KeyCode.W))
            {
                targetPanPosition = transform.position + cameraForwardDir * panAmount;
            }

            if (Input.GetKey(KeyCode.A))
            {
                targetPanPosition = transform.position + (Quaternion.Euler(0, 270, 0) * cameraForwardDir) * panAmount;
            }

            if (Input.GetKey(KeyCode.S))
            {
                targetPanPosition = transform.position + (Quaternion.Euler(0, 180, 0) * cameraForwardDir) * panAmount;

            }

            if (Input.GetKey(KeyCode.D))
            {
                targetPanPosition = transform.position + (Quaternion.Euler(0, 90, 0) * cameraForwardDir) * panAmount;
            }



            // up right
            if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.D))
            {
                targetPanPosition = transform.position + (Quaternion.Euler(0, 45, 0) * cameraForwardDir) * panAmount;
            }

            // down right
            if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.D))
            {
                targetPanPosition = transform.position + (Quaternion.Euler(0, 135, 0) * cameraForwardDir) * panAmount;
            }

            // down left
            if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.A))
            {
                targetPanPosition = transform.position + (Quaternion.Euler(0, 225, 0) * cameraForwardDir) * panAmount;
            }

            // up left
            if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A))
            {
                targetPanPosition = transform.position + (Quaternion.Euler(0, 315, 0) * cameraForwardDir) * panAmount;
            }


            panTowardsNewPosition = true;
        }

        // Gets camera's forward direction and sets y axis to 0, so it never moves higher or lower, only forward/backwards
        private Vector3 GetAdjustedForwardDirection()
        {
            Vector3 cameraForwardDir = transform.forward;
            cameraForwardDir.y = 0;

            return cameraForwardDir;
        }

        // Camera will pan faster or slower depending on how close to the ground it is
        private float GetKeyboardPanAmount(float panAmount)
        {
            return panAmount * transform.position.y;
        }







        // Set target rotation that camera will lerp towards
        // Use ref to make sure the rotation's new values when clamped are set globally so they work in Update().
        // Otherwise if the max rotation y is 90 and rotationY is 120, player will need to rotate finger down 30 degrees before camera starts rotating again.
        void SetCameraRotation(ref float rotationX, ref float rotationY)
        {
            // Clamp rotation -- don't clamp if debug is true
            if (cm.debug_ignoreRotationBoundaries == false)
                rotationY = cm.ClampRotationAngle(rotationY, cm.lowestAllowedRotationY, cm.highestAllowedRotationY);

            targetRotation = Quaternion.Euler(rotationY, rotationX, 0);
            rotateToNewPosition = true;
        }




        // Called every frame. Lerps camera towards target positions (panning, zooming, orbiting)
        private void MoveCameraToTargets()
        {
            // Note: If player uses WASD to pan and then rotates at same time, camera will never reach targetPanPosition, which is why camera freaks out.
            if (panTowardsNewPosition == true)
                panTowardsNewPosition = cm.PanCameraToNewPanPosition(targetPanPosition, panLerpSpeed);

            if (zoomToNewPosition == true || rotateToNewPosition == true)
            {
                pointToOrbitRotation = cm.GetOrbitPointForRotation();
                float thisFrameZoomDistanceFromOrbitPoint = GetCameraZoom(pointToOrbitRotation);
                Quaternion thisFrameRotation = GetCameraRotation();

                transform.rotation = thisFrameRotation;
                transform.position = pointToOrbitRotation - (transform.rotation * Vector3.forward * thisFrameZoomDistanceFromOrbitPoint);

                if (thisFrameZoomDistanceFromOrbitPoint == targetZoomDistance)
                    zoomToNewPosition = false;

                if (transform.rotation == targetRotation)
                    rotateToNewPosition = false;
            }
        }

        // Camera physically moves closer/further from target when zooming.
        // Function returns camera's next position for this frame, if there is one
        // If there is no new target to zoom towards, return the current zoom distance
        private float GetCameraZoom(Vector3 pointToOrbitRotation)
        {
            if (zoomToNewPosition == false)
                return targetZoomDistance;


            float currentZoomDistanceFromOrbitPoint = Vector3.Distance(transform.position, pointToOrbitRotation);
            float thisFrameZoomDistance;

            if (smoothZoom)
                thisFrameZoomDistance = Mathf.Lerp(currentZoomDistanceFromOrbitPoint, targetZoomDistance, Time.deltaTime * 10);
            else
                thisFrameZoomDistance = targetZoomDistance;

            return thisFrameZoomDistance;
        }

        // Returns camera's rotation for this frame
        private Quaternion GetCameraRotation()
        {
            if (rotateToNewPosition == false)
                return transform.rotation;


            Quaternion thisFrameRotation;

            if (smoothRotation)
                thisFrameRotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10);
            else
                thisFrameRotation = targetRotation;

            // Makes sure x and y are always 0 (otherwise lerp moves camera in slightly weird ways)
            Vector3 toEuler = thisFrameRotation.eulerAngles;
            thisFrameRotation = Quaternion.Euler(toEuler.x, toEuler.y, 0);

            return thisFrameRotation;
        }




        public float GetCameraSpeed(Vector3 cameraPositionLastFrame)
        {
            //Vector3 touchPositionLastFrame = Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition;
            float cameraCurrentSpeed = (transform.position - cameraPositionLastFrame).magnitude;

            if (cameraCurrentSpeed > 100)
                cameraCurrentSpeed = 100;

            //Debug.Log(cameraCurrentSpeed);

            return cameraCurrentSpeed;
        }
    }

}