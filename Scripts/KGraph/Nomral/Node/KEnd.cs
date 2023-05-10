
using XNode;

[CreateNodeMenu("normal/end",order = 1)]
public class KEnd : Node
{
    [Input(typeConstraint = TypeConstraint.Strict, connectionType = ConnectionType.Override)]
    public ResultLink resultLink;

    public int End
    {
        get
        {
            var port = GetPort(nameof(resultLink));
            if (port.Connection == null) return 0;
            return (int) port.GetInputValue();
        }
    }
}
