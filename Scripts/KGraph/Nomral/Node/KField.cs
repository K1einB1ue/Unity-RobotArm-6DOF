using System;
using XNode;


[CreateNodeMenu("normal/field",order = 1)]
public class KField : Node
{
    public enum FieldType
    {
        UnityEvent,
        FieldInfo,
    } 
    
    [Input(typeConstraint = TypeConstraint.Strict,connectionType = ConnectionType.Override)] public FieldLink fieldLink;
    public string fieldName;
    public FieldType fieldType;
    public object Value => GetInputPort(nameof(fieldLink)).GetInputValue();
}



