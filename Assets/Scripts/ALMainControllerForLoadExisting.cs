using Eq.Unity;
using System;
using System.Collections.Generic;
using System.IO;
using Tango;
using UnityEngine;

public class ALMainControllerForLoadExisting : BaseALMainController, ITangoEvent
{
    internal override bool StartTangoService()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        bool ret = true;

        if (!mLearningArea)
        {
            AreaDescription mostRecentAreaDescription = GetMostRecentAreaDescription();

            if (mostRecentAreaDescription != null)
            {
                mTangoApplication.m_areaDescriptionLearningMode = true;
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
