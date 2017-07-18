using System.Collections.Generic;
using Tango;
using UnityEngine;

public class MTMainController : BaseAndroidMainController, ITangoPose
{
    private const float MinTranslateSize = 1.0f;
    private const int MaxMotionTrackingData = int.MaxValue;
    public GameObject mMotionTrackingCapsule;
    internal TangoPointCloud mTangoPointCloud = null;
    internal TangoCoordinateFramePair mFramePair;
    internal Queue<InternalPoseData> mPoseList = new Queue<InternalPoseData>();
    internal InternalPoseData mLastQueuedPoseData;

    internal override void OnEnable()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        base.OnEnable();

        SetScreenTimeout(BaseAndroidMainController.NeverSleep);
        SetScreenOrientation(ScreenOrientation.Portrait);

        // 今はコールバックが不要なので、コメントアウト
        //FindObjectOfType<TangoApplication>().Register(this);
        //     or
        //PoseListener.RegisterTangoPoseAvailable(new OnTangoPoseAvailableEventHandler(OnTangoPoseAvailable));

        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    internal override void OnDisable()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        base.OnDisable();

        // 今はコールバックが不要なので、コメントアウト
        //FindObjectOfType<TangoApplication>().Unregister(this);
        //     or
        //PoseListener.UnregisterTangoPoseAvailable(new OnTangoPoseAvailableEventHandler(OnTangoPoseAvailable));

        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    internal override void Start()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        base.Start();

        mFramePair = new TangoCoordinateFramePair();
        mFramePair.baseFrame = TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_START_OF_SERVICE;
        mFramePair.targetFrame = TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_DEVICE;

        mTangoPointCloud = FindObjectOfType<TangoPointCloud>();
        if(mTangoPointCloud != null)
        {
            // Floorの検索を開始
            mTangoPointCloud.FindFloor();
        }

        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    internal override void Update()
    {
        base.Update();

        AddTrackingGameObject();

        if (mPoseList.Count > MaxMotionTrackingData)
        {
            InternalPoseData removePoseData = mPoseList.Dequeue();
            if (removePoseData != null)
            {
                Destroy(removePoseData.mPoseObject);
            }
        }
    }

    internal virtual void AddTrackingGameObject()
    {
        InternalPoseData poseData = new InternalPoseData();
        PoseProvider.GetPoseAtTime(poseData.mPoseData, 0.0, mFramePair);

        int size = mPoseList.Count;
        DVector3 trackingPositionDV3 = poseData.mPoseData.translation;
        mLogger.CategoryLog(LogCategoryMethodTrace, "trackingPositionDV3 = " + trackingPositionDV3.ToString());

        bool needAddPoint = false;
        if (mLastQueuedPoseData == null)
        {
            needAddPoint = true;
        }
        else
        {
            if (Mathf.Abs((float)(mLastQueuedPoseData.mPoseData.translation.Magnitude - poseData.mPoseData.translation.Magnitude)) > MinTranslateSize)
            {
                needAddPoint = true;
            }
        }

        if (needAddPoint)
        {
            // Google Tango -> Unityへ座標変換(YZ -> ZY)
            Vector3 trackingPositionV3 = new Vector3();
            Quaternion trackingOrientationQ = new Quaternion();
            TangoSupport.TangoPoseToWorldTransform(poseData.mPoseData, out trackingPositionV3, out trackingOrientationQ);

            float positionY = trackingPositionV3.y;
            if(mTangoPointCloud != null && mTangoPointCloud.m_floorFound)
            {
                positionY = mTangoPointCloud.m_floorPlaneY;
            }

            trackingPositionV3.y = positionY;
            poseData.mPoseObject = Instantiate(mMotionTrackingCapsule, trackingPositionV3, trackingOrientationQ);
            poseData.mPoseObject.SetActive(true);

            mPoseList.Enqueue(poseData);
            mLastQueuedPoseData = poseData;
        }
    }

    public void OnTangoPoseAvailable(TangoPoseData poseData)
    {
        // ITangoPoseを実装した上で、TangoApplication.Registerをコールすることでコールバックされるようになる。
        // またはPoseListener.RegisterTangoPoseAvailable(new OnTangoPoseAvailableEventHandler(OnTangoPoseAvailable))することでコールバックされるようになる。
        mLogger.CategoryLog(LogCategoryMethodIn, poseData);
        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    internal class InternalPoseData
    {
        internal TangoPoseData mPoseData = new TangoPoseData();
        internal GameObject mPoseObject = null;
    }
}
