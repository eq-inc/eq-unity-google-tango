using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControllableMTMainController : MTMainController
{
    private bool mSavingMotionTracking = false;
    public Text mSwitchSaveMotionTrackingButtonText = null;

    internal override void Start()
    {
        base.Start();
        SwitchMotionTracking(false);
    }

    public void ButtonClicked(Object targetObject)
    {
        if (targetObject.name.CompareTo("SwitchSaveAndRestoreMotionTracking") == 0)
        {
            if (mSavingMotionTracking)
            {
                // Motion Trackingを停止
                SwitchMotionTracking(false);
            }
            else
            {
                // Motion Trackingを開始
                SwitchMotionTracking(true);
            }
        }
    }

    internal override void AddTrackingGameObject()
    {
        if (mSavingMotionTracking)
        {
            base.AddTrackingGameObject();
        }
    }

    internal void SwitchMotionTracking(bool nextMode)
    {
        mSavingMotionTracking = nextMode;
        if (nextMode)
        {
            // 表示をMotion Tracking停止に変更
            mSwitchSaveMotionTrackingButtonText.text = "Stop Motion Tracking";
        }
        else
        {
            // 表示をMotion Tracking開始に変更
            mSwitchSaveMotionTrackingButtonText.text = "Start Motion Tracking";
        }
    }
}
