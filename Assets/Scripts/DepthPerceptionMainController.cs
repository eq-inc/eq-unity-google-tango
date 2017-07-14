using System.Text;
using Tango;
using UnityEngine;

public class DepthPerceptionMainController : BaseAndroidMainController, ITangoDepth, ITangoLifecycle
{
    internal TangoApplication mTangoApplication;
    internal TangoPointCloud mTangoPointCloud;
    internal System.Collections.Generic.Dictionary<UnityEngine.Plane, GameObject> mPlaneObjectTable = new System.Collections.Generic.Dictionary<UnityEngine.Plane, GameObject>();
    public GameObject mPlaneBase;

    // Use this for initialization
    internal override void Start()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        base.Start();

        SetScreenTimeout(BaseAndroidMainController.NeverSleep);
        SetScreenOrientation(ScreenOrientation.Portrait);

        mTangoApplication = FindObjectOfType<TangoApplication>();
        if(mTangoApplication != null)
        {
            mTangoApplication.EnableDepth = true;
            mTangoApplication.Register(this);
            mTangoApplication.RequestPermissions();
        }

        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    internal override void Update()
    {
        base.Update();

        if (Input.touchCount >= 1)
        {
            Touch touch = Input.touches[0];
            if (touch.phase == TouchPhase.Ended)
            {
                StartCoroutine(DisplayFoundPlane(touch));
            }
        }
    }

    public void OnTangoPermissions(bool permissionsGranted)
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        if (permissionsGranted)
        {
            mTangoPointCloud = FindObjectOfType<TangoPointCloud>();
            if (mTangoPointCloud != null)
            {
                mTangoPointCloud.Start();
            }

            mTangoApplication.Startup(null);
        }
        else
        {
            PopCurrentScene();
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

    public void OnTangoDepthAvailable(TangoUnityDepth tangoDepth)
    {
        mLogger.CategoryLog(LogCategoryMethodIn);

        /*
         * サンプルはOnTangoDepthAvailableがコールされた後にTangoPointCloud.FindPlaneを実施しているので、そっちの方が
         * TangoPointCloud.FindPlaneの成功率が上がると思われる。
         */

        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    internal System.Collections.IEnumerator DisplayFoundPlane(Touch touch)
    {
        mLogger.CategoryLog(LogCategoryMethodIn, "mTangoPointCloud = " + mTangoPointCloud);
        if (mTangoPointCloud != null)
        {
            UnityEngine.Camera camera = UnityEngine.Camera.main;
            UnityEngine.Vector3 foundPlaneCenter = new UnityEngine.Vector3();
            UnityEngine.Plane foundPlane = new UnityEngine.Plane();
            if (!mTangoPointCloud.FindPlane(camera, touch.position, out foundPlaneCenter, out foundPlane))
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
                Vector3 worldTouchPoint = camera.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, screenFoundPlaneCenter.z));

                // 疑似的に算出されたタッチ座標(ワールド座標)にオブジェクトを生成
                GameObject basePlane = Instantiate(mPlaneBase, worldTouchPoint, Quaternion.LookRotation(forward, up));
                basePlane.SetActive(true);
                mPlaneObjectTable[foundPlane] = basePlane;
            }
        }
        mLogger.CategoryLog(LogCategoryMethodOut);
    }
}
