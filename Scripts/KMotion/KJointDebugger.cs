
using System;
using System.Collections.Generic;
using UnityEngine;

public class KJointDebugger : MonoBehaviour
{
    internal readonly List<float> bufferAngles = new();
    public List<Color> colors = new() {Color.red, Color.green, Color.cyan, Color.blue, Color.magenta, Color.yellow};
    public float angleSlider;
    public float xDistance;
    public float yScale;
    private int _angleSize = 1;
    public int maxCapacity;
    public bool complex;
    private int _startIndex;
    

    public void Push(params float[] angles)
    {
        _angleSize = angles.Length;
        if (bufferAngles.Count >= maxCapacity * _angleSize)
        {
            for (var i = 0; i < _angleSize; i++)
            {
                bufferAngles[_startIndex * _angleSize + i] = angles[i];
            }
            _startIndex++;
            if (_startIndex == maxCapacity)
            {
                _startIndex = 0;
            }
        }
        else
        {
            for (var i = 0; i < _angleSize; i++)
            {
                bufferAngles.Add(angles[i]);
            }
        }
    }


    private void OnDrawGizmos()
    {
        var tran = transform;
        var position = tran.position;
        var rotation = tran.rotation;
        var robotSize = 0;
        var angles = bufferAngles;
        robotSize = _angleSize;

        if (complex)
        {
            Quaternion rot;
            var begin = Quaternion.AngleAxis(0, Vector3.right) * (Vector3.up * yScale);
            var pre = begin;
            Gizmos.color=Color.white;
            for (var i = 1; i < 360; i++)
            {
                var newP = Quaternion.AngleAxis(i, Vector3.right) * (Vector3.up * yScale);
                Gizmos.DrawLine((position + rotation * pre), (position + rotation * newP));
                pre = newP;
            }
            Gizmos.DrawLine(position + rotation * pre, position + rotation * begin);

            var tempRot = Quaternion.AngleAxis(angleSlider, Vector3.right);
            var tempVec = tempRot * Vector3.up * yScale;
            Gizmos.DrawLine(position + rotation * tempVec,
                position + rotation * (tempVec + xDistance * Vector3.right * angles.Count / robotSize));
            
            var prev = new Vector3[robotSize];
            if (angles.Count / robotSize > 0)
            {
                for (var j = 0; j < robotSize; j++)
                {
                    rot = Quaternion.AngleAxis(angles[_startIndex * robotSize + j], Vector3.right);
                    prev[j] = rot * (Vector3.up * yScale);
                }
            }

            var distance = 1;
            
            for (var i = _startIndex + 1; i < angles.Count / robotSize; i++)
            {
                for (var j = 0; j < robotSize; j++)
                {
                    Gizmos.color = colors[j];
                    rot = Quaternion.AngleAxis(angles[i * robotSize + j], Vector3.right);
                    var vec = Vector3.zero;
                    vec.x = distance++ * xDistance;
                    vec += rot * (Vector3.up * yScale);
                    Gizmos.DrawLine((position + rotation * prev[j]), (position + rotation * vec));
                    prev[j] = vec;
                }
            }
            
            for (var i = 0; i < _startIndex; i++)
            {
                for (var j = 0; j < robotSize; j++)
                {
                    Gizmos.color = colors[j];
                    rot = Quaternion.AngleAxis(angles[i * robotSize + j], Vector3.right);
                    var vec = Vector3.zero;
                    vec.x = distance++ * xDistance;
                    vec += rot * (Vector3.up * yScale);
                    Gizmos.DrawLine((position + rotation * prev[j]), (position + rotation * vec));
                    prev[j] = vec;
                }
            }
        }
        else
        {
            var distance = 1;
            var prev = new Vector3[robotSize];
            if (angles.Count / robotSize > 0)
            {
                for (var j = 0; j < robotSize; j++)
                {
                    prev[j] = new Vector3(_startIndex * xDistance, angles[_startIndex * robotSize + j] * yScale, 0);
                }
            }

            for (var i = _startIndex + 1; i < angles.Count / robotSize; i++)
            {
                for (var j = 0; j < robotSize; j++)
                {
                    Gizmos.color = colors[j];
                    var temp = new Vector3(distance++ * xDistance, angles[i * robotSize + j] * yScale, 0);
                    Gizmos.DrawLine((position + rotation * prev[j]), (position + rotation * temp));
                    prev[j] = temp;
                }
            }
            
            for (var i = 0; i < _startIndex; i++)
            {
                for (var j = 0; j < robotSize; j++)
                {
                    Gizmos.color = colors[j];
                    var temp = new Vector3(distance++ * xDistance, angles[i * robotSize + j] * yScale, 0);
                    Gizmos.DrawLine((position + rotation * prev[j]), (position + rotation * temp));
                    prev[j] = temp;
                }
            }
        }
    }
}
