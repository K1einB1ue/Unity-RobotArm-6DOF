
using System;
using System.Collections.Generic;
using UnityEngine;

public class CamVisionMove : MonoBehaviour
{
    public bool enable;
    public int stableNum;
    public int _stableCnt = 0;
    public KRobot robot;
    public KPid xPid = new();
    public KPid yPid = new();
    public int xTarget;
    public int yTarget;
    private int _xReal;
    private int _yReal;
    private bool _canSee = false;
    private bool _enable = false;
    
    public bool Enable
    {
        get => _enable;
        set => _enable = value;
    }

    public void SetReal(object offs)
    {
        var off = (List<object>)offs;
        var see = (int) (double) off[0];
        var xValue = (int) (double) off[1];
        var yValue = (int) (double) off[2];
        if (see==0) {
            _canSee = false;
        }else {
            _canSee = true;
            _xReal = xValue;
            _yReal = yValue;
        }
        
    }

    public void FixedUpdate()
    {

    }

    public bool UpdateCamVisionMove()
    {
        if (!robot) throw new Exception("没有绑定作用机械臂!");
        if (!_enable) return false;
        if (!_canSee)
        {
            Debug.Log("没有找到目标");
            return false;
        }
        if (_yReal == yTarget && _xReal == xTarget)
        {
            _stableCnt++;
            if (_stableCnt == stableNum)
            {
                _stableCnt = 0;
                return true;
            }
        }
        var yO = yPid.Update(yTarget, _yReal);
        var xO = xPid.Update(xTarget, _xReal);
        var rotation = transform.rotation;
        var yMove = rotation * Vector3.up * yO;
        var xMove = rotation * Vector3.right * xO;
        robot.MoveTo(yMove + xMove);
        return false;
    }
}
