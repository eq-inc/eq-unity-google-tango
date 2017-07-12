using Eq.Util;
using System;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

abstract public class BaseAndroidBehaviour : MonoBehaviour
{
    internal const long LogCategoryMethodIn = LogController.LogCategoryMethodIn;
    internal const long LogCategoryMethodTrace = LogController.LogCategoryMethodTrace;
    internal const long LogCategoryMethodOut = LogController.LogCategoryMethodOut;
    public long mOutputLogCategories = 0;
    internal LogController mLogger = new LogController();

    internal void CategoryLog(long category, params object[] contents)
    {
        mLogger.SetOutputLogCategory(mOutputLogCategories);
        mLogger.CategoryLog(category, contents);
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
