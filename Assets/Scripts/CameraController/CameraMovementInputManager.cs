using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEditor;
using CameraController;

namespace CameraController
{
    public class CameraMovementInputManager : MonoBehaviour
    {
        public CameraKeyboardInput cameraKeyboardInput;
        public CameraTouchInput cameraTouchInput;
        //public TerrainGridData terrain;
        public TerrainGenerator terrain;

        public Camera mainCamera;
        public Plane groundPlane;
        public Plane cameraPlane;


        // The highest and lowest camera can rotate around the orbit point on its y axis (otherwise camera can do 360 loops)
        [Header("Rotation Bounds")]
        public int highestAllowedRotationY = 80;
        public int lowestAllowedRotationY = 30;

        // How far camera can zoom towards or away from its orbit point
        [Header("Zoom Bounds")]
        public float maxAllowedZoom = 100;
        public float minAllowedZoom = 20;

        // When player is building a road and drags the end of it near the edge of the screen, these variables move camera in that direction
        [Header("Drag Road To Edge Of Screen")]
        public int dragEndOfRoad_EdgeScreenInputRange = 40;
        public float dragEndOfRoad_EdgeScreenPanSpeed = 0.3f;

        [Header("Debug")]
        public bool allowDragToPan = true;
        public bool debug_ignoreRotationBoundaries;
        public bool debug_ignoreZoomBoundaries;
        public bool debug_ignorePanBoundaries;
        public bool sceneCameraFollowGameCamera;
        public bool debug_rotationOrbitPoint;

        // Used to reset camera position if needed
        private Vector3 cameraStartingPosition;
        private Quaternion cameraStartingRotation;

        // Disabled when single finger input is needed for something else (such as building a road)
        //public bool AllowDragToPan { get; set; } = true;
        public bool AllowDragToPan { set { allowDragToPan = value; } get { return allowDragToPan; } }

        // Call this instead of Unity's Camera.Main, because that is liken to using GetComponent()
        public Camera MainCamera { get => mainCamera; }






        public void ResetDebugBools()
        {
            sceneCameraFollowGameCamera = false;

            Debug.Log("Resetting bools in CameraMovementInputManager!");
        }



        // Note: If you build game with both scripts enabled, Unity will detect touch as a mouse click and a finger click,
        // meaning touch will register twice per frame and throw off calculations.
        protected void Awake()
        {
            if (GetComponent<Camera>() == null)
                mainCamera = Camera.main;

            PrepareGroundPlane();
            UpdateCameraPlanePosition();

            cameraStartingPosition = transform.position;
            cameraStartingRotation = transform.rotation;


#if (UNITY_EDITOR)

            //cameraKeyboardInput.enabled = true;
            //cameraTouchInput.enabled = true;

#elif (UNITY_ANDROID || UNITY_IOS)

        cameraTouchInput.enabled = true;
        cameraKeyboardInput.enabled = false;

#else

        cameraKeyboardInput.enabled = true;
        cameraTouchInput.enabled = false;

#endif
        }



#if (UNITY_EDITOR)
        protected void Update()
        {
            if (sceneCameraFollowGameCamera)
                SceneCameraFollowGameCamera();
        }





        // Unity Editor mainCamera will move and rotate with the game mainCamera as player moves it.
        private void SceneCameraFollowGameCamera()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                sceneCameraFollowGameCamera = false;
                Debug.Log("sceneCameraFollowGameCamera set to false");
            }

            var sceneCamera = SceneView.lastActiveSceneView;
            if (sceneCamera != null)
            {
                sceneCamera.AlignViewToObject(transform);
                return;
            }

            if (sceneCamera == null)
                return;
        }
#endif













        // Plane acts as the ground and will be used to shoot raycasts at (because plane.RayCast() is cheaper that Physics.Raycast()).
        // Its position is the ground's transform position, and it is flat along the y axis.
        // Rays can hit it even if there is no ground there (meaning the mainCamera is out of bounds, staring at the void).
        // Plane is invisible and does not show as a game object in the hierarchy.
        public void PrepareGroundPlane()
        {
            groundPlane.SetNormalAndPosition(Vector3.up, terrain.gameObject.transform.position);
        }


        // Used for finger raycasts that shoot into the sky if the camera's orbit is really low and misses the ground.
        // Used mainly for pinch to zoom and rotation gestures
        // Spawns at the orbit point and rotates with camera.
        public void UpdateCameraPlanePosition()
        {
            cameraPlane.SetNormalAndPosition(-transform.forward, cameraTouchInput.pointToOrbitRotation);
        }

        // When camera gets stuck, reset its position and rotation to its starting positin
        public void ResetCameraPosition()
        {
            transform.position = cameraStartingPosition;
            transform.rotation = cameraStartingRotation;
        }

        // Plane is a game object that acts as the ground but is flat along the y axis.
        // Shoots ray from where finger is touching screen to the plane and returns position.
        public Vector3 ScreenToPlanePosition(Vector2 screenPos, int plane = 0)
        {
            if (plane == 0)
            {
                Ray rayStartPointAndDirection = mainCamera.ScreenPointToRay(screenPos);
                if (groundPlane.Raycast(rayStartPointAndDirection, out var planeHitPoint))
                    return rayStartPointAndDirection.GetPoint(planeHitPoint);
            }
            else if (plane == 1)
            {
                Ray rayStartPointAndDirection = mainCamera.ScreenPointToRay(screenPos);
                if (cameraPlane.Raycast(rayStartPointAndDirection, out var planeHitPoint))
                    return rayStartPointAndDirection.GetPoint(planeHitPoint);
            }

            // ray missed
            return Vector3.zero;
        }



        // Syncs variables to camera's current rotation so that it doesn't jump.
        // De-sync will happen when player uses two finger circle gesture to rotate, which is different from other gestures to orbit.
        public void SyncVariablesWithCamerasCurrentRotation(ref float rotationX, ref float rotationY)
        {
            Vector3 currentRotation = transform.rotation.eulerAngles;
            rotationX = currentRotation.y;
            rotationY = currentRotation.x;
        }



        // Shoots ray from center of screen to plane. The hit point is the position mainCamera will rotate around.
        public Vector3 GetOrbitPointForRotation()
        {
            Vector3 pointToOrbitRotation = Vector3.zero;

            Ray fromCenterOfScreenToWorld = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if (groundPlane.Raycast(fromCenterOfScreenToWorld, out float rayDistanceTillHit))
            {
                pointToOrbitRotation = fromCenterOfScreenToWorld.GetPoint(rayDistanceTillHit);

#if (UNITY_EDITOR)
                if (debug_rotationOrbitPoint)
                    DebugExtension.DebugWireSphere(pointToOrbitRotation, Color.blue, 4, 0.3f);
#endif

            }
            else
            {
                PrepareGroundPlane();
                throw new System.Exception("Camera doesn't know what point to orbit around because raycast didn't hit plane. It could be that this function was called before the plane was created.");
            }

            return pointToOrbitRotation;
        }


        // Forces rotation to be within these numbers. (Prevents mainCamera from going too far under ground, or from doing 360 degree flips).
        public float ClampRotationAngle(float currentAngle, float allowedMinimum, float allowedMaximum)
        {
            if (currentAngle < -360)
                currentAngle += 360;
            if (currentAngle > 360)
                currentAngle -= 360;

            return Mathf.Clamp(currentAngle, allowedMinimum, allowedMaximum);
        }


        // If player pans the mainCamera fast and lets go of screen,
        // Creates point mainCamera would have gone to if player continued panning at that speed in that direction.
        // In another function, mainCamera will lerp to this position to simulate momentum after player lets go of screen.
        public Vector3 SetMomentumPosition(Vector3 touchPositionBefore, Vector3 touchPositionNow, float cameraCurrentSpeed)
        {
            Vector3 cameraPanDirection = touchPositionBefore - touchPositionNow;
            Vector3 momentumPosition = transform.position + cameraPanDirection * cameraCurrentSpeed;
            return momentumPosition;
        }

        // Returns false if camera *is* at it target positions.
        // Camera will lerp towards pan position each frame, 
        // whether position is really close (WASD) or really far (from player dragging to pan and letting go really quickly)
        // This function creates point mainCamera would have gone to if player continued panning at that speed in that direction.
        // (see this gif to see how lerp works: https://www.reddit.com/r/gamedev/comments/cayb4f/basic_smooth_spring_movement/)
        public bool PanCameraToNewPanPosition(Vector3 targetPanPosition, float lerpSpeed)
        {
            if (VecCalc.CompareVectors(transform.position, targetPanPosition))
                return false;

            Vector3 nextFramePosition = Vector3.Lerp(transform.position, targetPanPosition, lerpSpeed * Time.deltaTime);
            Vector3 orbitPoint = ConvertNewPositionToOrbitPoint(nextFramePosition);
            if (CheckIfOrbitPointInBounds(orbitPoint))
            {
                transform.position = nextFramePosition;
                return true;
            }
            else
            {
                return false;
            }
        }

        // Takes the position camera wants to move to and shoots a ray from there to the ground to get what will be next frame's orbit point.
        // (ray start point is position camera will be at. ray's direction is the camera's forward direction.)
        public Vector3 ConvertNewPositionToOrbitPoint(Vector3 nextFramePosition)
        {
            Vector3 groundOrbitPoint = Vector3.zero;

            Ray newRay = new Ray();
            newRay.origin = nextFramePosition;
            newRay.direction = transform.forward;
            if (groundPlane.Raycast(newRay, out var planeHitPoint))
            {
                groundOrbitPoint = newRay.GetPoint(planeHitPoint);
                //DebugExtension.DebugWireSphere(groundOrbitPoint, Color.red, 4, 1000);
            }
            else
            {
                groundOrbitPoint = transform.position;
            }

            return groundOrbitPoint;
        }

        // Returns true if point camera is rotating around is in map boundaries.
        // Camera itself can go out of bounds while rotating around orbit point, but the orbit point must always be in bounds.
        public bool CheckIfOrbitPointInBounds(Vector3 orbitPoint)
        {
            if (debug_ignorePanBoundaries)
                return true;

            return terrain.PositionInMapBounds(orbitPoint);
        }







        // Moves camera if touch position is near edge of screen
        public void TouchEdgeOfScreenMoveCamera(Vector3 clickPosition)
        {
            if (MouseAtEdgeOfScreen())
            {
                Vector3 centerOfGround = GetOrbitPointForRotation();
                Vector3 direction = (clickPosition - centerOfGround).normalized;

                Vector3 newPosition = transform.position;
                newPosition += direction * dragEndOfRoad_EdgeScreenPanSpeed;
                Vector3 newPositionOrbitPoint = ConvertNewPositionToOrbitPoint(newPosition);
                if (CheckIfOrbitPointInBounds(newPositionOrbitPoint))
                    transform.position += direction * dragEndOfRoad_EdgeScreenPanSpeed;
            }
        }

        // Use mouse's pixel coordinates to determine if at edge of screen
        private bool MouseAtEdgeOfScreen()
        {
            if (Input.mousePosition.x < dragEndOfRoad_EdgeScreenInputRange || Input.mousePosition.x > Screen.width - dragEndOfRoad_EdgeScreenInputRange)
                return true;

            else if (Input.mousePosition.y < dragEndOfRoad_EdgeScreenInputRange || Input.mousePosition.y > Screen.height - dragEndOfRoad_EdgeScreenInputRange)
                return true;

            return false;
        }
    }

}