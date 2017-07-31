using System;
using System.Collections.Generic;
using UnityEngine;

class WallPerceptionMainController : DepthPerceptionMainController
{
    internal const int MultiKeyIndexForward = 0;
    internal const int MultiKeyIndexCenter = 1;
    internal List<Vector2> mCheckDepthScreenPositionList = new List<Vector2>();
    internal Eq.Unity.MultiKeyDictionary<PlaneData> mForwardCenterKeyDic = new Eq.Unity.MultiKeyDictionary<PlaneData>(new Type[] { Vector3.zero.GetType()/*Forward*/, Vector3.zero.GetType()/*Center*/});
    internal bool mFindingPlane = false;

    internal override void Start()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        base.Start();

        //Vector2 tempPosition = null;
        float screenWidth = UnityEngine.Screen.width;
        float screenHeight = UnityEngine.Screen.height;
        int positionCountOfWidth = 0;
        int positionCountOfHeight = 0;

        positionCountOfWidth = (int)(screenWidth / 10);
        positionCountOfHeight = (int)(screenHeight / 10);

        float spaceOfWidth = screenWidth / positionCountOfWidth;
        float spaceOfHeight = screenHeight / positionCountOfHeight;

        for (int indexOfHeight = 1; indexOfHeight <= positionCountOfHeight; indexOfHeight++)
        {
            for (int indexOfWidth = 1; indexOfWidth <= positionCountOfWidth; indexOfWidth++)
            {
                mCheckDepthScreenPositionList.Add(new Vector2(spaceOfWidth * indexOfWidth, spaceOfHeight * indexOfHeight));
            }
        }

        //Test();
        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    internal void Test()
    {
        Eq.Unity.MultiKeyDictionary<int> tempDictionary = new Eq.Unity.MultiKeyDictionary<int>(new Type[] { new Vector3().GetType(), new Vector3().GetType() });

        for (int i = 0; i < 10; i++)
        {
            Vector3 forward = new Vector3(i + 10, i + 100, i + 1000);
            Vector3 center = new Vector3(i + 50, i + 500, i + 5000);

            tempDictionary.Add(new object[] { forward, center }, i);
        }

        for (int i = 0; i < 10; i++)
        {
            if (!tempDictionary.ContainsKey(0, new Vector3(i + 10, i + 100, i + 1000)))
            {
                throw new Exception();
            }
            if (tempDictionary.ContainsKey(1, new Vector3(i + 10, i + 100, i + 1000)))
            {
                throw new Exception();
            }
            if (tempDictionary.ContainsKey(0, new Vector3(i + 50, i + 500, i + 5000)))
            {
                throw new Exception();
            }
            if (!tempDictionary.ContainsKey(1, new Vector3(i + 50, i + 500, i + 5000)))
            {
                throw new Exception();
            }
        }

        Dictionary<object[], int> tempRet = null;
        for (int i = 0; i < 10; i++)
        {
            tempRet = tempDictionary.Get(0, new Vector3(i + 10, i + 100, i + 1000));
            if (tempRet == null && tempRet.Count == 0)
            {
                throw new Exception();
            }
            else
            {
                foreach (int value in tempRet.Values)
                {
                    if (value != i)
                    {
                        throw new Exception();
                    }
                }
            }

            tempRet = tempDictionary.Get(1, new Vector3(i + 50, i + 500, i + 5000));
            if (tempRet == null && tempRet.Count == 0)
            {
                throw new Exception();
            }
            else
            {
                foreach (int value in tempRet.Values)
                {
                    if (value != i)
                    {
                        throw new Exception();
                    }
                }
            }
        }
    }

    internal override void Update()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        base.Update();

        StartCoroutine(FindPlane());
        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    public void ShowWallButtonClicked()
    {
        mLogger.CategoryLog(LogCategoryMethodIn, "mForwardCenterKeyDic.Count = " + mForwardCenterKeyDic.Count);
        StartCoroutine(ShowWall());
        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    public void DestroyPlaneGameObject(GameObject planeGameObject)
    {
        mLogger.CategoryLog(LogCategoryMethodIn, "mForwardCenterKeyDic.Count = " + mForwardCenterKeyDic.Count);

        if (mForwardCenterKeyDic.Count > 0)
        {
            for (int i = 0, size = mForwardCenterKeyDic.Count; i < size; i++)
            {
                KeyValuePair<object[], PlaneData> tempKeyValuePair = mForwardCenterKeyDic.ElementAt(i);

                if (planeGameObject.Equals(tempKeyValuePair.Value.mPlaneGameObject))
                {
                    Destroy(planeGameObject);
                    mForwardCenterKeyDic.Remove(tempKeyValuePair.Key);
                    break;
                }
            }
        }

        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    internal System.Collections.IEnumerator FindPlane()
    {
        UnityEngine.Camera camera = UnityEngine.Camera.main;
        UnityEngine.Vector3 foundPlaneCenter = new UnityEngine.Vector3();
        UnityEngine.Plane foundPlane;
        int notFoundCount = 0;

        if (!mFindingPlane)
        {
            try
            {
                mFindingPlane = true;

                foreach (Vector2 targetScreenPosition in mCheckDepthScreenPositionList)
                {
                    if (mTangoPointCloud.FindPlane(camera, targetScreenPosition, out foundPlaneCenter, out foundPlane))
                    {
                        if (!mForwardCenterKeyDic.ContainsKey(MultiKeyIndexCenter, foundPlaneCenter))
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
                            int addRet = mForwardCenterKeyDic.Add(new object[] { forward, foundPlaneCenter }, new PlaneData(foundPlane));
                            mLogger.CategoryLog(LogCategoryMethodTrace, "Add: ret = " + addRet);
                        }
                    }
                    else
                    {
                        notFoundCount++;
                    }

                    StartCoroutine(ShowWall());
                    yield return null;
                }

                if (notFoundCount > 0)
                {
                    mLogger.CategoryLog(LogCategoryMethodTrace, notFoundCount + " / " + mCheckDepthScreenPositionList.Count + (notFoundCount == 1 ? " is " : " are ") + "not found");
                }
            }
            finally
            {
                mFindingPlane = false;
            }
        }
    }

    internal System.Collections.IEnumerator ShowWall()
    {
        for (int i = 0, size = mForwardCenterKeyDic.Count; i < size; i++)
        {
            KeyValuePair<object[], PlaneData> keyValuePair = mForwardCenterKeyDic.ElementAt(i);
            PlaneData planeData = keyValuePair.Value;

            if (planeData.mPlaneGameObject == null)
            {
                object[] keyObjects = keyValuePair.Key;

                if ((keyObjects[MultiKeyIndexForward] is Vector3) && (keyObjects[MultiKeyIndexCenter] is Vector3))
                {
                    Vector3 forward, center;

                    forward = (Vector3)keyObjects[MultiKeyIndexForward];
                    center = (Vector3)keyObjects[MultiKeyIndexCenter];

                    planeData.mPlaneGameObject = Instantiate(mPlaneBase, center, Quaternion.LookRotation(forward, planeData.mPlane.normal));
                    SphereCollider collider = planeData.mPlaneGameObject.AddComponent<SphereCollider>();
                    collider.isTrigger = true;
                    collider.name = Time.frameCount.ToString();
                    collider.center = center;
                    collider.radius = 0.08f;
                    
                    mLogger.CategoryLog(LogCategoryMethodTrace, "add: name = " + collider.name + ", center = " + collider.center + ", radius = " + collider.radius);
                    //planeData.mPlaneGameObject.transform.localScale.Set(0.0001f, 1, 0.0001f);
                    planeData.mPlaneGameObject.SetActive(true);
                }

                yield return null;
            }
        }
    }

    internal class PlaneData
    {
        public Plane mPlane;
        public GameObject mPlaneGameObject;

        public PlaneData(Plane plane)
        {
            mPlane = plane;
        }
    }
}
