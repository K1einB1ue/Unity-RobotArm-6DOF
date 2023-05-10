
using System;
using System.Diagnostics;
using UnityEngine;
using ROS2;using UnityEditor;
using Debug = UnityEngine.Debug;

public abstract class KRosNode : MonoBehaviour
{
    private ROS2UnityComponent _ros2UnityComponent = null;
    public ROS2UnityComponent Ros
    {
        get
        {
            if (_ros2UnityComponent == null)
            {
                if (TryGetComponent(out ROS2UnityComponent rosC))
                {
                    _ros2UnityComponent = rosC;
                    return _ros2UnityComponent;
                }
                _ros2UnityComponent = gameObject.AddComponent<ROS2UnityComponent>();
            }
            return _ros2UnityComponent;
        }
    }
    
    public string nodeName;
    private string _nodeName;

    private ROS2Node _node = null;

    public ROS2Node Node
    {
        get
        {
            if (_node == null)
            {
                _node = Ros.CreateNode(nodeName);
                _nodeName = nodeName;
                OnNodeInit(_node);
                return _node;
            }

            if (nodeName != _nodeName)
            {
                _nodeName = nodeName;
                var newNode = Ros.CreateNode(nodeName);
                OnNodeRefresh(_node, newNode);
                Ros.RemoveNode(_node);
                _node = newNode;
                return _node;
            }

            return _node;
        }
    }

    protected abstract void OnNodeInit(ROS2Node newNode);
    protected abstract void OnNodeRefresh(ROS2Node preNode,ROS2Node newNode);
    protected abstract void OnNodeUpdate();

    private void OnEnable()
    {
        var _ = Node;
    }

    public void Refresh()
    {
        var _ = Node;
        Update();
    }

    private void Update()
    {
        if (nodeName != _nodeName)
        {
            _nodeName = nodeName;
            var newNode = Ros.CreateNode(nodeName);
            OnNodeRefresh(_node, newNode);
            Ros.RemoveNode(_node);
            _node = newNode;
        }
        OnNodeUpdate();
    }
}

[CustomEditor(typeof(KRosNode),true)]
public class KRosNodeEditor : Editor {
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (target is KRosNode node)
        {
            if (GUILayout.Button("刷新节点"))
            {
                node.Refresh();
                Debug.Log("刷新了节点");
            }
        }
    }
}

