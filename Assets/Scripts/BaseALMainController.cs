﻿using Eq.Unity;
using System;
using System.Collections.Generic;
using Tango;
using UnityEngine;

abstract public class BaseALMainController : BaseAndroidMainController, ITangoLifecycle, ITangoPose
{
    internal const float MinTranslateSize = 0.00001f;
    internal const int PermissionInit = 0;
    internal const int PermissionGranted = 1;
    internal const int PermissionDenied = -1;

    internal TangoApplication mTangoApplication;
    internal TangoPoseController mTangoPoseController;
    internal TangoEnums.TangoPoseStatusType mCurrentPoseStatus = TangoEnums.TangoPoseStatusType.TANGO_POSE_INVALID;
    internal int mPermissionResult = PermissionInit;
    internal PoseDataManager mPoseDataManager;
    internal PoseData mLastPoseData;
    internal List<PoseData> mPoseList;
    public GameObject mMotionTrackingCapsule;

    abstract internal bool StartTangoService();
    abstract internal bool StopTangoService();

    internal override void Start()
    {
        CategoryLog(LogCategoryMethodIn);
        base.Start();

        mTangoApplication = FindObjectOfType<TangoApplication>();
        if (mTangoApplication != null)
        {
            mTangoApplication.Register(this);
            mTangoApplication.RequestPermissions();
        }
        mTangoPoseController = FindObjectOfType<TangoPoseController>();
        CategoryLog(LogCategoryMethodOut);
    }

    internal override void OnEnable()
    {
        CategoryLog(LogCategoryMethodIn);
        base.OnEnable();
        CategoryLog(LogCategoryMethodOut);
    }

    internal override void OnDisable()
    {
        CategoryLog(LogCategoryMethodIn);
        base.OnDisable();

        if (mTangoApplication != null)
        {
            mTangoApplication.Unregister(this);
            StopTangoService();
        }

        CategoryLog(LogCategoryMethodOut);
    }

    public virtual void OnTangoPermissions(bool permissionsGranted)
    {
        CategoryLog(LogCategoryMethodIn);
        if (permissionsGranted)
        {
            mPermissionResult = PermissionGranted;
            CategoryLog(LogCategoryMethodTrace, "Permission = " + permissionsGranted + ", mCurrentPoseStatus = " + mCurrentPoseStatus);
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
        CategoryLog(LogCategoryMethodOut);
    }

    public void OnTangoServiceConnected()
    {
        CategoryLog(LogCategoryMethodIn);
        CategoryLog(LogCategoryMethodOut);
    }

    public void OnTangoServiceDisconnected()
    {
        CategoryLog(LogCategoryMethodIn);
        CategoryLog(LogCategoryMethodOut);
    }

    public void OnTangoPoseAvailable(TangoPoseData poseData)
    {
        if (poseData.status_code == TangoEnums.TangoPoseStatusType.TANGO_POSE_VALID)
        {
            TangoEnums.TangoCoordinateFrameType baseFrame = poseData.framePair.baseFrame;
            TangoEnums.TangoCoordinateFrameType targetFrame = poseData.framePair.targetFrame;

            CategoryLog(LogCategoryMethodTrace, "base = " + baseFrame + ", target = " + targetFrame);
            if (baseFrame == TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_AREA_DESCRIPTION &&
                 targetFrame == TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_START_OF_SERVICE)
            {
                // relocalizing(ADF)
                // UpdateTrackingGameObject(poseData);
            }
            else if (baseFrame == TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_AREA_DESCRIPTION &&
               targetFrame == TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_DEVICE)
            {
                // Area Learning
                AddTrackingGameObject(poseData);
            }
            else if (baseFrame == TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_START_OF_SERVICE &&
               targetFrame == TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_DEVICE)
            {
                if (this.GetType().FullName.CompareTo(Type.GetType("ALMainControllerForLoadExisting").FullName) != 0)  // 一時実装
                {
                    // Motion Tracking
                    AddTrackingGameObject(poseData);
                }
            }

        }
    }

    internal AreaDescription GetMostRecentAreaDescription()
    {
        CategoryLog(LogCategoryMethodIn);

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

        CategoryLog(LogCategoryMethodOut, mostRecent != null ? mostRecent.ToString() : "null");
        return mostRecent;
    }

    internal void AddTrackingGameObject(TangoPoseData poseData)
    {
        bool needAddPoint = false;
        DVector3 trackingPositionDV3 = poseData.translation;

        if (mLastPoseData != null)
        {
            DVector3 lastTranslation = new DVector3(mLastPoseData.translateX, mLastPoseData.translateY, mLastPoseData.translateZ);

            //CategoryLog(LogCategoryMethodTrace, "trackingPositionDV3 = " + trackingPositionDV3.ToString() + ", last pose data magnitude = " + lastTranslation.Magnitude + ", current pose data magnitude = " + trackingPositionDV3.Magnitude);
            if (Mathf.Abs((float)(lastTranslation.Magnitude - trackingPositionDV3.Magnitude)) > MinTranslateSize)
            {
                needAddPoint = true;
            }
        }
        else
        {
            needAddPoint = true;
        }

        CategoryLog(LogCategoryMethodTrace, "trackingPositionDV3 = " + trackingPositionDV3.ToString() + ", needAddPoint = " + needAddPoint);
        if (needAddPoint)
        {
            // Google Tango -> Unityへ座標変換(YZ -> ZY)＋少し見やすいようにYZ方向を補正
            Vector3 trackingPositionV3 = new Vector3((float)trackingPositionDV3.x, (float)trackingPositionDV3.z + 0.1f, (float)trackingPositionDV3.y + 0.1f);
            DVector4 trackingOrientationDV4 = poseData.orientation;
            Quaternion trackingOrientationQ = new Quaternion((float)trackingOrientationDV4.x, (float)trackingOrientationDV4.z, (float)trackingOrientationDV4.y, (float)trackingOrientationDV4.w);
            Instantiate(mMotionTrackingCapsule, trackingPositionV3, trackingOrientationQ).SetActive(true);

            DVector4 trackingOrientation = poseData.orientation;
            if (mLastPoseData == null)
            {
                mLastPoseData = new PoseData();
            }
            mLastPoseData.timestamp = poseData.timestamp;
            mLastPoseData.translateX = trackingPositionDV3.x;
            mLastPoseData.translateY = trackingPositionDV3.y;
            mLastPoseData.translateZ = trackingPositionDV3.z;
            mLastPoseData.orientateX = trackingOrientation.x;
            mLastPoseData.orientateY = trackingOrientation.y;
            mLastPoseData.orientateZ = trackingOrientation.z;
            mLastPoseData.orientateW = trackingOrientation.w;
        }
    }
}
