using UnityEngine;

public class Mathx
{
    public const float TWOPI = Mathf.PI * 2;
    public const float HALFPI = Mathf.PI * 0.5f;
    public const float ONEOVER90 = 1f / 90f;
    public const float ONEOVER180 = 1f / 180f;

    /**
     * Make sure an angle stays within the range -360 < angle < 360.
     */
    public static float WrapAngle(float angle)
    {
        if (angle < -360f)
            angle += 360f;
        if (angle > 360f)
            angle -= 360f;
        return angle;
    }

    public static float ClampAngle(float angle, float min, float max, float wrapPoint)
    {
        if (angle < -wrapPoint)
            angle += 360.0f;
        if (angle > wrapPoint)
            angle -= 360.0f;
        return Mathf.Clamp(angle, min, max);
    }

    /**
     * Clamps a value between -1 < 0 < 1.
     * Really, this is made redundant by Mathf.Clamp, innit?
     */
    public static float ClampUnitValue(float value)
    {
        if (value > 1f) value = 1f;
        if (value < -1f) value = -1f;
        return value;
    }

    /**
     * Determine the signed angle between two vectors, around an axis
     */
    public static float AngleAroundAxis(Vector3 v1, Vector3 v2, Vector3 a)
    {
        //v1.Normalize();
        //v2.Normalize();
        //a.Normalize();
        return Mathf.Atan2(
            Vector3.Dot(a, Vector3.Cross(v1, v2)),
            Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
    }

    public static Vector3 ProjectOnPlane(Vector3 inVector, Vector3 planeNormal)
    {
        return Vector3.Cross(planeNormal, (Vector3.Cross(inVector, planeNormal)));
    }
}
