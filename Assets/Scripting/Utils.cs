using RobotPlatformer.Player.Behavior;
using System.Collections;
using UnityEngine;

public static class Utils
{
    public static Vector3 Flatten(Vector3 vec, bool normalize)
    {
        vec.y = 0f;

        return normalize ? vec.normalized : vec;
    }

    public static float GetSquaredLateralVelocity(Vector3 velo)
    {
        return velo.x * velo.x + velo.z * velo.z;
    }

    public static Vector3 FlatAlignFromPerspective(Vector3 vector, Transform perspective)
    {
        Quaternion rotation = Quaternion.Euler(0f, perspective.eulerAngles.y, 0f);
        return rotation * vector;
    }

    public static int ComposeBehaviors(IBehavior[] behaviors)
    {
        int cumulative = 0;

        foreach (IBehavior behavior in behaviors)
        {
            cumulative |= 1 << behavior.GetTier();
        }

        return cumulative;
    }

#if UNITY_EDITOR
    public static void DebugBits(int i, int cap)
    {
        var b = new BitArray(new int[] { i });

        string output = "";

        for (int index = 0; index < cap; index+=1)
        {
            output += (b.Get(index) ? 1 : 0) + " ";
        }

        Debug.Log(output + " = " + i);
    }
#endif
}
