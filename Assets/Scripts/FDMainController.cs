using android.graphics;
using com.google.android.gms.vision;
using com.google.android.gms.vision.face;
using Eq.Unity;
using java.io;
using System.Collections.Generic;
using Tango;
using UnityEngine;

public class FDMainController : MTMainController, ITangoLifecycle, ITangoVideoOverlay
{
    private FaceDetector mFaceDetector;
    private CameraSource mCameraSource;
    private float mDetectDelta = 0;
    private AndroidJavaObject mMobileVisionAccessorJO;

    internal override void Start()
    {
        TangoApplication application = FindObjectOfType<TangoApplication>();
        application.Register(this);
        mMobileVisionAccessorJO = new AndroidJavaObject("jp.eq_inc.mobilevisionwrapper.Accessor");

        base.Start();
    }

    internal override void OnDestroy()
    {
        base.OnDestroy();
        if (mFaceDetector != null)
        {
            mFaceDetector.Release();
            mFaceDetector = null;
        }
    }

    public void OnTangoPermissions(bool permissionsGranted)
    {
    }

    public void OnTangoServiceConnected()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);

        //FaceDetector.Builder builder = new FaceDetector.Builder();
        //mFaceDetector = builder.Build();
        //mLogger.CategoryLog(LogCategoryMethodTrace, "FaceDetector is created: " + mFaceDetector);

        //MultiProcessor<Face>.Builder<Face> processorBuilder = new MultiProcessor<Face>.Builder<Face>(new LocalMultiTrackerFactory());
        //MultiProcessor<Face> processor = processorBuilder.Build();
        //mLogger.CategoryLog(LogCategoryMethodTrace, "MultiProcessor<Face> is created: " + processor);
        //mFaceDetector.SetProcessor(processor);

        //CameraSource.Builder cameraSourceBuilder = new CameraSource.Builder(mFaceDetector);
        //mCameraSource = cameraSourceBuilder.Build();
        //mLogger.CategoryLog(LogCategoryMethodTrace, "mCameraSource is created");

        //mCameraSource.Start();
        //VideoOverlayProvider.SetCallback(TangoEnums.TangoCameraId.TANGO_CAMERA_COLOR, APIOnImageAvailable);

        mMobileVisionAccessorJO.Call(
            "startDetectFace",
            AndroidHelper.GetUnityActivity(),
            null,
            10,
            new OnDetectedItemListener(
                mLogger, 
                delegate (List<Face> detectedItemList)
                {
                    mLogger.CategoryLog(LogCategoryMethodIn);
                    if (detectedItemList.Count > 0)
                    {
                        mLogger.CategoryLog(LogCategoryMethodTrace, "get detected item list");
                    }
                    else
                    {
                        mLogger.CategoryLog(LogCategoryMethodTrace, "oh no----");
                    }
                    mLogger.CategoryLog(LogCategoryMethodOut);
                }
            )
        );

        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    public void OnTangoServiceDisconnected()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);

        if (mFaceDetector != null)
        {
            mFaceDetector.Release();
            mFaceDetector = null;
        }

        mMobileVisionAccessorJO.Call("stopDetectFace");
        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    public void OnTangoImageAvailableEventHandler(TangoEnums.TangoCameraId cameraId, TangoUnityImageData imageBuffer)
    {
        mDetectDelta += Time.deltaTime;

        mLogger.CategoryLog(LogCategoryMethodIn, "FaceDetector = " + mFaceDetector + ", delta time = " + mDetectDelta);

        if (mFaceDetector != null && mDetectDelta > 1)
        {
            mDetectDelta = 0;
            DelegateAsyncTask<int, int, int> task = new DelegateAsyncTask<int, int, int>(delegate (int[] values)
            {
                mLogger.CategoryLog(LogCategoryMethodIn, "imageBuffer.data length is = " + imageBuffer.data.Length + ", format = " + imageBuffer.format + ", width = " + imageBuffer.width + ", height = " + imageBuffer.height + ", stride = " + imageBuffer.stride);
                lock (this)
                {
                    try
                    {
                        AndroidJNI.AttachCurrentThread();
                        mLogger.CategoryLog(LogCategoryMethodIn, "CurrentThread is attached to JVM");

                        YuvImage yuvImage = new YuvImage(imageBuffer.data, (int)imageBuffer.format, (int)imageBuffer.width, (int)imageBuffer.height, null);

                        mLogger.CategoryLog(LogCategoryMethodTrace, "yuvImage = " + yuvImage);
                        ByteArrayOutputStream outStream = new ByteArrayOutputStream();
                        mLogger.CategoryLog(LogCategoryMethodTrace, "outStream = " + outStream);
                        yuvImage.CompressToJpeg(
                            new android.graphics.Rect(0, 0, (int)imageBuffer.width, (int)imageBuffer.height),
                            100,
                            outStream);
                        mLogger.CategoryLog(LogCategoryMethodTrace, "yuvImage.CompressToJpeg is finished");

                        byte[] bitmapByteArray = outStream.ToByteArray();
                        mLogger.CategoryLog(LogCategoryMethodTrace, "bitmapByteArray = " + bitmapByteArray);
                        Bitmap imageBufferBitmap = BitmapFactory.decodeByteArray(bitmapByteArray, 0, bitmapByteArray.Length, null);
                        mLogger.CategoryLog(LogCategoryMethodTrace, "imageBufferBitmap = " + imageBufferBitmap);

                        Frame.Builder frameBuilder = new Frame.Builder();
                        mLogger.CategoryLog(LogCategoryMethodTrace, "frameBuilder = " + frameBuilder);
                        frameBuilder.SetBitmap(imageBufferBitmap);
                        mLogger.CategoryLog(LogCategoryMethodTrace, "imageBufferBitmap is set to frameBuilder");

                        List<Face> detectedList = mFaceDetector.Detect(frameBuilder.Build());
                        if ((detectedList != null) && (detectedList.Count > 0))
                        {
                            mLogger.CategoryLog(LogCategoryMethodTrace, detectedList.Count + "face is detected");
                        }

                        //AndroidJavaObject accessorJO = new AndroidJavaObject("jp.eq_inc.mobilevisionwrapper.Accessor", AndroidHelper.GetUnityActivity());
                        //mLogger.CategoryLog(LogCategoryMethodTrace, "accessorJO = " + accessorJO);
                        //AndroidJavaObject builderJO = accessorJO.Call<AndroidJavaObject>("getFrameBuilder");
                        //mLogger.CategoryLog(LogCategoryMethodTrace, "builderJO = " + builderJO);
                        //
                        //AndroidJavaClass accessor = new AndroidJavaClass("jp.eq_inc.mobilevisionwrapper.Accessor");
                        //AndroidJavaObject sparseArrayDetectedItems = accessor.CallStatic<AndroidJavaObject>("detectFaceForUnity", AndroidHelper.GetUnityActivity(), null);
                        //if (sparseArrayDetectedItems != null)
                        //{
                        //    mLogger.CategoryLog(LogCategoryMethodTrace, "get detected item list");
                        //}
                        //else
                        //{
                        //    mLogger.CategoryLog(LogCategoryMethodTrace, "oh no----");
                        //}
                    }
                    finally
                    {
                        AndroidJNI.DetachCurrentThread();
                    }
                }
                mLogger.CategoryLog(LogCategoryMethodOut);

                return 0;
            });
            //task.Execute(0);

            // 少し止まる
            //AndroidJavaClass accessor = new AndroidJavaClass("jp.eq_inc.mobilevisionwrapper.Accessor");
            //AndroidJavaObject sparseArrayDetectedItems = accessor.CallStatic<AndroidJavaObject>("detectFaceForUnity", AndroidHelper.GetUnityActivity(), null);
            //if (sparseArrayDetectedItems != null)
            //{
            //    mLogger.CategoryLog(LogCategoryMethodTrace, "get detected item list");
            //}
            //else
            //{
            //    mLogger.CategoryLog(LogCategoryMethodTrace, "oh no----");
            //}

            // クラスが見つけられなかった 
            task = new DelegateAsyncTask<int, int, int>(delegate (int[] values)
            {
                mLogger.CategoryLog(LogCategoryMethodIn, "imageBuffer.data length is = " + imageBuffer.data.Length + ", format = " + imageBuffer.format + ", width = " + imageBuffer.width + ", height = " + imageBuffer.height + ", stride = " + imageBuffer.stride);
                lock (this)
                {
                    try
                    {
                        AndroidJNI.AttachCurrentThread();
                        mLogger.CategoryLog(LogCategoryMethodIn, "CurrentThread is attached to JVM");

                        AndroidJavaObject accessorJO = new AndroidJavaObject("jp.eq_inc.mobilevisionwrapper.Accessor", AndroidHelper.GetUnityActivity());
                        mLogger.CategoryLog(LogCategoryMethodTrace, "accessorJO = " + accessorJO);
                        AndroidJavaObject builderJO = accessorJO.Call<AndroidJavaObject>("getFrameBuilder");
                        mLogger.CategoryLog(LogCategoryMethodTrace, "builderJO = " + builderJO);

                        AndroidJavaClass accessor = new AndroidJavaClass("jp.eq_inc.mobilevisionwrapper.Accessor");
                        AndroidJavaObject sparseArrayDetectedItems = accessor.CallStatic<AndroidJavaObject>("detectFaceForUnity", AndroidHelper.GetUnityActivity(), null);
                        if (sparseArrayDetectedItems != null)
                        {
                            mLogger.CategoryLog(LogCategoryMethodTrace, "get detected item list");
                        }
                        else
                        {
                            mLogger.CategoryLog(LogCategoryMethodTrace, "oh no----");
                        }
                    }
                    finally
                    {
                        AndroidJNI.DetachCurrentThread();
                    }
                }
                mLogger.CategoryLog(LogCategoryMethodOut);

                return 0;
            });
            //task.Execute(0);
        }
        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    private class LocalMultiTrackerFactory : MultiProcessor<Face>.Factory
    {
        public override Tracker create(AndroidJavaObject itemJO)
        {
            return new LocalTracker();
        }
    }

    private class LocalTracker : Tracker
    {
    }

    private class OnDetectedItemListener : AndroidJavaProxy
    {
        public delegate void OnDetected(List<Face> detectedItemList);

        private LogController mLogger;
        private OnDetected mCallback;

        public OnDetectedItemListener(LogController logger, OnDetected callback) : base("jp.eq_inc.mobilevisionwrapper.Accessor$OnDetectedItemListener")
        {
            mLogger = logger;
            mCallback = callback;
        }

        public void onDetected(AndroidJavaObject resultJO)
        {
            mLogger.CategoryLog(LogCategoryMethodIn);
            if (mCallback != null)
            {
                List<Face> detectedItemList = new List<Face>();
                SparseArrayUtil<Face>.ExchangeToList(
                    delegate (AndroidJavaObject sourceInstanceJO)
                    {
                        return new Face(sourceInstanceJO);
                    },
                    resultJO,
                    detectedItemList);
                mCallback(detectedItemList);
            }
            mLogger.CategoryLog(LogCategoryMethodIn);
        }
    }
}
