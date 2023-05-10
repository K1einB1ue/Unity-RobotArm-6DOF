using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[GuiComponent]
public class ImageSaver : GuiComponent
{
    public string imageFilePath;
    public string imageFileName;
    
    private static List<Type> _guiLayoutRequire = new() {typeof(CameraGui)};
    public override List<Type> GuiLayoutRequire => _guiLayoutRequire;

    public override void GuiLayout()
    {
        
    }

    public void SaveTexture(Texture texture)
    {
        ImageExport.SavePNG(texture, imageFilePath, imageFileName);
    }

    public void SaveKin(Texture texture)
    {
        ImageExport.SaveKin(texture, imageFilePath, imageFileName);
    }
    
    public override void GizmosLayout(Transform transform)
    {
        
    }
}