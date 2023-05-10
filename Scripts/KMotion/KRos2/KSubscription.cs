using System;
using ROS2;
using UnityEditor;
using UnityEngine;

public abstract class KSubscription<T> : KRosNode where T : Message, new()
{
    public string topic;
    private string _topic;

    private Subscription<T> _subscription = null;

    public event Action<T> OnMsg;

    protected override void OnNodeInit(ROS2Node newNode)
    {
        _subscription = newNode.CreateSubscription<T>(topic, msg => { OnMsg?.Invoke(msg); });
        _topic = topic;
    }

    protected override void OnNodeRefresh(ROS2Node preNode, ROS2Node newNode)
    {
        preNode.RemoveSubscription<T>(_subscription);
        _subscription = newNode.CreateSubscription<T>(topic, msg => { OnMsg?.Invoke(msg); });
        _topic = topic;
    }

    protected override void OnNodeUpdate()
    {
        if (topic == _topic) return;
        if (_subscription != null) Node.RemoveSubscription<T>(_subscription);
        _subscription = Node.CreateSubscription<T>(topic, msg => { OnMsg?.Invoke(msg); });
        _topic = topic;
    }
}

