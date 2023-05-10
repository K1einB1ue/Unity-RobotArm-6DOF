

using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;


[Serializable]
[GuiComponent]
public class KRobotInit : GuiComponent
{
    public KJoint.SimulationType simulationType;
    private static List<Type> _guiLayoutRequire = new() { typeof(KRobot) };
    public override List<Type> GuiLayoutRequire => _guiLayoutRequire;
    public override void GuiLayout()
    {
        var kRobot = GetArg<KRobot>(0);
        if (GUILayout.Button("机械臂逆解参数初始化"))
        {
            RobotInit(kRobot);
        }
        if (GUILayout.Button("机械臂模拟模式整体刷新"))
        {
            SimulationTypeInit(kRobot);
        }

        if (GUILayout.Button("机械臂角度复位"))
        {
            AngleRefresh(kRobot);
        }
    }

    public override void GizmosLayout(Transform transform)
    {
       
    }

    public void RobotInit(KRobot kRobot)
    {
        if (kRobot is KRobot6Dof kRobot6Dof)
        {
            var chain0 = kRobot6Dof.joint0.GetComponent<KChain>();
            var chain1 = kRobot6Dof.joint1.GetComponent<KChain>();
            var chain2 = kRobot6Dof.joint2.GetComponent<KChain>();
            var chain3 = kRobot6Dof.joint3.GetComponent<KChain>();
            var chain4 = kRobot6Dof.joint4.GetComponent<KChain>();
            var chain5 = kRobot6Dof.joint5.GetComponent<KChain>();
            if (!chain5.buildTool.isActuator)
            {
                Debug.LogError("这无法构建一个6Dof的机械臂");
                return;
            }

            var targetDefault = chain5.buildTool.targetDefault;
            if (!targetDefault)
            {
                Debug.LogError("没有绑定执行器");
                return;
            }
            
            kRobot6Dof.InitJoint012Constant(
                chain0.buildTool.nextJointOffset,
                chain1.buildTool.nextJointOffset,
                chain2.buildTool.nextJointOffset,
                chain3.buildTool.nextJointOffset,
                chain4.buildTool.nextJointOffset,
                targetDefault.transform.localPosition
            );
        }
    }

    public void AngleRefresh(KRobot kRobot)
    {
        kRobot.transform.Find((tran) =>
        {
            var joint = tran.GetComponent<KJoint>();
            if (joint)
            {
                Debug.Log($"{joint.name}:角度复位.");
                joint.Angle = 0;
            }
            return false;
        });
    }

    public void SimulationTypeInit(KRobot kRobot)
    {
        var ab = kRobot.GetComponent<ArticulationBody>();
        KJoint.SimulationAbMapping(ab, simulationType);
        kRobot.transform.Find((tran) =>
        {
            var joint = tran.GetComponent<KJoint>();
            if (joint)
            {
                joint.RefreshSimulationType(simulationType);
                EditorUtility.SetDirty(joint);
                AssetDatabase.SaveAssets();
            }
            return false;
        });
    }
}