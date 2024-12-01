using UnityEngine;
using System.Collections;

public static class Utils
{
    public static string TrimEnd(string str)
    {
        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] == '\0')
            {
                return str.Substring(0, i);
            }
        }
        return str;
    }

    public static Quaternion GetRotation(Matrix4x4 matrix)
    {
        return matrix.rotation;
    }

    public static Vector3 GetPosition(Matrix4x4 matrix)
    {
        return matrix.GetColumn(3);
    }

    public static Vector3 GetScale(Matrix4x4 matrix)
    {
        return matrix.lossyScale;
    }

    public static Vector3 MultiplyVector3(Vector3 a, Vector3 b)
    {
        Vector3 result = new Vector3();
        result.x = a.x * b.x;
        result.y = a.y * b.y;
        result.z = a.z * b.z;
        return result;
    }
}
