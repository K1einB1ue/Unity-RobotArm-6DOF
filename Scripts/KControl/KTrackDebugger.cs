
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class KTrackDebugger : MonoBehaviour
{
    public int slider;
    public KTrack track;
    public float xDistance;
    public float yScale;
    public bool complex;
    public List<Color> colors = new() {Color.red, Color.green, Color.cyan, Color.blue, Color.magenta, Color.yellow};
    internal int bufferRobotSize = 0;
    internal readonly List<float> bufferAngles = new();

    private void OnDrawGizmosSelected()
    {
        if (track)
        {
            var robot = track.robot;
            if (robot) {
                name = $"{robot.name}'s Debugger";
            }
        }
    }

    private void OnDrawGizmos()
    {
        var tran = transform;
        var position = tran.position;
        var rotation = tran.rotation;
        var robotSize = 0;
        List<float> angles = null;
        if (bufferRobotSize != 0)
        {
            robotSize = bufferRobotSize;
            angles = bufferAngles;
        }
        else
        {
            if (!track) return;
            var robot = track.robot;
            if (!robot) return;
            robotSize = robot.RobotSize;
            angles = track.angles;
        }

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
            
            
            var prev = new Vector3[robotSize];
            if (angles.Count / robotSize > 0)
            {
                for (var j = 0; j < robotSize; j++)
                {
                    rot = Quaternion.AngleAxis(angles[j], Vector3.right);
                    prev[j] = rot * (Vector3.up * yScale);
                }
            }

            for (var i = 1; i < angles.Count / robotSize; i++)
            {
                for (var j = 0; j < robotSize; j++)
                {
                    Gizmos.color = colors[j];
                    rot = Quaternion.AngleAxis(angles[i * robotSize + j], Vector3.right);
                    var vec = Vector3.zero;
                    vec.x = i * xDistance;
                    vec += rot * (Vector3.up * yScale);
                    Gizmos.DrawLine((position + rotation * prev[j]), (position + rotation * vec));
                    prev[j] = vec;
                }
            }

            if (slider == 0) return;
            begin = Quaternion.AngleAxis(0, Vector3.right) * (Vector3.up * yScale);
            pre = begin;
            var vecSlider = Vector3.zero;
            vecSlider.x = slider * xDistance;
            Gizmos.color = Color.white;
            for (var i = 1; i < 360; i++) {
                var newP = Quaternion.AngleAxis(i, Vector3.right) * (Vector3.up * yScale);
                Gizmos.DrawLine((position + rotation * (pre + vecSlider)),
                    (position + rotation * (newP + vecSlider)));
                pre = newP;
            }
            Gizmos.DrawLine(position + rotation * (pre + vecSlider), position + rotation * (begin + vecSlider));
        }
        else
        {
            var prev = new Vector3[robotSize];
            if (angles.Count / robotSize > 0)
            {
                for (var j = 0; j < robotSize; j++)
                {
                    prev[j] = new Vector3(0, angles[j] * yScale, 0);
                }
            }

            for (var i = 1; i < angles.Count / robotSize; i++)
            {
                for (var j = 0; j < robotSize; j++)
                {
                    Gizmos.color = colors[j];
                    var temp = new Vector3(i * xDistance, angles[i * robotSize + j] * yScale, 0);
                    Gizmos.DrawLine((position + rotation * prev[j]), (position + rotation * temp));
                    prev[j] = temp;
                }
            }
        }
    }
}

[CustomEditor(typeof(KTrackDebugger))]
public class KRobotDebugEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var debug = target as KTrackDebugger;
        base.OnInspectorGUI();
        if (debug == null) return;
        if (GUILayout.Button("缓存"))
        {
            var track = debug.track;
            if (!track) return;
            var robot = track.robot;
            if (!robot) return;
            debug.bufferAngles.Clear();
            debug.bufferAngles.AddRange(track.angles);
            debug.bufferRobotSize = robot.RobotSize;
        }

        if (GUILayout.Button("清空缓存"))
        {
            debug.bufferAngles.Clear();
            debug.bufferRobotSize = 0;
        }
    }
}
