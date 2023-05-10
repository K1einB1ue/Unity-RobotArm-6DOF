
using System.Collections.Generic;
using XNode;
[CreateNodeMenu("normal/start",order = 0)]
public class KStart : Node
{
    [Output()] public Link link;
    public NodePort StartPort => GetPort(nameof(link));

    public IEnumerable<T> ConnectNodes<T>()
    {
        var ports = StartPort.GetConnections();
        foreach (var port in ports)
        {
            if (port.node is T node)
            {
                yield return node;
            }
        }
    } 

    public override object GetValue(NodePort port)
    {
        return null;
    }
}
