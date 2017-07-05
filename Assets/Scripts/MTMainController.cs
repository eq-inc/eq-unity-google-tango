using System.Collections.Generic;
using Tango;
using UnityEngine;

public class MTMainController : BaseAndroidMainController, ITangoPose
{
    private const float MinTranslateSize = 1.0f;
    private const int MaxMotionTrackingData = int.MaxValue;
    public GameObject mMotionTrackingCapsule;
    internal TangoCoordinateFramePair mFramePair;
    internal Queue<InternalPoseData> mPoseList = new Queue<InternalPoseData>();
    internal InternalPoseData mLastQueuedPoseData;

    internal override void OnEnable()
    {
        CategoryLog(LogCategoryMethodIn);
        base.OnEnable();

        SetScreenTimeout(BaseAndroidMainController.NeverSleep);
        SetScreenOrientation(ScreenOrientation.Portrait);

        // 今はコールバックが不要なので、コメントアウト
        //FindObjectOfType<TangoApplication>().Register(this);
        //     or
        //PoseListener.RegisterTangoPoseAvailable(new OnTangoPoseAvailableEventHandler(OnTangoPoseAvailable));

        CategoryLog(LogCategoryMethodOut);
    }

    internal override void OnDisable()
    {
        CategoryLog(LogCategoryMethodIn);
        base.OnDisable();

        // 今はコールバックが不要なので、コメントアウト
        //FindObjectOfType<TangoApplication>().Unregister(this);
        //     or
        //PoseListener.UnregisterTangoPoseAvailable(new OnTangoPoseAvailableEventHandler(OnTangoPoseAvailable));

        CategoryLog(LogCategoryMethodOut);
    }

    internal override void Start()
    {
        CategoryLog(LogCategoryMethodIn);
        base.Start();

        mFramePair = new TangoCoordinateFramePair();
        mFramePair.baseFrame = TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_START_OF_SERVICE;
        mFramePair.targetFrame = TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_DEVICE;

        CategoryLog(LogCategoryMethodOut);
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
        CategoryLog(LogCategoryMethodTrace, "trackingPositionDV3 = " + trackingPositionDV3.ToString());

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
            // Google Tango -> Unityへ座標変換(YZ -> ZY)＋少し見やすいようにYZ方向を補正
            Vector3 trackingPositionV3 = new Vector3((float)trackingPositionDV3.x, (float)trackingPositionDV3.z + 0.1f, (float)trackingPositionDV3.y + 0.1f);
            DVector4 trackingOrientationDV4 = poseData.mPoseData.orientation;
            Quaternion trackingOrientationQ = new Quaternion((float)trackingOrientationDV4.x, (float)trackingOrientationDV4.z, (float)trackingOrientationDV4.y, (float)trackingOrientationDV4.w);
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
        CategoryLog(LogCategoryMethodIn, poseData);
        CategoryLog(LogCategoryMethodOut);
    }

    internal class InternalPoseData
    {
        internal TangoPoseData mPoseData = new TangoPoseData();
        internal GameObject mPoseObject = null;
    }
}
