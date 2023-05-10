
using System;
using System.Collections.Generic;
using UnityEngine;

public class KData : MonoBehaviour
{
    public int loopSum;
    public int sampleForEachDrop = 3;
    public KRandDrop randDrop;
    public KRandCam randCam;
    private int _loopIndex = 0;
    private int _selectIndex = 0;
    private List<KGrabable> _grabables;
    private KGrabable _target;
    private float _timeCnt;
    private int _orderIndex = 0;
    private bool _finFlag = true;
    private void Awake()
    {
        _grabables = randDrop.GetDropsAs<KGrabable>();
        Debug.Log($"找到了{_grabables.Count}");
    }

    private void OrderBlock(int match,int nextOrd, float time, Action action)
    {
        if (_orderIndex == match)
        {
            if (_timeCnt > time)
            {
                action.Invoke();
                _timeCnt = 0;
                _orderIndex = nextOrd;
            }
        }
    }
    
    private void OrderBlock(int match,int nextOrd, float time, Func<bool> action, Func<int> fallback)
    {
        if (_orderIndex == match)
        {
            if (_timeCnt > time)
            {
                var flag = action.Invoke();
                _timeCnt = 0;
                _orderIndex = nextOrd;
                if (!flag)
                {
                    _orderIndex = fallback();
                }
            }
        }
    }
    
    private void OrderBlock(int match,int nextOrd, Action action)
    {
        if (_orderIndex == match)
        {
            action.Invoke();
            _timeCnt = 0;
            _orderIndex = nextOrd;
        }
    }
    
    
    private void Update()
    {
        if (_finFlag) return;
        _timeCnt += Time.unscaledDeltaTime;
        OrderBlock(0, 1, () =>
        {
            Time.timeScale = 1f;
            randDrop.EndDrop();
            randDrop.BeginDrop();
        });
    }

    private void LateUpdate()
    {
        if (_finFlag) return;
        OrderBlock(1,2,3f, () =>
        {
            _target = _grabables[_selectIndex++];
            if (_selectIndex == _grabables.Count)
            {
                _selectIndex = 0;
            }
            Time.timeScale = 0;
        });
        var times = sampleForEachDrop;
        for (var i = 0; i < times; i++)
        {
            OrderBlock(2 + i * 2, 3 + i * 2, 0, () =>
            {
                return randCam.RandomTransform(_target.transform);
            }, () =>
            {
                if (_selectIndex == 0)
                {
                    _selectIndex = _grabables.Count - 1;
                }
                else
                {
                    _selectIndex--;
                }
                return 0;
            });
            OrderBlock(3 + i * 2, 4 + i * 2, 0.2f, () =>
            {
                randCam.Detail.Append(_target.GetDetail(randCam.cam.transform));
                var sp = randCam.cam.WorldToScreenPoint(_target.GrabPosition);
                randCam.Detail.Append(',');
                randCam.Detail.Append($"{sp.x},{sp.y}");
                randCam.Capture();
            });
            if (i == times - 1)
            {
                OrderBlock(4 + i * 2, 0, () =>
                {
                    _loopIndex++;
                    if (_loopIndex >= loopSum)
                    {
                        _finFlag = true;
                        _loopIndex = 0;
                        Debug.Log("结束");
                    }
                });
            }
        }
    }

    public void Begin()
    {
        if (!_finFlag) return;
        _finFlag = false;
        Debug.Log("开始");
    }
/*
    private void OnDrawGizmos()
    {
        var camTran = randCam.cam.transform;
        var camRot = camTran.rotation;
        var camPos = camTran.position;
        var x = camRot * Vector3.right;
        var y = camRot * Vector3.up;
        var z = camRot * Vector3.forward;
        var dir = test.transform.position - camPos;
        var xLen = Vector3.Dot(x, dir);
        var yLen = Vector3.Dot(y, dir);
        var zLen = Vector3.Dot(z, dir);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(camPos, camPos + x * xLen);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(camPos, camPos + y * yLen);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(camPos, camPos + z * zLen);
    }
    */
}
