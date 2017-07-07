using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using Tango;

public class ALMainControllerForLearning : BaseALMainController
{
    private bool mLearningArea = false;

    delegate AreaDescription SaveAreaDescription();
    private IEnumerator SaveCurrentAreaDescription()
    {
        yield return InnerSaveCurrentAreaDescription();
    }

    internal AreaDescription InnerSaveCurrentAreaDescription()
    {
        CategoryLog(LogCategoryMethodIn);
        AreaDescription ret = AreaDescription.SaveCurrent();
        mTangoApplication.Shutdown();
        CategoryLog(LogCategoryMethodOut);

        return ret;
    }

    internal override bool StartTangoService()
    {
        CategoryLog(LogCategoryMethodIn);
        bool ret = true;

        if (!mLearningArea)
        {
            AreaDescription mostRecentAreaDescription = GetMostRecentAreaDescription();

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

            new CallbackAsncTask<string, int, List<PoseData>>(new LoadPoseDataCallback(this)).Execute();
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
            StartCoroutine(SaveCurrentAreaDescription());
            mLearningArea = false;
        }
        else
        {
            ret = false;
        }

        CategoryLog(LogCategoryMethodOut, "ret = " + ret);
        return ret;
    }

    internal void LoadPoseData()
    {
        mPoseList = mPoseDataManager.Load(PoseDataManager.TypeAreaLearning);
    }

    private class LoadPoseDataCallback : CallbackAsncTask<string, int, List<PoseData>>.IResultCallback
    {
        private BaseALMainController mController;

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
            // 処理なし
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
            List<PoseData> ret = mController.mPoseDataManager.Load(PoseDataManager.TypeAreaLearning);

            if(ret != null && ret.Count > 0)
            {
                TangoPoseData tangoPoseData = new TangoPoseData();
                TangoCoordinateFramePair pair = new TangoCoordinateFramePair();
                pair.baseFrame = TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_AREA_DESCRIPTION;
                pair.targetFrame = TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_DEVICE;

                foreach (PoseData poseData in ret)
                {
                    PoseProvider.GetPoseAtTime(tangoPoseData, poseData.timestamp, pair);
                    if((tangoPoseData.status_code == TangoEnums.TangoPoseStatusType.TANGO_POSE_VALID) && (poseData.timestamp == tangoPoseData.timestamp))
                    {
                        mController.AddTrackingGameObject(tangoPoseData);
                    }
                }
            }

            return ret;
        }
    }

    private class SavePoseDataCallback : CallbackAsncTask<string, int, bool>.IResultCallback
    {
        private ALMainControllerForLearning mController;

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
            // 処理なし
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
            return mController.mPoseDataManager.Save(PoseDataManager.TypeAreaLearning);
        }
    }
}
