
using System;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
[RequireComponent(typeof(ArticulationBody))]
public abstract class KRobot : MonoBehaviour
{
    private bool _moving = false;
    public bool Moving => _moving;
    [Flags]
    public enum IKError
    {
        None = 0,
        AngleError = 1<<0,
        AnalyticalError = 1<<1,
        Mixture = AngleError | AnalyticalError
    }
    public bool jacobianMode = false;
    public JacobianInfo jacobianInfo = new();
    public abstract int RobotSize { get; }
    public abstract IKError IK(List<float> ikStore,Vector3 vec, Quaternion rot);
    public abstract void Move(List<float> angles, int index);
    protected abstract List<(KMath.Transform,Vector3)> CurrentInfo { get; }
    
    public void MoveTo(Vector3 vec, Quaternion rot, float time, int divNum)
    {
        var infos = CurrentInfo;
        var vecNow = infos[^1].Item1.offset;
        var rotNow = infos[^1].Item1.rotation;
        var slerp = new KMath.Vector3Slerp(
            vecNow, vec, 
            infos[0].Item1.rotation * infos[0].Item2,
            infos[0].Item1.offset
            );
        var angles = new List<float>();
        for (var i = 0; i < divNum; i++)
        {
            var t = i / (float) divNum;
            var newVec = slerp.Sample(t);
            var newRot = Quaternion.Slerp(rotNow, rot, t);
            var error = IK(angles, newVec, newRot);
            if (error != IKError.None)
            {
                Debug.LogError("解析失败!");
                return;
            }
        }
        var index = 0;
        _moving = true;
        if (Application.isPlaying)
        {
            StartCoroutine(KTimer.TimeLoop(() =>
            {
                if (index == divNum)
                {
                    _moving = false;
                    return true;
                }
                Move(angles, index++);
                return false;
            }, time / divNum));
        }else {
            EditorCoroutineRunner.StartEditorCoroutine(KTimer.TimeLoop(() =>
            {
                if (index == divNum)
                {
                    _moving = false;
                    return true;
                }
                Move(angles, index++);
                return false;
            }, time / divNum));
        }
    }

    public void MoveTo(Transform tran, float time, int divNum)
    {
        MoveTo(tran.position, tran.rotation, time, divNum);
    }

    public void MoveTo(Vector3 offset, float time, int divNum)
    {
        var infos = CurrentInfo;
        var vecNow = infos[^1].Item1.offset;
        var rotNow = infos[^1].Item1.rotation;
        var slerp = new KMath.Vector3Slerp(
            vecNow, vecNow + offset,
            infos[0].Item1.rotation * infos[0].Item2,
            infos[0].Item1.offset
        );
        var angles = new List<float>();
        for (var i = 0; i < divNum; i++)
        {
            var t = i / (float) divNum;
            var newVec = slerp.Sample(t);
            var error = IK(angles, newVec, rotNow);
            if (error != IKError.None)
            {
                Debug.LogError("解析失败!");
                return;
            }
        }
        var index = 0;
        _moving = true;
        if (Application.isPlaying)
        {
            StartCoroutine(KTimer.TimeLoop(() =>
            {
                if (index == divNum)
                {
                    _moving = false;
                    return true;
                }
                Move(angles, index++);
                return false;
            }, time / divNum));
        }else {
            EditorCoroutineRunner.StartEditorCoroutine(KTimer.TimeLoop(() =>
            {
                if (index == divNum)
                {
                    _moving = false;
                    return true;
                }
                Move(angles, index++);
                return false;
            }, time / divNum));
        }
    }

    public void MoveTo(Vector3 offset)
    {
        var infos = CurrentInfo;
        var vecNow = infos[^1].Item1.offset;
        var rotNow = infos[^1].Item1.rotation;
        var newVec = vecNow + offset;
        var angles = new List<float>();
        var error = IK(angles, newVec, rotNow);
        if (error != IKError.None)
        {
            Debug.LogError("解析失败!");
            return;
        }
        Move(angles, 0);
    }
}

public class AnalyticalException : Exception
{
    public AnalyticalException(string log):base(log){}
}
public class AngleException : Exception
{
    public AngleException(string log):base(log){}
}


[Serializable]
public class JacobianInfo
{
    public KPid positionPid;
    public KPid rotationPid;
}