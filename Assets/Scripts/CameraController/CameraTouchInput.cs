using UnityEngine;
using UnityEngine.EventSystems;

// Script taken and evolved from: https://www.youtube.com/watch?v=KkYco_7-ULA&t=438s.

namespace CameraController
{
    [RequireComponent(typeof(CameraMovementInputManager))]
    public class CameraTouchInput : MonoBehaviour
    {
        public CameraMovementInputManager cm;

        private bool fingerStartedOverUI;

        [Header("Pan General")]
        public float maxPanSpeed = 5;
        private bool panTowardsMomentumPosition;
        private Vector3 momentumPosition;

        [Header("Rotation General")]
        public float rotationSpeed = 0.3f;
        public bool invertHorizontalRotation;
        public bool invertVerticalRotation;
        public Vector3 pointToOrbitRotation;
        [SerializeField] private float rotationX;
        [SerializeField] private float rotationY;


        // Gestures
        [Header("Two Finger Close Together")]
        [SerializeField] private int twoFingerCloseTogetherRange = 20;      // ToDo: Number is pixel size which may be different per every phone. Will need to find a number and divide it from device's pixel size so range is same for everyone.
        [SerializeField] private bool twoFingerCloseTogetherGesture;
        private bool allowTwoFingerTogetherGesture;

        [Header("Registering Double/Triple Tap")]
        public float doubleTapTimeAllotted = 0.4f;
        [SerializeField] private float timeSinceFirstTap;
        public int secondTapRange = 100;
        private Vector3 firstTapPosition;
        [SerializeField] private bool doubleTapped;
        [SerializeField] private bool tripleTapped;
        [SerializeField] private bool quickDoubleTapped;
        private bool checkForQuickDoubleTap;

        // When player double taps and lets go
        [Header("Quick Double Tap Zoom")]
        public float quickDoubleTapZoomAmount = 0.5f;
        public float quickDoubleTapZoomSpeed = 5;
        private Vector3 quickDoubleTapZoomPosition;

        // player double taps and swipes instead of letting go on second tap
        [Header("Double Tap Swipe Zoom")]
        public float doubleTapZoomSpeed = 0.1f;

        // player double taps and swipes instead of letting go on second tap
        [Header("Double Tap Swipe Rotate")]
        //

        private int lastFrameTouchCount;


        public bool DoubleTapped { get { return doubleTapped; } }
        public bool TripleTapped { get { return tripleTapped; } }





        private void Start()
        {
            // So that variables don't start at 0. Otherwise camera will jump to a really low rotation (almost flat on ground) when gesture is started.
            cm.SyncVariablesWithCamerasCurrentRotation(ref rotationX, ref rotationY);

            // If player zooms at start of game before ever rotating, zoom needs this point to be defined
            pointToOrbitRotation = cm.GetOrbitPointForRotation();
        }

        private void Update()
        {
            // if (PauseButton.GamePaused)
            // {
            //     // So that TouchPhase.Began works when clicking resume.
            //     lastFrameTouchCount = 100;
            //     return;
            // }

            GetTouchInput();

            if (panTowardsMomentumPosition)
                panTowardsMomentumPosition = cm.PanCameraToNewPanPosition(momentumPosition, 1);
        }



        void GetTouchInput()
        {
            if (Input.touchCount >= 1)
            {
                panTowardsMomentumPosition = false;
                quickDoubleTapped = false;
            }

            if (Input.touchCount != 2)
                allowTwoFingerTogetherGesture = true;

            if (FingerStartingOnUI(ref fingerStartedOverUI))
                return;

            CheckForDoubleTap();


            // Double Tap Quick Zoom
            // (Continues to run until quickDoubleTapped is false.)
            if (quickDoubleTapped && Input.touchCount == 0)
            {
                Vector3 newCameraPosition = Vector3.Lerp(transform.position, quickDoubleTapZoomPosition, quickDoubleTapZoomSpeed * Time.deltaTime);
                ZoomCamera(newCameraPosition);
            }

            // Double Tap Swipe Zoom
            else if (doubleTapped && Input.touchCount == 1)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Moved)
                {
                    Vector3 touchPositionLastFrame = Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition;
                    Vector3 touchPositionNow = Input.GetTouch(0).position;
                    float distanceTouchTraveledY = touchPositionLastFrame.y - touchPositionNow.y;

                    Vector3 newCameraPosition = transform.position + transform.forward * distanceTouchTraveledY * doubleTapZoomSpeed;
                    ZoomCamera(newCameraPosition);
                }
            }

            // Triple Tap Swipe Rotate
            else if (tripleTapped && Input.touchCount == 1)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began || lastFrameTouchCount != Input.touchCount)
                {
                    cm.SyncVariablesWithCamerasCurrentRotation(ref rotationX, ref rotationY);
                    pointToOrbitRotation = cm.GetOrbitPointForRotation();
                }

                if (Input.GetTouch(0).phase == TouchPhase.Moved)
                {
                    Vector2 deltaPosition = Input.GetTouch(0).deltaPosition;

                    if (invertHorizontalRotation)
                        rotationX -= deltaPosition.x * rotationSpeed;
                    else
                        rotationX += deltaPosition.x * rotationSpeed;

                    if (invertVerticalRotation)
                        rotationY += deltaPosition.y * rotationSpeed;
                    else
                        rotationY -= deltaPosition.y * rotationSpeed;

                    RotateCamera(pointToOrbitRotation, ref rotationX, ref rotationY);
                }
            }

            // // Three Finger Rotate
            // else if (Input.touchCount == 3)
            // {
            //     if (Input.GetTouch(0).phase == TouchPhase.Began || Input.GetTouch(1).phase == TouchPhase.Began || Input.GetTouch(2).phase == TouchPhase.Began || lastFrameTouchCount != Input.touchCount)
            //     {
            //         cm.SyncVariablesWithCamerasCurrentRotation(ref rotationX, ref rotationY);
            //         pointToOrbitRotation = cm.GetOrbitPointForRotation();
            //     }

            //     if (Input.GetTouch(1).phase == TouchPhase.Moved)
            //     {
            //         Vector2 deltaPosition = Input.GetTouch(1).deltaPosition;

            //         if (invertHorizontalRotation)
            //             rotationX -= deltaPosition.x * rotationSpeed;
            //         else
            //             rotationX += deltaPosition.x * rotationSpeed;

            //         if (invertVerticalRotation)
            //             rotationY += deltaPosition.y * rotationSpeed;
            //         else
            //             rotationY -= deltaPosition.y * rotationSpeed;

            //         RotateCamera(pointToOrbitRotation, ref rotationX, ref rotationY);
            //     }
            // }

            // Single Finger Pan
            else if (Input.touchCount == 1 && cm.AllowDragToPan)
            {
                Vector3 touchPositionLastFrame = cm.ScreenToPlanePosition(Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition);
                Vector3 touchPositionNow = cm.ScreenToPlanePosition(Input.GetTouch(0).position);

                if (Input.GetTouch(0).phase == TouchPhase.Moved)
                {
                    Vector3 newPosDirectionAndDistance = touchPositionLastFrame - touchPositionNow;

                    float touchDistance = (touchPositionLastFrame - touchPositionNow).magnitude;

                    // If last frame ray is shooting at sky, and this frame ray hits ground, distance will be great and camera will jump. This stops that.
                    if (touchDistance > 100)
                        return;

                    // If camera orbit angle is really low, this prevents camera from panning too fast when player drags
                    else if (rotationY < 30 && touchDistance > maxPanSpeed)
                        newPosDirectionAndDistance *= 0.2f;

                    Vector3 newCameraPosition = transform.position;
                    newCameraPosition.x += newPosDirectionAndDistance.x;
                    newCameraPosition.z += newPosDirectionAndDistance.z;

                    Vector3 newOrbitPoint = cm.ConvertNewPositionToOrbitPoint(newCameraPosition);
                    if (cm.CheckIfOrbitPointInBounds(newOrbitPoint))
                    {
                        transform.position = newCameraPosition;
                    }
                }

                if (Input.GetTouch(0).phase == TouchPhase.Ended)
                {
                    float cameraCurrentSpeed = GetCameraSpeed();
                    if (cameraCurrentSpeed > 3)
                    {
                        momentumPosition = cm.SetMomentumPosition(touchPositionLastFrame, touchPositionNow, cameraCurrentSpeed);
                        panTowardsMomentumPosition = true;
                    }
                }
            }

            else if (Input.touchCount == 2)
            {
                // Two finger gestures shoot at this plane instead of ground plane
                cm.UpdateCameraPlanePosition();

                if (allowTwoFingerTogetherGesture)
                    twoFingerCloseTogetherGesture = CheckForTwoFingerTogetherGesture();

                if (twoFingerCloseTogetherGesture == false)
                    allowTwoFingerTogetherGesture = false;

                // // Two finger pan
                // if (twoFingerCloseTogetherGesture)
                // {
                //     Vector3 touchPositionZeroLastFrame = cm.ScreenToPlanePosition(Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition);
                //     Vector3 touchPositionOneLastFrame = cm.ScreenToPlanePosition(Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition);
                //     Vector3 touchPositionZeroNow = cm.ScreenToPlanePosition(Input.GetTouch(0).position);
                //     Vector3 touchPositionOneNow = cm.ScreenToPlanePosition(Input.GetTouch(1).position);

                //     // Gets point inbetween two fingers this frame and last frame
                //     Vector3 touchPositionLastFrameAverage = (touchPositionZeroLastFrame + touchPositionOneLastFrame) / 2;
                //     Vector3 touchPositionNowAverage = (touchPositionZeroNow + touchPositionOneNow) / 2;

                //     if (Input.GetTouch(0).phase == TouchPhase.Moved)
                //     {
                //         Vector3 newPosDirectionAndDistance = touchPositionLastFrameAverage - touchPositionNowAverage;

                //         float touchDistance = (touchPositionLastFrameAverage - touchPositionNowAverage).magnitude;

                //         // If last frame ray is shooting at sky, and this frame ray hits ground, distance will be great and camera will jump. This stops that.
                //         if (touchDistance > 100)
                //             return;

                //         // If camera orbit angle is really low, this prevents camera from panning too fast when player drags
                //         else if (rotationY < 30 && touchDistance > maxPanSpeed)
                //             newPosDirectionAndDistance *= 0.2f;

                //         Vector3 newCameraPosition = transform.position;
                //         newCameraPosition.x += newPosDirectionAndDistance.x;
                //         newCameraPosition.z += newPosDirectionAndDistance.z;

                //         Vector3 newOrbitPoint = cm.ConvertNewPositionToOrbitPoint(newCameraPosition);
                //         if (cm.CheckIfOrbitPointInBounds(newOrbitPoint))
                //         {
                //             transform.position = newCameraPosition;
                //         }
                //     }

                // Two finger Orbit Rotation
                if (twoFingerCloseTogetherGesture)
                {
                    if (Input.GetTouch(0).phase == TouchPhase.Began || Input.GetTouch(1).phase == TouchPhase.Began || lastFrameTouchCount != Input.touchCount)
                    {
                        cm.SyncVariablesWithCamerasCurrentRotation(ref rotationX, ref rotationY);
                        pointToOrbitRotation = cm.GetOrbitPointForRotation();
                    }

                    if (Input.GetTouch(0).phase == TouchPhase.Moved)
                    {
                        Vector2 deltaPosition = Input.GetTouch(0).deltaPosition;

                        if (invertHorizontalRotation)
                            rotationX -= deltaPosition.x * rotationSpeed;
                        else
                            rotationX += deltaPosition.x * rotationSpeed;

                        if (invertVerticalRotation)
                            rotationY += deltaPosition.y * rotationSpeed;
                        else
                            rotationY -= deltaPosition.y * rotationSpeed;

                        RotateCamera(pointToOrbitRotation, ref rotationX, ref rotationY);
                    }
                }

                // Rotate and Zoom at the same time
                // (Source: https://www.youtube.com/watch?v=KkYco_7-ULA&t=438s)
                else if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Moved)
                {
                    allowTwoFingerTogetherGesture = false;

                    pointToOrbitRotation = cm.GetOrbitPointForRotation();

                    // Shoots rays at camera plane. not ground plane (notice second parameter).
                    var touchZeroCurrentPos = cm.ScreenToPlanePosition(Input.GetTouch(0).position, 1);
                    var touchOneCurrentPos = cm.ScreenToPlanePosition(Input.GetTouch(1).position, 1);
                    var touchZeroLastFramePos = cm.ScreenToPlanePosition(Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition, 1);
                    var touchOneLastFramePos = cm.ScreenToPlanePosition(Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition, 1);

                    // Point to zoom towards or rotate around
                    //Vector3 anchorPosition = GetAnchorPosition(touchZeroCurrentPos, touchOneCurrentPos, touchZeroLastFramePos, touchOneLastFramePos);
                    Vector3 anchorPosition = pointToOrbitRotation;

                    // Pinch to Zoom
                    float amountToZoom = Vector3.Distance(touchZeroCurrentPos, touchOneCurrentPos) /
                                Vector3.Distance(touchZeroLastFramePos, touchOneLastFramePos);

                    // edge case
                    if (amountToZoom == 0 || amountToZoom > 10)
                        return;

                    Vector3 newCameraPosition = Vector3.LerpUnclamped(anchorPosition, transform.position, 1 / amountToZoom);
                    Vector3 newOrbitPoint = cm.ConvertNewPositionToOrbitPoint(newCameraPosition);
                    if (cm.CheckIfOrbitPointInBounds(newOrbitPoint))
                        ZoomCamera(newCameraPosition);



                    // Rotate
                    // (rotates around center of screen (orbit point) instead of fingers. if you want to rotate around fingers, change first parameter to anchorPosition (and make sure new position is in bounds)
                    transform.RotateAround(pointToOrbitRotation, cm.groundPlane.normal, Vector3.SignedAngle(touchOneCurrentPos - touchZeroCurrentPos, touchOneLastFramePos - touchZeroLastFramePos, cm.groundPlane.normal));
                }


                if (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(1).phase == TouchPhase.Ended)
                {
                    allowTwoFingerTogetherGesture = true;
                }
            }

            // Debug -- Reset Camera Position
            else if (Input.touchCount == 5)
            {
                cm.ResetCameraPosition();
            }

            lastFrameTouchCount = Input.touchCount;
        }





        // Runs every frame. Checks for Double and Triple taps.
        private void CheckForDoubleTap()
        {
            if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                if (timeSinceFirstTap >= doubleTapTimeAllotted)
                {
                    timeSinceFirstTap = 0;
                    firstTapPosition = Input.GetTouch(0).position;
                }

                else if (CheckIfSecondTapInRangeOfFirstTap(firstTapPosition, Input.GetTouch(0).position, secondTapRange))
                {
                    //timeSinceFirstTap = 0;

                    if (doubleTapped == false)
                    {
                        doubleTapped = true;
                        tripleTapped = false;
                    }
                    else
                    {
                        doubleTapped = false;
                        tripleTapped = true;
                    }
                }
            }
            else if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                if (doubleTapped && timeSinceFirstTap < doubleTapTimeAllotted)
                {
                    if (CheckIfSecondTapInRangeOfFirstTap(firstTapPosition, Input.GetTouch(0).position, secondTapRange))
                    {
                        // If player has double tapped and lets go of screen, wait for timer to end to see if player will make triple tap gesture, before regiserting as a quick double tap.
                        checkForQuickDoubleTap = true;
                    }
                }
            }
            else
            {
                // Player is not double/triple tapping if the timer has ran out and there is no finger on the screen.
                if (timeSinceFirstTap >= doubleTapTimeAllotted)
                {
                    if (Input.touchCount == 0)
                    {
                        doubleTapped = false;
                        tripleTapped = false;
                    }
                }

                // Increment double tap timer (uses real time no matter what device's FPS is)
                if (timeSinceFirstTap <= doubleTapTimeAllotted)
                    timeSinceFirstTap += Time.deltaTime;
            }




            // If player double tapped, let go of screen, and did not triple tap, player made quickDoubleTap gesture
            if (checkForQuickDoubleTap)
            {
                if (timeSinceFirstTap >= doubleTapTimeAllotted)
                {
                    checkForQuickDoubleTap = false;

                    if (Input.touchCount == 0 && TripleTapped == false)
                    {
                        quickDoubleTapped = true;
                        quickDoubleTapZoomPosition = GetQuickDoubleTapZoomPosition();
                    }
                }
            }
        }

        // Both tap positions must be within the given range to return true.
        private bool CheckIfSecondTapInRangeOfFirstTap(Vector3 firstScreenPosition, Vector3 secondScreenPosition, float range)
        {
            float secondTouchDistance = Vector3.Distance(firstScreenPosition, secondScreenPosition);
            if (secondTouchDistance <= range)
                return true;
            else
                return false;
        }

        // The position camera will slowly zoom towards if player quickly taps twice and then stops touching the screen.
        private Vector3 GetQuickDoubleTapZoomPosition()
        {
            //quickDoubleTapZoomPosition = transform.position + transform.forward * quickDoubleTapZoomAmount;
            //return quickDoubleTapZoomPosition;

            //Vector3 touchPositionNow = ScreenToPlanePosition(Input.GetTouch(0).position);
            //if (CheckIfPositionInBounds(firstTapPosition))
            //{
            //    quickDoubleTapZoomPosition = Vector3.Lerp(transform.position, firstTapPosition, 0.1f);
            //    DebugExtension.DebugWireSphere(quickDoubleTapZoomPosition, Color.yellow, 4, 1000);
            //    Debug.Log("pos is: " + quickDoubleTapZoomPosition);
            //    return quickDoubleTapZoomPosition;
            //}


            // This is already on the plane. Just check if this position is in bounds.
            Vector3 firstTapToPlane = cm.ScreenToPlanePosition(firstTapPosition);
            Debug.Log("first tap from screen to plane is " + firstTapToPlane);
            if (cm.CheckIfOrbitPointInBounds(firstTapToPlane))
            {
                quickDoubleTapZoomPosition = Vector3.Lerp(transform.position, firstTapToPlane, quickDoubleTapZoomAmount);
                DebugExtension.DebugWireSphere(quickDoubleTapZoomPosition, Color.yellow, 4, 1000);
                Debug.Log("pos is: " + quickDoubleTapZoomPosition);
                return quickDoubleTapZoomPosition;
            }

            quickDoubleTapZoomPosition = transform.position;
            return quickDoubleTapZoomPosition;

            //if (CheckIfPositionInBounds(firstTapToPlane))
            //{
            //    quickDoubleTapZoomPosition = Vector3.Lerp(transform.position, firstTapToPlane, 0.3f);
            //    DebugExtension.DebugWireSphere(quickDoubleTapZoomPosition, Color.yellow, 4, 1000);
            //    Debug.Log("pos is: " + quickDoubleTapZoomPosition);
            //    return quickDoubleTapZoomPosition;
            //}

            //DebugExtension.DebugWireSphere(quickDoubleTapZoomPosition, Color.yellow, 4, 1000);
            //Debug.Log("missed");
            //return transform.position;




            //if (CheckIfPositionInBounds(firstTapPosition))
            //{
            //    Vector3 screenToPlanePos = ScreenToPlanePosition(firstTapPosition);
            //    quickDoubleTapZoomPosition = Vector3.Lerp(transform.position, screenToPlanePos, 0.3f);
            //    DebugExtension.DebugWireSphere(quickDoubleTapZoomPosition, Color.yellow, 4, 1000);
            //    Debug.Log("pos is: " + quickDoubleTapZoomPosition);
            //    return quickDoubleTapZoomPosition;
            //}

            //Debug.Log("missed");
            //quickDoubleTapZoomPosition = transform.position;
            //return quickDoubleTapZoomPosition;
        }

        //private Vector3 GetNextPositionOrbitPosition()
        //{

        //}

        // Send the position player wants camera to pan to. Shoot a ray from position to the ground plane to get the orbit point.
        // If that orbit point is within map bounds, move camera. Otherwise, don't.
        // (Plane.Raycast() is a lot less expensive than Physics.Raycast())
        //private bool CheckIfPositionInBounds(Vector3 position)
        //{
        //    Vector3 groundOrbitPoint = Vector3.zero;

        //    Vector3 rayStartPoint = position;
        //    Vector3 rayDirection = transform.forward;

        //    Ray newRay = new Ray();
        //    newRay.origin = rayStartPoint;
        //    newRay.direction = rayDirection;
        //    if (plane.Raycast(newRay, out var planeHitPoint))
        //    {
        //        groundOrbitPoint = newRay.GetPoint(planeHitPoint);
        //        DebugExtension.DebugWireSphere(groundOrbitPoint, Color.red, 4, 1000);
        //        //Debug.Log("ground orbit point is: " + groundOrbitPoint);
        //    }
        //    else
        //    {
        //        return false;
        //    }

        //    bool positionInBound = true;

        //    if (groundOrbitPoint.x < minX)
        //        positionInBound = false;

        //    else if (groundOrbitPoint.x > maxX)
        //        positionInBound = false;

        //    if (groundOrbitPoint.z < minZ)
        //        positionInBound = false;

        //    else if (groundOrbitPoint.z > maxZ)
        //        positionInBound = false;

        //    return positionInBound;
        //}



        // If two touches are close together, this gesture is true.
        private bool CheckForTwoFingerTogetherGesture()
        {
            if (Input.touchCount == 2)
            {
                if (Vector3.Distance(cm.ScreenToPlanePosition(Input.GetTouch(0).position, 1), cm.ScreenToPlanePosition(Input.GetTouch(1).position, 1)) < twoFingerCloseTogetherRange)
                {
                    return true;
                }
            }

            return false;
        }







        // The base finger camera will rotate around, zoom towards, or use to pan.
        // The finger that moved least this frame is the anchor finger
        private Vector3 GetAnchorPosition(Vector3 touchZeroCurrentPos, Vector3 touchOneCurrentPos, Vector3 touchZeroLastFramePos, Vector3 touchOneLastFramePos)
        {
            float touchZeroDist = Vector3.Distance(touchZeroCurrentPos, touchZeroLastFramePos);
            float touchOneDist = Vector3.Distance(touchOneCurrentPos, touchOneLastFramePos);
            if (touchZeroDist > touchOneDist)
            {
                return touchOneCurrentPos;
            }
            else
            {
                return touchZeroCurrentPos;
            }
        }


        //// Two finger gesture like zooming, except player is moving both fingers in a circle to rotate camera like a picture
        //private bool CheckIfMakingCircleRotateGesture(Vector3 touchZeroCurrentPos, Vector3 touchOneCurrentPos, Vector3 touchZeroLastFramePos, Vector3 touchOneLastFramePos)
        //{
        //    if (Input.GetTouch(0).phase == TouchPhase.Began || Input.GetTouch(1).phase == TouchPhase.Began || lastFrameTouchCount != lastFrameTouchCount)
        //        twoFingerStartingDistance = Vector3.Distance(touchZeroCurrentPos, touchOneCurrentPos);

        //    // distance from their last positions last frame
        //    float touchZeroDistFromLastFrame = Vector3.Distance(touchZeroCurrentPos, touchZeroLastFramePos);
        //    float touchOneDistFromLastFrame = Vector3.Distance(touchOneCurrentPos, touchOneLastFramePos);

        //    // if both fingers move a certain amount
        //    if (touchZeroDistFromLastFrame > 1 && touchOneDistFromLastFrame > 1)
        //    {
        //        // and distance between them is the same as it was when they touched the screen at the beginning, gesture is true
        //        float twoFingerCurrentDistance = Vector3.Distance(touchZeroCurrentPos, touchOneCurrentPos);
        //        if (twoFingerCurrentDistance > twoFingerStartingDistance - 1 && twoFingerCurrentDistance < twoFingerStartingDistance + 1)
        //        {
        //            circleRotationGesture = true;
        //        }
        //        else
        //        {
        //            circleRotationGesture = false;
        //        }
        //    }

        //    return circleRotationGesture;
        //}









        // Checks if zoom is in allowed range and if so, applies new position to camera
        // (Camera physically moves forward/backwards along its local z axis when zooming)
        private bool ZoomCamera(Vector3 newCameraPosition)
        {
            // Debug
            if (cm.debug_ignoreZoomBoundaries)
            {
                transform.position = newCameraPosition;
                return true;
            }


            float distanceFromOrbitPoint = Vector3.Distance(newCameraPosition, pointToOrbitRotation);
            if (distanceFromOrbitPoint < cm.minAllowedZoom || distanceFromOrbitPoint > cm.maxAllowedZoom)
                return false;

            transform.position = newCameraPosition;
            return true;
        }

        // Use ref to make sure the rotation's new values when clamped are set globally so they work in Update().
        // Otherwise if the max rotation y is 90 and rotationY is 120, player will need to rotate finger down 30 degrees before camera starts rotating again.
        private void RotateCamera(Vector3 pointToOrbitRotation, ref float rotationX, ref float rotationY)
        {
            // Clamp rotation -- don't clamp if debug is true
            if (cm.debug_ignoreRotationBoundaries == false)
                rotationY = cm.ClampRotationAngle(rotationY, cm.lowestAllowedRotationY, cm.highestAllowedRotationY);

            Quaternion newRotation = Quaternion.Euler(rotationY, rotationX, 0);
            float zoomDistanceFromOrbitPoint = Vector3.Distance(pointToOrbitRotation, transform.position);
            Vector3 newPosition = pointToOrbitRotation - (newRotation * Vector3.forward * zoomDistanceFromOrbitPoint);

            transform.rotation = newRotation;
            transform.position = newPosition;
        }





        // The distance mainCamera moved this frame from last frame is its speed.
        public float GetCameraSpeed()
        {
            Vector3 touchPositionLastFrame = Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition;

            // Note: Doesn't make sense. using camera's position with my touch positions?
            float cameraCurrentSpeed = (transform.position - touchPositionLastFrame).magnitude;

            if (cameraCurrentSpeed > 10)
                cameraCurrentSpeed = 10;

            return cameraCurrentSpeed;
        }







        // If this is a finger's first frame touching the screen, and it touched a UI element, don't do camera gesture.
        // (After this, fingers can drag over (under?) UI elements. As long as this isn't its first touch).
        private bool FingerStartingOnUI(ref bool fingerStartedOverUI)
        {
            // If true, bool can't reset until every finger lets go of screen
            if (Input.touchCount <= 0)
            {
                fingerStartedOverUI = false;
            }

            // For each finger, determine if any has just began touching screen, if so, and touched UI element, bool is true
            else
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (Input.GetTouch(i).phase == TouchPhase.Began)
                        if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                            fingerStartedOverUI = true;
                }
            }

            return fingerStartedOverUI;
        }
    }
}

