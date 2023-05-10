using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class MeshExport
{
    public static void SaveMesh(MeshFilter mf, string filePath,string fileName)
    {
        Debug.Log($"模型保存至:{filePath}/{fileName}.obj");
        using StreamWriter sw = new StreamWriter($"{filePath}/{fileName}.obj");
        sw.Write(MeshToString(mf, fileName));
        AssetDatabase.Refresh();
    }
    
    public static string MeshToString(MeshFilter mf,string name)
    {
        var mesh = mf.sharedMesh;
        if (!mesh || mf.GetComponent<Renderer>() == null)
        {
            Debug.LogError("Mesh Error!");
            return null;
        }
        var sb = new StringBuilder();
        sb.Append($"g {name}\n");
        
        foreach (var vec in mesh.vertices)
        {
            sb.Append($"v {-vec.x:f5} {vec.y:f5} {vec.z:f5}\n");
        }
        sb.Append("\n");
        foreach (var nor in mesh.normals)
        {
            sb.Append($"vn {nor.x} {nor.y} {nor.z}\n");
        }
        sb.Append("\n");
        foreach (var uv in mesh.uv)
        {
            sb.Append($"vt {uv.x} {uv.y}\n");
        }
        var mats = mf.GetComponent<Renderer>().sharedMaterials;
        for (var i = 0; i < mats.Length; i++)
        {
            sb.Append("\n");
            sb.Append($"usemtl {mats[i].name}\n");
            var triangles = mesh.GetTriangles(i);
            for (var j = 0; j < triangles.Length; j += 3)
            {
                var index0 = triangles[j] + 1;
                var index1 = triangles[j + 1] + 1;
                var index2 = triangles[j + 2] + 1;
                sb.Append($"f {index2} {index1} {index0}\n");
            }
        }
        return sb.ToString();
    } 
}
