
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using XNode;

public class KImage : Node
{
    [Output(dynamicPortList = true)] [TextArea]
    public List<string> imageNames;

    private static HashSet<string> _initList = new();
    private RenderTexture _camRT = null;
    private List<Texture2D> _buffer = new List<Texture2D>();
    protected override void Init()
    {
        _camRT = null;
        _initList.Clear();
        _buffer.Clear();
    }
    
    public override object GetValue(NodePort port)
    {
        var cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("没有绑定主摄像头");
            return null;
        }

        var width = cam.pixelWidth;
        var height = cam.pixelHeight;
        if (imageNames.DyPortAct(port, index => {
                var str = imageNames[index];
                if (str == "rgb")
                {
                    var rt = new RenderTexture(width, height, 32);
                    cam.targetTexture = rt;
                    var img = new Texture2D(width, height, TextureFormat.RGBA32, false);
                    cam.Render();
                    var pre = RenderTexture.active;
                    RenderTexture.active = rt;
                    img.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                    cam.targetTexture = null;
                    RenderTexture.active = pre;
                    DestroyImmediate(rt);
                    var bytes = img.EncodeToPNG();
                    return bytes;
                }
                var texture = Shader.GetGlobalTexture(str);
                if (texture == null)
                {
                    Debug.LogError($"没有找到对应贴图:{str}");
                    return null;
                }
                var t2D = texture.ToTexture2D(width, height, texture.graphicsFormat);
                return t2D.EncodeToPNG();
            }, out var ret,nameof(imageNames))) return ret;
        return base.GetValue(port);
    }

    public void RuntimeInit(string imageName, Action<byte[]> send, Func<bool> enable)
    {
        if (_initList.Contains(imageName))
        {
            Debug.LogError("尝试多次读取同一贴图!");
            return;
        }

        _initList.Add(imageName);
        _buffer.Add(null);
        var bufferIndex = _buffer.Count - 1;

        var cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("没有绑定主摄像头");
            return;
        }

        var width = cam.pixelWidth;
        var height = cam.pixelHeight;
        cam.depthTextureMode |= DepthTextureMode.Depth;
        if (imageName == "rgb")
        {
            RenderPipelineManager.endCameraRendering += (context, came) =>
            {
                if (!enable()) return;
                if (!ReferenceEquals(came, cam)) return;
                _camRT.ToTexture2D(width, height, GraphicsFormat.R32G32B32_SFloat, _buffer, bufferIndex);
                var bytes = _buffer[bufferIndex].EncodeToJPG();
                send(BitConverter.GetBytes(bytes.Length));
                send(bytes);
            };
            _camRT = new RenderTexture(width, height, 32);
            cam.targetTexture = _camRT;
        }
        else
        {
            RenderPipelineManager.endCameraRendering += (context, came) =>
            {
                if (!enable()) return;
                if (!ReferenceEquals(came, cam)) return;
                var image = Shader.GetGlobalTexture(imageName);
                var imageT = image.ToTexture2D(width, height, GraphicsFormat.R32_SFloat);
                var bytes = imageT.EncodeToJPG();
                send(BitConverter.GetBytes(bytes.Length));
                send(bytes);
            };
        }
    }

}
