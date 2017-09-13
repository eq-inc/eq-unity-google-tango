using System;
using com.google.android.gms.vision;
using com.google.android.gms.vision.face;
using UnityEngine;
using Tango;

public class FDMainController : MTMainController, ITangoLifecycle
{
    private FaceDetector mFaceDetector;
    private CameraSource mCameraSource;

    internal override void Start()
    {
        TangoApplication application = FindObjectOfType<TangoApplication>();
        application.Register(this);

        base.Start();
    }

    internal override void OnDestroy()
    {
        base.OnDestroy();
        if(mFaceDetector != null)
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

        MultiProcessor<Face>.Builder<Face> processorBuilder = new MultiProcessor<Face>.Builder<Face>(new LocalMultiTrackerFactory());
        MultiProcessor<Face> processor = processorBuilder.Build();
        mFaceDetector.SetProcessor(processor);

        CameraSource.Builder cameraSourceBuilder = new CameraSource.Builder(mFaceDetector);
        mCameraSource = cameraSourceBuilder.Build();
        mCameraSource.Start();

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
