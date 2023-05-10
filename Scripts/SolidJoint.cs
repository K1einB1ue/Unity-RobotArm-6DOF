using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class SolidMath
{
    public class RotationMatrix
    {
        public enum RotationAxis
        {
            X,
            Y,
            Z
        }
        private readonly float[,] _data = new float[3, 3];

        public float this[int row, int col]
        {
            set => _data[row, col] = value;
            get => _data[row, col];
        }
        
        //由于Unity使用左手坐标系,所以以下四元数转变为旋转矩阵为JPL表达,而不是Hamilton表达
        public RotationMatrix(Quaternion q)
        {
            var (q0, q1, q2, q3) = (q.w, q.x, q.y, q.z);
            _data[0, 0] = 1 - 2 * q2.Square() - 2 * q3.Square();
            _data[0, 1] = 2 * q1 * q2 + 2 * q0 * q3;
            _data[0, 2] = 2 * q1 * q3 - 2 * q0 * q2;
            _data[1, 0] = 2 * q1 * q2 - 2 * q0 * q3;
            _data[1, 1] = 1 - 2 * q1.Square() - 2 * q3.Square();
            _data[1, 2] = 2 * q2 * q3 + 2 * q0 * q1;
            _data[2, 0] = 2 * q1 * q3 + 2 * q0 * q2;
            _data[2, 1] = 2 * q2 * q3 - 2 * q0 * q1;
            _data[2, 2] = 1 - 2 * q1.Square() - 2 * q2.Square();
        }

        public RotationMatrix(float angle, RotationAxis axis)
        {
            switch (axis)
            {
               case RotationAxis.X:
                   _data[0, 0] = 1; 
                   _data[0, 1] = 0; 
                   _data[0, 2] = 0;
                   _data[1, 0] = 0; 
                   _data[1, 1] = math.cos(angle); 
                   _data[1, 2] = math.sin(angle);
                   _data[2, 0] = 0; 
                   _data[2, 1] = -math.sin(angle); 
                   _data[2, 2] = math.cos(angle);
                   break;
               case RotationAxis.Y:
                   _data[0, 0] = math.cos(angle);
                   _data[0, 1] = 0;
                   _data[0, 2] = -math.sin(angle);
                   _data[1, 0] = 0;
                   _data[1, 1] = 1;
                   _data[1, 2] = 0;
                   _data[2, 0] = math.sin(angle);
                   _data[2, 1] = 0;
                   _data[2, 2] = math.cos(angle);
                   break;
               case RotationAxis.Z: 
                   _data[0, 0] = math.cos(angle);
                   _data[0, 1] = math.sin(angle);
                   _data[0, 2] = 0;
                   _data[1, 0] = -math.sin(angle);
                   _data[1, 1] = math.cos(angle);
                   _data[1, 2] = 0;
                   _data[2, 0] = 0;
                   _data[2, 1] = 0;
                   _data[2, 2] = 1;
                   break;
               default:
                   throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
        }

        public RotationMatrix Dot(RotationMatrix m)
        {
            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    _data[i, j] *= m[j, i];
                }
            }
            return this;
        }

        public float Det()
        {
            return + _data[0, 0] * (_data[1, 1] * _data[2, 2] - _data[1, 2] * _data[2, 1])
                   - _data[0, 1] * (_data[1, 0] * _data[2, 2] - _data[1, 2] * _data[2, 0])
                   + _data[0, 2] * (_data[1, 0] * _data[2, 1] - _data[1, 1] * _data[2, 0]);
        }

        public RotationMatrix Inverse()
        {
            var det = Det();
            if (det == 0) throw new Exception("no Inverse!");
            float a = _data[0, 0], b = _data[0, 1], c = _data[0, 2];
            float d = _data[1, 0], e = _data[1, 1], f = _data[1, 2];
            float g = _data[2, 0], h = _data[2, 1], i = _data[2, 2];
            _data[0, 0] = (e * i - f * h) / det;
            _data[0, 1] = (h * c - b * i) / det;
            _data[0, 2] = (b * f - c * e) / det;
            _data[1, 0] = (f * g - i * d) / det;
            _data[1, 1] = (i * a - g * c) / det;
            _data[1, 2] = (c * d - a * f) / det;
            _data[2, 0] = (d * h - e * g) / det;
            _data[2, 1] = (g * b - h * a) / det;
            _data[2, 2] = (a * e - b * d) / det;
            return this;
        }

        public static Vector3 operator *(RotationMatrix m, Vector3 v3) => new Vector3(
            m[0, 0] * v3.x + m[1, 0] * v3.y + m[2, 0] * v3.z,
            m[0, 1] * v3.x + m[1, 1] * v3.y + m[2, 1] * v3.z,
            m[0, 2] * v3.x + m[1, 2] * v3.y + m[2, 2] * v3.z
        );
        
    }
    
    public static float Dot(this Vector3 v, Vector3 v3)
    {
        return v.x * v3.x + v.y * v3.y + v.z * v3.z;
    }

    public static Vector3 Eff(this Vector3 v, Vector3 v3)
    {
        return new Vector3(v.x * v3.x, v.y * v3.y, v.z * v3.z);
    }

    public static float Square(this float num)
    {
        return num * num;
    }

    public static (float joint0Angle, float joint1Angle, float joint2Angle) 
    GetRrrAnglePosition(
            float joint0X,float joint0Z, 
            float joint12X,float joint12Y, 
            float l0, float l1, float l2
    ) {
        //Debug.Log($"{joint12X},{joint12Y}");
        var tempSquare = joint12X.Square() + joint12Y.Square();
        var joint0Angle = math.atan2(joint0X, joint0Z);
        var joint2Angle = math.acos((tempSquare - l1.Square() - l2.Square()) / (2.0f * l1 * l2));
        var joint1Offset = math.acos((l2.Square() - tempSquare - l1.Square()) / (-2.0f * l1 * math.sqrt(tempSquare)));
        var joint1Base = math.atan2(joint12Y, joint12X);
        var joint1Angle = 0f;
        if (joint2Angle >= 0)
        {
            joint1Angle = joint1Base - joint1Offset;
        }
        else
        {
            joint1Angle = joint1Base + joint1Offset;
        }
                          
        //Debug.Log($"{joint1Base * Mathf.Rad2Deg},{joint1Offset * Mathf.Rad2Deg},{joint0Angle * Mathf.Rad2Deg},{joint1Angle * Mathf.Rad2Deg},{joint2Angle * Mathf.Rad2Deg}");
        return (joint0Angle * Mathf.Rad2Deg, joint1Angle * Mathf.Rad2Deg, joint2Angle * Mathf.Rad2Deg);
    }
    
    
    public static (float joint0Angle, float joint1Angle, float joint2Angle)
    GetRrrAngleZyzRotation(Quaternion target)
    {
        var rm =  new RotationMatrix(target);
        var joint1Angle = math.atan2(math.sqrt(rm[1, 0].Square() + rm[1, 2].Square()), rm[1, 1]);
        var temp = math.sin(joint1Angle);
        var joint0Angle = math.atan2(rm[1, 0] / temp, rm[1, 2] / temp);
        var joint2Angle = math.atan2(rm[0, 1] / temp, -rm[2, 1] / temp);
        return (joint0Angle * Mathf.Rad2Deg, joint1Angle * Mathf.Rad2Deg, joint2Angle * Mathf.Rad2Deg);
    }
}

public class SolidJoint : MonoBehaviour
{
    private static float AxisLength = 10;

    public bool displayJointSpace;
    public bool displayJointAxis;
    
    public bool overrideFlag;
    public float overrideAngle;
    
    public Vector3 rotationAxis;

    private float _angle;



    private void OnDrawGizmosSelected()
    {
        if (overrideFlag)
        {
            _angle = overrideAngle;
        }
        if(displayJointAxis) JointDisplay();
        if(displayJointSpace) JointSpaceDisplay();
    }

    public void JointSpaceDisplay()
    {
        var x = Vector3.right;
        var y = Vector3.up;
        var z = Vector3.forward;
        var tran = transform;
        var quat = tran.rotation;
        var pos = tran.position;
        x = quat * x;
        y = quat * y;
        z = quat * z;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(pos, pos + x * 10);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(pos, pos + y * 10);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(pos, pos + z * 10);
    }
    

    public float JointAngle
    {
        get => _angle;
        set
        {
            var rotation = Quaternion.Euler(0, 0, 0);
            if (transform.parent)
            {
                rotation = transform.parent.rotation;
            }
            if (!overrideFlag) _angle = value;
            transform.rotation = rotation*Quaternion.AngleAxis(_angle, rotationAxis);

            for (var i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).TryGetComponent<SolidJoint>(out var son))
                {
                    son.JointAngle = son._angle;
                }
            }
            
        }
    }

    public void SetJointAngle(float angle)
    {
        var rotation = Quaternion.Euler(0, 0, 0);
        if (transform.parent)
        {
            rotation = transform.parent.rotation;
        }
        if (!overrideFlag) _angle = angle;
        transform.rotation = rotation * Quaternion.AngleAxis(_angle, rotationAxis);
    }
    
    private void JointDisplay()
    {
        //计算旋转轴
        var axis = rotationAxis.normalized;
        //当前的变换对象
        var trans = transform;
        //旋转轴在变换后的方向
        axis = trans.rotation * axis;
        var position = trans.position;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(position-axis*AxisLength,position+axis*AxisLength); 
    }
    
    public Vector3 Localize(Vector3 v3,bool draw)
    {
        var rotation = Quaternion.Euler(0, 0, 0);
        if (transform.parent)
        {
            rotation = transform.parent.rotation;
        }
        
        var pos = transform.position;
        v3 -= pos;
        var uY = rotation * Vector3.up;
        var uX = rotation * Vector3.right;
        var uZ = rotation * Vector3.forward;
        var vecRet = new Vector3(uX.Dot(v3), uY.Dot(v3), uZ.Dot(v3));
        if (draw)
        {
            Debug.DrawLine(pos, pos + uX * vecRet.x);
            Debug.DrawLine(pos, pos + uY * vecRet.y);
            Debug.DrawLine(pos, pos + uZ * vecRet.z);
        }

        return vecRet;
    }
    
    public Quaternion Localize(Quaternion q4)
    {
        var quatRet = Quaternion.Inverse(transform.rotation) * q4;

        return quatRet;
    }
    
    
}



