using Eq.Unity;
using System;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Eq.Unity
{
    abstract public class BaseAndroidBehaviour : MonoBehaviour
    {
        internal const System.Int64 LogCategoryMethodIn = LogController.LogCategoryMethodIn;
        internal const System.Int64 LogCategoryMethodTrace = LogController.LogCategoryMethodTrace;
        internal const System.Int64 LogCategoryMethodOut = LogController.LogCategoryMethodOut;
        internal const System.Int64 LogCategoryMethodError = LogController.LogCategoryMethodError;
        public System.Int64 mOutputLogCategories = 0;
        internal LogController mLogger = new LogController();

        internal virtual void OnEnable()
        {
            mLogger.SetOutputLogCategory(mOutputLogCategories);
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
}
