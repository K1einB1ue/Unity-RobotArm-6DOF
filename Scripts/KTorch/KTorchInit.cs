using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;



[Serializable]
[GuiComponent]
public class KTorchInit : GuiComponent
{
    public string ipAddress;
    public int port;
    private static List<Type> _guiLayoutRequire = new() { };
    private Socket _sender;
    public override List<Type> GuiLayoutRequire => _guiLayoutRequire;

    

    void SocketInit()
    {
        if (_sender != null)
        {
            Debug.LogWarning("无法多次初始化");
            return;
        }
        var ip = IPAddress.Parse(ipAddress);
        var remoteEp = new IPEndPoint(ip, port);
        _sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            _sender.Connect(remoteEp);
            KTorch.connection = _sender;
            Debug.Log("成功连接");
        }
        catch (Exception e)
        {
            _sender = null;
            Debug.LogError("无法连接");
        }
    }

    void SocketShutdown()
    {
        if (_sender == null)
        {
            Debug.LogWarning("无法多次关闭");
            return;
        }
        _sender.Shutdown(System.Net.Sockets.SocketShutdown.Both);
        _sender.Close();
        _sender = null;
        KTorch.connection = null;
        Debug.Log("成功关闭");
    }
    

    public override void GuiLayout()
    {
        if (GUILayout.Button("初始化Socket"))
        {
            SocketInit();
        }

        if (GUILayout.Button("关闭Socket"))
        {
            SocketShutdown();
        }
        
    }

    public override void GizmosLayout(Transform transform)
    {
        throw new NotImplementedException();
    }
}


public static class KTorch
{
    public static Socket Connection => connection;
    internal static Socket connection = null;
}