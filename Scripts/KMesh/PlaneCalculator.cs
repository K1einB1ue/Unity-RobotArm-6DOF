
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[GuiComponent]
public class PlaneCalculator : GizmosComponent
{
    public Vector3 normal;
    public Vector3 vec;
    public float radius;
    public int divNum;
    
    public Quaternion quatOutPut;
    public float angleOutPut;
    public Quaternion GetRotation()
    {
        var axis = Vector3.Cross(normal, vec).normalized;
        var lerpTo = Vector3.Cross(axis, normal).normalized;
        var angle = MathF.Acos(Vector3.Dot(lerpTo, axis)) * Mathf.Rad2Deg;
        var rotation = Quaternion.AngleAxis(angle, axis);
        quatOutPut = rotation;
        return rotation;
    }

    public float GetAngle()
    {
        var axis = Vector3.Cross(normal, vec).normalized;
        var lerpTo = Vector3.Cross(axis, normal).normalized;
        var angle = MathF.Acos(Vector3.Dot(lerpTo, axis)) * Mathf.Rad2Deg;
        angleOutPut = angle;
        return angle;
    }
    private static List<Type> _guiLayoutRequire = new();
    public override List<Type> GuiLayoutRequire => _guiLayoutRequire;

    public override void GuiLayout()
    {
        if (GUILayout.Button("计算旋转"))
        {
            GetRotation();
        }
        if (GUILayout.Button("计算角度"))
        {
            GetAngle();
        }
    }

    public override void GizmosLayout(Transform transform)
    {
        DrawPlane(transform);
    }
    
        
    public void DrawPlane(Transform transform)
    {
        var worldNormal = transform.rotation * normal.normalized;
        var worldPosition = transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(worldPosition, worldPosition + worldNormal * radius);
        
        //Quaternion q;
        //vector a = cross(v1, v2);
        //q.xyz = a;
        //q.w = sqrt((v1.Length ^ 2) * (v2.Length ^ 2)) + dot(v1, v2);
        
        
        if (divNum < 2) return;
        /*
        var vert = Vector3.right;
        if (Vector3.Dot(Vector3.up, worldNormal) != 1)
        {
            var axis = Vector3.Cross(Vector3.up, worldNormal);
            var rotation = new Quaternion(axis.x, axis.y, axis.z,
                1 + Vector3.Dot(worldNormal, Vector3.up)).normalized;
            vert = rotation * Vector3.right;
            Debug.Log(axis);
        }
        */

        var vert = KTransform.GetVerticalVec(worldNormal);
        
        Gizmos.color = Color.green;
        var begin = worldPosition + vert * radius;
        var from = begin;
        for (var i = 1; i < divNum; i++)
        {
            var to = worldPosition + Quaternion.AngleAxis((360.0f / divNum) * i, worldNormal) * vert * radius;
            Gizmos.DrawLine(from, to);
            from = to;
        }
        Gizmos.DrawLine(from, begin);
    }
}
