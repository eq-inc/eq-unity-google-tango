using System.Collections;
using System.Collections.Generic;
using Tango;
using UnityEngine;

public class MTMainController : BaseAndroidMainController
{
    private const int MaxMotionTrackingData = int.MaxValue;
    public GameObject mMotionTrackingCapsule;
    internal TangoCoordinateFramePair mPosePair;
    internal Queue<InternalPoseData> mPoseList = new Queue<InternalPoseData>();

    internal override void OnEnable()
    {
        CategoryLog(LogCategoryMethodIn);
        base.OnEnable();

        SetScreenTimeout(BaseAndroidMainController.NeverSleep);
        SetScreenOrientation(ScreenOrientation.Portrait);

        CategoryLog(LogCategoryMethodOut);
    }

    internal override void Start()
    {
        CategoryLog(LogCategoryMethodIn);
        base.Start();

        mPosePair = new TangoCoordinateFramePair();
        mPosePair.baseFrame = TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_START_OF_SERVICE;
        mPosePair.targetFrame = TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_DEVICE;

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
        PoseProvider.GetPoseAtTime(poseData.mPoseData, 0.0, mPosePair);

        int size = mPoseList.Count;
        DVector3 trackingPositionDV3 = poseData.mPoseData.translation;
        CategoryLog(LogCategoryMethodTrace, "trackingPositionDV3 = " + trackingPositionDV3.ToString());

        // Google Tango -> Unityへ座標変換(YZ -> ZY)＋少し見やすいようにYZ方向を補正
        Vector3 trackingPositionV3 = new Vector3((float)trackingPositionDV3.x, (float)trackingPositionDV3.z + 0.1f, (float)trackingPositionDV3.y + 0.1f);
        DVector4 trackingOrientationDV4 = poseData.mPoseData.orientation;
        Quaternion trackingOrientationQ = new Quaternion((float)trackingOrientationDV4.x, (float)trackingOrientationDV4.z, (float)trackingOrientationDV4.y, (float)trackingOrientationDV4.w);
        poseData.mPoseObject = Instantiate(mMotionTrackingCapsule, trackingPositionV3, trackingOrientationQ);
        poseData.mPoseObject.SetActive(true);

        mPoseList.Enqueue(poseData);
    }

    internal class InternalPoseData
    {
        internal TangoPoseData mPoseData = new TangoPoseData();
        internal GameObject mPoseObject = null;
    }
}
