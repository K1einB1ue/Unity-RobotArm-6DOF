
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[GuiComponent]
public class AngleCalculator : GuiComponent
{
    public Vector3 vecFrom;
    public Vector3 vecTo;

    public Quaternion quatOutPut;
    public float angleOutPut;
    
    public Quaternion GetRotation()
    {
        var axis = Vector3.Cross(vecFrom, vecTo);
        var from = vecFrom.normalized;
        var to = vecTo.normalized;
        var angle = MathF.Acos(Vector3.Dot(from, to)) * Mathf.Rad2Deg;
        var rotation = Quaternion.AngleAxis(angle, axis);
        quatOutPut = rotation;
        return rotation;
    }

    public float GetAngle()
    {
        var from = vecFrom.normalized;
        var to = vecTo.normalized;
        var angle = MathF.Acos(Vector3.Dot(from, to)) * Mathf.Rad2Deg;
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

    }

}
