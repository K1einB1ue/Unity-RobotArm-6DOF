using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[GuiComponent]
public class KRobotDebug : GuiComponent
{
    private static List<Type> _guiLayoutRequire = new() { typeof(KRobot) };
    public override List<Type> GuiLayoutRequire => _guiLayoutRequire;
    public Transform moveTarget;
    public float time;
    public int divNum;
    public override void GuiLayout()
    {
        var robot = GetArg<KRobot>(0);
        if (GUILayout.Button("移动"))
        {
            robot.MoveTo(moveTarget.position, moveTarget.rotation, time, divNum);
        }
    }

    public override void GizmosLayout(Transform transform)
    {
        throw new NotImplementedException();
    }
}
