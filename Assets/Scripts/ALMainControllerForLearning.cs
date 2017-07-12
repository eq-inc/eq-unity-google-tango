using Eq.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Tango;

public class ALMainControllerForLearning : BaseALMainController, ITangoEvent
{
    private bool mLearningArea = false;

    internal override bool StartTangoService()
    {
        CategoryLog(LogCategoryMethodIn);
        bool ret = true;

        if (!mLearningArea)
        {
            AreaDescription mostRecentAreaDescription = GetMostRecentAreaDescription();

            CategoryLog(LogCategoryMethodTrace, "Area Learning = " + mTangoApplication.m_enableAreaLearning + ", area description learning mode = " + mTangoApplication.m_areaDescriptionLearningMode + ", ADFLoading = " + mTangoApplication.m_enableADFLoading + ", EnableAreaDescriptions = " + mTangoApplication.EnableAreaDescriptions);
            mTangoApplication.m_areaDescriptionLearningMode = true;
            if (mostRecentAreaDescription != null)
            {
                mPoseDataManager = PoseDataManager.GetInstance(mostRecentAreaDescription.m_uuid);
                mTangoApplication.Startup(mostRecentAreaDescription);
            }
            else
            {
                mPoseDataManager = PoseDataManager.GetInstance(null);
                mTangoApplication.Startup(null);
            }

            mPoseDataManager.EnableDebugLog((mLogger.GetOutputLogCategory() & LogCategoryMethodTrace) == LogCategoryMethodTrace);

            CallbackAsncTask<string, int, List<PoseData>> task = new CallbackAsncTask<string, int, List<PoseData>>(new LoadPoseDataCallback(this));
            task.EnableDebugLog((mLogger.GetOutputLogCategory() & LogCategoryMethodTrace) == LogCategoryMethodTrace);
            task.Execute();
            mLearningArea = true;
        }
        else
        {
            ret = false;
        }

        CategoryLog(LogCategoryMethodOut, "ret = " + ret);
        return ret;
    }

    internal override bool StopTangoService()
    {
        CategoryLog(LogCategoryMethodIn);
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

        CategoryLog(LogCategoryMethodOut, "ret = " + ret);
        return ret;
    }

    private class LoadPoseDataCallback : CallbackAsncTask<string, int, List<PoseData>>.IResultCallback
    {
        private BaseALMainController mController;
        private string mRootDataPath;

        public LoadPoseDataCallback(BaseALMainController controller)
        {
            if (controller == null)
            {
                throw new ArgumentNullException("controller == null");
            }

            mController = controller;
        }

        void CallbackAsncTask<string, int, List<PoseData>>.IResultCallback.OnPreExecute()
        {
#if UNITY_EDITOR
            mRootDataPath = Directory.GetCurrentDirectory();
#else
            mRootDataPath = UnityEngine.Application.persistentDataPath;
#endif
        }

        void CallbackAsncTask<string, int, List<PoseData>>.IResultCallback.OnProgressUpdate(params int[] values)
        {
            // 処理なし
        }

        void CallbackAsncTask<string, int, List<PoseData>>.IResultCallback.OnPostExecute(List<PoseData> result, params string[] parameters)
        {
            mController.mPoseList = result;
        }

        List<PoseData> CallbackAsncTask<string, int, List<PoseData>>.ICallback.DoInBackground(params string[] parameters)
        {
            List<PoseData> ret = mController.mPoseDataManager.Load(mRootDataPath, PoseDataManager.TypeAreaLearning);

            //if(ret != null && ret.Count > 0)
            //{
            //    TangoPoseData tangoPoseData = new TangoPoseData();
            //    TangoCoordinateFramePair pair = new TangoCoordinateFramePair();
            //    pair.baseFrame = TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_AREA_DESCRIPTION;
            //    pair.targetFrame = TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_DEVICE;

            //    foreach (PoseData poseData in ret)
            //    {
            //        PoseProvider.GetPoseAtTime(tangoPoseData, poseData.timestamp, pair);
            //        if((tangoPoseData.status_code == TangoEnums.TangoPoseStatusType.TANGO_POSE_VALID) && (poseData.timestamp == tangoPoseData.timestamp))
            //        {
            //            mController.AddTrackingGameObject(tangoPoseData);
            //        }
            //    }
            //}

            return ret;
        }
    }

    private class SavePoseDataCallback : CallbackAsncTask<string, int, bool>.IResultCallback
    {
        private ALMainControllerForLearning mController;
        private string mRootDataPath;

        public SavePoseDataCallback(ALMainControllerForLearning controller)
        {
            if (controller == null)
            {
                throw new ArgumentNullException("controller == null");
            }

            mController = controller;
        }

        void CallbackAsncTask<string, int, bool>.IResultCallback.OnPreExecute()
        {
#if UNITY_EDITOR
            mRootDataPath = Directory.GetCurrentDirectory();
#else
            mRootDataPath = UnityEngine.Application.persistentDataPath;
#endif
        }

        void CallbackAsncTask<string, int, bool>.IResultCallback.OnProgressUpdate(params int[] values)
        {
            // 処理なし
        }

        void CallbackAsncTask<string, int, bool>.IResultCallback.OnPostExecute(bool result, params string[] parameters)
        {
            // 処理なし
        }

        bool CallbackAsncTask<string, int, bool>.ICallback.DoInBackground(params string[] parameters)
        {
            AreaDescription ret = AreaDescription.SaveCurrent();
            mController.mPoseDataManager.SetUuid(ret.m_uuid);
            return mController.mPoseDataManager.Save(mRootDataPath, PoseDataManager.TypeAreaLearning);
        }
    }

    public void OnTangoEventAvailableEventHandler(TangoEvent tangoEvent)
    {
        if (tangoEvent.type == TangoEnums.TangoEventType.TANGO_EVENT_AREA_LEARNING
            && tangoEvent.event_key == "AreaDescriptionSaveProgress")
        {
            CategoryLog(LogCategoryMethodTrace, "Saving AreaLearned: " + (float.Parse(tangoEvent.event_value) * 100) + "%");
        }
    }
}
