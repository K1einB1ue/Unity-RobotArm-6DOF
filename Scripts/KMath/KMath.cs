


using System;
using Unity.Mathematics;
using UnityEngine;

public static class KMath
{
    public class Transform
    {
        internal Quaternion rotation;
        internal Vector3 offset;
        public Quaternion Rotation => rotation;
        public Vector3 Offset => offset;
        public Quaternion InvRotation => Quaternion.Inverse(rotation);
        public Transform(){}

        public Transform(Quaternion rotation, Vector3 offset)
        {
            this.rotation = rotation;
            this.offset = offset;
        }
    }
    
    public static Transform Next(this Transform root,Transform jointTrans)
    {
        var rot = jointTrans.rotation * root.rotation;
        var offset = root.offset + rot * jointTrans.offset;
        return new Transform {rotation = rot, offset = offset};
    }

    public static Transform Next(this Transform root, Quaternion rotation, Vector3 offset)
    {
        var rot = rotation * root.rotation;
        var off = root.offset + rotation * offset;
        return new Transform {rotation = rot, offset = off};
    }
    
    public static Transform Next(this Transform root, Vector3 axis, float angle, Vector3 offset)
    {
        var newAxis = root.rotation * axis;
        var jointRotation = Quaternion.AngleAxis(angle, newAxis);
        var rot = jointRotation * root.rotation;
        var off = root.offset + rot * offset;
        return new Transform {rotation = rot, offset = off};
    }

    public class Vector3Slerp
    {
        private readonly float _radiusFrom;
        private readonly float _radiusTo;
        private readonly Vector3 _axisDirFrom;
        private readonly Vector3 _axis;
        private readonly Vector3 _axisPoint;
        private readonly float _yFrom;
        private readonly float _yTo;
        private readonly Vector3 _angleAxis;
        private readonly float _angleOffset;
        public Vector3Slerp(Vector3 from, Vector3 to, Vector3 axis, Vector3 axisPoint)
        {
            axis = axis.normalized;
            _axis = axis;
            _axisPoint = axisPoint;
            var axisToFrom = from - axisPoint;
            var axisToTo = to - axisPoint;
            var axisDirFrom = Vector3.Cross(axis, Vector3.Cross(axisToFrom, axis)).normalized;
            var axisDirTo = Vector3.Cross(axis, Vector3.Cross(axisToTo, axis)).normalized;
            _axisDirFrom = axisDirFrom;
            _yFrom = Vector3.Dot(axisToFrom, axis);
            _yTo = Vector3.Dot(axisToTo, axis);
            _radiusFrom = Vector3.Dot(axisDirFrom, axisToFrom);
            _radiusTo = Vector3.Dot(axisDirTo, axisToTo);
            _angleOffset = math.acos(Vector3.Dot(axisDirTo, axisDirFrom)) * Mathf.Rad2Deg;
            _angleAxis = Vector3.Cross(axisDirFrom, axisDirTo);
        }

        public Vector3 Sample(float time)
        {
            var rot = Quaternion.AngleAxis(_angleOffset * time, _angleAxis);
            var y = (_yTo - _yFrom) * time + _yFrom;
            var p = y * _axis + _axisPoint;
            var dir = rot * _axisDirFrom;
            var radius = (_radiusTo - _radiusFrom) * time + _radiusFrom;
            return radius * dir + p;
        }
    }



    public class RotationMatrix
    {
        public enum RotationAxis
        {
            X,
            Y,
            Z
        }
        private readonly float[,] _data = new float[3, 3];

        public float this[int index]
        {
            set => _data[index / 3, index % 3] = value;
            get => _data[index / 3, index % 3];
        }
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

        public override string ToString()
        {
            return
                $"00:{_data[0, 0]},01:{_data[0, 1]},02:{_data[0, 2]}\n10:{_data[1, 0]},11:{_data[1, 1]},12:{_data[1, 2]}\n20:{_data[2, 0]},21:{_data[2, 1]},22:{_data[2, 2]}";
        }
    }

    
    
}
