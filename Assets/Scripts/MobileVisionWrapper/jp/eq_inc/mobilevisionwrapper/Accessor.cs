using com.google.android.gms.vision.barcode;
using com.google.android.gms.vision.face;
using com.google.android.gms.vision.text;
using Eq.Unity;
using System;
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

        public Accessor(LogController logger)
        {
            mLogger = logger;
            AndroidJavaClass accessor = new AndroidJavaClass("jp.eq_inc.mobilevisionwrapper.Accessor");
            mAndroidJO = accessor.CallStatic<AndroidJavaObject>("createAccessorForUnity", AndroidHelper.GetUnityActivity());
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

        public void StartDetect(int intervalMS)
        {
            OnFaceDetectedItemListener faceDetectedListener = null;
            OnBarcodeDetectedItemListener barcodeDetectedListener = null;
            OnTextRecognizedItemListener textRecognizedListener = null;

            if(mOnFaceDetectedDelegater != null)
            {
                faceDetectedListener = new OnFaceDetectedItemListener(mLogger, mOnFaceDetectedDelegater);
            }
            if (mOnBarcodeDetectedDelegater != null)
            {
                barcodeDetectedListener = new OnBarcodeDetectedItemListener(mLogger, mOnBarcodeDetectedDelegater);
            }
            if (mOnTextRecognizedDelegater != null)
            {
                textRecognizedListener = new OnTextRecognizedItemListener(mLogger, mOnTextRecognizedDelegater);
            }

            mAndroidJO.Call("startDetect", intervalMS, faceDetectedListener, barcodeDetectedListener, textRecognizedListener);
        }

        public void StopDetect()
        {
            mAndroidJO.Call("stopDetect");
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
