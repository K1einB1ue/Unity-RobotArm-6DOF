
using XNode;

[CreateNodeMenu("normal/error",order = 2)]
public class KError : Node
{
    [Input(typeConstraint = TypeConstraint.Strict, connectionType = ConnectionType.Override)]
    public ResultLink errorLink;

    public int Error{
        get
        {
            var port = GetPort(nameof(errorLink));
            if (port.Connection == null) return 0;
            return (int) port.GetInputValue();
        }
    }
}
