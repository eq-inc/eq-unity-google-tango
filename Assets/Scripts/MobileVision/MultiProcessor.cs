using UnityEngine;

namespace com.google.android.gms.vision
{
    public class MultiProcessor<T> : Detector<T>.Processor<T>
    {
        private MultiProcessor(AndroidJavaObject builderJO)
        {
            mProcessorJO = builderJO.Call<AndroidJavaObject>("build");
        }

        public override void ReceiveDetections(Detector<T>.Detections<T> detections)
        {
            mProcessorJO.Call("ReceiveDetections", detections.DetectionsJO);
        }

        public override void Release()
        {
            mProcessorJO.Call("release");
        }

        public class Builder<T>
        {
            private AndroidJavaObject mBuilderJO;

            public AndroidJavaObject BuilderJO
            {
                get
                {
                    return mBuilderJO;
                }
            }

            public Builder(Factory factory)
            {
                mBuilderJO = new AndroidJavaObject("com.google.android.gms.vision.MultiProcessor$Builder", factory);
            }

            public MultiProcessor<T> Build()
            {
                return new MultiProcessor<T>(mBuilderJO);
            }

            public Builder<T> SetMaxGapFrames(int maxGapFrames)
            {
                mBuilderJO.Call("setMaxGapFrames", maxGapFrames);
                return this;
            }
        }

        abstract public class Factory : AndroidJavaProxy
        {
            public Factory() : base("com.google.android.gms.vision.MultiProcessor$Factory")
            {
                // 処理なし
            }

            abstract public Tracker create(AndroidJavaObject itemJO);
        }
    }
}
