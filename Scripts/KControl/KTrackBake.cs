
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class KTrackBake : KTrackList
{
    public KTrack loadTarget;
    public KRobot displayTarget;
    public List<float> angles = new();
    public List<int> lengths = new();
    public List<float> timeIntervals = new();
    

    public void Clear()
    {
        angles.Clear();
        lengths.Clear();
        timeIntervals.Clear();
    }

    public void Add(List<float> angle,float timeInterval)
    {
        angles.AddRange(angle);
        lengths.Add(angle.Count);
        timeIntervals.Add(timeInterval);
    }

    public void Add(KTrack track)
    {
        Add(track.angles, track.timeInterval);
    }

    public override void TrackUpdate(KRobot robot)
    {
        if (!finish) return;
        finish = false;
        var lengthSum = lengths[0];
        var angleIndex = 0;
        var lengthIndex = 0;

        if (Application.isPlaying)
        {
            StartCoroutine(KTimer.TimeLoop(() =>
            {
                robot.Move(angles, angleIndex);
                angleIndex++;
                if (angleIndex * robot.RobotSize >= angles.Count)
                {
                    finish = true;
                    return -1;
                }

                while (angleIndex * robot.RobotSize >= lengthSum)
                {
                    lengthIndex++;
                    lengthSum += lengths[lengthIndex];
                }

                return timeIntervals[lengthIndex];
            }));
        }else if (Application.isEditor)
        {
            EditorCoroutineRunner.StartEditorCoroutine(KTimer.TimeLoop(() =>
            {
                robot.Move(angles, angleIndex);
                angleIndex++;
                if (angleIndex * robot.RobotSize >= angles.Count)
                {
                    finish = true;
                    return -1;
                }

                while (angleIndex * robot.RobotSize >= lengthSum)
                {
                    lengthIndex++;
                    lengthSum += lengths[lengthIndex];
                }

                return timeIntervals[lengthIndex];
            }));
        }
    }
}

[CustomEditor(typeof(KTrackBake))]
public class KTrackBakeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var trackBake = target as KTrackBake;
        if (trackBake == null) return;
        base.OnInspectorGUI();
        if (GUILayout.Button("加载轨迹")) {
            if (trackBake.loadTarget) {
                trackBake.Add(trackBake.loadTarget);
            }
        }if (GUILayout.Button("清除")) {
            trackBake.Clear();
        }if (GUILayout.Button("展示")) {
            if (trackBake.displayTarget) {
                trackBake.TrackUpdate(trackBake.displayTarget);
            }
        }
    }
}
