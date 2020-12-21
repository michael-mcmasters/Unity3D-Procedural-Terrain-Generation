using UnityEngine;
using UnityEditor;

public class VecCalc
{
    // Convert world point to local position
    // (If you want relativePoint to be affected by scale, divide difference by the scale you want)
    public static Vector3 InvTransformPoint(Vector3 basePoint, Quaternion baseForward, Vector3 relativePoint)
    {
        Vector3 difference = relativePoint - basePoint;
        return Quaternion.Inverse(baseForward) * new Vector3(difference.x, difference.y, difference.z);
    }

    public static Vector3 InvTransformPoint(GameObject gameObject, Vector3 relativePoint)
    {
        return InvTransformPoint(gameObject.transform.position, gameObject.transform.rotation, relativePoint);
    }






    // Creates new point local to base point and converts it to world position
    // (To calculate forwardDirection before passing, use Quaternion.LookRotation((pointB - pointA).normalize)
    public static Vector3 TransformPoint(Vector3 basePoint, Quaternion forwardDirection, Vector3 newPosition)
    {
        return basePoint + forwardDirection * newPosition;
    }

    public static Vector3 TransformPoint(GameObject gameObject, Vector3 newPosition)
    {
        return TransformPoint(gameObject.transform.position, gameObject.transform.rotation, newPosition);
    }







    // Returns new point x units away from the base point in the forward direction
    // (To get forwardDirection, use (pointB - pointA).normalize)
    public static Vector3 NewPointInDirection(Vector3 basePoint, Vector3 forwardDirection, float unitsToMove)
    {
        return basePoint + forwardDirection * unitsToMove;
    }

    public static Vector3 NewPointInDirection(GameObject gameObject, float unitsToMove)
    {
        return gameObject.transform.position + gameObject.transform.forward * unitsToMove;
    }






    // Check if vectors are equal by checking their distance from one another.
    // Use this when (Vector1 == Vector2) returns false when it should return true because of floating point precision errors.
    public static bool CompareVectors(Vector3 firstVector, Vector3 secondVector)
    {
        float distance = Vector3.Distance(firstVector, secondVector);
        if (distance > -0.001f && distance < 0.001f)
            return true;

        return false;
    }

    // Return true if floats are equal (with floating point precision checked)
    // Code from: https://docs.unity3d.com/ScriptReference/Mathf.Epsilon.html
    public static bool CompareFloats(float a, float b)
    {
        if (a >= b - Mathf.Epsilon && a <= b + Mathf.Epsilon)
        {
            return true;
        }
        else
        {
            return false;
        }
    }








    // Calculate point of intersection between two 2D lines (lines that share same y axis). Returns true if intersected and false if not.
    // For 3D, use ClosestPointsOnTwoLines(). Source: https://wiki.unity3d.com/index.php/3d_Math_functions.
    public static bool PointOfIntersection2D(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {
        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        // is coplanar, and not parrallel
        if (Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
        {
            float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
            intersection = linePoint1 + (lineVec1 * s);
            return true;
        }
        else
        {
            intersection = Vector3.zero;
            return false;
        }
    }

    // 3D point of intersection.
    // Two non-parallel lines which may or may not touch each other have a point on each line which are closest
    // to each other. This function finds those two points. If the lines are not parallel, the function 
    // outputs true, otherwise false. Source: https://wiki.unity3d.com/index.php/3d_Math_functions.
    public static bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {

        closestPointLine1 = Vector3.zero;
        closestPointLine2 = Vector3.zero;

        float a = Vector3.Dot(lineVec1, lineVec1);
        float b = Vector3.Dot(lineVec1, lineVec2);
        float e = Vector3.Dot(lineVec2, lineVec2);

        float d = a * e - b * b;

        // lines are not parallel
        if (d != 0.0f)
        {

            Vector3 r = linePoint1 - linePoint2;
            float c = Vector3.Dot(lineVec1, r);
            float f = Vector3.Dot(lineVec2, r);

            float s = (b * f - c * e) / d;
            float t = (a * f - c * b) / d;

            closestPointLine1 = linePoint1 + lineVec1 * s;
            closestPointLine2 = linePoint2 + lineVec2 * t;

            return true;
        }

        else
        {
            return false;
        }
    }
}