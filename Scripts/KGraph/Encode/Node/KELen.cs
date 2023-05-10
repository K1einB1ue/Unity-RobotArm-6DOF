
using System;
using System.Text.RegularExpressions;
using UnityEngine;

[CreateNodeMenu("encode/len",order = 0)]
public class KELen : KENode
{
    [Input(connectionType = ConnectionType.Override)]
    public FieldLink fieldLink;
    [Input(typeConstraint = TypeConstraint.Strict, connectionType = ConnectionType.Override)]
    public Link prevLink;
    [Output(typeConstraint = TypeConstraint.Strict, connectionType = ConnectionType.Override)]
    public Link nextLink;

    private bool _runtimeInit = false;
    protected override void Init()
    {
        _runtimeInit = false;
    }

    public override void Send(Action<byte[]> send, Func<bool> enable)
    {
        if (Application.isPlaying)
        {
            if (_runtimeInit) return;
            _runtimeInit = true;
            var port = GetPort(nameof(fieldLink)).Connection;
            if (port.node is KImage node)
            {
                var fieldName = port.fieldName;
                Debug.Log(fieldName);
                var match = Regex.Match(fieldName, "[1-9]+[0-9]*$");
                var index = Convert.ToInt32(match.Value);
                var str = node.imageNames[index];
                node.RuntimeInit(str, send, enable);
            }
        }else {
            var obj = GetInputPort(nameof(fieldLink)).GetInputValue();
            if (obj is byte[] bytes)
            {
                var lenBytes = BitConverter.GetBytes(bytes.Length);
                send.Invoke(lenBytes);
                send.Invoke(bytes);
                foreach (var node in GetNodes<EncodeNode>(nameof(nextLink)))
                {
                    node.Send(send, enable);
                }
            }
            else
            {
                Debug.LogError($"{name},期望为:byte[],实际为:{obj.GetType()}");
            }
        }
    }
    
    
}
