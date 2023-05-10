using System;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[CreateNodeMenu("decode/fields",order = 3)]
public class KDFields : KDFRNode<List<object>>
{
    [Input] public Link prevLink;
    [Output()] public Link link;
    public List<ArrayConvertType> fields;
    public override void Decode(byte[] bytes, int offset, int length)
    {
        Field ??= new List<object>();
        Field.Clear();
        var fieldOff = 0;
        foreach (var type in fields)
        {
            var off = typeLen(type);
            if (length < offset + fieldOff + off) return;
            // ReSharper disable once HeapView.BoxingAllocation
            object value = type switch
            {
                ArrayConvertType.Double => BitConverter.ToDouble(bytes, offset+fieldOff),
                ArrayConvertType.Float => BitConverter.ToSingle(bytes, offset+fieldOff),
                ArrayConvertType.Int16 => BitConverter.ToInt16(bytes, offset+fieldOff),
                ArrayConvertType.Int32 => BitConverter.ToInt32(bytes, offset+fieldOff),
                ArrayConvertType.Int64 => BitConverter.ToInt64(bytes, offset+fieldOff),
                ArrayConvertType.Uint16 => BitConverter.ToUInt16(bytes, offset+fieldOff),
                ArrayConvertType.Uint32 => BitConverter.ToUInt32(bytes, offset+fieldOff),
                ArrayConvertType.Uint64 => BitConverter.ToUInt64(bytes, offset+fieldOff),
                _ => throw new ArgumentOutOfRangeException()
            };
            fieldOff += off;
            Field.Add(value);
        }
        Result = offset + fieldOff;
        foreach (var node in GetNodes<DecodeNode>(nameof(link)))
        {
            node.Decode(bytes, offset + fieldOff, length);
        }
    }

    private int typeLen(ArrayConvertType type)
    {
        return type switch
        {
            ArrayConvertType.Double => 8,
            ArrayConvertType.Float => 4,
            ArrayConvertType.Int16 => 2,
            ArrayConvertType.Int32 => 4,
            ArrayConvertType.Int64 => 8,
            ArrayConvertType.Uint16 => 2,
            ArrayConvertType.Uint32 => 4,
            ArrayConvertType.Uint64 => 8,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
    
}

