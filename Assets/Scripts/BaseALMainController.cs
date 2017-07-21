using Eq.Unity;
using System;
using System.Collections.Generic;
using System.IO;
using Tango;
using UnityEngine;

abstract public class BaseALMainController : BaseAndroidMainController, ITangoLifecycle, ITangoPose
{
    internal const float MinTranslateSize = 1f;
    internal const int PermissionInit = 0;
    internal const int PermissionGranted = 1;
    internal const int PermissionDenied = -1;
    internal const String DefaultPoseDataType = PoseDataManager.TypeAreaLearning;

    internal TangoApplication mTangoApplication;
    internal TangoPoseController mTangoPoseController;
    internal TangoPointCloud mTangoPointCloud;
    internal TangoEnums.TangoPoseStatusType mCurrentPoseStatus = TangoEnums.TangoPoseStatusType.TANGO_POSE_INVALID;
    internal int mPermissionResult = PermissionInit;
    internal PoseDataManager mPoseDataManager;
    internal PoseData mLastPoseData;
    internal List<PoseData> mPoseList;
    internal bool mLearningArea = false;
    public GameObject mMotionTrackingCapsule;
    public GameObject mLoadedMotionTrackingCapsule;

    abstract internal bool StartTangoService();
    abstract internal bool StopTangoService();

    internal override void Start()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        base.Start();

        mTangoApplication = FindObjectOfType<TangoApplication>();
        if (mTangoApplication != null)
        {
            mTangoApplication.Register(this);
            mTangoApplication.RequestPermissions();
        }
        mTangoPoseController = FindObjectOfType<TangoPoseController>();
        mTangoPointCloud = FindObjectOfType<TangoPointCloud>();
        if(mTangoPointCloud != null)
        {
            mTangoPointCloud.FindFloor();
        }
        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    internal override void OnEnable()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        base.OnEnable();
        SetScreenTimeout(BaseAndroidMainController.NeverSleep);
        SetScreenOrientation(ScreenOrientation.Portrait);
        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    internal override void OnDisable()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        base.OnDisable();

        if (mTangoApplication != null)
        {
            mTangoApplication.Unregister(this);
        }

        if(mPoseDataManager != null)
        {
            mPoseDataManager.Remove(DefaultPoseDataType);
        }

        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    internal override bool Back()
    {
        StopTangoService();
        return true;
    }

    public virtual void OnTangoPermissions(bool permissionsGranted)
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        if (permissionsGranted)
        {
            mPermissionResult = PermissionGranted;
            mLogger.CategoryLog(LogCategoryMethodTrace, "Permission = " + permissionsGranted + ", mCurrentPoseStatus = " + mCurrentPoseStatus);
            if (!StartTangoService())
            {
                this.Back();
            }
        }
        else
        {
            Debug.LogError("permission denied");
            mPermissionResult = PermissionDenied;
            this.Back();
        }
        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    public void OnTangoServiceConnected()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    public void OnTangoServiceDisconnected()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    public void OnTangoPoseAvailable(TangoPoseData poseData)
    {
        if (poseData.status_code == TangoEnums.TangoPoseStatusType.TANGO_POSE_VALID)
        {
            TangoEnums.TangoCoordinateFrameType baseFrame = poseData.framePair.baseFrame;
            TangoEnums.TangoCoordinateFrameType targetFrame = poseData.framePair.targetFrame;

            mLogger.CategoryLog(LogCategoryMethodTrace, "base = " + baseFrame + ", target = " + targetFrame);
            if (baseFrame == TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_AREA_DESCRIPTION &&
               targetFrame == TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_DEVICE) 
            {
                // Area Learning
                PoseData addPoseData = AddTrackingGameObject(poseData, mMotionTrackingCapsule);
                if (addPoseData != null)
                {
                    mPoseDataManager.Add(PoseDataManager.TypeAreaLearning, addPoseData.Clone());
                }
            }
            else if (baseFrame == TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_AREA_DESCRIPTION &&
                 targetFrame == TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_START_OF_SERVICE)
            {
                // relocalizing(ADF)
                UpdateTrackingGameObject();
            }
        }
    }

    internal AreaDescription GetMostRecentAreaDescription()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);

        AreaDescription mostRecent = null;

        try
        {
            AreaDescription[] list = AreaDescription.GetList();
            AreaDescription.Metadata mostRecentMetadata = null;

            if (list.Length > 0)
            {
                mostRecent = list[0];
                mostRecentMetadata = mostRecent.GetMetadata();
                foreach (AreaDescription areaDescription in list)
                {
                    AreaDescription.Metadata metadata = areaDescription.GetMetadata();
                    mLogger.CategoryLog(LogCategoryMethodTrace, "Area Description Name = " + areaDescription.GetMetadata().m_name);

                    if (metadata.m_dateTime > mostRecentMetadata.m_dateTime)
                    {
                        mostRecent = areaDescription;
                        mostRecentMetadata = metadata;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        mLogger.CategoryLog(LogCategoryMethodOut, mostRecent != null ? mostRecent.ToString() : "null");
        return mostRecent;
    }

    internal PoseData GetPoseDataFromTangoPoseData(TangoPoseData fromTangoPoseData, PoseData toPoseData, bool setToFloor)
    {
        PoseData poseData = toPoseData;
        if(poseData == null)
        {
            poseData = new PoseData();
        }

        poseData.timestamp = fromTangoPoseData.timestamp;
        poseData.SetTranslation(fromTangoPoseData.translation.ToVector3());
        poseData.SetOrientation(fromTangoPoseData.orientation);

        if (setToFloor && mTangoPointCloud != null && mTangoPointCloud.m_floorFound)
        {
            // Google Tangoの座標系はRight-Hand系なのに、TangoPointCloud.m_floorPlaneYはLeft-Hand系のような名称になっている。
            // そのため変換時はRight-Hand系の高さ方向のZ軸に設定する
            poseData.translateZ = mTangoPointCloud.m_floorPlaneY;
        }

        return poseData;
    }

    internal virtual PoseData AddTrackingGameObject(TangoPoseData poseData, GameObject itemPrefab)
    {
        mLogger.CategoryLog(LogCategoryMethodIn);

        bool needAddPoint = false;
        PoseData addPoseData = null;
        DVector3 trackingPositionDV3 = poseData.translation;

        if (mLastPoseData != null)
        {
            DVector3 lastTranslation = new DVector3(mLastPoseData.translateX, mLastPoseData.translateY, mLastPoseData.translateZ);

            //mLogger.CategoryLog(LogCategoryMethodTrace, "trackingPositionDV3 = " + trackingPositionDV3.ToString() + ", last pose data magnitude = " + lastTranslation.Magnitude + ", current pose data magnitude = " + trackingPositionDV3.Magnitude);
            if (Mathf.Abs((float)(lastTranslation.Magnitude - trackingPositionDV3.Magnitude)) > MinTranslateSize)
            {
                needAddPoint = true;
            }
        }
        else
        {
            needAddPoint = true;
        }

        mLogger.CategoryLog(LogCategoryMethodTrace, "trackingPositionDV3 = " + trackingPositionDV3.ToString() + ", needAddPoint = " + needAddPoint);
        if (needAddPoint)
        {
            // TangoPoseDataからPoseDataへの単純変換
            mLastPoseData = GetPoseDataFromTangoPoseData(poseData, mLastPoseData, false);

            // Google Tango -> Unityへ座標変換(YZ -> ZY)
            Vector3 trackingPositionV3 = new Vector3();
            Quaternion trackingOrientationQ = new Quaternion();
            TangoSupport.TangoPoseToWorldTransform(poseData, out trackingPositionV3, out trackingOrientationQ);

            // GameObjectの生成
            GameObject addGameObject = Instantiate(itemPrefab, trackingPositionV3, trackingOrientationQ);
            addGameObject.SetActive(true);
            mLastPoseData.SetTargetGameObject(addGameObject);

            addPoseData = mLastPoseData;
        }

        mLogger.CategoryLog(LogCategoryMethodOut);
        return addPoseData;
    }

    internal void UpdateTrackingGameObject()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);

        List<PoseData>.Enumerator enumerator = mPoseDataManager.GetEnumerator(DefaultPoseDataType);
        TangoCoordinateFramePair pair = new TangoCoordinateFramePair();
        pair.baseFrame = TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_AREA_DESCRIPTION;
        pair.targetFrame = TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_DEVICE;
        TangoPoseData tangoPoseData = new TangoPoseData();

        while (enumerator.MoveNext())
        {
            PoseData poseData = enumerator.Current;
            PoseProvider.GetPoseAtTime(tangoPoseData, poseData.timestamp, pair);

            if(tangoPoseData.status_code == TangoEnums.TangoPoseStatusType.TANGO_POSE_VALID)
            {
                poseData = GetPoseDataFromTangoPoseData(tangoPoseData, poseData, false);

                // Google Tango -> Unityへ座標変換(YZ -> ZY)
                Vector3 trackingPositionV3 = new Vector3();
                Quaternion trackingOrientationQ = new Quaternion();
                TangoSupport.TangoPoseToWorldTransform(tangoPoseData, out trackingPositionV3, out trackingOrientationQ);

                GameObject targetGameObject = poseData.getTargetGameObject();
                if(targetGameObject != null)
                {
                    mLogger.CategoryLog(LogCategoryMethodTrace, "update position and rotation");
                    targetGameObject.transform.position = poseData.GetTranslation();
                    targetGameObject.transform.rotation = poseData.GetOrientation();
                }
            }
            else
            {
                mLogger.CategoryLog(LogCategoryMethodTrace, "tango pose data is not valid");
            }
        }

        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    internal class LoadPoseDataCallback : CallbackAsncTask<string, int, List<PoseData>>.IResultCallback
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

            if (ret != null && ret.Count > 0)
            {
                TangoPoseData tangoPoseData = new TangoPoseData();
                foreach (PoseData poseData in ret)
                {
                    tangoPoseData.timestamp = poseData.timestamp;
                    tangoPoseData.translation = new DVector3(poseData.translateX, poseData.translateY, poseData.translateZ);
                    tangoPoseData.orientation = new DVector4(poseData.orientateX, poseData.orientateY, poseData.orientateZ, poseData.orientateW);

                    mController.AddTrackingGameObject(tangoPoseData, mController.mLoadedMotionTrackingCapsule);
                }
            }

            return ret;
        }
    }

    internal class SavePoseDataCallback : CallbackAsncTask<string, int, bool>.IResultCallback
    {
        private BaseALMainController mController;

        public SavePoseDataCallback(BaseALMainController controller)
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
            // シーンを終了
            mController.PopCurrentScene();
        }

        bool CallbackAsncTask<string, int, bool>.ICallback.DoInBackground(params string[] parameters)
        {
            mController.mLogger.CategoryLog(LogCategoryMethodIn);

            bool ret = false;
            AreaDescription saveALRet = AreaDescription.SaveCurrent();

            if (saveALRet != null)
            {
                AreaDescription.Metadata metaData = saveALRet.GetMetadata();
                metaData.m_name = "test";
                saveALRet.SaveMetadata(metaData);

                mController.mPoseDataManager.SetUuid(saveALRet.m_uuid);
                ret = mController.mPoseDataManager.Save(PoseDataManager.TypeAreaLearning);
            }

            mController.mLogger.CategoryLog(LogCategoryMethodOut);

            return ret;
        }
    }
}
