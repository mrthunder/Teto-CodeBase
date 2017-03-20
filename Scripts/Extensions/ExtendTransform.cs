using UnityEngine;
using System.Collections;

public static class ExtendTransform  {

    /// <summary>
    /// Set only the x from the local position
    /// </summary>
    public static void SetLocalPosX(this Transform t,float x)
    {
        Vector3 newPos = t.localPosition;
        newPos.x = x;
        t.localPosition = newPos;
    }

    /// <summary>
    /// Set only the y from the local position
    /// </summary>
    public static void SetLocalPosY(this Transform t, float y)
    {
        Vector3 newPos = t.localPosition;
        newPos.y = y;
        t.localPosition = newPos;
    }

    /// <summary>
    /// Set only the z from the local position
    /// </summary>
    public static void SetLocalPosZ(this Transform t, float z)
    {
        Vector3 newPos = t.localPosition;
        newPos.z = z;
        t.localPosition = newPos;
    }


}
