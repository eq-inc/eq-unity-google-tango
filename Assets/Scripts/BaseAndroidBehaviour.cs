using System;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

abstract public class BaseAndroidBehaviour : MonoBehaviour
{
    internal const long LogCategorySceneControl = 0x1;
    internal const long LogCategoryMethodIn = 0x2;
    internal const long LogCategoryMethodTrace = 0x4;
    internal const long LogCategoryMethodOut = 0x8;
    public long mOutputLogCategories = 0;

    internal void CategoryLog(long category, params object[] contents)
    {
        if ((mOutputLogCategories & category) == category)
        {
            switch (category)
            {
                case LogCategoryMethodIn:
                case LogCategoryMethodOut:
                    {
                        StringBuilder contentBuilder = new StringBuilder();
                        contentBuilder
                            .Append(new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name)
                            .Append("(")
                            .Append(((category == LogCategoryMethodIn) ? "IN" : "OUT") + ")");
                        if (contents != null && contents.Length > 0)
                        {
                            contentBuilder.Append(": ");
                            foreach (object content in contents)
                            {
                                contentBuilder.Append(content);
                            }
                        }

                        System.Diagnostics.Debug.WriteLine(contentBuilder.ToString());
                    }
                    break;
                case LogCategoryMethodTrace:
                    {
                        StringBuilder contentBuilder = new StringBuilder();
                        StackFrame lastStackFrame = new System.Diagnostics.StackTrace(true).GetFrame(1);

                        contentBuilder
                            .Append(lastStackFrame.GetMethod())
                            .Append("(")
                            .Append(System.IO.Path.GetFileName(lastStackFrame.GetFileName()))
                            .Append(":")
                            .Append(lastStackFrame.GetFileLineNumber())
                            .Append(")");
                        if (contents != null && contents.Length > 0)
                        {
                            contentBuilder.Append(": ");
                            foreach (object content in contents)
                            {
                                contentBuilder.Append(content);
                            }
                        }

                        System.Diagnostics.Debug.WriteLine(contentBuilder.ToString());
                    }
                    break;
                default:
                    if (contents != null && contents.Length > 0)
                    {
                        StringBuilder contentBuilder = new StringBuilder();
                        foreach (string content in contents)
                        {
                            contentBuilder.Append(content);
                        }
                        System.Diagnostics.Debug.WriteLine(contentBuilder.ToString());
                    }
                    break;
            }
        }
    }

    internal bool SetTextInUIComponent(Component topComponent, string targetComponentName, string content)
    {
        bool ret = false;
        Text targetComponent = GetTextComponent(topComponent, targetComponentName);

        if (targetComponent != null)
        {
            try
            {
                targetComponent.text = content;
                ret = true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e); ;
            }
        }

        return ret;
    }

    internal Text GetTextComponent(Component topComponent, string targetComponentName)
    {
        Text ret = null;
        Component[] childComponents = topComponent.GetComponentsInChildren<Component>();

        if (childComponents != null)
        {
            foreach (Component child in childComponents)
            {
                if (child != null)
                {
                    if (child.name.CompareTo(targetComponentName) == 0)
                    {
                        ret = child as Text;
                    }
                    // 再帰コールすると無限に入ってしまうので、再帰コールしない
                    //else
                    //{
                    //    ret = GetTargetChildComponent(child, targetComponentName);
                    //}

                    if (ret != null)
                    {
                        break;
                    }
                }
            }
        }

        return ret;
    }
}
