using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KChain : GuiDisplayer
{
    public BuildTool buildTool;
    public DebugTool debugTool;
}

[Serializable]
[GuiComponent]
public class BuildTool : GuiComponent
{
    public Vector3 nextJointOffset;
    public bool isActuator;
    public List<KJoint> linkTarget = new();
    public List<Vector3> linkOffsets = new();
    public override bool Display => isActuator;
    public KTarget targetDefault;


    private static List<Type> _guiLayoutRequire = new() {typeof(Transform)};
    public override List<Type> GuiLayoutRequire => _guiLayoutRequire;
    
    
    
    public override void GuiLayout()
    {
        var tran = GetArg<Transform>(0);
        if (GUILayout.Button("组装"))
        {
            BuildUp(tran);
        }
    }

    public override void GizmosLayout(Transform transform)
    {
        
    }

    public void BuildUp(Transform tran)
    {
        if (isActuator)
        {
            if (linkTarget.Count != linkOffsets.Count)
            {
                throw new Exception("链接目标和链接点数量不一致");
            }

            for (int i = 0; i < linkTarget.Count; i++)
            {
                var kJoint = linkTarget[i];
                var sonTran = kJoint.transform;
                sonTran.localPosition = linkOffsets[i];
                sonTran.localRotation = kJoint.LocalRotation;
            }

            if (!targetDefault)
            {
                for (var i = 0; i < tran.childCount; i++)
                {
                    var sonTran = tran.GetChild(i);
                    if (sonTran.TryGetComponent<KTarget>(out var target))
                    {
                        targetDefault = target;
                        Debug.Log($"已自动绑定 target:\"{target.name}\"");
                        break;
                    }
                }
                if (!targetDefault) Debug.LogError("未在操作器上绑定工具坐标系偏转!");
            }

            if (targetDefault)
            {
                Debug.Log("构建成功");
            }
        }
        for (var i = 0; i < tran.childCount; i++)
        {
            var sonTran = tran.GetChild(i);
            if (!sonTran.TryGetComponent<KChain>(out var chain)) continue;
            if (!isActuator)
            {
                var kJoint = chain.GetComponent<KJoint>();
                sonTran.localPosition = nextJointOffset;
                sonTran.localRotation = kJoint.LocalRotation;
            }
            chain.buildTool.BuildUp(sonTran);
            return;
        }
    }
}


[Serializable]
[GuiComponent]
public class DebugTool : GuiComponent
{
    private static float _axisLength = 10;
    public bool kJointAxisDisplay;
    public bool kJointDebugMode;
    public float kJointDebugAngle;
    public bool isBase;
    public override bool Enable => !isBase;
    public override bool Display => true;

    private static List<Type> _guiLayoutRequire = new() {typeof(KJoint)};
    public override List<Type> GuiLayoutRequire => _guiLayoutRequire;
    public override void GuiLayout()
    {
        var kJoint = GetArg<KJoint>(0);
        if (GUILayout.Button("复位角度"))
        {
            kJoint.Angle = 0;
        }
        if (kJointDebugMode)
            kJoint.Angle = kJointDebugAngle;
        else kJoint.RefreshAngle();
    }

    public override void GizmosLayout(Transform transform)
    {
        if (kJointAxisDisplay)
        {
            var kJoint = transform.GetComponent<KJoint>();
            var axis = kJoint.angleAxis.normalized;
            Gizmos.color = new Color(axis.x, axis.y, axis.z);
            axis = transform.rotation * (axis * _axisLength);
            var position = transform.position;
            Gizmos.DrawLine(position - axis, position + axis);
        }
    }
}
