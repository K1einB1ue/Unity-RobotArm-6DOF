
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class GizmosComponent : GuiComponent
{
    public bool display = false;
    public override bool Display => display;
}
public abstract class GuiComponent : IGuiComponent
{
    private readonly List<object> _guiLayoutArg = new();
    public virtual bool Enable => true;
    public virtual bool Display => false;
    public List<object> GuiLayoutArg => _guiLayoutArg;
    public abstract List<Type> GuiLayoutRequire { get; }
    public abstract void GuiLayout();
    public abstract void GizmosLayout(Transform transform);

    public T GetArg<T>(int index) where T : class
    {
        return _guiLayoutArg[index] as T;
    }
}
public interface IGuiComponent
{
    bool Enable { get; }
    bool Display { get; }
    List<object> GuiLayoutArg { get; }
    List<Type> GuiLayoutRequire { get; }
    void GuiLayout();
    void GizmosLayout(Transform transform);
}
