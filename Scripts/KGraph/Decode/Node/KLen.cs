
using System;
using UnityEngine.Events;
using XNode;

public enum ConvertType
{
    Int16,
    Int32,
    Int64,
}

[CreateNodeMenu("decode/len",order = 0)]
public class KLen : KDNode
{
    public ConvertType convertType;
    [Output()] public long len;


    public override void Decode(byte[] bytes, int offset, int length)
    {
        var newOff = convertType switch
        {
            ConvertType.Int16 => 2,
            ConvertType.Int32 => 4,
            ConvertType.Int64 => 8,
            _ => 0
        };
        if (length < offset + newOff) return;
        len = convertType switch
        {
            ConvertType.Int16 => BitConverter.ToInt16(bytes,offset),
            ConvertType.Int32 => BitConverter.ToInt32(bytes,offset),
            ConvertType.Int64 => BitConverter.ToInt64(bytes,offset),
            _ => 0
        };

    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(len)) return len;
        return base.GetValue(port);
    }
}
