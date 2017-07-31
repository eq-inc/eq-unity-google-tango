using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tango;

public class CloudAreaDescriptionFile : BaseALMainController
{
    internal override bool StartTangoService()
    {
        mTangoApplication.Startup(GetMostRecentAreaDescription());
        return true;
    }

    internal override bool StopTangoService()
    {
        return true;
    }

    public override void OnTangoPoseAvailable(TangoPoseData poseData)
    {
        bool needCallBaseMethod = true;

        if(poseData.status_code == TangoEnums.TangoPoseStatusType.TANGO_POSE_VALID)
        {
            TangoEnums.TangoCoordinateFrameType baseFrame = poseData.framePair.baseFrame;
            TangoEnums.TangoCoordinateFrameType targetFrame = poseData.framePair.targetFrame;

            mLogger.CategoryLog(LogCategoryMethodTrace, "baseFrame = " + baseFrame + ", targetFrame = " +  targetFrame);
            if(baseFrame == TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_GLOBAL_WGS84 && targetFrame == TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_DEVICE)
            {
                AddTrackingGameObject(poseData, mMotionTrackingCapsule);
                needCallBaseMethod = false;
            }
            else if(baseFrame == TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_START_OF_SERVICE && targetFrame == TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_DEVICE)
            {
                TangoCoordinateFramePair pair = new TangoCoordinateFramePair();
                TangoPoseData tempPoseData = new TangoPoseData();

                pair.baseFrame = TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_GLOBAL_WGS84;
                pair.targetFrame = TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_DEVICE;
                PoseProvider.GetPoseAtTime(tempPoseData, 0, pair);

                if(tempPoseData.status_code == TangoEnums.TangoPoseStatusType.TANGO_POSE_VALID)
                {
                    mLogger.CategoryLog(LogCategoryMethodTrace, "poseData.translation = " + poseData.translation + ", poseData.orientation = " + poseData.orientation);
                    mLogger.CategoryLog(LogCategoryMethodTrace, "tempPoseData.translation = " + tempPoseData.translation + ", tempPoseData.orientation = " + tempPoseData.orientation);
                }
                else
                {
                    TangoEnums.TangoCoordinateFrameType[] targetFrames = new TangoEnums.TangoCoordinateFrameType[] {
                        TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_GLOBAL_WGS84,
                        TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_AREA_DESCRIPTION,
                        TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_START_OF_SERVICE,
                        TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_PREVIOUS_DEVICE_POSE,
                        TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_DEVICE,
                        TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_IMU,
                        TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_DISPLAY,
                        TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_CAMERA_COLOR,
                        TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_CAMERA_DEPTH,
                        TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_CAMERA_FISHEYE,
                        TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_INVALID,
                        TangoEnums.TangoCoordinateFrameType.TANGO_MAX_COORDINATE_FRAME_TYPE,
                    };

                    mLogger.CategoryLog(LogCategoryMethodTrace, "tempPoseData is not TANGO_POSE_VALID");
                    pair.baseFrame = TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_GLOBAL_WGS84;
                    for (int i=0, size=targetFrames.Length; i<size; i++)
                    {
                        pair.targetFrame = targetFrames[i];
                        mLogger.CategoryLog(LogCategoryMethodTrace, "call PoseProvider.GetPoseAtTime(S)");
                        PoseProvider.GetPoseAtTime(tempPoseData, 0, pair);
                        mLogger.CategoryLog(LogCategoryMethodTrace, "call PoseProvider.GetPoseAtTime(E)");
                        if (tempPoseData.status_code == TangoEnums.TangoPoseStatusType.TANGO_POSE_VALID)
                        {
                            mLogger.CategoryLog(LogCategoryMethodTrace, "baseFrame = " + pair.baseFrame + ", targetFrame = " + pair.targetFrame);
                            mLogger.CategoryLog(LogCategoryMethodTrace, "tempPoseData.translation = " + tempPoseData.translation + ", tempPoseData.orientation = " + tempPoseData.orientation);
                        }
                    }

                    DMatrix4x4 globalMatrix;
                    if(mTangoApplication.GetGlobalTLocal(out globalMatrix))
                    {
                        if (globalMatrix.Equals(DMatrix4x4.Identity))
                        {
                            mLogger.CategoryLog(LogCategoryMethodTrace, "Is TangoApplication.m_enableCloudADF false?: " + mTangoApplication.m_enableCloudADF);
                        }
                        else
                        {
                            mLogger.CategoryLog(LogCategoryMethodTrace, "get globalMatrix");
                        }
                    }
                    else
                    {
                        mLogger.CategoryLog(LogCategoryMethodTrace, "TangoApplication.GetGlobalTLocal returns false");
                    }
                }
            }
        }

        if (needCallBaseMethod)
        {
            base.OnTangoPoseAvailable(poseData);
        }
    }
}
