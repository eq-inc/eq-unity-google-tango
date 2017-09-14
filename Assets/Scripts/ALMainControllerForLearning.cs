using Eq.Unity;
using Tango;

public class ALMainControllerForLearning : BaseALMainController, ITangoEvent
{
    internal override bool StartTangoService()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        bool ret = true;

        if (!mLearningArea)
        {
            // 古いADFは全て削除(実験用)
            AreaDescription[] allAreaDescriptionArray = AreaDescription.GetList();
            if(allAreaDescriptionArray != null && allAreaDescriptionArray.Length > 0)
            {
                foreach (AreaDescription areaDescription in allAreaDescriptionArray)
                {
                    mLogger.CategoryLog(LogCategoryMethodTrace, "remove AreaDescription and PoseDataManager: uuid = " + areaDescription.m_uuid);
                    PoseDataManager.RemoveInstance(areaDescription.m_uuid);
                    areaDescription.Delete();
                }
            }

            mPoseDataManager = PoseDataManager.GetInstance(null);
            mPoseDataManager.EnableDebugLog((mLogger.GetOutputLogCategory() & LogCategoryMethodTrace) == LogCategoryMethodTrace);
            mTangoApplication.Startup(null);
            mLearningArea = true;
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
            task.CopyLogController(mLogger);
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
