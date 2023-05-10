
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using XNode;

[Serializable]
public class FieldBindingPack
{
    public string fieldName;
    public UnityEvent<object> callback;
}



[ExecuteAlways]
public abstract class KGraphMono : MonoBehaviour
{
    
    public List<FieldBindingPack> packs;
    
    public KGraph kGraph;
    private Dictionary<string, UnityEvent<object>> _callbackMap = null;
    public Dictionary<string, UnityEvent<object>> CallBack
    {
        get
        {
            if (_callbackMap != null) return _callbackMap;
            _callbackMap = new Dictionary<string, UnityEvent<object>>();
            foreach (var pack in packs)
            {
                _callbackMap.Add(pack.fieldName, pack.callback);
            }
            return _callbackMap;
        }
    }
    
    public void UpdatePacks()
    {
        var hash = new HashSet<string>();
        var changed = false;
        var repeatList = new List<FieldBindingPack>();
        foreach (var pack in packs)
        {
            if (!hash.Add(pack.fieldName))
            {
                repeatList.Add(pack);
                changed = true;
            }
        }
        
        var fields = kGraph.Fields;
        foreach (var field in fields)
        {
            if (!hash.Contains(field.fieldName))
            {
                var pack = new FieldBindingPack()
                {
                    fieldName = field.fieldName,
                    callback = new UnityEvent<object>()
                };
                packs.Add(pack);
                changed = true;
            }
            else
            {
                hash.Remove(field.fieldName);
            }
        }

        foreach (var fieldName in hash)
        {
            packs.Remove(packs.Find(pack => pack.fieldName == fieldName));
            changed = true;
        }

        foreach (var repeat in repeatList)
        {
            packs.Remove(repeat);
        }

        if (changed)
        {
            _callbackMap = null;
            EditorUtility.SetDirty(this);
            AssetDatabase.Refresh();
        }
    }

    private void OnEnable()
    {
        EventKeepRefresh();
    }
    
    public void Refresh()
    {
        if (!kGraph) throw new Exception("没有绑定节点图");
        _callbackMap = null;
        kGraph.Refresh();
        packs.Clear();
        foreach (var node in kGraph.Fields)
        {
            switch (node.fieldType)
            {
                case KField.FieldType.UnityEvent:
                    packs.Add(new FieldBindingPack
                    {
                        fieldName = node.fieldName,
                        callback = new UnityEvent<object>()
                    });
                    break;
                case KField.FieldType.FieldInfo:
                    break;
            }
        }

        EditorUtility.SetDirty(this);
        AssetDatabase.Refresh();
    }

    public void EventKeepRefresh()
    {
        if (!kGraph) throw new Exception("没有绑定节点图");
        _callbackMap = null;
        kGraph.Refresh();
        EditorUtility.SetDirty(this);
        AssetDatabase.Refresh();
    }

    public void Invoke()
    {
        foreach (var node in kGraph.Fields)
        {
            if (node.fieldType == KField.FieldType.UnityEvent)
            {
                CallBack[node.fieldName].Invoke(node.Value);
            }
        }
    }

    public bool Error => kGraph.CheckError();
    
}

[CustomEditor(typeof(KGraphMono),true)]
public class KFieldBinderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var binding =  target as KGraphMono;
        if (binding == null) return;
        if (GUILayout.Button("刷新变量绑定"))
        {
            binding.Refresh();
        }
    }
}
