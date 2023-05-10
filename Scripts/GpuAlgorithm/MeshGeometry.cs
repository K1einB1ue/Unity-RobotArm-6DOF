using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MeshGeometry
{
    
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct SliceInfo
    {
        public Vector3 slicePoint;
        public int hasSlice;
    }
    public static ComputeShader MeshSlice =>
        AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/ComputeShader/MeshSlice.compute");
    
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct RaycastInfo
    {
        public Vector3 raycastPoint;
        public int hasIntersection;
    }
    
    public static ComputeShader MeshIntersection =>
        AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/ComputeShader/MeshIntersection.compute");
    
    public static Vector3 MidPoint(List<Vector3> points)
    {
        if (points == null) return Vector3.zero;
        var sum = new Vector3(0, 0, 0);
        foreach (var point in points)
        {
            sum += point;
        }
        return sum / points.Count;
    }

    public static List<Vector3> Raycast(Mesh mesh, List<int> triangleNums, List<(Vector3,Vector3)> rays)
    {
        var ret = new List<Vector3>();
        var meshIntersection = MeshIntersection;
        var raycastK = meshIntersection.FindKernel("MeshRaycast");
        /*
        StructuredBuffer<float3> _sourceVertices;
        StructuredBuffer<int> _sourceIndices;
        float3 _position;
        float3 _direction;
        int _triangleNum;

        RWStructuredBuffer<RaycastInfo> _raycastPoints;
        */
        var sourceVertices = new GraphicsBuffer(GraphicsBuffer.Target.Structured, mesh.vertices.Length, sizeof(float)*3);
        var sourceIndices = new GraphicsBuffer(GraphicsBuffer.Target.Structured, mesh.triangles.Length, sizeof(int)*1);
        
        var raycastPoints = new GraphicsBuffer(GraphicsBuffer.Target.Structured, mesh.triangles.Length * 2,
            sizeof(float) * 3 + sizeof(int) * 1);

        meshIntersection.SetInt("_triangleNum", triangleNums.Count);
        meshIntersection.SetBuffer(raycastK, "_sourceVertices", sourceVertices);
        meshIntersection.SetBuffer(raycastK, "_sourceIndices", sourceIndices);
        meshIntersection.SetBuffer(raycastK, "_raycastPoints", raycastPoints);
        
        sourceVertices.SetData(mesh.vertices);
        var meshTriangles = mesh.triangles;
        var trianglesData = new int[triangleNums.Count * 3];
        for (var i = 0; i < triangleNums.Count; i++)
        {
            Debug.Log(triangleNums[i]);
            trianglesData[i * 3] = meshTriangles[triangleNums[i] * 3];
            trianglesData[i * 3 + 1] = meshTriangles[triangleNums[i] * 3 + 1];
            trianglesData[i * 3 + 2] = meshTriangles[triangleNums[i] * 3 + 2];
        }
        sourceIndices.SetData(trianglesData);
        var raycastInfos = new RaycastInfo[triangleNums.Count];
        
        foreach (var ray in rays)
        {
            var (position, direction) = ray;
            meshIntersection.SetVector("_position", position);
            meshIntersection.SetVector("_direction", direction);
            // 找到所需的调度大小，保证每个三角形都将被遍历
            meshIntersection.GetKernelThreadGroupSizes(raycastK, out var sizeX, out _, out _);
            // 这里Mathf.CeilToInt 向上取整 保证使用足够的派遣队列
            var dispatchSize = Mathf.CeilToInt((float)triangleNums.Count / sizeX);
            //调用
            meshIntersection.Dispatch(raycastK, dispatchSize, 1, 1);
            
            
            raycastPoints.GetData(raycastInfos);
        
            foreach (var raycastInfo in raycastInfos)
            {
                if (raycastInfo.hasIntersection != 0)
                {
                    ret.Add(raycastInfo.raycastPoint);
                }
            }
        }
        Debug.Log($"共找到{ret.Count}");
        sourceVertices.Release();
        sourceIndices.Release();
        raycastPoints.Release();
        return ret;
    }

    public static (List<Vector3>,List<int>) Slice(Mesh mesh, Vector3 position, Vector3 normal, float outerRadius = float.MaxValue, float innerRadius = 0f)
    {
        var ret = new List<Vector3>();
        var slicedTriangles = new HashSet<int>();
        var meshSlice = MeshSlice;
        var sliceK = meshSlice.FindKernel("MeshSlice");
        

        //得到模型空间的面(面由法向量和面上一点构成)
        var normalizedNormal = normal.normalized;
        //把面的数据结构变为法向量和面到原点的距离
        var planeDistance = Vector3.Dot(normalizedNormal, position);
        
        var sourceVertices = new GraphicsBuffer(GraphicsBuffer.Target.Structured, mesh.vertices.Length, sizeof(float)*3);
        var sourceIndices = new GraphicsBuffer(GraphicsBuffer.Target.Structured, mesh.triangles.Length, sizeof(int)*1);

        var slicePoints = new GraphicsBuffer(GraphicsBuffer.Target.Structured, mesh.triangles.Length * 2,
            sizeof(float) * 3 + sizeof(int) * 1);
        
        meshSlice.SetFloat("_innerRadius",innerRadius);
        meshSlice.SetFloat("_outerRadius",outerRadius);
        meshSlice.SetFloat("_slicePlaneDistance", planeDistance);
        meshSlice.SetInt("_triangleNum", mesh.triangles.Length);
        meshSlice.SetVector("_slicePlaneNormal", normalizedNormal);
        meshSlice.SetVector("_slicePlaneMidPoint", position);
        meshSlice.SetBuffer(sliceK, "_sourceVertices", sourceVertices);
        meshSlice.SetBuffer(sliceK, "_sourceIndices", sourceIndices);
        meshSlice.SetBuffer(sliceK, "_slicePoints", slicePoints);

        sourceVertices.SetData(mesh.vertices);
        sourceIndices.SetData(mesh.triangles);
        
        // 找到所需的调度大小，保证每个三角形都将被遍历
        meshSlice.GetKernelThreadGroupSizes(sliceK, out var sizeX, out _, out _);
        // 这里Mathf.CeilToInt 向上取整 保证使用足够的派遣队列
        var dispatchSize = Mathf.CeilToInt(mesh.triangles.Length / 3.0f / sizeX);
        //调用
        meshSlice.Dispatch(sliceK, dispatchSize, 1, 1);

        var sliceInfos = new SliceInfo[mesh.triangles.Length / 3 * 2];
        slicePoints.GetData(sliceInfos);

        for (var i = 0; i < sliceInfos.Length; i++)
        {
            var sliceInfo = sliceInfos[i];
            if (sliceInfo.hasSlice != 0)
            {
                slicedTriangles.Add(i / 2);
                ret.Add(sliceInfo.slicePoint);
            }
        }
        
        var retSliced = new List<int>();
        foreach (var value in slicedTriangles)
        {
            retSliced.Add(value);
        }

        sourceVertices.Release();
        sourceIndices.Release();
        slicePoints.Release();
        
        return (ret,retSliced);
    }
}

