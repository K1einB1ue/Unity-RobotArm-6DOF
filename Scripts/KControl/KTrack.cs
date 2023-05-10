
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public enum KLinkType
{
    Null = 1,
    Line = 2,
    Bezier = 3,
    Bezierr = 4,
    Bezierrr = 5,
}
[ExecuteInEditMode]
public class KTrack : MonoBehaviour
{
    public bool display;
    public int divNum = 100;
    public Mesh displayMesh;
    public Material displayMaterial;
    public Vector3 displayScale = Vector3.one;
    public GameObject effortTarget;
    public KTrackPoint startPoint;
    public List<KTrackPoint> points = new();
    public List<KLinkType> types = new();
    private List<KLink> _links = new();
    public List<KLinkDescription> descriptions = new();
    public KRobot robot;
    public float timeInterval = 0.05f;
    public List<float> angles = new();


    private void OnDrawGizmos()
    {
        if(!display) return;
        if(!DataCheck) return;
        InitLinks(_links);
        
        
        if (robot)
        {
            if (Selection.Contains(gameObject))
            {
                goto Select;
            }
            foreach (var t in Selection.transforms)
            {
                if (ReferenceEquals(t.parent, transform))
                {
                    goto Select;
                }
            }
        }
        {
            Gizmos.color = Color.white;
            var pos0 = startPoint.transform.position;
            for (var i = 0; i < _links.Count; i++)
            {
                for (var j = 1; j <= divNum; j++)
                {
                    var (pos1, _) = _links[i].Lerp((float) j / divNum);
                    Gizmos.DrawLine(pos0, pos1);
                    pos0 = pos1;
                }
            }
            return;
        }
        Select:
        {
            var pos0 = startPoint.transform.position;
            for (var i = 0; i < _links.Count; i++)
            {
                for (var j = 1; j <= divNum; j++)
                {
                    var (pos1, rot) = _links[i].Lerp((float) j / divNum);
                    var err = robot.IK(null, pos1, rot);
                    Gizmos.color = err switch
                    {
                        KRobot.IKError.None => Color.green,
                        KRobot.IKError.Mixture => Color.black,
                        KRobot.IKError.AnalyticalError => Color.magenta,
                        KRobot.IKError.AngleError => Color.red,
                        _ => Gizmos.color
                    };
                    Gizmos.DrawLine(pos0, pos1);
                    pos0 = pos1;
                }
            }
        }
    }

    public void Remove(KTrackPoint point)
    {
        if (ReferenceEquals(point, startPoint))
        {
            Debug.LogWarning("不要删除起始点(程序将会重新生成一个起始点)");
            TryAddStart();
            return;
        }
        var (pointIndex, typeIndex) = Find(point);

        if (pointIndex >= 0)
        {
            points.RemoveAt(pointIndex);
            var type = (KLinkType) ((int) types[typeIndex] - 1);
            if (type == KLinkType.Null)
            {
                types.RemoveAt(typeIndex);
                descriptions.RemoveAt(typeIndex);
            }
            else
            {
                types[typeIndex] = type;
            }
            RefreshName();
        }
        else
        {
            Debug.LogError("未找到对应点");
        }
    }

    private void RefreshName()
    {
        TryAddStart();
        startPoint.name = types.Count == 0 ? "Start" : $"Start:{types[0].ToString()}";
        Process((int index,KTrackPoint point,KLinkType type) =>
        {
            point.name = $"Point({index}):{type.ToString()}";
        });
    }

    private (int pointIndex, int typeIndex) Find(KTrackPoint point)
    {
        TryAddStart();
        if (ReferenceEquals(point, startPoint))
        {
            return (-1, -1);
        }
        var beginNum = 0;
        for (var i = 0; i < types.Count; i++)
        {
            var type = types[i];
            var size = (int)type - 1;
            for (var j = 0; j < size; j++)
            {
                if (ReferenceEquals(point, points[beginNum + j]))
                {
                    return (beginNum + j, i);
                }
            }
            beginNum += size;
        }

        return (-1, -1);
    }

    private void Process(Action<int,KTrackPoint,KLinkType> action)
    {
        var beginNum = 0;
        for (var i = 0; i < types.Count; i++)
        {
            var type = types[i];
            var size = (int)type - 1;
            for (var j = 0; j < size; j++)
            {
                action.Invoke(beginNum + j, points[beginNum + j], type);
            }
            beginNum += size;
        }
    }

    public void UpdateDisplay()
    {
        {
            startPoint.transform.localScale = displayScale;
            if (!startPoint.TryGetComponent<MeshFilter>(out var mf))
            {
                mf = startPoint.AddComponent<MeshFilter>();
            }

            mf.sharedMesh = displayMesh;

            if (!startPoint.TryGetComponent<MeshRenderer>(out var mr))
            {
                mr = startPoint.AddComponent<MeshRenderer>();
            }

            mr.material = displayMaterial;
        }
        foreach (var point in points)
        {
            point.transform.localScale = displayScale;
            if (!point.TryGetComponent<MeshFilter>(out var mf))
            {
                mf = point.AddComponent<MeshFilter>();
            }
            mf.sharedMesh = displayMesh;
            
            if (!point.TryGetComponent<MeshRenderer>(out var mr))
            {
                mr = point.AddComponent<MeshRenderer>();
            }
            mr.material = displayMaterial;
        }
    }

    public void SetDisplay(bool display)
    {
        {
            if (!startPoint.TryGetComponent<MeshRenderer>(out var mr))
            {
                mr = startPoint.AddComponent<MeshRenderer>();
            }
            mr.enabled = display;
        }
        foreach (var point in points)
        {
            if (!point.TryGetComponent<MeshRenderer>(out var mr))
            {
                mr = point.AddComponent<MeshRenderer>();
            }

            mr.enabled = display;
        }
    }

    public void BakeTrack(List<(Vector3, Quaternion)> trackStore,float interval)
    {
        if (!DataCheck) return;
        List<KLink> links = new();
        InitLinks(links);
        var lengths = InitLengths(links);
        var divSum = 0;
        for (var i = 0; i < descriptions.Count; i++)
        {
            var des = descriptions[i];
            switch (des.constraint)
            {
                case KLinkConstraint.Time:
                    divSum = Mathf.CeilToInt(des.time / interval);
                    break;
                case KLinkConstraint.Speed:
                    divSum = Mathf.CeilToInt(lengths[i] / des.speed / interval);
                    break;
            }

            if (i == 0)
            {
                trackStore.Add(links[0].Lerp(0));
            }
            for (var j = 1; j <= divSum; j++)
            {
                trackStore.Add(links[i].Lerp((float) j / divSum));
            }
        }
    }
    
    
    public void RunBakedTrack(float interval)
    {
        var index = 0;
        if (Application.isEditor) {
            EditorCoroutineRunner.StartEditorCoroutine(KTimer.TimeLoop(() =>
            {
                if (index >= angles.Count / robot.RobotSize) return true;
                robot.Move(angles, index);
                index++;
                return false;
            }, interval));
        } else if (Application.isPlaying) {
            StartCoroutine(KTimer.TimeLoop(() =>
            {
                if (index >= angles.Count / robot.RobotSize) return true;
                robot.Move(angles, index);
                index++;
                return false;
            }, interval));
        }
    }

    public void RunTrack(float interval)
    {
        if (!DataCheck) return;
        List<KLink> links = new();
        List<KLinkDescription> linkDescriptions = new(descriptions);
        InitLinks(links);
        var lengths = InitLengths(links);
        
        var linkCnt = 0;
        var divCnt = 0;
        var divSum = 0;
        
        var des = linkDescriptions[0];
        switch (des.constraint)
        {
            case KLinkConstraint.Time:
                divSum = Mathf.CeilToInt(des.time / interval);
                break;
            case KLinkConstraint.Speed:
                divSum = Mathf.CeilToInt(lengths[linkCnt] / des.speed / interval);
                break;
        }

        var tran = effortTarget.transform;
        if (Application.isEditor)
        {
            EditorCoroutineRunner.StartEditorCoroutine(KTimer.TimeLoop(() =>
            {
                var (pos, rot) = links[linkCnt].Lerp((float) divCnt / divSum);
                tran.position = pos;
                tran.rotation = rot;
                divCnt++;
                if (divCnt == divSum + 1)
                {
                    divCnt = 1;
                    linkCnt++;
                    if (linkCnt == links.Count) return true;
                    var des = linkDescriptions[linkCnt];
                    switch (des.constraint)
                    {
                        case KLinkConstraint.Time:
                            divSum = Mathf.CeilToInt(des.time / interval);
                            break;
                        case KLinkConstraint.Speed:
                            divSum = Mathf.CeilToInt(lengths[linkCnt] / des.speed / interval);
                            break;
                    }
                }
                return false;
            }, interval));
        } else if (Application.isPlaying) {
            StartCoroutine(KTimer.TimeLoop(() =>
            {
                var (pos, rot) = links[linkCnt].Lerp((float) divCnt / divSum);
                tran.position = pos;
                tran.rotation = rot;
                divCnt++;
                if (divCnt == divSum + 1)
                {
                    divCnt = 1;
                    linkCnt++;
                    if (linkCnt == links.Count) return true;
                    var des = linkDescriptions[linkCnt];
                    switch (des.constraint)
                    {
                        case KLinkConstraint.Time:
                            divSum = Mathf.CeilToInt(des.time / interval);
                            break;
                        case KLinkConstraint.Speed:
                            divSum = Mathf.CeilToInt(lengths[linkCnt] / des.speed / interval);
                            break;
                    }
                }
                return false;
            }, interval));
        }
    }

    public void Clear()
    {
        KTrackPoint.DestroyCallEnable = false;
        foreach (var point in points)
        {
            DestroyImmediate(point);
        }

        KTrackPoint.DestroyCallEnable = true;
        points.Clear();
        types.Clear();
        descriptions.Clear();
    }

    public void Add(KLinkType type)
    {
        TryAddStart();
        AddPoint((int)type - 1);
        types.Add(type);
        descriptions.Add(new KLinkDescription());
        RefreshName();
        UpdateDisplay();
    }

    private void InitLinks(List<KLink> links)
    {
        links.Clear();
        var indexNum = 0;
        for (var i = 0; i < types.Count; i++)
        {
            if (i == 0)
            {
                switch (types[i])
                {
                    case KLinkType.Line:
                        var line = new KLine
                        {
                            t0 = startPoint.transform,
                            t1 = points[indexNum].transform
                        };
                        links.Add(line);
                        break;
                    case KLinkType.Bezier:
                        var bezier = new KBezier
                        {
                            t0 = startPoint.transform,
                            t1 = points[indexNum++].transform,
                            t2 = points[indexNum].transform
                        };
                        links.Add(bezier);
                        break;
                    case KLinkType.Bezierr:
                        var bezierr = new KBezierr
                        {
                            t0 = startPoint.transform,
                            t1 = points[indexNum++].transform,
                            t2 = points[indexNum++].transform,
                            t3 = points[indexNum].transform
                        };
                        links.Add(bezierr);
                        break;
                    case KLinkType.Bezierrr:
                        var bezierrr = new KBezierrr
                        {
                            t0 = startPoint.transform,
                            t1 = points[indexNum++].transform,
                            t2 = points[indexNum++].transform,
                            t3 = points[indexNum++].transform,
                            t4 = points[indexNum].transform
                        };
                        links.Add(bezierrr);
                        break;
                }
            }
            else
            {
                switch (types[i])
                {
                    case KLinkType.Line:
                        var line = new KLine
                        {
                            t0 = points[indexNum++].transform,
                            t1 = points[indexNum].transform
                        };
                        links.Add(line);
                        break;
                    case KLinkType.Bezier:
                        var bezier = new KBezier
                        {
                            t0 = points[indexNum++].transform,
                            t1 = points[indexNum++].transform,
                            t2 = points[indexNum].transform
                        };
                        links.Add(bezier);
                        break;
                    case KLinkType.Bezierr:
                        var bezierr = new KBezierr
                        {
                            t0 = points[indexNum++].transform,
                            t1 = points[indexNum++].transform,
                            t2 = points[indexNum++].transform
                        };
                        bezierr.t2 = points[indexNum].transform;
                        links.Add(bezierr);
                        break;
                    case KLinkType.Bezierrr:
                        var bezierrr = new KBezierrr
                        {
                            t0 = points[indexNum++].transform,
                            t1 = points[indexNum++].transform,
                            t2 = points[indexNum++].transform,
                            t3 = points[indexNum++].transform,
                            t4 = points[indexNum].transform
                        };
                        links.Add(bezierrr);
                        break;
                }
            }
        }
    }

    private List<float> InitLengths(List<KLink> links)
    {
        List<float> ret = new();
        for (var i = 0; i < links.Count; i++)
        {
            var length = links[i].Length(divNum);
            ret.Add(length);
        }
        return ret;
    }

    private bool DataCheck => points.Count != 0 && startPoint;

    private void TryAddStart()
    {
        if (startPoint) return;
        var go = new GameObject();
        go.transform.parent = transform;
        go.name = $"Start";
        var kTrackPoint = go.AddComponent<KTrackPoint>();
        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();
        kTrackPoint.root = this;
        startPoint = kTrackPoint;
    }
    private void AddPoint(int num)
    {
        for (var i = 0; i < num; i++)
        {
            var go = new GameObject();
            go.transform.parent = transform;
            go.name = $"Point({points.Count})";
            var kTrackPoint = go.AddComponent<KTrackPoint>();
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            kTrackPoint.root = this;
            points.Add(kTrackPoint);
        }
    }
}



[CustomEditor(typeof(KTrack))]
public class KTrackEditor : Editor
{
    public KTrack KTrack => target as KTrack;
    private List<(Vector3, Quaternion)> _track = new();

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("添加直线"))
        {
            KTrack.Add(KLinkType.Line);
        }
        if (GUILayout.Button("添加贝塞尔曲线"))
        {
            KTrack.Add(KLinkType.Bezier);
        }
        if (GUILayout.Button("添加贝塞尔尔曲线"))
        {
            KTrack.Add(KLinkType.Bezierr);
        }
        if (GUILayout.Button("添加贝塞尔尔尔曲线"))
        {
            KTrack.Add(KLinkType.Bezierrr);
        }
        
        if (GUILayout.Button("清空"))
        {
            KTrack.Clear();
        }
        if (GUILayout.Button("更新显示"))
        {
            KTrack.UpdateDisplay();
        }

        if (GUILayout.Button("停止显示"))
        {
            KTrack.SetDisplay(false);
        }
        if (GUILayout.Button("继续显示"))
        {
            KTrack.SetDisplay(true);
        }
        if (GUILayout.Button("展示路径效果"))
        {
            KTrack.SetDisplay(false);
            KTrack.RunTrack(KTrack.timeInterval);
        }
        
        if (KTrack.robot)
        {
            if (GUILayout.Button("烘焙运动轨迹"))
            {
                _track.Clear();
                KTrack.BakeTrack(_track,KTrack.timeInterval);
                var eT = KTrack.effortTarget.transform;
                var index = 0;
                if (Application.isEditor)
                {
                    EditorCoroutineRunner.StartEditorCoroutine(KTimer.TimeLoop(() =>
                    {
                        if (index >= _track.Count) return true;
                        eT.position = _track[index].Item1;
                        eT.rotation = _track[index].Item2;
                        index++;
                        return false;
                    }, KTrack.timeInterval));
                } else if (Application.isPlaying) {
                    KTrack.StartCoroutine(KTimer.TimeLoop(() =>
                    {
                        if (index >= _track.Count) return true;
                        eT.position = _track[index].Item1;
                        eT.rotation = _track[index].Item2;
                        index++;
                        return false;
                    }, KTrack.timeInterval));
                }
                KTrack.angles.Clear();
                for (var i = 0; i < _track.Count; i++)
                {
                    KTrack.robot.IK(KTrack.angles, _track[i].Item1, _track[i].Item2);
                }
            }

            if (GUILayout.Button("播放烘焙运动轨迹"))
            {
                KTrack.RunBakedTrack(KTrack.timeInterval);
            }
            
        }
    }
}

[Serializable]
public abstract class KLink
{
    public abstract (Vector3 pos,Quaternion rot) Lerp(float time);

    public abstract float Length(int sampleNum);

    public float BezierLength(List<Transform> trans,int sampleNum)
    {
        //由于高阶贝塞尔曲线没有解析解,所以选择梯度法.
        float length = 0;
        var (begPoint, _) = BezierRecursion(trans, 0);
        for (var i = 1; i <= sampleNum; i++)
        {
            var (point,_) = BezierRecursion(trans, (float) i / sampleNum);
            length += Vector3.Distance(begPoint, point);
            begPoint = point;
        }
        return length;
    }

    private List<Vector3> _pos = new();
    private List<Vector3> _newPos = new();
    private List<Quaternion> _rot = new();
    private List<Quaternion> _newRot = new();
    public (Vector3, Quaternion) BezierRecursion(List<Transform> trans,float time)
    {
        var newFlag = false;
        _pos.Clear();
        _newPos.Clear();
        _rot.Clear();
        _newRot.Clear();
        var num = trans.Count - 1;
        for (var i = 0; i < trans.Count; i++) {
            _pos.Add(trans[i].position);
            _rot.Add(trans[i].rotation);
        }

        while (num != 0) {
            if (!newFlag) {
                for (var i = 0; i < _pos.Count - 1; i++) {
                    _newPos.Add(Vector3.Lerp(_pos[i], _pos[i + 1], time));
                    _newRot.Add(Quaternion.SlerpUnclamped(_rot[i], _rot[i + 1], time));
                }
                _pos.Clear();
                _rot.Clear();
            }else {
                for (var i = 0; i < _newPos.Count - 1; i++) {
                    _pos.Add(Vector3.Lerp(_newPos[i], _newPos[i + 1], time));
                    _rot.Add(Quaternion.SlerpUnclamped(_newRot[i], _newRot[i + 1], time));
                }
                _newPos.Clear();
                _newRot.Clear();
            }
            newFlag = !newFlag;
            num--;
        }
        return newFlag ? (_newPos[0], _newRot[0]) : (_pos[0], _rot[0]);
    }
}


[Serializable]
public class KBezier : KLink
{
    public Transform t0, t1, t2;
    public override (Vector3 pos,Quaternion rot) Lerp(float time)
    {
        return BezierRecursion(new List<Transform>() {t0, t1, t2}, time);
    }

    public override float Length(int sampleNum)
    {
        return BezierLength(new List<Transform>() {t0, t1, t2}, sampleNum);
    }
}

[Serializable]
public class KBezierr : KLink
{
    public Transform t0, t1, t2, t3;
    public override (Vector3 pos,Quaternion rot) Lerp(float time)
    {
        return BezierRecursion(new List<Transform>() {t0, t1, t2, t3}, time);
    }

    public override float Length(int sampleNum)
    {
        return BezierLength(new List<Transform>() {t0, t1, t2, t3}, sampleNum);
    }
}

[Serializable]
public class KBezierrr : KLink
{
    public Transform t0, t1, t2, t3, t4;
    public override (Vector3 pos,Quaternion rot) Lerp(float time)
    {
        return BezierRecursion(new List<Transform>() {t0, t1, t2, t3, t4}, time);
    }

    public override float Length(int sampleNum)
    {
        return BezierLength(new List<Transform>() {t0, t1, t2, t3, t4}, sampleNum);
    }
}

[Serializable]
public class KLine : KLink
{
    public Transform t0, t1;
    public override (Vector3 pos,Quaternion rot) Lerp(float time)
    {
        var p0 = t0.position;
        var p1 = t1.position;
        var q0 = t0.rotation;
        var q1 = t1.rotation;
        var p = Vector3.Lerp(p0, p1, time);
        var q = Quaternion.SlerpUnclamped(q0, q1, time);
        return (p, q);
    }

    public override float Length(int sampleNum)
    {
        return Vector3.Distance(t0.position, t1.position);
    }
}

public enum KLinkConstraint
{
    Speed,
    Time,
}

[Serializable]
public class KLinkDescription
{
    public KLinkConstraint constraint;
    public float speed;
    public float time;

    public KLinkDescription()
    {
        constraint = KLinkConstraint.Time;
        time = 2f;
    }
}