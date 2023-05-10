using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;








[Serializable]
public class VectorMeasure
{
    public Vector3 vec0;
    public Vector3 vec1;
    public Vector3 vec2;
    
}

[Serializable]
public class TriangleMeasure
{
    public Vector3 vec0;
    public Vector3 vec1;
    public Vector3 vec2;

    public Vector3 vecBegin;
    public Vector3 vecDir;
    public void DisplayTriangle()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(vec0,vec1);
        Gizmos.DrawLine(vec1,vec2);
        Gizmos.DrawLine(vec2,vec0);
        var normal = Vector3.Cross(vec1 - vec0, vec2 - vec1).normalized;
        var distance = Vector3.Dot(normal, vec0);
        var pointDistance = Vector3.Dot(normal, vecBegin);
        var isSameDir = Vector3.Dot(normal, vecDir) > 0;
        var isFarThanPlane = pointDistance.Square() > distance.Square();
        
            
        var zeroPointProjection = normal * distance;
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(Vector3.zero, 0.05f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(zeroPointProjection,0.05f);
        Gizmos.DrawLine(zeroPointProjection, zeroPointProjection + normal);
        var rayPointToTriangle =  vec0 - vecBegin;
        var rayPointDistance = Vector3.Dot(normal, rayPointToTriangle);
        var rayPointProjection = vecBegin + normal * rayPointDistance;
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(vecBegin, 0.05f);
        Gizmos.DrawSphere(rayPointProjection,0.05f);
        Gizmos.DrawLine(vecBegin, vecBegin + vecDir);
        var hitDir = Vector3.Cross(Vector3.Cross(normal, vecDir), normal).normalized;
        Gizmos.DrawLine(rayPointProjection, rayPointProjection + hitDir);
        if (KTransform.GetLineLineIntersection(out var intersection, vecBegin, vecDir, rayPointProjection, hitDir))
        {
            if ((isSameDir && !isFarThanPlane)||(!isSameDir && isFarThanPlane))
            {
                Gizmos.color = Color.red;
            }
            else if (KTransform.CheckInTriangle(vec0, vec1, vec2, intersection))
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.magenta;
            }
            Gizmos.DrawSphere(intersection,0.05f);
        }
        else
        {
            Debug.Log("No intersection!");
        }

        //DisplayCross();
    }

    public float Angle;
    public void DisplayCross()
    {
        var rot = Quaternion.AngleAxis(Angle, Vector3.up);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(Vector3.zero,Vector3.up);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(Vector3.zero,rot * Vector3.right);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(Vector3.zero, Vector3.Cross(Vector3.up, rot * Vector3.right));
    }

}

public class MeshAdjustment : MonoBehaviour
{
    public string objFilePath;
    public string objFileName;
    
    public bool displayMeasurePoints;
    public Vector3 meshRoot;

    public Quaternion meshRotation;
    //public TriangleMeasure triangleMeasure;
    public MeshMeasure meshMeasure;
    internal List<Vector3> _measurePoints = new();

    internal List<int> _measureTriangles = new();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnDrawGizmosSelected()
    {
        DrawAdjustedMeshRoot();
        DrawSlicePlane();
        DrawMeasurePoints();
        //triangleMeasure.DisplayTriangle();
    }

    private void DrawAdjustedMeshRoot()
    {
        Gizmos.DrawSphere(transform.position + meshRoot, 1);
    }
    
    public void DrawSlicePlane()
    {
        var normal = meshMeasure.normal;
        var position = meshMeasure.position;
        var outerRadius = meshMeasure.outerRadius;
        var divNum = meshMeasure.divNum;
        var innerRadius = meshMeasure.innerRadius;

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
    
    public void DrawMeasurePoints()
    {
        if (!displayMeasurePoints) return;
        Gizmos.color = Color.green;
        foreach (var point in _measurePoints)
        {
            var trans = transform;
            var temp = trans.rotation * point;
            temp = trans.position + temp;
            Gizmos.DrawSphere(temp, 0.05f);
        }
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}

[CustomEditor(typeof(MeshAdjustment))]
public class MeshAdjustmentEditor : Editor
{
    public void AdjustVertices(Func<Vector3,Vector3> func)
    {
        var mono = target as MonoBehaviour;
        if (!mono) throw new NullReferenceException();
        if (!mono.TryGetComponent<MeshFilter>(out var mf)) throw new NullReferenceException();
        var mesh = mf.sharedMesh;
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

    public MeshAdjustment MeshAdjustment
    {
        get
        {
            var ma = target as MeshAdjustment;
            if (!ma) throw new NullReferenceException();
            return ma;
        }
    }

    public MeshFilter MeshFilter
    {
        get
        {
            var mono = target as MonoBehaviour;
            if (!mono) throw new NullReferenceException();
            if (!mono.TryGetComponent<MeshFilter>(out var mf)) throw new NullReferenceException();
            return mf;
        }
    }

    public Mesh Mesh
    {
        get
        {
            var mono = target as MonoBehaviour;
            if (!mono) throw new NullReferenceException();
            if (!mono.TryGetComponent<MeshFilter>(out var mf)) throw new NullReferenceException();
            var mesh = mf.mesh;
            return mesh;
        }
    }

    public Mesh SharedMesh
    {
        get
        {
            var mono = target as MonoBehaviour;
            if (!mono) throw new NullReferenceException();
            if (!mono.TryGetComponent<MeshFilter>(out var mf)) throw new NullReferenceException();
            var mesh = mf.sharedMesh;
            return mesh;
        }
    }



    
    
    
    public override void OnInspectorGUI()
    {
        
        
        
        base.OnInspectorGUI();
        if (GUILayout.Button("调整位置"))
        {
            var meshRoot = MeshAdjustment.meshRoot;
            AdjustVertices(v => v - meshRoot);
            MeshAdjustment.transform.position += meshRoot;
            for (var i = 0; i < MeshAdjustment.transform.childCount; i++)
            {
                MeshAdjustment.transform.GetChild(i).transform.position -= meshRoot;
            }
        }

        if (GUILayout.Button("调整旋转"))
        {
            var meshRotation = MeshAdjustment.meshRotation;
            AdjustVertices(v => meshRotation * v);
        }

        if (GUILayout.Button("GPU切割网格"))
        {
            var meshMeasure = MeshAdjustment.meshMeasure;
            (MeshAdjustment._measurePoints,MeshAdjustment._measureTriangles) = meshMeasure.MeshSlice(SharedMesh);
        }

        if (GUILayout.Button("GPU采样网格"))
        {
            var meshMeasure = MeshAdjustment.meshMeasure;
            MeshAdjustment._measurePoints = meshMeasure.MeshSample(SharedMesh, MeshAdjustment._measureTriangles);
        }

        if (GUILayout.Button("得到测量点均值"))
        {
            var midPoint = MeshGeometry.MidPoint(MeshAdjustment._measurePoints);
            MeshAdjustment._measurePoints = new List<Vector3>();
            MeshAdjustment._measurePoints.Add(midPoint);
            var point = (Vector3) (MeshAdjustment.transform.localToWorldMatrix * midPoint);
            Debug.Log($"{point.x:f8},{point.y:f8},{point.z:f8}");
        }

        if (GUILayout.Button("圆形匹配"))
        {
            ShapeMatch.RoundMatch(null);
        }

        if (GUILayout.Button("保存模型"))
        {
            MeshExport.SaveMesh(MeshFilter, MeshAdjustment.objFilePath, MeshAdjustment.objFileName);
        }
        
    }
}
