
using System;
using System.Collections.Generic;


[CreateNodeMenu("decode/array",order = 0)]
public class KArray : KDNode
{
    [Input()] public int len;
    public ArrayConvertType arrayConvertType;
    private object _buffer;
    public override void Decode(byte[] bytes, int offset, int length)
    {
        throw new System.NotImplementedException();
    }
}

public enum ArrayConvertType
{
    Int16,
    Int32,
    Int64,
    Uint16,
    Uint32,
    Uint64,
    Float,
    Double,
}

