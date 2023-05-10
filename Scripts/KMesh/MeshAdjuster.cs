
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[GuiComponent]
public class MeshAdjuster : GizmosComponent
{
    public Vector3 targetPosition;
    public Quaternion targetRotation;
    
    private static List<Type> _guiLayoutRequire = new() {typeof(Transform),typeof(MeshFilter)};
    public override List<Type> GuiLayoutRequire => _guiLayoutRequire;
    public override void GuiLayout()
    {
        var tran = GetArg<Transform>(0);
        var mf = GetArg<MeshFilter>(1);
        if (GUILayout.Button("调整模型空间原点"))
        {
            MeshPositionAdjust(tran, mf.sharedMesh);
        }
        if (GUILayout.Button("调整模型的旋转"))
        {
            MeshRotationAdjust(tran, mf.sharedMesh);
        }

        if (GUILayout.Button("调整旋转至单位旋转,但模型在世界空间中不变"))
        {
            MeshTransformAdjust(tran, mf.sharedMesh);
        }
    }

    public void MeshPositionAdjust(Transform tran,Mesh mesh)
    {
        MeshAdjust(mesh, v => v - targetPosition);
        tran.position += targetPosition;
        for (var i = 0; i < tran.childCount; i++)
        {
            tran.GetChild(i).transform.position -= targetPosition;
        }
    }
    
    public void MeshRotationAdjust(Transform tran,Mesh mesh)
    {
        var rotation = Quaternion.Inverse(targetRotation);
        MeshAdjust(mesh, v => rotation * v);
        rotation.ToAngleAxis(out var angle, out var axis);
        for (var i = 0; i < tran.childCount; i++)
        {
            tran.GetChild(i).transform.RotateAround(tran.position, axis, angle);
        }
    }

    public void MeshTransformAdjust(Transform tran, Mesh mesh)
    {
        var rotation = tran.rotation;
        MeshAdjust(mesh, v => rotation * v);
        tran.rotation = Quaternion.identity;
        rotation.ToAngleAxis(out var angle, out var axis);
        for (var i = 0; i < tran.childCount; i++)
        {
            tran.GetChild(i).transform.RotateAround(tran.position, axis, angle);
        }
    }
    
    public void MeshAdjust(Mesh mesh,Func<Vector3,Vector3> func)
    {
        var vertices = mesh.vertices;
        
        for (var i = 0; i < vertices.Length; i++)
        {
            vertices[i] = func(vertices[i]);
        }
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.Optimize();
    }

    public override void GizmosLayout(Transform transform)
    {
        DrawTargetPosition(transform);
        DrawTargetRotation(transform);
    }

    public void DrawTargetPosition(Transform transform)
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(transform.rotation * (targetPosition + transform.position), 0.05f);
    }

    public void DrawTargetRotation(Transform transform)
    {
        var rotation = Quaternion.Inverse(transform.rotation);
        var position = transform.position;
        var x= rotation *Vector3.right;
        var y= rotation *Vector3.up;
        var z= rotation *Vector3.forward;
        var tX = targetRotation * x;
        var tY = targetRotation * y;
        var tZ = targetRotation * z;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(position, position + x);
        Gizmos.DrawLine(position, position + tX);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(position, position + y);
        Gizmos.DrawLine(position, position + tY);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(position, position + z);
        Gizmos.DrawLine(position, position + tZ);
    }
}
