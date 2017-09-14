using System;
using com.google.android.gms.vision;
using com.google.android.gms.vision.face;
using UnityEngine;
using Tango;
using java.io;
using android.graphics;
using System.Runtime.InteropServices;
using Eq.Unity;
using System.Collections.Generic;

public class FDMainController : MTMainController, ITangoLifecycle, ITangoVideoOverlay
{
    private FaceDetector mFaceDetector;
    private CameraSource mCameraSource;
    private float mDetectDelta = 0;

    internal override void Start()
    {
        TangoApplication application = FindObjectOfType<TangoApplication>();
        application.Register(this);

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

        FaceDetector.Builder builder = new FaceDetector.Builder();
        mFaceDetector = builder.Build();
        mLogger.CategoryLog(LogCategoryMethodTrace, "FaceDetector.Builder is created");

        MultiProcessor<Face>.Builder<Face> processorBuilder = new MultiProcessor<Face>.Builder<Face>(new LocalMultiTrackerFactory());
        MultiProcessor<Face> processor = processorBuilder.Build();
        mLogger.CategoryLog(LogCategoryMethodTrace, "MultiProcessor<Face> is created");
        mFaceDetector.SetProcessor(processor);

        //CameraSource.Builder cameraSourceBuilder = new CameraSource.Builder(mFaceDetector);
        //mCameraSource = cameraSourceBuilder.Build();
        //mLogger.CategoryLog(LogCategoryMethodTrace, "mCameraSource is created");

        //mCameraSource.Start();
        //VideoOverlayProvider.SetCallback(TangoEnums.TangoCameraId.TANGO_CAMERA_COLOR, APIOnImageAvailable);

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

        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    public void OnTangoImageAvailableEventHandler(TangoEnums.TangoCameraId cameraId, TangoUnityImageData imageBuffer)
    {
        mLogger.CategoryLog(LogCategoryMethodIn);

        mDetectDelta += Time.deltaTime;

        if (mFaceDetector != null && mDetectDelta > 3)
        {
            DelegateAsyncTask<int, int, int> task = new DelegateAsyncTask<int, int, int>(delegate(int[] index)
            {
                mLogger.CategoryLog(LogCategoryMethodIn);
                YuvImage yuvImage = new YuvImage(imageBuffer.data, (int)imageBuffer.format, (int)imageBuffer.width, (int)imageBuffer.height, null);
                ByteArrayOutputStream outStream = new ByteArrayOutputStream();
                yuvImage.CompressToJpeg(
                    new android.graphics.Rect(0, 0, (int)imageBuffer.width, (int)imageBuffer.height),
                    100,
                    outStream);
                byte[] bitmapByteArray = outStream.ToByteArray();
                Bitmap imageBufferBitmap = BitmapFactory.decodeByteArray(bitmapByteArray, 0, bitmapByteArray.Length, null);
                Frame.Builder frameBuilder = new Frame.Builder();
                frameBuilder.SetBitmap(imageBufferBitmap);

                List<Face> detectedList = mFaceDetector.Detect(frameBuilder.Build());
                if((detectedList != null) && (detectedList.Count > 0))
                {
                    mLogger.CategoryLog(LogCategoryMethodTrace, detectedList.Count + "face is detected");
                }

                mLogger.CategoryLog(LogCategoryMethodOut);
                return 0;
            });
            task.CopyLogController(mLogger);
            task.Execute();

            mDetectDelta = 0;
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
}
