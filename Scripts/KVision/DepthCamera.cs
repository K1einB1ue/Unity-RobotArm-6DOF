using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[Serializable]
[GuiComponent]
public class DepthCamera : GuiComponent
{
    private Texture _texture;

    private static readonly List<Type> GUILayoutRequire = new() {typeof(Camera),typeof(CameraGui)};
    public override List<Type> GuiLayoutRequire => GUILayoutRequire;
    public override void GuiLayout()
    {
        var cam = GetArg<Camera>(0);
        var gui = GetArg<CameraGui>(1);
        if (GUILayout.Button("开始"))
        {
            cam.depthTextureMode |= DepthTextureMode.Depth;
            RenderPipelineManager.beginCameraRendering += OnPreRender;
            RenderPipelineManager.endCameraRendering += OnPostRender;
        }
        if (GUILayout.Button("停止"))
        {
            RenderPipelineManager.beginCameraRendering -= OnPreRender;
            RenderPipelineManager.endCameraRendering -= OnPostRender;
        }
        if (GUILayout.Button("拍摄"))
        {
            //gui.imageSaver.SaveTexture(_texture,0);
            var width = cam.pixelWidth;
            var height = cam.pixelHeight;
            var tmp = new Texture2D(width, height, GraphicsFormat.R32_SFloat /* _texture.graphicsFormat*/,
                TextureCreationFlags.None);
            var tmpRT = RenderTexture.GetTemporary(_texture.width, _texture.height);
            Graphics.Blit(_texture, tmpRT);
            var preRT = RenderTexture.active;
            RenderTexture.active = tmpRT;
            tmp.ReadPixels(new Rect(0, _texture.height - height, width, height), 0, 0);
            tmp.Apply();
            RenderTexture.active = preRT;
            RenderTexture.ReleaseTemporary(tmpRT);
            var connection = KTorch.Connection;
            if (connection != null)
            {
                var data = tmp.EncodeToPNG();
                var length = data.Length;
                var lenData = BitConverter.GetBytes(length);
                connection.Send(lenData);
                connection.Send(data);
                gui.imageSaver.SaveKin(tmp);
            }
            else
            {
                gui.imageSaver.SaveTexture(tmp);
            }
        }
    }


    private void OnPreRender(ScriptableRenderContext context, Camera camera)
    {
        
    }
    private void OnPostRender(ScriptableRenderContext context, Camera camera)
    {
        _texture = Shader.GetGlobalTexture("_CameraDepthTexture");
    }

    public override void GizmosLayout(Transform transform)
    {
        
    }

}
