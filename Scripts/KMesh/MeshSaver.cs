
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[GuiComponent]
public class MeshSaver : GuiComponent
{
    public string objFilePath;
    public string objFileName;
    
    private static List<Type> _guiLayoutRequire = new() {typeof(MeshFilter)};
    public override List<Type> GuiLayoutRequire => _guiLayoutRequire;

    public override void GuiLayout()
    {
        var mf = GetArg<MeshFilter>(0);
        if (GUILayout.Button("保存模型"))
        {
            MeshExport.SaveMesh(mf, objFilePath, objFileName);
        }
    }

    public override void GizmosLayout(Transform transform)
    {
        
    }
}
