
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Threading;

[ExecuteAlways]
public class KSocket : KGraphMono
{
    public string ipAddress;
    public int port;
    private Socket _socket;
    private byte[] _buffer;
    private int _bufferRecvLen;
    private Thread _recvThread;

    
    public void Update()
    {
        UpdatePacks();
        if (_socket == null) return;
        if (!_socket.Connected) return;
        var recvNum = _socket.Available;
        
        
        if (_bufferRecvLen + recvNum > _buffer.Length)
        {
            var tmp = _buffer;
            _buffer = new byte[_buffer.Length * 2];
            tmp.CopyTo(_buffer, _bufferRecvLen);
        }

        if (recvNum != 0)
        {
            _socket.Receive(_buffer, _bufferRecvLen, recvNum, SocketFlags.None);
            _bufferRecvLen += recvNum;
        }


        var offset = Decode(_buffer, _bufferRecvLen);
        if (Error)
        {
            offset = 0;
            _bufferRecvLen = 0;
        }
        if (offset != 0)
        {
            Invoke();
            _bufferRecvLen -= offset;
            if (_bufferRecvLen != 0)
            {
                Array.Copy(_buffer, offset, _buffer, 0, _bufferRecvLen);
            }
        }
        
        Send(bytes =>
        {
            _socket.Send(bytes);
        }, () =>
        {
            if (_socket == null) return false;
            return _socket.Connected;
        });
    }

    public void DebugLog(object input)
    {
        var list = input as List<object>;
        if (list == null) return;
        foreach (var o in list)
        {
            Debug.Log(o);
        }
    }
    public void Connect()
    {
        _bufferRecvLen = 0;
        _buffer = new byte[512];
        var ip = IPAddress.Parse(ipAddress);
        var remoteEp = new IPEndPoint(ip, port);
        if (_socket != null)
        {
            _socket.Close();
            
        }
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            _socket.Connect(remoteEp);
            Debug.Log("连接成功");
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }
    }

    public void Close()
    {
        if (_socket != null)
        {
            _socket.Close();
        }
        Debug.Log("断开成功");
    }
    
    
    public int Decode(byte[] bytes, int length)
    {
        var starts = kGraph.GetNodes<KStart>();
        foreach (var start in starts)
        {
            foreach (var node in start.ConnectNodes<DecodeNode>())
            {
                node.Decode(bytes, 0, length);
            }
        }

        return kGraph.CheckEnd();
    }

    public void Send(Action<byte[]> send, Func<bool> enable)
    {
        var starts = kGraph.GetNodes<KStart>();
        foreach (var start in starts)
        {
            foreach (var node in start.ConnectNodes<EncodeNode>())
            {
                node.Send(send, enable);
            }
        }
    }

}

[CustomEditor(typeof(KSocket),true)]
public class KSocketEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var kSocket = target as KSocket;
        base.OnInspectorGUI();
        if (GUILayout.Button("连接"))
        {
            // ReSharper disable once PossibleNullReferenceException
            kSocket.EventKeepRefresh();
            kSocket.Connect();
        }
        if (GUILayout.Button("断开"))
        {
            // ReSharper disable once PossibleNullReferenceException
            kSocket.Close();
        }
    }
}

