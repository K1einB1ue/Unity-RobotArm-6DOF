
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

[Serializable]
public class ItemPoolConfig
{
    public GameObject prefab;
    public int size;
}

[Serializable]
public class InstanceState
{
    public InstanceState(GameObject instance)
    {
        this.instance = instance;
    }
    public GameObject instance;
    public float rand;
}
public class KRandDrop : MonoBehaviour
{
    public float dropInterval;
    public List<ItemPoolConfig> configs = new();
    public event Action OnDropEnd;
    public event Action OnDropBeg;
    
    private List<GameObject> _instance = new();
    private readonly List<InstanceState> _instanceIndex = new();
    private Vector3 _p0, _p1, _p2, _p3;
    private bool _ended = true;

    private bool _dropLock = false;
    private void OnDrawGizmos()
    {
        var tran = transform;
        var pos = tran.position;
        var rot = tran.rotation;
        var shape = tran.lossyScale;
        var width = shape.x / 2f;
        var height = shape.z / 2f;
        var off0 = new Vector3(width, 0f, height);
        var off1 = new Vector3(-width, 0f, height);
        var p0Dir = rot * off0;
        var p1Dir = rot * -off1;
        var p2Dir = rot * -off0;
        var p3Dir = rot * off1;
        var p0 = pos + p0Dir;
        var p1 = pos + p1Dir;
        var p2 = pos + p2Dir;
        var p3 = pos + p3Dir;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(p0, p1);
        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p3, p0);
    }

    private void Awake()
    {
        foreach (var iI in _instanceIndex)
        {
            Destroy(iI.instance);
        }
        _instanceIndex.Clear();
        foreach (var config in configs)
        {
            for (var i = 0; i < config.size; i++)
            {
                var instance = Instantiate(config.prefab, Vector3.zero, Quaternion.identity);
                instance.SetActive(false);
                _instanceIndex.Add(new InstanceState(instance));
            }
        }
        var tran = transform;
        var pos = tran.position;
        var rot = tran.rotation;
        var shape = tran.lossyScale;
        var width = shape.x / 2f;
        var height = shape.z / 2f;
        var off0 = new Vector3(width, 0f, height);
        var off1 = new Vector3(-width, 0f, height);
        var p0Dir = rot * off0;
        var p1Dir = rot * -off1;
        var p2Dir = rot * -off0;
        var p3Dir = rot * off1;
        _p0 = pos + p0Dir;
        _p1 = pos + p1Dir;
        _p2 = pos + p2Dir;
        _p3 = pos + p3Dir;
        _ended = false;
    }

    public List<T> GetDropsAs<T>()
    {
        List<T> ret = new();
        foreach (var iI in _instanceIndex)
        {
            var com = iI.instance.GetComponent<T>();
            if(com ==null) Debug.LogWarning($"{iI.instance.name}没有:{typeof(T).FullName}");
            ret.Add(com);
        }

        return ret;
    }

    
    

    public void BeginDrop()
    {
        if (_instanceIndex.Count == 0) return;
        if (_dropLock) return;
        if (!_ended)
        {
            _ended = true;
            OnDropEnd?.Invoke();
            foreach (var iI in _instanceIndex)
            {
                iI.instance.SetActive(false);
            }
        }
        foreach (var iS in _instanceIndex)
        {
            iS.rand = Random.Range(0f, 1f);
        }
        _instanceIndex.Sort((x, y) => x.rand.CompareTo(y.rand));
        var index = 0;
        var sum = _instanceIndex.Count;
        _dropLock = true;
        OnDropBeg?.Invoke();
        StartCoroutine(KTimer.TimeLoop(() =>
        {
            var go = _instanceIndex[index++].instance;
            var tran = go.transform;
            var xRand = Random.Range(0f, 1f);
            var yRand = Random.Range(0f, 1f);
            var tmpX0 = Vector3.Lerp(_p0, _p1, xRand);
            var tmpX1 = Vector3.Lerp(_p3, _p2, xRand);
            tran.position = Vector3.Lerp(tmpX0, tmpX1, yRand);
            tran.rotation = Random.rotation;
            go.SetActive(true);
            if (index >= sum)
            {
                _ended = false;
                _dropLock = false;
                return true;
            }
            return false;
        }, dropInterval));
    }
    

    public void EndDrop()
    {
        if (_instanceIndex.Count == 0) return;
        if (_dropLock) return;
        if (_ended) return;
        OnDropEnd?.Invoke();
        foreach (var iI in _instanceIndex)
        {
            iI.instance.SetActive(false);
        }

        _ended = true;
    }
    
}
