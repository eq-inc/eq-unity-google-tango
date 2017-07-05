using System;
using System.Collections;
using System.Collections.Generic;
using Tango;
using UnityEngine;
using UnityEngine.UI;

public class ALMainController : BaseAndroidMainController, ITangoLifecycle, ITangoEvent, ITangoPose
{
    private const int PermissionInit = 0;
    private const int PermissionGranted = 1;
    private const int PermissionDenied = -1;

    private TangoApplication mTangoApplication;
    public bool mLearningMode = false;
    public GameObject mLearningSwitchButton;
    private TangoEnums.TangoPoseStatusType mCurrentPoseStatus = TangoEnums.TangoPoseStatusType.TANGO_POSE_INVALID;
    private int mPermissionResult = PermissionInit;
    private bool mLearningArea = false;

    internal override void Start()
    {
        CategoryLog(LogCategoryMethodIn);
        base.Start();

        mTangoApplication = FindObjectOfType<TangoApplication>();
        if (mTangoApplication != null)
        {
            mTangoApplication.Register(this);
            mTangoApplication.RequestPermissions();
        }

        // 一旦モードに関わらず非表示
        mLearningSwitchButton.SetActive(false);
        SwitchAreaLearning(false);
        CategoryLog(LogCategoryMethodOut);
    }

    internal override void OnEnable()
    {
        CategoryLog(LogCategoryMethodIn);
        base.OnEnable();
        CategoryLog(LogCategoryMethodOut);
    }

    internal override void OnDisable()
    {
        CategoryLog(LogCategoryMethodIn);
        base.OnDisable();

        if (mTangoApplication != null)
        {
            mTangoApplication.Unregister(this);
        }

        CategoryLog(LogCategoryMethodOut);
    }

    public void OnTangoPermissions(bool permissionsGranted)
    {
        CategoryLog(LogCategoryMethodIn);
        if (permissionsGranted)
        {
            mPermissionResult = PermissionGranted;
            switch (mCurrentPoseStatus)
            {
                case TangoEnums.TangoPoseStatusType.TANGO_POSE_VALID:
                    if (!Startup())
                    {
                        this.Back();
                    }
                    break;
                case TangoEnums.TangoPoseStatusType.TANGO_POSE_INITIALIZING:
                    break;
                default:
                    break;
            }
        }
        else
        {
            Debug.LogError("permission denied");
            mPermissionResult = PermissionDenied;
            this.Back();
        }
        CategoryLog(LogCategoryMethodOut);
    }

    public void OnTangoServiceConnected()
    {
        CategoryLog(LogCategoryMethodIn);
        CategoryLog(LogCategoryMethodOut);
    }

    public void OnTangoServiceDisconnected()
    {
        CategoryLog(LogCategoryMethodIn);
        CategoryLog(LogCategoryMethodOut);
    }

    public void OnTangoEventAvailableEventHandler(TangoEvent tangoEvent)
    {
        CategoryLog(LogCategoryMethodIn);
        CategoryLog(LogCategoryMethodOut);
    }

    public void OnTangoPoseAvailable(TangoPoseData poseData)
    {
        CategoryLog(LogCategoryMethodIn);
        mCurrentPoseStatus = poseData.status_code;

        switch (mCurrentPoseStatus)
        {
            case TangoEnums.TangoPoseStatusType.TANGO_POSE_VALID:
                if (mPermissionResult == PermissionGranted)
                {
                    Startup();

                    if (mLearningMode)
                    {
                        mLearningSwitchButton.SetActive(true);
                    }
                }
                else if (mPermissionResult == PermissionDenied)
                {
                    this.Back();
                }
                break;
            case TangoEnums.TangoPoseStatusType.TANGO_POSE_INITIALIZING:
                break;
            default:
                this.Back();
                break;
        }
        CategoryLog(LogCategoryMethodOut);
    }

    public void ButtonClicked(UnityEngine.Object targetObject)
    {
        CategoryLog(LogCategoryMethodIn);
        if (mLearningMode)
        {
            if (targetObject.name.Equals("LearningControlButton"))
            {
                SwitchAreaLearning(!mLearningArea);
            }
        }
        CategoryLog(LogCategoryMethodOut);
    }

    private bool Startup()
    {
        CategoryLog(LogCategoryMethodIn);
        bool ret = true;
        AreaDescription[] list = AreaDescription.GetList();
        AreaDescription mostRecent = null;
        AreaDescription.Metadata mostRecentMetadata = null;

        if (list.Length > 0)
        {
            mostRecent = list[0];
            mostRecentMetadata = mostRecent.GetMetadata();
            foreach (AreaDescription areaDescription in list)
            {
                AreaDescription.Metadata metadata = areaDescription.GetMetadata();
                if (metadata.m_dateTime > mostRecentMetadata.m_dateTime)
                {
                    mostRecent = areaDescription;
                    mostRecentMetadata = metadata;
                }
            }

            mTangoApplication.Startup(mostRecent);
        }
        else
        {
            if (mLearningMode)
            {
                mTangoApplication.Startup(null);
            }
            else
            {
                Debug.LogError("none area description file");
                ret = false;
            }
        }

        CategoryLog(LogCategoryMethodOut, "ret = " + ret);
        return ret;
    }

    private void SwitchAreaLearning(bool nextAreaLearningStatus)
    {
        CategoryLog(LogCategoryMethodIn);
        Text learningSwitchButtonText = mLearningSwitchButton.GetComponent<Text>();

        mLearningArea = nextAreaLearningStatus;
        if (nextAreaLearningStatus)
        {
            learningSwitchButtonText.text = "Stop Area learning";
        }
        else
        {
            learningSwitchButtonText.text = "Start Area learning";

            // 停止したときに、領域学習の保存を実施
            StartCoroutine(SaveCurrentAreaDescription());
        }
        CategoryLog(LogCategoryMethodOut, "mLearningArea = " + mLearningArea);
    }

    delegate void SaveAreaDescription();
    private IEnumerator SaveCurrentAreaDescription()
    {
        SaveAreaDescription tempDelegate = delegate ()
        {
            CategoryLog(LogCategoryMethodIn);
            AreaDescription.SaveCurrent();
            CategoryLog(LogCategoryMethodOut);
        };

        yield return tempDelegate;
    }

}
