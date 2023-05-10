using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public static class KTransform
{

    public static List<Transform> Find(this Transform tran, Func<Transform, bool> match)
    {
        var ret = new List<Transform>();
        if (match.Invoke(tran))
        {
            ret.Add(tran);
        }
        for (var i = 0; i < tran.childCount; i++)
        {
            ret.AddRange(tran.GetChild(i).Find(match));
        }
        return ret;
    }

    public static Quaternion VecToVecRotation(Vector3 from,Vector3 to)
    {
        if (from.magnitude == 0 || to.magnitude == 0) return quaternion.identity;
        from = from.normalized;
        to = to.normalized;
        var axis = Vector3.Cross(from, to);
        var angle = MathF.Acos(Vector3.Dot(from, to));
        var rot = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, axis);
        return rot;
    }

    public static Vector3 GetProjection(Vector3 normal, Vector3 fromPlaneToPoint, Vector3 point)
    {
        normal = normal.normalized;
        var distance = Vector3.Dot(fromPlaneToPoint, normal);
        return point - distance * normal;
    }

    public static Vector3 GetZeroPointProjection(Vector3 normal, Vector3 planePoint)
    {
        normal = normal.normalized;
        var distance = Vector3.Dot(planePoint, normal);
        return distance * normal;
    }

    public static bool GetLineLineIntersection(out Vector3 intersection, Vector3 p1, Vector3 v1, Vector3 p2, Vector3 v2)
    {
        intersection = Vector3.zero;
        if (Math.Abs(Vector3.Dot(v1, v2) - 1) < 1E-8)
        {
            Debug.Log("共线");
            return false;
        }
        var startPointSeg = p2 - p1;
        var vecS1 = Vector3.Cross(v1, v2);            // 有向面积1
        var vecS2 = Vector3.Cross(startPointSeg, v2); // 有向面积2
        var num = Vector3.Dot(startPointSeg, vecS1);

        // 判断两这直线是否共面
        if (num >= 1E-05 || num <= -1E-05)
        {
            Debug.Log("不共面");
            return false;
        }

        // 有向面积比值，利用点乘是因为结果可能是正数或者负数
        var num2 = Vector3.Dot(vecS2, vecS1) / vecS1.sqrMagnitude;
 
        intersection = p1 + v1 * num2;
        return true;
    }

    public static bool CheckInTriangle(Vector3 vec0, Vector3 vec1, Vector3 vec2, Vector3 normal, Vector3 point)
    {
        var vec01 = vec1 - vec0;
        var vec0P = point - vec0;
        var vec02 = vec2 - vec0;
        var vec12 = vec2 - vec1;
        var vec1P = point - vec1;
        var side0 = Vector3.Dot(Vector3.Cross(vec01, vec0P), normal) > 0;
        var side1 = Vector3.Dot(Vector3.Cross(vec0P, vec02), normal) > 0;
        var side2 = Vector3.Dot(Vector3.Cross(vec12, vec1P), normal) > 0;
        return side0 && side1 && side2;
    }
    
    public static bool CheckInTriangle(Vector3 vec0, Vector3 vec1, Vector3 vec2, Vector3 point)
    {
        var vec01 = vec1 - vec0;
        var vec0P = point - vec0;
        var vec02 = vec2 - vec0;
        var vec12 = vec2 - vec1;
        var vec1P = point - vec1;
        var normal = Vector3.Cross(vec01, vec12);
        var side0 = Vector3.Dot(Vector3.Cross(vec01, vec0P), normal) > 0;
        var side1 = Vector3.Dot(Vector3.Cross(vec0P, vec02), normal) > 0;
        var side2 = Vector3.Dot(Vector3.Cross(vec12, vec1P), normal) > 0;
        return side0 && side1 && side2;
    }

    public static Vector3 GetVerticalVec(Vector3 vec)
    {
        if (vec.magnitude == 0) return Vector3.zero;
        vec = vec.normalized;
        if (Math.Abs(Vector3.Dot(vec, Vector3.up) - 1) < 1E-8) return Vector3.right;
        var axis = Vector3.Cross(Vector3.up, vec);
        var rotation = new Quaternion(axis.x, axis.y, axis.z,
            1 + Vector3.Dot(vec, Vector3.up)).normalized;
        return rotation * Vector3.right;
    }
    
    public static Vector3 GetVerticalVec(Vector3 vec,Vector3 vecOnXZPlane)
    {
        if (vec.magnitude == 0) return Vector3.zero;
        vecOnXZPlane.y = 0;
        vecOnXZPlane = vecOnXZPlane.normalized;
        if (Math.Abs(Vector3.Dot(vec, Vector3.up) - 1) < 1E-8) return vecOnXZPlane;
        var axis = Vector3.Cross(Vector3.up, vec);
        var rotation = new Quaternion(axis.x, axis.y, axis.z,
            1 + Vector3.Dot(vec, Vector3.up)).normalized;
        return rotation * vecOnXZPlane;
    }
    
    public static Vector3 GetVerticalVec(Vector3 vec,float angle)
    {
        var preRot = Quaternion.AngleAxis(angle, Vector3.up);
        var rotVec = preRot * Vector3.right;
        if (vec.magnitude == 0) return Vector3.zero;
        if (Math.Abs(Vector3.Dot(vec, Vector3.up) - 1) < 1E-8) return rotVec;
        var axis = Vector3.Cross(Vector3.up, vec);
        var rotation = new Quaternion(axis.x, axis.y, axis.z,
            1 + Vector3.Dot(vec, Vector3.up)).normalized;
        return rotation * rotVec;
    }
    
}
