using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

internal static class NetworkPackager
{
    public static string Package(Vector3 vec)
    {
        return vec.x + ";" + vec.y + ";" + vec.z;
    }

    public static string Package(Quaternion q)
    {
        return q.x + ";" + q.y + ";" + q.z + ";" + q.w;
    }

    public static Quaternion UnpackgeQuaternion(string q)
    {
        string[] components = q.Split(";");

        return new Quaternion(float.Parse(components[0]), float.Parse(components[1]), float.Parse(components[2]), float.Parse(components[3]));
    }

    public static Vector3 UnpackageVector3(string vector)
    {
        string[] components = vector.Split(";");

        return new Vector3(float.Parse(components[0]), float.Parse(components[1]), float.Parse(components[2]));
    }
}