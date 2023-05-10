
using System;
using UnityEngine;

[Serializable]
public class KPid
{
    public float p, i, d;
    public float dt = 0.02f;
    private float _errorSum = 0;
    private float _preError = 0;
    public bool deathZone;
    public float deathZoneRange;
    public bool integralMin;
    public float integralMinRange;
    public bool maxOutput;
    public float max;
    public bool minOutput;
    public float min;

    public float Update(float target, float groundTruth)
    {
        var error = target - groundTruth;
        if (deathZone)
        {
            if (Mathf.Abs(error) < Mathf.Abs(deathZoneRange))
            {
                _errorSum = 0;
                _preError = 0;
                return 0f;
            }
        }

        if (integralMin)
        {
            if (error <= integralMinRange)
            {
                _errorSum += error * dt;
            }
            else
            {
                _errorSum = 0;
            }
        }
        else
        {
            _errorSum += error * dt;
        }
        var o = p * error + i * _errorSum + d * ((error - _preError) / dt);
        _preError = error;

        if (maxOutput)
        {
            if (o > max)
            {
                return max;
            }
        }

        if (minOutput)
        {
            if (o < min)
            {
                return min;
            }
        }
        return o;
    }
    

}
