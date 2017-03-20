using UnityEngine;

/*
 * Teto Fast Distance (Unity Victory3.distance slow because SqrRoot)
 * ONLY use when Accuracy is not required
 * By Gordon Niemann
 * Beta Build - Nov 1st 2016
 */

public static class ExtendVector3
{
    public static float FastDistance(this Vector3 origin, Vector3 target)
    {
        return (origin - target).sqrMagnitude;
    }

    public static bool IsCloseTo(this Vector3 origin, Vector3 target, float range)
    {
        return origin.FastDistance(target) < Mathf.Pow(range, 2);
    } 
}