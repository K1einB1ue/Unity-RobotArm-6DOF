
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using XNode;

public static class KNodeExtern
{
    public static bool DyPortAct(this IList list, NodePort port, Func<int, object> action, out object ret, string fieldName){
        for (var i = 0; i < list.Count; i++)
        {
            if (port.fieldName == $"{fieldName} {i}")
            {
                ret = action(i);
                return true;
            }
        }

        ret = null;
        return false;
    }
}
