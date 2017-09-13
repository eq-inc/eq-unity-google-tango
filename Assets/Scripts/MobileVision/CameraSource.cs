using com.google.android.gms.vision.barcode;
using com.google.android.gms.vision.face;
using com.google.android.gms.vision.text;
using UnityEngine;

namespace com.google.android.gms.vision
{
    public class CameraSource
    {
        private AndroidJavaObject mCameraSourceJO;

        private CameraSource(AndroidJavaObject builderJO)
        {
            AndroidJavaObject activityJO = AndroidHelper.GetUnityActivity();
            mCameraSourceJO = builderJO.Call<AndroidJavaObject>("build");
        }

        public void Start()
        {
            mCameraSourceJO = mCameraSourceJO.Call<AndroidJavaObject>("start");
        }

        public void Stop()
        {
            mCameraSourceJO.Call("stop");
        }

        public void Release()
        {
            mCameraSourceJO.Call("release");
        }

        public class Builder
        {
            private AndroidJavaObject mBuilderJO;

            public Builder(Detector<Face> detector)
            {
                Initialize(detector.DetectorJO);
            }

            public Builder(Detector<Barcode> detector)
            {
                Initialize(detector.DetectorJO);
            }

            public Builder(Detector<TextBlock> detector)
            {
                Initialize(detector.DetectorJO);
            }

            private void Initialize(AndroidJavaObject detectorJO)
            {
                AndroidJavaObject activityJO = AndroidHelper.GetUnityActivity();
                mBuilderJO = new AndroidJavaObject("com.google.android.gms.vision.CameraSource$Builder", activityJO, detectorJO);
            }

            public CameraSource Build()
            {
                return new CameraSource(mBuilderJO);
            }
        }
    }
}
