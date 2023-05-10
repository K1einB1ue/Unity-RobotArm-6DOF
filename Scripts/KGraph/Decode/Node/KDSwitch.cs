using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using XNode;

[CreateNodeMenu("decode/switch",order = 1)]
public class KDSwitch : KDRNode
{
    [Input] public Link prevLink;
    [Output(dynamicPortList = true)][TextArea] public List<string> switches;
    public override void Decode(byte[] bytes, int offset, int length)
    {
        var maxLen= 0;
        var invokeFlag = false;
        for (int i = 0; i < switches.Count; i++)
        {
            var flag = true;
            var match = switches[i];
            var len = match.Length;
            if (len > maxLen) maxLen = len;
            if(length < offset + len)
                continue;
            for (var j = 0; j < match.Length; j++)
            {
                if (bytes[offset + j] != match[j])
                {
                    flag = false;
                    break;
                }
            }
            if(!flag) continue;
            foreach (var node in GetNodes<DecodeNode>(nameof(switches),i))
            {
                node.Decode(bytes, offset + match.Length, length);
            }
            invokeFlag = true;
        }

        if (!invokeFlag && length >= maxLen) Result = 1;
        else Result = 0;
    }
}


