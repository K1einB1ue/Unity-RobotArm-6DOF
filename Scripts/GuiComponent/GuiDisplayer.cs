
using System;
using UnityEditor;
using UnityEngine;

public abstract class GuiDisplayer : MonoBehaviour
{
    private void OnDrawGizmosSelected()
    {
        var fields = GetType().GetFields();
        foreach (var field in fields)
        {
            var fieldType = field.FieldType;
            if (fieldType.IsDefined(typeof(GuiComponentAttribute), true))
            {
                var guiComponent = (IGuiComponent) field.GetValue(this);
                if (guiComponent.Display && guiComponent.Enable)
                {
                    guiComponent.GizmosLayout(transform);
                }
            }
        }
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class GuiComponentAttribute : Attribute { }

[CustomEditor(typeof(GuiDisplayer),true)]
public class GuiDisplayerEditor : Editor
{
    public GuiDisplayer Displayer => target as GuiDisplayer;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var displayer = Displayer;
        var fields = displayer.GetType().GetFields();
        foreach (var field in fields)
        {
            var fieldType = field.FieldType;
            EditorGUILayout.PropertyField(serializedObject.FindProperty(field.Name));
            if (fieldType.IsDefined(typeof(GuiComponentAttribute), true))
            {
                GUILayout.Space(10);
                var guiComponent = (IGuiComponent) field.GetValue(displayer);
                if (guiComponent.Enable)
                {
                    for (var index = 0; index < guiComponent.GuiLayoutRequire.Count; index++)
                    {
                        var require = guiComponent.GuiLayoutRequire[index];
                        object component;
                        if (require == typeof(Transform))
                        {
                            component = displayer.transform;
                        }
                        else
                        {
                            if (!displayer.TryGetComponent(require, out var com))
                            {
                                throw new Exception("没有找到对应Component");
                            }

                            component = com;
                        }

                        var argList = guiComponent.GuiLayoutArg;
                        while (argList.Count <= index)
                        {
                            argList.Add(component);
                        }

                        argList[index] = component;
                    }
                    guiComponent.GuiLayout();
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}

