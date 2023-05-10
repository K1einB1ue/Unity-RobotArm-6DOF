
using System;
using XNode;
[Serializable]
public class FieldLink { }

[Serializable]
public class ResultLink { }

[Serializable]
public class Link { }

public abstract class KRNode : Node
{
    [Output(typeConstraint = TypeConstraint.Strict)] public ResultLink resultLink;
    private int _result;
    protected int Result { get => _result; set => _result = value; }
    
    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(resultLink))
        {
            var temp = _result;
            _result = 0;
            return temp;
        }
        return base.GetValue(port);
    }
}

public abstract class KFNode<T> : Node where T : class
{
    [Output(typeConstraint = TypeConstraint.Strict)] public FieldLink fieldLink;
    private object _field = null;
    protected virtual T Field { get => _field as T; set => _field = value; }
    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(fieldLink)) return _field;
        return base.GetValue(port);
    }
}

public abstract class KFRNode<T> : Node where T : class
{
    [Output(typeConstraint = TypeConstraint.Strict)] public FieldLink fieldLink;
    [Output(typeConstraint = TypeConstraint.Strict)] public ResultLink resultLink;
    private int _result;
    protected int Result { get => _result; set => _result = value; }
    private object _field = null;
    protected virtual T Field { get => _field as T; set => _field = value; }
    
    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(resultLink))
        {
            var temp = _result;
            _result = 0;
            return temp;
        }
        if (port.fieldName == nameof(fieldLink)) return _field;
        return base.GetValue(port);
    }
}

