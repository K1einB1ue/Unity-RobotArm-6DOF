
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[Flags]
public enum KDataType
{
    PlayingMode = 1<<0,
    EditorMode = 1<<1,
}

public class KCamData : MonoBehaviour
{
    public KDataType dataType;

}




