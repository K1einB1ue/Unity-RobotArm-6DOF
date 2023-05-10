using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;




[Serializable]
public class MotorSimInfo
{
    public bool enable = true;
    public KPid torqueAnglePid;
    public ForceMode forceMode;
}

[Serializable]
public class StepperSimInfo
{
    public bool enable = true;
    public int divNumPerRound;
}

[Serializable]
public class DigitalTwinInfo
{
    public bool enable = true;
    public enum DisplayType
    {
        VirtualFirst,
        RealFirst
    }

    public DisplayType displayType;
    public float angleOffset;
}



[RequireComponent(typeof(ArticulationBody))]
public class KJoint : MonoBehaviour
{
    public KJointDebugger debugger;
    public bool angleRangeEnable = false;
    public Vector2 angleRange = new(-360, 360);
    public Vector3 angleAxis;
    private float _angle;
    public bool debug;
    
    public enum SimulationType
    {
        None,
        Doll,
        Motor,
        Stepper,
        DigitalTwin,
    }

    #region SimulationTypeSwitch
    public static void SimulationAbMapping(ArticulationBody ab,SimulationType s)
    {
        ab.enabled = s switch
        {
            SimulationType.None => false,
            SimulationType.Doll => true,
            SimulationType.Motor => true,
            SimulationType.Stepper => true,
            SimulationType.DigitalTwin => false,
            _ => throw new ArgumentOutOfRangeException(nameof(s), s, null)
        };
        ab.xDrive = s switch
        {
            SimulationType.Motor => new ArticulationDrive {
                lowerLimit = float.NegativeInfinity, 
                upperLimit = float.PositiveInfinity,
            },
            SimulationType.Stepper=>new ArticulationDrive
            {
                lowerLimit = 0f,
                upperLimit = 0f,
            },
            _ => ab.xDrive
        };
    }
    public void RefreshSimulationType(SimulationType s)
    {
        if (s != simulationType) {
            var ab = GetComponent<ArticulationBody>();
            SimulationAbMapping(ab, s);
            simulationType = s;
        }
    }
    #endregion
    

    private ArticulationBody _ab;
    public SimulationType simulationType;
    private KObserverField<SimulationType> _obSimulationType;
    public MotorSimInfo motorSimInfo;
    public StepperSimInfo stepperSimInfo;
    public DigitalTwinInfo digitalTwinInfo;



    private void OnEnable()
    {
        RefreshSimulationType(simulationType);
        _ab = GetComponent<ArticulationBody>();
        _obSimulationType = new KObserverField<SimulationType>(simulationType,
        (pre, cur) => RefreshSimulationType(cur));

    }
    

    private void FixedUpdate()
    {
        #region PlayingMode
        if (Application.isPlaying) _obSimulationType.Update(simulationType);
        #endregion
        
        switch (simulationType)
        {
            case SimulationType.None:
                break;
            case SimulationType.Motor:
            {
                
                var diff = Quaternion.Inverse(transform.localRotation) * Quaternion.AngleAxis(_angle, angleAxis);
                diff.ToAngleAxis(out var angle, out var axis);
                if (Vector3.Dot(angleAxis, axis) < 0) {
                    angle = 360f - angle;
                }if (angle > 180f) {
                    angle -= 360f;
                }
                if (motorSimInfo.enable)
                {
                    var o = motorSimInfo.torqueAnglePid.Update(0, -angle);
                    _ab.AddTorque(transform.rotation * angleAxis.normalized * o, motorSimInfo.forceMode);
                }
                if (debugger)
                {
                    debugger.Push(angle, _angle);
                }
                break;
                
            }
            case SimulationType.Stepper:
            {
                var diffQuat= Quaternion.Inverse(transform.localRotation) * Quaternion.AngleAxis(_angle, angleAxis);
                diffQuat.ToAngleAxis(out var angle, out var axis);
                if (Vector3.Dot(angleAxis,axis)<0) {
                    angle = 360f - angle;
                } if (angle > 180f) {
                    angle -= 360f;
                }
                var newAngle = _ab.xDrive.lowerLimit + angle;
                if (stepperSimInfo.enable)
                {
                    _ab.xDrive = new ArticulationDrive
                    {
                        lowerLimit = newAngle,
                        upperLimit = newAngle,
                    };
                }
                break;
            }
            case SimulationType.Doll:
                break;
            case SimulationType.DigitalTwin:
                break;
        }

    }

    public float Angle
    {
        get
        {
            #region PlayingMode
            if (Application.isPlaying) _obSimulationType.Update(simulationType);
            #endregion

            #region EditorMode
            else if (Application.isEditor) RefreshSimulationType(simulationType);
            #endregion

            return _angle;
        }
        set
        {
            #region playingMode

            if (Application.isPlaying)
            {
                _obSimulationType.Update(simulationType);
            }
            #endregion

            #region EditorMode
            else if (Application.isEditor)
            {
                RefreshSimulationType(simulationType);

                _angle = value;
                if (math.isnan(value)) {
                    Debug.LogError($"{name}:发生了解析错误.");
                } 
                if (angleRangeEnable) {
                    if (value > angleRange.y || value < angleRange.x) Debug.LogError($"{name}:发生了角度越界.");
                }
                var quat = Quaternion.AngleAxis(_angle, angleAxis);
                transform.localRotation = quat;
                return;
            }
            #endregion

            if (math.isnan(value)) {
                throw new AnalyticalException($"{name}:发生了解析错误.");
            } if (angleRangeEnable) {
                if (value > angleRange.y || value < angleRange.x) throw new AngleException($"{name}:发生了角度越界.");
            }
            
            _angle = value;
            
            switch (simulationType)
            {
                case SimulationType.None:
                {
                    var quat = Quaternion.AngleAxis(_angle, angleAxis);
                    transform.localRotation = quat;
                    return;
                }
                case SimulationType.Motor:
                    return;
                case SimulationType.Stepper:
                    return;
                case SimulationType.Doll:
                    return;
            }
        }
    }
    
    

    public KRobot.IKError AngleCheck(float angle)
    {
        var error = KRobot.IKError.None;
        if (math.isnan(angle))
        {
            error |= KRobot.IKError.AnalyticalError;
        } if (angleRangeEnable)
        {
            if (angle > angleRange.y || angle < angleRange.x) error |= KRobot.IKError.AngleError;
        }
        return error;
    }

    public void RefreshAngle()
    {
        var quat = Quaternion.AngleAxis(_angle , angleAxis);
        transform.localRotation = quat;
    }
    
    public Quaternion LocalRotation => Quaternion.AngleAxis(_angle , angleAxis);

}
