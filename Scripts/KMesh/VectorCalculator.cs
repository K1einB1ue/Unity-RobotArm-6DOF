using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[GuiComponent]
public class VectorCalculator : GuiComponent
{
    public Vector3 posFrom;
    public Vector3 posTo;
    public Vector3 offsetOutput;
    
    public Vector3 GetOffset()
    {
        var offset = posTo - posFrom;
        offsetOutput = offset;
        return offset;
    }

    private static List<Type> _guiLayoutRequire = new();
    public override List<Type> GuiLayoutRequire => _guiLayoutRequire;

    public override void GuiLayout()
    {
        if (GUILayout.Button("计算偏移"))
        {
            GetOffset();
        }
    }

    public override void GizmosLayout(Transform transform)
    {
        
    }
}