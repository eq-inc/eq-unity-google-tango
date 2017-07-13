using Eq.Unity;
using System;
using System.Collections.Generic;
using System.IO;
using Tango;
using UnityEngine;

public class ALMainControllerForLoadExisting : BaseALMainController
{
    internal override bool StartTangoService()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        bool ret = true;
        AreaDescription mostRecentAreaDescription = GetMostRecentAreaDescription();

        if (mostRecentAreaDescription != null)
        {
            mPoseDataManager = PoseDataManager.GetInstance(mostRecentAreaDescription.m_uuid);
            mTangoApplication.Startup(mostRecentAreaDescription);
            new CallbackAsncTask<string, int, List<PoseData>>(new LoadPoseDataCallback(this)).Execute();
        }
        else
        {
            Debug.LogError("none area description file");
            ret = false;
        }

        mPoseDataManager.EnableDebugLog((mLogger.GetOutputLogCategory() & LogCategoryMethodTrace) == LogCategoryMethodTrace);
        mLogger.CategoryLog(LogCategoryMethodOut, "ret = " + ret);
        return ret;
    }

    internal override bool StopTangoService()
    {
        mTangoApplication.Shutdown();

        return true;
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
            mRootDataPath = Application.persistentDataPath;
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

            if (ret != null && ret.Count > 0)
            {
                TangoPoseData tangoPoseData = new TangoPoseData();
                TangoCoordinateFramePair pair = new TangoCoordinateFramePair();
                pair.baseFrame = TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_AREA_DESCRIPTION;
                pair.targetFrame = TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_DEVICE;

                foreach (PoseData poseData in ret)
                {
                    PoseProvider.GetPoseAtTime(tangoPoseData, poseData.timestamp, pair);
                    if ((tangoPoseData.status_code == TangoEnums.TangoPoseStatusType.TANGO_POSE_VALID) && (poseData.timestamp == tangoPoseData.timestamp))
                    {
                        mController.AddTrackingGameObject(tangoPoseData);
                    }
                }
            }

            return ret;
        }
    }
}
