using com.google.android.gms.vision.barcode;
using com.google.android.gms.vision.face;
using com.google.android.gms.vision.text;
using jp.eq_inc.mobilevisionwrapper;
using System.Collections.Generic;
using Tango;
using System;
using System.Collections;
using UnityEngine;
using android.graphics;

public class FDMainController : MTMainController, ITangoLifecycle
{
    private static readonly int DetectPerSecond = 30;

    private Accessor mAccessor;
    public GameObject mDetectedGO;
    private List<GameObject> mShownDetectedMarkList = new List<GameObject>();

    internal override void Start()
    {
        TangoApplication application = FindObjectOfType<TangoApplication>();
        application.Register(this);

        // Mobile Vision Accessorを生成
        mAccessor = new Accessor(mLogger);
        mAccessor.SetClassificationType(FaceDetector.ALL_CLASSIFICATIONS);
        mAccessor.SetLandmarkType(FaceDetector.ALL_LANDMARK);
        mAccessor.SetMode(FaceDetector.ACCURATE_MODE);
        mAccessor.SetOnFaceDetectedDelegater(delegate (List<Face> detectedItemList)
        {
            mLogger.CategoryLog(LogCategoryMethodIn);

            if(mShownDetectedMarkList.Count > 0)
            {
                foreach (GameObject detectedMarkGO in mShownDetectedMarkList)
                {
                    Destroy(detectedMarkGO);
                }
                mShownDetectedMarkList.Clear();
            }

            if (detectedItemList.Count > 0)
            {
                mLogger.CategoryLog(LogCategoryMethodTrace, "get detected item list");

                Camera mainCamera = Camera.main;
                for(int i=0; i<detectedItemList.Count; i++)
                {
                    PointF faceSp = detectedItemList[i].GetPosition();
                    Vector3 faceWp = mainCamera.ScreenToWorldPoint(new Vector3(faceSp.x, 0, faceSp.y));

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
            if (detectedItemList.Count > 0)
            {
                mLogger.CategoryLog(LogCategoryMethodTrace, "get detected item list");
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
            if (detectedItemList.Count > 0)
            {
                mLogger.CategoryLog(LogCategoryMethodTrace, "get detected item list");
            }
            else
            {
                mLogger.CategoryLog(LogCategoryMethodTrace, "oh no----");
            }
            mLogger.CategoryLog(LogCategoryMethodOut);
        });

        base.Start();
    }

    internal override void OnDestroy()
    {
        base.OnDestroy();

        // Java側の顔認識機を停止
        mAccessor.StopDetect();
    }

    public void OnTangoPermissions(bool permissionsGranted)
    {
        // 処理なし
    }

    public void OnTangoServiceConnected()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);

        // Java側にて10ms毎に顔認識を実施させる
        mAccessor.StartDetect(10);

        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    public void OnTangoServiceDisconnected()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);

        // Java側の顔認識機を停止
        mAccessor.StopDetect();

        mLogger.CategoryLog(LogCategoryMethodOut);
    }
}
