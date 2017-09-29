using android.graphics;
using com.google.android.gms.vision.barcode;
using com.google.android.gms.vision.face;
using com.google.android.gms.vision.text;
using Eq.Unity;
using jp.eq_inc.mobilevisionwrapper;
using System.Collections.Generic;
using Tango;
using UnityEngine;

public class FDMainController : MTMainController, ITangoLifecycle, ITangoVideoOverlay
{
    private static readonly int DetectPerSecond = 30;

    private Accessor mAccessor;
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

        // Mobile Vision Accessorを生成
        mAccessor = new Accessor(new CommonRoutine(), mLogger);
        mAccessor.SetClassificationType(FaceDetector.ALL_CLASSIFICATIONS);
        mAccessor.SetLandmarkType(FaceDetector.ALL_LANDMARK);
        mAccessor.SetMode(FaceDetector.ACCURATE_MODE);
        mAccessor.SetOnFaceDetectedDelegater(delegate (List<Face> detectedItemList)
        {
            mLogger.CategoryLog(LogCategoryMethodIn);

            if (mShownDetectedMarkList.Count > 0)
            {
                foreach (GameObject detectedMarkGO in mShownDetectedMarkList)
                {
                    Destroy(detectedMarkGO);
                }
                mShownDetectedMarkList.Clear();
            }

            if ((detectedItemList != null) && (detectedItemList.Count > 0))
            {
                mLogger.CategoryLog(LogCategoryMethodTrace, "face: get detected item list");

                Camera mainCamera = Camera.main;
                for (int i = 0; i < detectedItemList.Count; i++)
                {
                    PointF faceSp = detectedItemList[i].GetPosition();
                    mLogger.CategoryLog(LogCategoryMethodTrace, "face screen position: " + faceSp.x + ", " + faceSp.y);
                    Vector3 faceWp = mainCamera.ScreenToWorldPoint(new Vector3(faceSp.x, faceSp.y, 0));
                    mLogger.CategoryLog(LogCategoryMethodTrace, "face world position: " + faceWp);

                    int nearestPointIndex = mTangoPointCloud.FindClosestPoint(mainCamera, new Vector2(faceWp.x, faceWp.z), 1);
                    if(nearestPointIndex >= 0)
                    {
                        faceWp.y = mTangoPointCloud.m_points[nearestPointIndex].y;
                    }

                    GameObject detectedMarkGO = Instantiate(mDetectedGO, faceWp, Quaternion.identity);
                    detectedMarkGO.SetActive(true);
                }
            }
            else
            {
                mLogger.CategoryLog(LogCategoryMethodTrace, "oh no----");
            }
            mLogger.CategoryLog(LogCategoryMethodOut);
        });
        mAccessor.SetOnBarcodeDetectedDelegater(delegate (List<Barcode> detectedItemList)
        {
            mLogger.CategoryLog(LogCategoryMethodIn);
            if ((detectedItemList != null) && (detectedItemList.Count > 0))
            {
                mLogger.CategoryLog(LogCategoryMethodTrace, "barcode: get detected item list");
            }
            else
            {
                mLogger.CategoryLog(LogCategoryMethodTrace, "oh no----");
            }
            mLogger.CategoryLog(LogCategoryMethodOut);
        });
        mAccessor.SetOnTextRecognizedDelegater(delegate (List<TextBlock> detectedItemList)
        {
            mLogger.CategoryLog(LogCategoryMethodIn);
            if ((detectedItemList != null) && (detectedItemList.Count > 0))
            {
                mLogger.CategoryLog(LogCategoryMethodTrace, "text: get detected item list");
            }
            else
            {
                mLogger.CategoryLog(LogCategoryMethodTrace, "oh no----");
            }
            mLogger.CategoryLog(LogCategoryMethodOut);
        });

        mDetectTask = new DelegateAsyncTask<int, int, int>(delegate (int[] parameters)
                {
                    mLogger.CategoryLog(LogCategoryMethodIn);

                    try
                    {
                        AndroidJNI.AttachCurrentThread();

                        TangoUnityImageData imageBuffer = null;
                        while (!mDetectTask.IsCanceled())
                        {
                            lock (mDetectTask)
                            {
                                if (mGraphicBuffer == null)
                                {
                                    System.Threading.Monitor.Wait(mDetectTask);
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
                                mAccessor.SetImageBuffer(imageBuffer.data, (int)imageBuffer.format, (int)imageBuffer.width, (int)imageBuffer.height, null);
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

        mAccessor.StopDetect();

        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    public void OnTangoPermissions(bool permissionsGranted)
    {
        // 処理なし
    }

    public void OnTangoServiceConnected()
    {
        mAccessor.StartDetect(1000 / DetectPerSecond);
    }

    public void OnTangoServiceDisconnected()
    {
        mAccessor.StopDetect();
    }

    public void OnTangoImageAvailableEventHandler(TangoEnums.TangoCameraId cameraId, TangoUnityImageData imageBuffer)
    {
        mLogger.CategoryLog(LogCategoryMethodIn);

        lock (mDetectTask)
        {
            mGraphicBuffer = imageBuffer;
            System.Threading.Monitor.Pulse(mDetectTask);
        }

        mLogger.CategoryLog(LogCategoryMethodOut);
    }
}
