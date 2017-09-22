using android.graphics;
using com.google.android.gms.vision;
using com.google.android.gms.vision.barcode;
using com.google.android.gms.vision.face;
using com.google.android.gms.vision.text;
using jp.eq_inc.mobilevisionwrapper;
using System.Collections.Generic;
using Tango;
using UnityEngine;
using System;
using Eq.Unity;
using java.io;

public class FDMainController : MTMainController, ITangoLifecycle, ITangoVideoOverlay
{
    private static readonly int DetectPerSecond = 30;

    private FaceDetector mFaceDetector;
    private Frame.Builder mFrameBuilder;
    public GameObject mDetectedGO;
    private List<GameObject> mShownDetectedMarkList = new List<GameObject>();
    private DelegateAsyncTask<int, int, int> mDetectTask;
    private System.Threading.ManualResetEvent mResetEvent = new System.Threading.ManualResetEvent(false);
    private TangoUnityImageData mGraphicBuffer;

    internal override void Start()
    {
        base.Start();

        TangoApplication application = FindObjectOfType<TangoApplication>();
        application.Register(this);

        FaceDetector.Builder faceDetectorBuilder = new FaceDetector.Builder();
        faceDetectorBuilder.SetClassificationType(FaceDetector.ALL_CLASSIFICATIONS);
        faceDetectorBuilder.SetLandmarkType(FaceDetector.ALL_LANDMARK);
        faceDetectorBuilder.SetMode(FaceDetector.ACCURATE_MODE);
        mFaceDetector = faceDetectorBuilder.Build();

        mFrameBuilder = new Frame.Builder();

        mDetectTask = new DelegateAsyncTask<int, int, int>(delegate (int[] parameters)
        {
            mLogger.CategoryLog(LogCategoryMethodIn);

            try
            {
                AndroidJNI.AttachCurrentThread();

                TangoUnityImageData imageBuffer = null;
                while (!mDetectTask.IsCanceled())
                {
                    lock (mDetectTask){
                        if (mGraphicBuffer == null)
                        {
                            mLogger.CategoryLog(LogCategoryMethodTrace, "sleep in");
                            System.Threading.Monitor.Wait(mDetectTask);
                            mLogger.CategoryLog(LogCategoryMethodTrace, "sleep out");
                        }

                        imageBuffer = mGraphicBuffer;
                        mGraphicBuffer = null;
                    }

                    if (mShownDetectedMarkList.Count > 0)
                    {
                        foreach (GameObject detectedMarkGO in mShownDetectedMarkList)
                        {
                            Destroy(detectedMarkGO);
                        }
                        mShownDetectedMarkList.Clear();
                    }

                    if (imageBuffer != null)
                    {
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

                        mFrameBuilder.SetBitmap(imageBufferBitmap);
                        mLogger.CategoryLog(LogCategoryMethodTrace, "imageBufferBitmap is set to frameBuilder");

                        List<Face> detectedList = mFaceDetector.Detect(mFrameBuilder.Build());
                        if ((detectedList != null) && (detectedList.Count > 0))
                        {
                            mLogger.CategoryLog(LogCategoryMethodTrace, "get detected item list");

                            Camera mainCamera = Camera.main;
                            for (int i = 0; i < detectedList.Count; i++)
                            {
                                PointF faceSp = detectedList[i].GetPosition();
                                Vector3 faceWp = mainCamera.ScreenToWorldPoint(new Vector3(faceSp.x, 0, faceSp.y));

                                GameObject detectedMarkGO = Instantiate(mDetectedGO, faceWp, Quaternion.identity);
                                detectedMarkGO.SetActive(true);
                            }
                        }
                        else
                        {
                            mLogger.CategoryLog(LogCategoryMethodTrace, "oh no----");
                        }
                    }
                }
            }
            finally
            {
                AndroidJNI.DetachCurrentThread();
            }

            mLogger.CategoryLog(LogCategoryMethodOut);
            return 0;
        });
        mDetectTask.Execute(0);
    }

    internal override void OnDestroy()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);

        base.OnDestroy();
        lock (mDetectTask)
        {
            mDetectTask.Cancel();
            System.Threading.Monitor.Pulse(mDetectTask);
        }

        if (mFaceDetector != null)
        {
            mFaceDetector.Release();
            mFaceDetector = null;
        }

        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    public void OnTangoPermissions(bool permissionsGranted)
    {
        // 処理なし
    }

    public void OnTangoServiceConnected()
    {
        // 処理なし
    }

    public void OnTangoServiceDisconnected()
    {
        // 処理なし
    }

    public void OnTangoImageAvailableEventHandler(TangoEnums.TangoCameraId cameraId, TangoUnityImageData imageBuffer)
    {
        mLogger.CategoryLog(LogCategoryMethodIn);

        lock (mDetectTask)
        {
            mGraphicBuffer = imageBuffer;
            mLogger.CategoryLog(LogCategoryMethodTrace, "send signal in");
            System.Threading.Monitor.Pulse(mDetectTask);
            mLogger.CategoryLog(LogCategoryMethodTrace, "send signal out");
        }

        mLogger.CategoryLog(LogCategoryMethodOut);
    }
}
