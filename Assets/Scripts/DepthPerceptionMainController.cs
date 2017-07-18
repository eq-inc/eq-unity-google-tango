using System.Text;
using Tango;
using UnityEngine;
using UnityEngine.EventSystems;

public class DepthPerceptionMainController : BaseAndroidMainController, ITangoDepth, ITangoLifecycle
{
    internal TangoApplication mTangoApplication;
    internal TangoPointCloud mTangoPointCloud;
    internal TangoPointCloudFloor mTangoPointCloudFloor;
    internal System.Collections.Generic.Dictionary<UnityEngine.Plane, GameObject> mPlaneObjectTable = new System.Collections.Generic.Dictionary<UnityEngine.Plane, GameObject>();
    internal bool mFindingFloor = false;
    internal bool mShowPointCloud = false;
    public GameObject mPlaneBase;
    public GameObject mFloorBase;

    public void ButtonClicked(Object targetObject)
    {
        ButtonClicked(targetObject, null);
    }

    public void ButtonClicked(BaseEventData eventData)
    {
        PointerEventData pointerEventData = eventData as PointerEventData;

        if(pointerEventData != null)
        {
            ButtonClicked(pointerEventData.pointerPress, pointerEventData);
        }
    }

    public void ButtonClicked(Object targetObject, BaseEventData eventData)
    {
        string targetObjectName = targetObject.name;

        mLogger.CategoryLog(LogCategoryMethodIn, "target object name = " + targetObjectName);
        if (targetObjectName.CompareTo("FindFloorButton") == 0)
        {
            StartCoroutine(FindFloor());
        }
        else if (targetObjectName.CompareTo("ShowPointCloudButton") == 0)
        {
            if(mTangoPointCloud != null)
            {
                mShowPointCloud = (!mShowPointCloud);

                MeshRenderer renderer = mTangoPointCloud.GetComponent<MeshRenderer>();

                if (renderer != null)
                {
                    mLogger.CategoryLog(LogCategoryMethodTrace, "change renderer.enabled from " + renderer.enabled + " to " + mShowPointCloud);
                    renderer.enabled = mShowPointCloud;
                    mTangoPointCloud.m_updatePointsMesh = mShowPointCloud;
                    UnityEngine.UI.Text buttonText = GameObject.Find("ShowPointCloudButton").transform.Find("ShowPointCloudText").gameObject.GetComponent<UnityEngine.UI.Text>();

                    mLogger.CategoryLog(LogCategoryMethodTrace, "buttonText != " + (buttonText != null));
                    if (buttonText != null)
                    {
                        if (mShowPointCloud)
                        {
                            buttonText.text = "Hide Point Cloud";
                        }
                        else
                        {
                            buttonText.text = "Show Point Cloud";
                        }
                    }
                }
            }
        }
        else if (targetObjectName.CompareTo("EventHandlerPanel") == 0)
        {
            PointerEventData pointerEventData = eventData as PointerEventData;
            if(pointerEventData != null)
            {
                StartCoroutine(DisplayFoundPlane(pointerEventData.pressPosition));
            }
        }

        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    // Use this for initialization
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

    internal override void Update()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        base.Update();

        if((mTangoPointCloud != null) && mTangoPointCloud.m_floorFound)
        {
            mLogger.CategoryLog(LogCategoryMethodTrace, "floor y position = " + mTangoPointCloud.m_floorPlaneY);
        }

        mLogger.CategoryLog(LogCategoryMethodOut);
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

            mLogger.CategoryLog(LogCategoryMethodTrace, "Find TangoPointCloudFloor");
            mTangoPointCloudFloor = FindObjectOfType<TangoPointCloudFloor>();
            mTangoPointCloudFloor.gameObject.SetActive(false);

            mLogger.CategoryLog(LogCategoryMethodTrace, "call TangoApplication.Startup");
            mTangoApplication.Startup(null);

            GameObject.Find("FindFloorButton").SetActive(true);
            GameObject.Find("ShowPointCloudButton").SetActive(true);
        }
        else
        {
            PopCurrentScene();
        }
        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    public void OnTangoServiceConnected()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    public void OnTangoServiceDisconnected()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        GameObject.Find("FindFloorButton").SetActive(false);
        GameObject.Find("ShowPointCloudButton").SetActive(false);
        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    public void OnTangoDepthAvailable(TangoUnityDepth tangoDepth)
    {
        mLogger.CategoryLog(LogCategoryMethodIn);

        /*
         * サンプルはOnTangoDepthAvailableがコールされた後にTangoPointCloud.FindPlaneを実施しているので、そっちの方が
         * TangoPointCloud.FindPlaneの成功率が上がると思われる。
         */

        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    internal System.Collections.IEnumerator DisplayFoundPlane(Vector2 targetScreenPosition)
    {
        mLogger.CategoryLog(LogCategoryMethodIn, "mTangoPointCloud = " + mTangoPointCloud);
        if (mTangoPointCloud != null)
        {
            UnityEngine.Camera camera = UnityEngine.Camera.main;
            UnityEngine.Vector3 foundPlaneCenter = new UnityEngine.Vector3();
            UnityEngine.Plane foundPlane = new UnityEngine.Plane();
            if (!mTangoPointCloud.FindPlane(camera, targetScreenPosition, out foundPlaneCenter, out foundPlane))
            {
                mLogger.CategoryLog(LogCategoryMethodTrace, "not found plane");
                yield break;
            }

            if (!mPlaneObjectTable.ContainsKey(foundPlane))
            {
                mLogger.CategoryLog(LogCategoryMethodTrace, "first find plane: plane center = " + foundPlaneCenter.ToString());

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

                /*
                 * タッチされた場所の深度を測定して、そこのplaneに合うplaneを表示しようとしているけど、スクリーン座標のタッチ座標はZ軸方向の値が存在しない。
                 * その状態でCamera.ScreenToWorldPointを実行するとXY座標もずれてしまうので、見つかったplaneの中心座標(ワールド座標)をスクリーン座標化し、
                 * それで得られたplaneの中心座標のZ軸の値を疑似的にタッチ位置のZ軸方向の座標として使用する。
                 */
                // planeの中心座標(ワールド座標)をスクリーン座標に変換
                Vector3 screenFoundPlaneCenter = camera.WorldToScreenPoint(foundPlaneCenter);

                // タッチ座標(XY軸方向のみのスクリーン座標)にplaneの中心座標(スクリーン座標)のZ軸方向の値を設定した上で、ワールド座標に変換
                Vector3 worldTouchPoint = camera.ScreenToWorldPoint(new Vector3(targetScreenPosition.x, targetScreenPosition.y, screenFoundPlaneCenter.z));

                bool useFloorPlane = false;
                if (mTangoPointCloud.m_floorFound)
                {
                    if(mTangoPointCloud.m_floorPlaneY == worldTouchPoint.y)
                    {
                        useFloorPlane = true;
                    }
                }

                GameObject prefabGameObject = null;
                if (useFloorPlane)
                {
                    prefabGameObject = mFloorBase;
                }
                else
                {
                    prefabGameObject = mPlaneBase;
                }

                // 疑似的に算出されたタッチ座標(ワールド座標)にオブジェクトを生成
                GameObject basePlane = Instantiate(prefabGameObject, worldTouchPoint, Quaternion.LookRotation(forward, up));
                basePlane.SetActive(true);
                mPlaneObjectTable[foundPlane] = basePlane;
            }
        }
        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    System.Collections.IEnumerator FindFloor()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        if (mTangoPointCloud != null)
        {
            mLogger.CategoryLog(LogCategoryMethodTrace, "m_floorFound = " + mTangoPointCloud.m_floorFound);
            if (!mTangoPointCloud.m_floorFound)
            {
                try
                {
                    if (!mFindingFloor)
                    {
                        mFindingFloor = true;    // TangoPointCloud.FindFloorの多重コール抑止
                        mLogger.CategoryLog(LogCategoryMethodTrace, "call TangoPointCloud.FindFloor");
                        mTangoPointCloud.FindFloor();
                    }

                    // TangoPointCloud.m_floorFoundがON(Floorが見つかる)になるまで、コルーチンを回す
                    while (!mTangoPointCloud.m_floorFound)
                    {
                        mLogger.CategoryLog(LogCategoryMethodTrace, "waiting for finding floor");
                        yield return new WaitForEndOfFrame();
                    }
                }
                finally
                {
                    mFindingFloor = false;
                }
            }

            if ((mTangoPointCloudFloor != null) && (mTangoPointCloudFloor.gameObject.activeInHierarchy == false))
            {
                // TangoPointCloudFloorで管理しているPlaneを表示させる
                mLogger.CategoryLog(LogCategoryMethodTrace, "show plane for floor");
                mTangoPointCloudFloor.gameObject.SetActive(true);
            }
        }
        mLogger.CategoryLog(LogCategoryMethodOut);
    }
}
