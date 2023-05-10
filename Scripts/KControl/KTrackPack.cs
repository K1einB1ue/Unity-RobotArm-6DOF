
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class KTrackPack : KTrackList
{
    public KRobot displayTarget;
    public List<KTrack> tracks = new();

    public override void TrackUpdate(KRobot robot)
    {
        if (!finish) return;
        finish = false;
        var angleIndex = 0;
        var lengthIndex = 0;
        var lengthSum = tracks[0].angles.Count;

        var sameTime = true;
        var time = tracks[0].timeInterval;
        for (var i = 1; i < tracks.Count; i++) {
            if (Math.Abs(tracks[i].timeInterval - time) > 1E-8) {
                sameTime = false;
                break;
            }
        }

        if (sameTime)
        {
            if (Application.isPlaying) {
                StartCoroutine(KTimer.TimeLoop(() => {
                    robot.Move(tracks[lengthIndex].angles, angleIndex);
                    angleIndex++;

                    while (angleIndex * robot.RobotSize >= lengthSum)
                    {
                        lengthIndex++;
                        if (lengthIndex >= tracks.Count)
                        {
                            finish = true;
                            return true;
                        }

                        lengthSum = tracks[lengthIndex].angles.Count;
                        angleIndex = 0;
                    }

                    return false;
                }, time));
            }else if (Application.isEditor) {
                EditorCoroutineRunner.StartEditorCoroutine(KTimer.TimeLoop(() => {
                    robot.Move(tracks[lengthIndex].angles, angleIndex);
                    angleIndex++;

                    while (angleIndex * robot.RobotSize >= lengthSum)
                    {
                        lengthIndex++;
                        if (lengthIndex >= tracks.Count)
                        {
                            finish = true;
                            return true;
                        }

                        lengthSum = tracks[lengthIndex].angles.Count;
                        angleIndex = 0;
                    }

                    return false;
                }, time));
            }
        }
        else
        {
            if (Application.isPlaying) {
                StartCoroutine(KTimer.TimeLoop(() =>
                {
                    robot.Move(tracks[lengthIndex].angles, angleIndex);
                    angleIndex++;

                    while (angleIndex * robot.RobotSize >= lengthSum)
                    {
                        lengthIndex++;
                        if (lengthIndex >= tracks.Count)
                        {
                            finish = true;
                            return -1;
                        }
                        lengthSum = tracks[lengthIndex].angles.Count;
                        angleIndex = 0;
                    }

                    return tracks[lengthIndex].timeInterval;
                }));
            }else if (Application.isEditor) {
                EditorCoroutineRunner.StartEditorCoroutine(KTimer.TimeLoop(() =>
                {
                    robot.Move(tracks[lengthIndex].angles, angleIndex);
                    angleIndex++;

                    while (angleIndex * robot.RobotSize >= lengthSum)
                    {
                        lengthIndex++;
                        if (lengthIndex >= tracks.Count)
                        {
                            finish = true;
                            return -1;
                        }
                        lengthSum = tracks[lengthIndex].angles.Count;
                        angleIndex = 0;
                    }

                    return tracks[lengthIndex].timeInterval;
                }));
            }
        }
    }

}

[CustomEditor(typeof(KTrackPack))]
public class KTrackPackEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var trackBake = target as KTrackPack;
        if (trackBake == null) return;
        base.OnInspectorGUI();
        if (GUILayout.Button("展示")) {
            if (trackBake.displayTarget) {
                trackBake.TrackUpdate(trackBake.displayTarget);
            }
        }
    }
}