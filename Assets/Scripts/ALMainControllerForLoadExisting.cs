using Eq.Unity;
using System.Collections.Generic;
using Tango;
using UnityEngine;

public class ALMainControllerForLoadExisting : BaseALMainController, ITangoEvent
{
    private const string StartMotionTrackingContent = "Start Motion Tracking";
    private const string StopMotionTrackingContent = "Stop Motion Tracking";

    private UnityEngine.UI.Text mSwitchMotionTrackingTextComponent;
    private bool mEnableMotionTracking = false;

    public void SwitchMotionTrackingButtonClick()
    {
        if(mSwitchMotionTrackingTextComponent != null)
        {
            if (mEnableMotionTracking)
            {
                // Start -> Stop
                mSwitchMotionTrackingTextComponent.text = StartMotionTrackingContent;
            }
            else
            {
                // Stop -> Start
                mSwitchMotionTrackingTextComponent.text = StopMotionTrackingContent;
            }
        }

        mEnableMotionTracking = (!mEnableMotionTracking);
    }

    internal override void Start()
    {
        base.Start();

        GameObject switchMotionTrackingText = GameObject.Find("SwitchMotionTrackingText");
        if(switchMotionTrackingText != null)
        {
            mSwitchMotionTrackingTextComponent = switchMotionTrackingText.GetComponent<UnityEngine.UI.Text>();
            mSwitchMotionTrackingTextComponent.text = StartMotionTrackingContent;
        }
    }

    internal override bool StartTangoService()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        bool ret = true;

        if (!mLearningArea)
        {
            AreaDescription mostRecentAreaDescription = GetMostRecentAreaDescription();

            if (mostRecentAreaDescription != null)
            {
                mPoseDataManager = PoseDataManager.GetInstance(mostRecentAreaDescription.m_uuid);
                mTangoApplication.Startup(mostRecentAreaDescription);
                mLearningArea = true;
                new CallbackAsncTask<string, int, List<PoseData>>(new LoadPoseDataCallback(this)).Execute();
            }
            else
            {
                mLogger.CategoryLog(LogCategoryMethodError, "none area description file");
                ret = false;
            }

            mPoseDataManager.EnableDebugLog((mLogger.GetOutputLogCategory() & LogCategoryMethodTrace) == LogCategoryMethodTrace);
        }
        else
        {
            ret = false;
        }

        mLogger.CategoryLog(LogCategoryMethodOut, "ret = " + ret);
        return ret;
    }

    internal override bool StopTangoService()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        bool ret = true;

        if (mLearningArea)
        {
            // 停止したときに、領域学習の保存を実施
            CallbackAsncTask<string, int, bool> task = new CallbackAsncTask<string, int, bool>(new SavePoseDataCallback(this));
            task.EnableDebugLog((mLogger.GetOutputLogCategory() & LogCategoryMethodTrace) == LogCategoryMethodTrace);
            task.Execute();
            mLearningArea = false;
        }
        else
        {
            ret = false;
        }

        mLogger.CategoryLog(LogCategoryMethodOut, "ret = " + ret);
        return ret;
    }

    internal override PoseData AddTrackingGameObject(TangoPoseData poseData, GameObject itemPrefab)
    {
        PoseData ret = null;

        if (itemPrefab.Equals(mLoadedMotionTrackingCapsule) || mEnableMotionTracking)
        {
            ret = base.AddTrackingGameObject(poseData, itemPrefab);
        }

        return ret;
    }

    public void OnTangoEventAvailableEventHandler(TangoEvent tangoEvent)
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        if (tangoEvent.type == TangoEnums.TangoEventType.TANGO_EVENT_AREA_LEARNING
            && tangoEvent.event_key == "AreaDescriptionSaveProgress")
        {
            mLogger.CategoryLog(LogCategoryMethodTrace, "Saving AreaLearned: " + (float.Parse(tangoEvent.event_value) * 100) + "%");
        }
        mLogger.CategoryLog(LogCategoryMethodOut);
    }
}
