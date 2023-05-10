

using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[GuiComponent]
public class MeshMeasure : GizmosComponent
{
    private enum StateEnum
    {
        Null,
        Slice,
        Sample,
        Average
    }
    
    public Vector3 position;
    public Vector3 normal;
    public float outerRadius;
    public float innerRadius;
    public int divNum = 720;
    public Vector3 singleRayPosition;
    public Vector3 singleRayDirection;

    public Vector3 averageOutput;
    public Vector3 averageOutputTransToWorld;
    private List<int> _triangle = new();
    private List<Vector3> _points = new();
    private StateEnum _stateEnum = StateEnum.Null;
    public (List<Vector3>,List<int>) MeshSlice(Mesh mesh)
    {
        var ret = MeshGeometry.Slice(mesh, position, normal, outerRadius, innerRadius);
        _points = ret.Item1;
        _triangle = ret.Item2;
        _stateEnum = StateEnum.Slice;
        return ret;
    }

    public List<Vector3> MeshSample(Mesh mesh, List<int> triangles)
    {
        if (triangles.Count == 0)
        {
            Debug.Log("没有可以采样的三角面");
            return new List<Vector3>();
        }
        List<(Vector3, Vector3)> rays = new();
        var vert = KTransform.GetVerticalVec(normal);
        for (var i = 0; i < divNum; i++)
        {
            var dir = Quaternion.AngleAxis((360.0f / divNum) * i, normal) * vert;
            rays.Add((position, dir));
        }
        var ret = MeshGeometry.Raycast(mesh, triangles, rays);
        _points = ret;
        _stateEnum = StateEnum.Sample;
        return _points;
    }

    public List<Vector3> SingleRayMeshSample(Mesh mesh, List<int> triangles,Transform tran)
    {
        var rotation = Quaternion.Inverse(tran.rotation);
        List<(Vector3, Vector3)> rays = new() {(rotation * singleRayPosition, rotation * singleRayDirection)};
        var ret = MeshGeometry.Raycast(mesh, triangles, rays);
        _points = ret;
        _stateEnum = StateEnum.Sample;
        return _points;
    }

    public Vector3 GetAverage()
    {
        var ret = MeshGeometry.MidPoint(_points);
        averageOutput = ret;
        _stateEnum = StateEnum.Average;
        return ret;
    }
    

    private static List<Type> _guiLayoutRequire = new() {typeof(Transform),typeof(MeshFilter)};
    public override bool Display => display;
    public override List<Type> GuiLayoutRequire => _guiLayoutRequire;

    public override void GuiLayout()
    {
        var tran = GetArg<Transform>(0);
        var mf = GetArg<MeshFilter>(1);
        if (GUILayout.Button("GPU片元线段切割"))
        {
            MeshSlice(mf.sharedMesh);
        }
        if (GUILayout.Button("GPU对切割的片元射线采样"))
        {
            MeshSample(mf.sharedMesh, _triangle);
        }

        if (GUILayout.Button("GPU单射线采样"))
        {
            SingleRayMeshSample(mf.sharedMesh, _triangle,tran);
        }
        if (GUILayout.Button("得到采样点平均值"))
        {
            GetAverage();
            averageOutputTransToWorld = tran.localToWorldMatrix * averageOutput;
        }
    }

    public override void GizmosLayout(Transform transform)
    {
        DrawSlicePlane(transform);
        DrawPoints(transform);
        DrawSingleRay(transform);
    }







    public void DrawSingleRay(Transform transform)
    {
        var worldDirection = singleRayDirection;
        var worldPosition = singleRayPosition + transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(worldPosition, worldPosition + worldDirection);
    }
    
    
    public void DrawSlicePlane(Transform transform)
    {
        var worldNormal = transform.rotation * normal.normalized;
        var worldPosition = position + transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(worldPosition, worldPosition + worldNormal * outerRadius);
        
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
        var outerBegin = worldPosition + vert * outerRadius;
        var outerFrom = outerBegin;
        for (var i = 1; i < divNum; i++)
        {
            var to = worldPosition + Quaternion.AngleAxis((360.0f / divNum) * i, worldNormal) * vert * outerRadius;
            Gizmos.DrawLine(outerFrom, to);
            outerFrom = to;
        }
        Gizmos.DrawLine(outerFrom, outerBegin);
        Gizmos.color = Color.blue;
        var innerBegin = worldPosition + vert * innerRadius;
        var innerFrom = innerBegin;
        for (var i = 1; i < divNum; i++)
        {
            var to = worldPosition + Quaternion.AngleAxis((360.0f / divNum) * i, worldNormal) * vert * innerRadius;
            Gizmos.DrawLine(innerFrom, to);
            innerFrom = to;
        }
        Gizmos.DrawLine(innerFrom, innerBegin);
    }
    
    public void DrawPoints(Transform transform)
    {
        switch (_stateEnum)
        {
            case StateEnum.Null:return;
            case StateEnum.Slice:
            {
                Gizmos.color = Color.green;
                foreach (var point in _points)
                {
                    var temp = transform.rotation * point;
                    temp = transform.position + temp;
                    Gizmos.DrawSphere(temp, 0.05f);
                }

                return;
            }
            case StateEnum.Sample:
            {
                Gizmos.color = Color.cyan;
                foreach (var point in _points)
                {
                    var temp = transform.rotation * point;
                    temp = transform.position + temp;
                    Gizmos.DrawSphere(temp, 0.05f);
                }

                return;
            }
            case StateEnum.Average:
            {
                Gizmos.color = Color.red;
                var temp = transform.rotation * averageOutput;
                temp = transform.position + temp;
                Gizmos.DrawSphere(temp, 0.05f);
                return;
            }
        }
    }
}
