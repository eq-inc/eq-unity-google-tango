using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

public class BaseAndroidBehaviour : MonoBehaviour {
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

                        UnityEngine.Debug.Log(contentBuilder.ToString());
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

                        UnityEngine.Debug.Log(contentBuilder.ToString());
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
                        UnityEngine.Debug.Log(contentBuilder.ToString());
                    }
                    break;
            }
        }
    }
}
