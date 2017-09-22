using com.google.android.gms.vision.barcode;
using com.google.android.gms.vision.face;
using com.google.android.gms.vision.text;
using Eq.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jp.eq_inc.mobilevisionwrapper
{
    public class Accessor : BaseAndroidJavaObjectWrapper
    {
        public delegate void OnDetectedCallback<T>(List<T> detectedItemList);

        private LogController mLogger;
        private OnDetectedCallback<Face> mOnFaceDetectedDelegater;
        private OnDetectedCallback<Barcode> mOnBarcodeDetectedDelegater;
        private OnDetectedCallback<TextBlock> mOnTextRecognizedDelegater;
        private List<Face> mFaceDetectedItemList;
        private List<Barcode> mBarcodeDetectedItemList;
        private List<TextBlock> mTextRecognizedItemList;
        private CommonRoutine mRoutine;
        private Boolean mDetecting = false;

        public Accessor(CommonRoutine routine, LogController logger)
        {
            mRoutine = routine;
            mLogger = logger;

            AndroidJavaClass accessor = new AndroidJavaClass("jp.eq_inc.mobilevisionwrapper.Accessor");
            mAndroidJO = accessor.CallStatic<AndroidJavaObject>("createAccessorForUnity", AndroidHelper.GetUnityActivity());
        }

        private void OnFaceDetectedCallback(List<Face> detectedItemList)
        {
            lock (this)
            {
                mFaceDetectedItemList = detectedItemList;
            }
        }

        private void OnBarcodeDetectedCallback(List<Barcode> detectedItemList)
        {
            lock (this)
            {
                mBarcodeDetectedItemList = detectedItemList;
            }
        }

        private void OnTextRecognizedCallback(List<TextBlock> detectedItemList)
        {
            lock (this)
            {
                mTextRecognizedItemList = detectedItemList;
            }
        }

        public void SetOnFaceDetectedDelegater(OnDetectedCallback<Face> delegater)
        {
            mOnFaceDetectedDelegater = delegater;
        }

        public void SetOnBarcodeDetectedDelegater(OnDetectedCallback<Barcode> delegater)
        {
            mOnBarcodeDetectedDelegater = delegater;
        }

        public void SetOnTextRecognizedDelegater(OnDetectedCallback<TextBlock> delegater)
        {
            mOnTextRecognizedDelegater = delegater;
        }

        public void SetClassificationType(int classificationType)
        {
            mAndroidJO.Call("setClassificationType", classificationType);
        }

        public void SetLandmarkType(int landmarkType)
        {
            mAndroidJO.Call("setLandmarkType", landmarkType);
        }

        public void SetMinFaceSize(float proportionalMinFaceSize)
        {
            mAndroidJO.Call("setMinFaceSize", proportionalMinFaceSize);
        }

        public void SetMode(int mode)
        {
            mAndroidJO.Call("setMode", mode);
        }

        public void SetProminentFaceOnly(Boolean prominentFaceOnly)
        {
            mAndroidJO.Call("setProminentFaceOnly", prominentFaceOnly);
        }

        public void SetTrackingEnabled(Boolean trackingEnabled)
        {
            mAndroidJO.Call("setTrackingEnabled", trackingEnabled);
        }

        public void SetBarcodeFormats(int format)
        {
            mAndroidJO.Call("setBarcodeFormats", format);
        }

        public void SetImageBuffer(byte[] imageBuffer, int imageFormat, int imageWidth, int imageHeight, int[] imageStride)
        {
            mAndroidJO.Call("setImageBuffer", imageBuffer, imageFormat, imageWidth, imageHeight, imageStride);
        }

        public void StartDetect(int intervalMS)
        {
            mDetecting = true;
            mRoutine.StartCoroutine(Looper());

            OnFaceDetectedItemListener faceDetectedListener = null;
            OnBarcodeDetectedItemListener barcodeDetectedListener = null;
            OnTextRecognizedItemListener textRecognizedListener = null;

            if(mOnFaceDetectedDelegater != null)
            {
                faceDetectedListener = new OnFaceDetectedItemListener(mLogger, OnFaceDetectedCallback);
            }
            if (mOnBarcodeDetectedDelegater != null)
            {
                barcodeDetectedListener = new OnBarcodeDetectedItemListener(mLogger, OnBarcodeDetectedCallback);
            }
            if (mOnTextRecognizedDelegater != null)
            {
                textRecognizedListener = new OnTextRecognizedItemListener(mLogger, OnTextRecognizedCallback);
            }

            mAndroidJO.Call("startDetect", intervalMS, faceDetectedListener, barcodeDetectedListener, textRecognizedListener);
        }

        public void StopDetect()
        {
            mDetecting = false;
            mRoutine.DestroyComponent();
            mAndroidJO.Call("stopDetect");
        }

        private IEnumerator Looper()
        {
            List<Face> faceDetectedItemList;
            List<Barcode> barcodeDetectedItemList;
            List<TextBlock> textRecognizedItemList;

            while (mDetecting)
            {
                if(mOnFaceDetectedDelegater != null)
                {
                    lock (this)
                    {
                        faceDetectedItemList = mFaceDetectedItemList;
                        if (faceDetectedItemList != null)
                        {
                            mFaceDetectedItemList = null;
                        }
                    }

                    mOnFaceDetectedDelegater(faceDetectedItemList);
                }
                yield return null;

                if (mOnBarcodeDetectedDelegater != null)
                {
                    lock (this)
                    {
                        barcodeDetectedItemList = mBarcodeDetectedItemList;
                        if (barcodeDetectedItemList != null)
                        {
                            mBarcodeDetectedItemList = null;
                        }
                    }

                    mOnBarcodeDetectedDelegater(barcodeDetectedItemList);
                }
                yield return null;

                if (mOnTextRecognizedDelegater != null)
                {
                    lock (this)
                    {
                        textRecognizedItemList = mTextRecognizedItemList;
                        if (textRecognizedItemList != null)
                        {
                            mTextRecognizedItemList = null;
                        }
                    }

                    mOnTextRecognizedDelegater(textRecognizedItemList);
                }
                yield return null;
            }
        }

        abstract internal class OnDetectedItemListener<T> : AndroidJavaProxy
        {
            internal LogController mLogger;
            internal OnDetectedCallback<T> mCallback;

            public OnDetectedItemListener(LogController logger, OnDetectedCallback<T> callback) : base("jp.eq_inc.mobilevisionwrapper.Accessor$OnDetectedItemListener")
            {
                mLogger = logger;
                mCallback = callback;
            }

            public void onDetected(AndroidJavaObject result)
            {
                List<T> wrappedItemList = SparseArrayUtil<T>.ExchangeToList(WrapAndroidJavaObject, result);
                mCallback(wrappedItemList);
            }

            abstract internal T WrapAndroidJavaObject(AndroidJavaObject sourceInstanceJO);
        }

        private class OnFaceDetectedItemListener : OnDetectedItemListener<Face>
        {
            public OnFaceDetectedItemListener(LogController logger, OnDetectedCallback<Face> callback) : base(logger, callback)
            {
                // èàóùÇ»Çµ
            }

            internal override Face WrapAndroidJavaObject(AndroidJavaObject sourceInstanceJO)
            {
                return new Face(sourceInstanceJO);
            }
        }

        private class OnBarcodeDetectedItemListener : OnDetectedItemListener<Barcode>
        {
            public OnBarcodeDetectedItemListener(LogController logger, OnDetectedCallback<Barcode> callback) : base(logger, callback)
            {
                // èàóùÇ»Çµ
            }

            internal override Barcode WrapAndroidJavaObject(AndroidJavaObject sourceInstanceJO)
            {
                return new Barcode(sourceInstanceJO);
            }
        }

        private class OnTextRecognizedItemListener : OnDetectedItemListener<TextBlock>
        {
            public OnTextRecognizedItemListener(LogController logger, OnDetectedCallback<TextBlock> callback) : base(logger, callback)
            {
                // èàóùÇ»Çµ
            }

            internal override TextBlock WrapAndroidJavaObject(AndroidJavaObject sourceInstanceJO)
            {
                return new TextBlock(sourceInstanceJO);
            }
        }
    }
}
