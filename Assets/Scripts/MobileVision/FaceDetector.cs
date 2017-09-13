using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.google.android.gms.vision.face
{
    public class FaceDetector : Detector<Face>
    {
        //public static readonly int ACCURATE_MODE = new AndroidJavaClass("com.google.android.gms.vision.face.FaceDetector").Get<int>("ACCURATE_MODE");
        //public static readonly int ALL_CLASSIFICATIONS = new AndroidJavaClass("com.google.android.gms.vision.face.FaceDetector").Get<int>("ALL_CLASSIFICATIONS");
        //public static readonly int FAST_MODE = new AndroidJavaClass("com.google.android.gms.vision.face.FaceDetector").Get<int>("FAST_MODE");
        //public static readonly int NO_CLASSIFICATIONS = new AndroidJavaClass("com.google.android.gms.vision.face.FaceDetector").Get<int>("NO_CLASSIFICATIONS");
        //public static readonly int NO_LANDMARKS = new AndroidJavaClass("com.google.android.gms.vision.face.FaceDetector").Get<int>("NO_LANDMARKS");

        private FaceDetector(AndroidJavaObject builderJO)
        {
            mDetectorJO = builderJO.Call<AndroidJavaObject>("build");
        }

        public override List<Face> Detect(Frame frame)
        {
            List<Face> ret = new List<Face>();
            AndroidJavaObject detectedItemSparseArray = mDetectorJO.Call<AndroidJavaObject>("detect", frame.FrameJO);
            int arraySize = detectedItemSparseArray.Call<int>("size");

            for (int i = 0; i < arraySize; i++)
            {
                AndroidJavaObject detectedItem = detectedItemSparseArray.Call<AndroidJavaObject>("valueAt", i);
            }

            return ret;
        }

        public class Builder
        {
            private AndroidJavaObject mFaceDetectorBuilderJO;

            public Builder()
            {
                AndroidJavaObject activityJO = AndroidHelper.GetUnityActivity();
                mFaceDetectorBuilderJO = new AndroidJavaObject("com.google.android.gms.vision.face.FaceDetector$Builder", activityJO);

                //int ACCURATE_MODE = new AndroidJavaClass("com.google.android.gms.vision.face.FaceDetector").Get<int>("ACCURATE_MODE");
                //int ALL_CLASSIFICATIONS = new AndroidJavaClass("com.google.android.gms.vision.face.FaceDetector").Get<int>("ALL_CLASSIFICATIONS");
                //int FAST_MODE = new AndroidJavaClass("com.google.android.gms.vision.face.FaceDetector").Get<int>("FAST_MODE");
                //int NO_CLASSIFICATIONS = new AndroidJavaClass("com.google.android.gms.vision.face.FaceDetector").Get<int>("NO_CLASSIFICATIONS");
                //int NO_LANDMARKS = new AndroidJavaClass("com.google.android.gms.vision.face.FaceDetector").Get<int>("NO_LANDMARKS");
            }

        public Builder SetClassificationType(int classificationType)
            {
                mFaceDetectorBuilderJO = mFaceDetectorBuilderJO.Call<AndroidJavaObject>("setClassificationType", classificationType);
                return this;
            }

            public Builder SetLandmarkType(int landmarkType)
            {
                mFaceDetectorBuilderJO = mFaceDetectorBuilderJO.Call<AndroidJavaObject>("setLandmarkType", landmarkType);
                return this;
            }

            public Builder SetMinFaceSize(float proportionalMinFaceSize)
            {
                mFaceDetectorBuilderJO = mFaceDetectorBuilderJO.Call<AndroidJavaObject>("setMinFaceSize", proportionalMinFaceSize);
                return this;
            }

            public Builder SetMode(int mode)
            {
                mFaceDetectorBuilderJO.Call("setMode", mode);
                return this;
            }

            public Builder SetProminentFaceOnly(Boolean prominentFaceOnly)
            {
                mFaceDetectorBuilderJO = mFaceDetectorBuilderJO.Call<AndroidJavaObject>("setProminentFaceOnly", prominentFaceOnly);
                return this;
            }

            public Builder SetTrackingEnabled(Boolean trackingEnabled)
            {
                mFaceDetectorBuilderJO = mFaceDetectorBuilderJO.Call<AndroidJavaObject>("setTrackingEnabled", trackingEnabled);
                return this;
            }

            public FaceDetector Build()
            {
                return new FaceDetector(mFaceDetectorBuilderJO);
            }
        }
    }
}
