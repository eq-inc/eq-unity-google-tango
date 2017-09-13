using UnityEngine;

namespace com.google.android.gms.vision
{
    public class Frame
    {
        private AndroidJavaObject mFrameJO;

        internal Frame(AndroidJavaObject builderJO)
        {
            mFrameJO = builderJO.Call<AndroidJavaObject>("build");
        }

        internal AndroidJavaObject FrameJO
        {
            get
            {
                return mFrameJO;
            }
        }

        public class Builder
        {
            private AndroidJavaObject mBuilderJO;

            public Builder()
            {
                AndroidJavaObject activityJO = AndroidHelper.GetUnityActivity();
                mBuilderJO = new AndroidJavaObject("com.google.android.gms.vision.Frame$Builder");
            }

            public Frame Build()
            {
                return new Frame(mBuilderJO);
            }
        }

        public class Metadata
        {
            private AndroidJavaObject mMetadataJO;

            public Metadata()
            {
                mMetadataJO = new AndroidJavaObject("com.google.android.gms.vision.Frame$Metadata");
            }

            public Metadata(AndroidJavaObject metadataJO)
            {
                mMetadataJO = metadataJO;
            }

            internal AndroidJavaObject MetadataJO
            {
                get
                {
                    return mMetadataJO;
                }
            }
        }
    }
}
