using Eq.Unity;
using System;
using System.Collections.Generic;
using Tango;
using UnityEngine;

public class LocationMainController : BaseALMainController
{
    private enum OriginalLocationStatus
    {
        Init, Measuring, Measured, Error,
    };

    private const double LongRadiusM = 6378137.000;     // a
    private const double ShortRadiusM = 6356752.314245;    // b
    private const double MajorEccentricityPow2 = 0.00669437999019758;   // 第一離心率e^2

    private OriginalLocationStatus mOriginalLocationStatus = OriginalLocationStatus.Init;
    private LocationPairPoseData mOriginLocationPairPoseData = null;
    private LocationPairPoseData mCurrentLocationPairPoseData = null;
    private Dictionary<TangoPoseData, InnerLocationInfo> mPoseLocationDic = new Dictionary<TangoPoseData, InnerLocationInfo>();
    private UnityEngine.UI.Button mNaviButton = null;
    private UnityEngine.UI.InputField mDestAddressText = null;
    private GameObject mCurrentNaviLine = null;

    public string mApiKey = null;
    public GameObject mNavigationLinePrefab;

    internal override bool StartTangoService()
    {
        mNaviButton = GameObject.Find("NavigationButton").GetComponent<UnityEngine.UI.Button>();
        mNaviButton.enabled = false;
        mDestAddressText = GameObject.Find("InputDestinationField").GetComponentInChildren<UnityEngine.UI.InputField>();

        mTangoApplication.Startup(GetMostRecentAreaDescription());
        return true;
    }

    internal override bool StopTangoService()
    {
        return true;
    }

    internal override void Update()
    {
        base.Update();

        switch (mOriginalLocationStatus)
        {
            case OriginalLocationStatus.Init:
                // GPS開始
                Input.location.Start();
                mOriginalLocationStatus = OriginalLocationStatus.Measuring;
                break;
            case OriginalLocationStatus.Measuring:
                if(Input.location.lastData.timestamp != 0)
                {
                    TangoPoseData tangoPoseData = new TangoPoseData();
                    TangoCoordinateFramePair pair = new TangoCoordinateFramePair();
                    pair.baseFrame = TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_START_OF_SERVICE;
                    pair.targetFrame = TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_DEVICE;

                    PoseProvider.GetPoseAtTime(tangoPoseData, 0, pair);
                    mOriginLocationPairPoseData = new LocationPairPoseData(tangoPoseData, Input.location.lastData);
                    mCurrentLocationPairPoseData = new LocationPairPoseData(tangoPoseData, Input.location.lastData);
                    mOriginalLocationStatus = OriginalLocationStatus.Measured;

                    mNaviButton.enabled = true;
                }
                break;
            case OriginalLocationStatus.Measured:
                break;
            case OriginalLocationStatus.Error:
                break;
        }
    }

    public override void OnTangoPoseAvailable(TangoPoseData poseData)
    {
        if(mOriginalLocationStatus == OriginalLocationStatus.Measured)
        {
            if (poseData.status_code == TangoEnums.TangoPoseStatusType.TANGO_POSE_VALID)
            {
                TangoEnums.TangoCoordinateFrameType baseFrame = poseData.framePair.baseFrame;
                TangoEnums.TangoCoordinateFrameType targetFrame = poseData.framePair.targetFrame;

                mLogger.CategoryLog(LogCategoryMethodTrace, "baseFrame = " + baseFrame + ", targetFrame = " + targetFrame);
                if (baseFrame == TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_START_OF_SERVICE && targetFrame == TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_DEVICE)
                {
                    mCurrentLocationPairPoseData.Update(poseData, Input.location.lastData);
                }
            }
        }
    }

    public void NavigationButtonClick()
    {
        PrepareNavigation();
    }

    internal void PrepareNavigation()
    {
        // 現在のナビゲーションラインを削除
        if(mCurrentNaviLine != null)
        {
            Destroy(mCurrentNaviLine);
            mCurrentNaviLine = null;
        }

        GoogleMapsAPI api = new GoogleMapsAPI(mApiKey);
        ResponseDirections direction = api.GetDirectionsFromCurrentPosition(GoogleMapsAPI.TransferMode.Walking, mDestAddressText.text);

        if(direction != null && direction.status.CompareTo("OK") == 0)
        {
            Route navigationRoute = direction.routes[0];
            List<Vector3> naviLineMilestoneList = new List<Vector3>();

            // 最初だけ現在位置を基準にする
            Vector3 startWorldPosition = mCurrentLocationPairPoseData.GetWorldPosition();
            LatLng startLocation = new LatLng();
            startLocation.lat = mCurrentLocationPairPoseData.GetLatitude();
            startLocation.lng = mCurrentLocationPairPoseData.GetLongitude();

            naviLineMilestoneList.Add(startWorldPosition);
            for (int i=0, size=navigationRoute.legs.Length; i<size; i++)
            {
                Leg naviLeg = navigationRoute.legs[i];
                LatLng endLocation = navigationRoute.legs[i].end_location;

                // 基準位置と中継地点の間の実距離(メートル)を算出 => World座標と同じ単位になる
                double locationDistanceM = GetDistanceM(startLocation.lat, startLocation.lng, endLocation.lat, endLocation.lng);

                // 基準位置の緯線を基準にした中継地点までの角度(radian)を算出
                float radian = GetRadianByLatitudeLine(startLocation.lat, startLocation.lng, endLocation.lat, endLocation.lng);

                // 中継地点までの距離と角度から中継地点のWorld座標を算出
                double destX = 0, destY = 0;
                GetWorldPositionFromLocation(locationDistanceM, radian, startWorldPosition.x, startWorldPosition.y, out destX, out destY);

                // 中継地点をマイルストーンとして保存
                Vector3 endWorldPosition = new Vector3((float)destX, 0, (float)destY);
                naviLineMilestoneList.Add(endWorldPosition);

                startWorldPosition = endWorldPosition;
                startLocation = endLocation;
            }

            // 基準地点から中継地点にかけて線を引く
            mCurrentNaviLine = Instantiate(mNavigationLinePrefab, startWorldPosition, Quaternion.identity);    // TODO
            mCurrentNaviLine.GetComponent<LineRenderer>().SetPositions(naviLineMilestoneList.ToArray());
            mCurrentNaviLine.SetActive(true);
        }
    }

    internal void GetWorldPositionFromLocation(double distanceM, float radian, double srcWpX, double srcWpY, out double destX, out double destY)
    {
        destX = srcWpX + (float)(distanceM * Math.Cos(radian));
        destY = srcWpY + (float)(distanceM * Math.Sin(radian));
    }

    internal double GetDistanceM(float lat1, float lng1, float lat2, float lng2)
    {
        float dx = lng2 - lng1;
        float dy = lat2 - lat1;
        double uy = (lat1 + lat2) / 2;
        double W = Math.Sqrt(1 - MajorEccentricityPow2 * Math.Pow(Math.Sin(uy), 2));
        double M = (LongRadiusM * (1 - MajorEccentricityPow2)) / Math.Pow(W, 3);
        double N = LongRadiusM / W;

        return Math.Sqrt(Math.Pow(Math.Pow(dy * M, 2), 2) + Math.Pow(Math.Pow(dx * N * Math.Cos(uy), 2), 2));
    }

    internal float GetRadianByLatitudeLine(double lat1, double lng1, double lat2, double lng2)
    {
        double lat12 = lat2 - lat1;
        double lng12 = lng2 - lng1;

        return (float)Math.Atan2(lat12, lng12);
    }

    private class InnerLocationInfo
    {
        internal float mAltitude;
        internal float mHorizontalAccuracy;
        internal float mLatitude;
        internal float mLongitude;
        internal double mTimestamp;
        internal float mVerticalAccuracy;

        public InnerLocationInfo(LocationInfo locationInfo)
        {
            Update(locationInfo);
        }

        public void Update(LocationInfo locationInfo)
        {
            mAltitude = locationInfo.altitude;
            mHorizontalAccuracy = locationInfo.horizontalAccuracy;
            mLatitude = locationInfo.latitude;
            mLongitude = locationInfo.longitude;
            mTimestamp = locationInfo.timestamp;
            mVerticalAccuracy = locationInfo.verticalAccuracy;
        }
    }

    private class LocationPairPoseData
    {
        private TangoPoseData mTangoPoseData;
        private InnerLocationInfo mInnerLocationInfo;

        public LocationPairPoseData(TangoPoseData poseData, LocationInfo locationInfo)
        {
            mTangoPoseData = poseData;
            mInnerLocationInfo = new InnerLocationInfo(locationInfo);
        }

        public void Update(TangoPoseData poseData, LocationInfo locationInfo)
        {
            mTangoPoseData = poseData;
            mInnerLocationInfo.Update(locationInfo);
        }

        public float GetLatitude()
        {
            return mInnerLocationInfo.mLatitude;
        }

        public float GetLongitude()
        {
            return mInnerLocationInfo.mLongitude;
        }

        public float GetAltitude()
        {
            return mInnerLocationInfo.mAltitude;
        }

        public double GetWorldPositionX()
        {
            return mTangoPoseData.translation.x;
        }

        public double GetWorldPositionY()
        {
            return mTangoPoseData.translation.y;
        }

        public double GetWorldPositionZ()
        {
            return mTangoPoseData.translation.y;
        }

        public Vector3 GetWorldPosition()
        {
            return mTangoPoseData.translation.ToVector3();
        }
    }
}
