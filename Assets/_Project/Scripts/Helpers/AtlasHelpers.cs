using UnityEngine;

public static class AtlasHelpers
{
    public static int Sign(float value)
    {
        if (value == 0)
        {
            return 0;
        }
        return (int)Mathf.Sign(value);
    }

    public static bool SameSign(float a, float b)
    {
        return Sign(a).Equals(Sign(b));
    }

    public static void WarnOrThrow(bool shouldThrow, string message)
    {
        if (shouldThrow)
        {
            throw new System.Exception(message);
        }
        else
        {
            Debug.LogWarning(message);
        }
    }
}