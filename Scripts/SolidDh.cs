using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SolidTrans
{
    internal Vector3 position;
    internal Quaternion rotation;

    public SolidTrans(Vector3 position, Quaternion rotation)
    {
        this.position = position;
        this.rotation = rotation;
    }
}

//以弧度制,和左手四元数描述
public class SolidDh
{
    //两个坐标系的Z轴夹角
    public float angle;
    //两个坐标系Z轴之间的垂直距离
    public float distance;
    //两个坐标系在Z轴的距离
    public float zDistance;
    //两个坐标系在XY重叠时所需要的Z轴的转角;
    public float zAngle;

    public SolidDh(float angle, float distance, float zDistance, float zAngle)
    {
        this.angle = angle;
        this.distance = distance;
        this.zDistance = zDistance;
        this.zAngle = zAngle;
    }
    

    public static SolidDh Gen(Transform from, Transform to)
    {
        var z = Vector3.forward;
        var x = Vector3.right;
        var fRotation = from.rotation;
        var tRotation = to.rotation;
        var zFrom = fRotation * z;
        var zTo = tRotation * z;
        var xTo = tRotation * x;
        var vBetween = to.position - from.position;
        var angle = MathF.Acos(Vector3.Dot(zFrom, zTo));
        var distance = Vector3.Dot(vBetween, Vector3.Cross(zFrom, zTo).normalized);
        var zDistance = Vector3.Dot(vBetween, zTo);
        
        var tempAxis = Vector3.Cross(zFrom, zTo);
        var tempRotation = new Quaternion(tempAxis.x, tempAxis.y, tempAxis.z,
            1 + Vector3.Dot(zFrom, zTo)).normalized;

        var midRotation = tempRotation * fRotation;
        var xMid = midRotation * x;

        var zAngle = MathF.Acos(Vector3.Dot(xTo, xMid));
        return new SolidDh(angle, distance, zDistance, zAngle);
    }

    public static SolidTrans operator *(SolidDh dh, SolidTrans tran)
    {
        return null;
    }
}
