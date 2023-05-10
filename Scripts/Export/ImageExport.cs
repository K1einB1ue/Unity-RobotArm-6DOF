
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public static class ImageExport
{
    
    public static Texture2D ToTexture2D(this Texture src, int width, int height, GraphicsFormat graphicsFormat)
    {
        var texture2D = new Texture2D(width, height, graphicsFormat, TextureCreationFlags.None);
        var tmpRT = RenderTexture.GetTemporary(src.width, src.height);
        Graphics.Blit(src, tmpRT);
        var preRT = RenderTexture.active;
        RenderTexture.active = tmpRT;
        texture2D.ReadPixels(new Rect(0, src.height - height, width, height), 0, 0);
        texture2D.Apply();
        RenderTexture.active = preRT;
        RenderTexture.ReleaseTemporary(tmpRT);
        return texture2D;
    }
    
    public static void ToTexture2D(this Texture src, int width, int height, GraphicsFormat graphicsFormat,List<Texture2D> buffer,int index)
    {
        var target = buffer[index];
        if (target == null) {
            buffer[index] = new Texture2D(width, height, graphicsFormat, TextureCreationFlags.None);
            target = buffer[index];
        } else if (target.width != width || target.height != height) {
            buffer[index] = new Texture2D(width, height, graphicsFormat, TextureCreationFlags.None);
            target = buffer[index];
        }
        var tmpRT = RenderTexture.GetTemporary(src.width, src.height);
        Graphics.Blit(src, tmpRT);
        var preRT = RenderTexture.active;
        RenderTexture.active = tmpRT;
        target.ReadPixels(new Rect(0, src.height - height, width, height), 0, 0);
        target.Apply();
        RenderTexture.active = preRT;
        RenderTexture.ReleaseTemporary(tmpRT);
    }
    
    public static void SavePNG(Texture texture, string filePath,string fileName)
    {
        Debug.Log($"图片保存至:{filePath}/{fileName}.png");
        var width = texture.width;
        var height = texture.height;
        var png = new Texture2D(width, height, TextureFormat.ARGB32, false);
        var tmpRT = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.Default);
        Graphics.Blit(texture, tmpRT);
        var preRT = RenderTexture.active;
        RenderTexture.active = tmpRT;
        png.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        png.Apply();
        RenderTexture.active = preRT;
        RenderTexture.ReleaseTemporary(tmpRT);
        var bytes = png.EncodeToPNG();
        using var fs = File.Open($"{filePath}/{fileName}.png", FileMode.Create);
        using var bw = new BinaryWriter(fs);
        bw.Write(bytes);
        
    }
    
    
    
    public static void SavePNG(Texture2D texture, string filePath,string fileName)
    {
        Debug.Log($"图片保存至:{filePath}/{fileName}.png");
        var bytes = texture.EncodeToPNG();
        using var fs = File.Open($"{filePath}/{fileName}.png", FileMode.Create);
        using var bw = new BinaryWriter(fs);
        bw.Write(bytes);
    }
    
    public static void SaveKin(Texture texture, string filePath, string fileName)
    {
        Debug.Log($"图片保存至:{filePath}/{fileName}.kin");
        var width = texture.width;
        var height = texture.height;
        var png = new Texture2D(width, height, TextureFormat.ARGB32, false);
        var tmpRT = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.Default);
        Graphics.Blit(texture, tmpRT);
        var preRT = RenderTexture.active;
        RenderTexture.active = tmpRT;
        png.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        png.Apply();
        RenderTexture.active = preRT;
        RenderTexture.ReleaseTemporary(tmpRT);
        var bytes = png.EncodeToPNG();
        using var fs = File.Open($"{filePath}/{fileName}.kin", FileMode.Create);
        using var bw = new BinaryWriter(fs);
        var length = bytes.Length;
        var lenData = BitConverter.GetBytes(length);
        bw.Write(lenData);
        bw.Write(bytes);
    }

    public static void SaveKin(Texture2D texture, string filePath, string fileName)
    {
        Debug.Log($"图片保存至:{filePath}/{fileName}.kin");
        var bytes = texture.EncodeToPNG();
        using var fs = File.Open($"{filePath}/{fileName}.kin", FileMode.Create);
        using var bw = new BinaryWriter(fs);
        bw.Write(bytes);
    }

    public static Texture2D ToTexture2D(this RenderTexture texture)
    {
        var width = texture.width;
        var height = texture.height;
        var preRT = RenderTexture.active;
        var texture2D = new Texture2D(width, height, TextureFormat.ARGB32, false);
        RenderTexture.active = texture;
        texture2D.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture2D.Apply();
        RenderTexture.active = preRT;
        return texture2D;
    }
}
