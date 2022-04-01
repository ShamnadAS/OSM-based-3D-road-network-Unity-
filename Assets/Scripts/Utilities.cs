using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
    //Angle in the range [0 , 360]
    public static float DirAngle(Vector3 a, Vector3 b)
    {
        var angle = Vector3.SignedAngle(a, b, Vector3.up);
        return angle < 0 ? angle + 360 : angle;
    }

    //Intersection of the lines ab and cd
    public static Vector3 LineIntersection(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        // Line AB represented as a1x + b1y = c1 
        float a1 = b.z - a.z;
        float b1 = a.x - b.x;
        float c1 = a1 * (a.x) + b1 * (a.z);

        // Line CD represented as a2x + b2y = c2 
        float a2 = d.z - c.z;
        float b2 = c.x - d.x;
        float c2 = a2 * (c.x) + b2 * (c.z);

        float determinant = a1 * b2 - a2 * b1;

        float x = (b2 * c1 - b1 * c2) / determinant;
        float y = (a1 * c2 - a2 * c1) / determinant;

        return new Vector3(x, 0f, y);
    }

    //Relative postion of a point with respect to the line ab
    //True if positive, False if negative
    public static bool PointRelativePositon(Vector3 a, Vector3 b, Vector3 point)
    {
        // Line AB represented as a1x + b1y = c1 
        float a1 = b.z - a.z;
        float b1 = a.x - b.x;
        float c1 = a1 * (a.x) + b1 * (a.z);

        return a1 * point.x + b1 * point.z - c1 > 0 ? true : false;
    }
}
