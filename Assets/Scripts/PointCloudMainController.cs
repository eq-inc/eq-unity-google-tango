using Eq.Unity;
using System;
using System.Collections.Generic;
using Tango;
using UnityEngine;

class PointCloudMainController : BaseAndroidMainController, ITangoLifecycle
{
    internal TangoApplication mTangoApplication;
    internal TangoPointCloud mTangoPointCloud;
    internal Dictionary<Vector3, GameObject> mPointGameObjectDic = new Dictionary<Vector3, GameObject>();
    internal bool mCreatingPointCloud;
    public GameObject mPointPrefab;

    internal override void Start()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        base.Start();

        SetScreenTimeout(BaseAndroidMainController.NeverSleep);
        SetScreenOrientation(ScreenOrientation.Portrait);

        mTangoApplication = FindObjectOfType<TangoApplication>();
        if (mTangoApplication != null)
        {
            mTangoApplication.EnableDepth = true;
            mTangoApplication.Register(this);
            mTangoApplication.RequestPermissions();
        }

        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    public void OnTangoServiceConnected()
    {
        // 処理なし
    }

    public void OnTangoServiceDisconnected()
    {
        // 処理なし
    }

    public void OnTangoPermissions(bool permissionsGranted)
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        if (permissionsGranted)
        {
            mTangoPointCloud = FindObjectOfType<TangoPointCloud>();
            if (mTangoPointCloud != null)
            {
                mLogger.CategoryLog(LogCategoryMethodTrace, "call TangoPointCloud.Start");
                mTangoPointCloud.Start();
            }

            mLogger.CategoryLog(LogCategoryMethodTrace, "call TangoApplication.Startup");
            mTangoApplication.Startup(null);
        }
        else
        {
            PopCurrentScene();
        }
        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    internal override void Update()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        base.Update();

        if (mTangoPointCloud != null)
        {
            StartCoroutine(CreatePointCloudGameObject());
        }

        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    internal System.Collections.IEnumerator CreatePointCloudGameObject()
    {
        if (!mCreatingPointCloud)
        {
            try
            {
                mCreatingPointCloud = true;

                int pointCount = mTangoPointCloud.m_pointsCount;
                if (pointCount > 0)
                {
                    int targetCount = 10;
                    int stride = 10;
                    int sameCount = 0;
                    mLogger.CategoryLog(LogCategoryMethodTrace, "point count = " + pointCount + ", target count = " + targetCount);
                    for (int i = 0; (i < pointCount) && (targetCount > 0); i += stride)
                    {
                        for (int j = 0; j < stride; j++)
                        {
                            if (i + j >= pointCount || targetCount == 0)
                            {
                                break;
                            }

                            try
                            {
                                Vector3 pointVector = mTangoPointCloud.m_points[i];

                                if (mPointGameObjectDic.ContainsKey(pointVector) == false)
                                {
                                    UnityEngine.Camera camera = UnityEngine.Camera.main;
                                    UnityEngine.Vector3 foundPlaneCenter = new UnityEngine.Vector3();
                                    UnityEngine.Plane foundPlane = new UnityEngine.Plane();
                                    Vector3 pointScreenVector = camera.WorldToScreenPoint(pointVector);
                                    if (!mTangoPointCloud.FindPlane(camera, pointScreenVector, out foundPlaneCenter, out foundPlane))
                                    {
                                        mLogger.CategoryLog(LogCategoryMethodTrace, "not found plane");
                                    }
                                    else
                                    {
                                        // Ensure the location is always facing the camera.  This is like a LookRotation, but for the Y axis.
                                        Vector3 up = foundPlane.normal;
                                        Vector3 forward;
                                        if (Vector3.Angle(foundPlane.normal, camera.transform.forward) < 175)
                                        {
                                            Vector3 right = Vector3.Cross(up, camera.transform.forward).normalized;
                                            forward = Vector3.Cross(right, up).normalized;
                                        }
                                        else
                                        {
                                            // Normal is nearly parallel to camera look direction, the cross product would have too much
                                            // floating point error in it.
                                            forward = Vector3.Cross(up, camera.transform.right);
                                        }

                                        GameObject pointCloudDotGameObject = Instantiate(mPointPrefab, pointVector, Quaternion.LookRotation(forward, up));
                                        pointCloudDotGameObject.SetActive(true);
                                        mPointGameObjectDic[pointVector] = pointCloudDotGameObject;

                                        targetCount--;
                                    }
                                }
                                else
                                {
                                    sameCount++;
                                }
                            }
                            catch (Exception e)
                            {
                                mLogger.CategoryLog(LogCategoryMethodError, e);
                            }
                        }

                        yield return null;
                    }

                    mLogger.CategoryLog(LogCategoryMethodTrace, "same point count = " + sameCount);
                }
            }
            finally
            {
                mCreatingPointCloud = false;
            }
        }
    }
}
