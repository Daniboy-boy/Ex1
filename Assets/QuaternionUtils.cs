using System;
using UnityEngine;

public class QuaternionUtils
{
    // The default rotation order of Unity. May be used for testing
    public static readonly Vector3Int UNITY_ROTATION_ORDER = new Vector3Int(1,2,0);

    // Returns the product of 2 given quaternions
    public static Vector4 Multiply(Vector4 q1, Vector4 q2)
    {
        return new Vector4(
            q1.w*q2.x + q1.x*q2.w + q1.y*q2.z - q1.z*q2.y,
            q1.w*q2.y + q1.y*q2.w + q1.z*q2.x - q1.x*q2.z,
            q1.w*q2.z + q1.z*q2.w + q1.x*q2.y - q1.y*q2.x,
            q1.w*q2.w - q1.x*q2.x - q1.y*q2.y - q1.z*q2.z
        );
    }

    // Returns the conjugate of the given quaternion q
    public static Vector4 Conjugate(Vector4 q)
    {
        return new Vector4(-q.x, -q.y, -q.z, q.w);
    }

    // Returns the Hamilton product of given quaternions q and v
    public static Vector4 HamiltonProduct(Vector4 q, Vector4 v)
    {
        return Multiply(Multiply(q, v), Conjugate(q));
    }

    // Returns a quaternion representing a rotation of theta degrees around the given axis
    public static Vector4 AxisAngle(Vector3 axis, float theta)
    {
        axis.Normalize();
        return new Vector4(Mathf.Sin(theta/2) * axis.x,
                           Mathf.Sin(theta/2) * axis.y,
                           Mathf.Sin(theta/2) * axis.z,
                           Mathf.Cos(theta/2));
    }

    // Returns a quaternion representing the given Euler angles applied in the given rotation order
    public static Vector4 FromEuler(Vector3 euler, Vector3Int rotationOrder)
    {
        var axisAngX = AxisAngle(Vector3.right, euler.x);
        var axisAngY = AxisAngle(Vector3.right, euler.y);
        var axisAngZ = AxisAngle(Vector3.right, euler.z);

        var rotationVec = new Vector4(0, 0, 0, 1);
        for (int i = 0; i < 3; ++i)
        {
            var rotateAng = rotationOrder.x == i ? axisAngX : rotationOrder.y == i ? axisAngY : axisAngZ;
            rotationVec = Multiply(rotationVec, rotateAng);
        }
        return rotationVec.normalized;
    }

    // Returns a spherically interpolated quaternion between q1 and q2 at time t in [0,1]
    public static Vector4 Slerp(Vector4 q1, Vector4 q2, float t)
    {
        q1.Normalize();
        q2.Normalize();
        var q1q2Minus1 = Multiply(q1, Conjugate(q2)).normalized;
        var theta = 2 * Mathf.Acos(q1q2Minus1.w);
        
        if (Mathf.Abs(theta) > Mathf.PI / 2)
            theta = 2 * Mathf.Acos(-q1q2Minus1.w);
        
        if (Mathf.Sin(theta) == 0)
            return q1;

        float q1Factor = Mathf.Sin((1 - t) * theta) / Mathf.Sin(theta);
        float q2Factor = Mathf.Sin(t * theta) / Mathf.Sin(theta);
        return (q1Factor * q1 + q2Factor * q2).normalized;
    }
}