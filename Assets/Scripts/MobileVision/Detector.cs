using com.google.android.gms.vision.face;
using Eq.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.google.android.gms.vision
{
    public abstract class Detector<T>
    {
        abstract public List<T> Detect(Frame frame);

        internal AndroidJavaObject mDetectorJO;

        internal AndroidJavaObject DetectorJO
        {
            get
            {
                return mDetectorJO;
            }
        }

        public void SetProcessor(Processor<T> processor)
        {
            mDetectorJO.Call("setProcessor", processor.ProcessorJO);
        }

        public Boolean IsOperational()
        {
            return mDetectorJO.Call<Boolean>("isOperational");
        }

        public void ReceiveFrame(Frame frame)
        {
            mDetectorJO.Call("receiveFrame", frame.FrameJO);
        }

        public void Release()
        {
            mDetectorJO.Call("release");
        }

        public Boolean SetFocus(int id)
        {
            return mDetectorJO.Call<Boolean>("setFocus", id);
        }

        abstract public class Processor<T>
        {
            internal AndroidJavaObject mProcessorJO;

            internal AndroidJavaObject ProcessorJO
            {
                get
                {
                    return mProcessorJO;
                }
            }

            abstract public void ReceiveDetections(Detections<T> detections);

            abstract public void Release();
        }

        abstract public class Detections<T>
        {
            internal AndroidJavaObject mDetectionsJO;

            public Detections()
            {
                mDetectionsJO = new AndroidJavaObject("com.google.android.gms.vision.Detector$Detections");
            }

            public Boolean DetectorIsOperational()
            {
                return mDetectionsJO.Call<Boolean>("detectorIsOperational");
            }

            abstract public List<T> GetDetectedItems();

            public Frame.Metadata GetFrameMetadata()
            {
                AndroidJavaObject frameMetadataJO = mDetectionsJO.Call<AndroidJavaObject>("getFrameMetadata");
                return new Frame.Metadata(frameMetadataJO);
            }

            internal AndroidJavaObject DetectionsJO
            {
                get
                {
                    return mDetectionsJO;
                }
            }
        }

        public class FaceDetections : Detections<Face>
        {
            public override List<Face> GetDetectedItems()
            {
                List<Face> ret = new List<Face>();
                AndroidJavaObject detectedItemSparseArray = mDetectionsJO.Call<AndroidJavaObject>("getDetectedItems");
                SparseArrayUtil<Face>.ExchangeToList(
                    delegate (AndroidJavaObject detectedItem)
                    {
                        Face tempRet = new Face(detectedItem);
                        return tempRet;
                    },
                    detectedItemSparseArray);
                return ret;
            }
        }
    }
}
