
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XNode;

[CreateAssetMenu(menuName = "KGraph/graph",fileName = "KGraph",order = 0)]
public class KGraph : NodeGraph
{
    public static bool Dynamic = true;
    

    public IEnumerable<T> GetNodes<T>()
    {
        foreach (var node in nodes)
        {
            if (node is T n)
            {
                yield return n;
            }
        }
    }

    private List<KField> _fields = null;
    public List<KField> Fields
    {
        get
        {
            if (Dynamic) return new List<KField>(GetNodes<KField>());
            if (_fields != null) return _fields;
            _fields = new List<KField>();
            _fields.AddRange(GetNodes<KField>());
            return _fields;
        }
    }

    private List<KError> _errors = null;
    public List<KError> Errors
    {
        get
        {
            if (Dynamic) return new List<KError>(GetNodes<KError>());
            if (_errors != null) return _errors;
            _errors = new List<KError>();
            _errors.AddRange(GetNodes<KError>());
            return _errors;
        }
    }

    private List<KEnd> _ends = null;
    public List<KEnd> Ends
    {
        get
        {
            if (Dynamic) return new List<KEnd>(GetNodes<KEnd>());
            if (_ends != null) return _ends;
            _ends = new List<KEnd>();
            _ends.AddRange(GetNodes<KEnd>());
            return _ends;
        }
    }
    
    public enum CheckType
    {
        Max,
        Min,
        Sum,
    }

    public bool CheckError()
    {
        foreach (var error in Errors)
        {
            if (error.Error != 0)
            {
                Debug.LogError($"{error.name}:发生报错!");
                return true;
            }
        }
        return false;
    }
    
    public int CheckEnd(CheckType checkType = CheckType.Max)
    {
        var ret = 0;
        switch (checkType)
        {
            case CheckType.Max:
            {
                foreach (var end in Ends)
                {
                    var value = end.End;
                    if (value > ret)
                    {
                        ret = value;
                    }
                }
                break;
            }
            case CheckType.Min:
            {
                foreach (var end in Ends)
                {
                    var value = end.End;
                    if (value < ret)
                    {
                        ret = value;
                    }
                }
                break;
            }
            case CheckType.Sum:
            {
                foreach (var end in Ends)
                {
                    ret += end.End;
                }
                break;
            }
        }
        return ret;
    }

    public void Refresh()
    {
        _errors = null;
        _fields = null;
        _ends = null;
    }
}
