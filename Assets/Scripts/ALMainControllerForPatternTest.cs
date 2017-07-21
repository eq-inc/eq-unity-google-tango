using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Tango;
using UnityEngine;

public class ALMainControllerForPatternTest : BaseAndroidMainController, ITangoLifecycle, ITangoPose
{
    private const string StartPoseMode = "Start";
    private const string StopPoseMode = "Stop";
    private const string AreaDescriptionNone = "None";
    private const string AreaDescriptionUnname = "Unname";
    private UnityEngine.UI.Text mNotificationText = null;
    private UnityEngine.UI.Text mSwitchModeButtonText = null;
    private bool mRunningPoseMode = false;
    private TangoApplication mTangoApplication;
    private AreaDescription mUsedAreaDescription = null;
    public GameObject mRelocalizing;

    internal override void OnEnable()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        base.OnEnable();
        SetScreenTimeout(BaseAndroidMainController.NeverSleep);
        SetScreenOrientation(ScreenOrientation.Portrait);
        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    internal override void OnDisable()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        base.OnDisable();

        if (mTangoApplication != null)
        {
            mTangoApplication.Unregister(this);
        }

        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    internal override void Start()
    {
        base.Start();

        GameObject switchModeButtonGO = GameObject.Find("SwitchModeButton");
        if (switchModeButtonGO != null)
        {
            mSwitchModeButtonText = switchModeButtonGO.GetComponentInChildren<UnityEngine.UI.Text>();
            SwitchPoseMode(false);
        }

        GameObject NotificationTextGO = GameObject.Find("NotificationText");
        if (NotificationTextGO != null)
        {
            mNotificationText = NotificationTextGO.GetComponentInChildren<UnityEngine.UI.Text>();
        }

        mRelocalizing.SetActive(false);

        mTangoApplication = FindObjectOfType<TangoApplication>();
        if(mTangoApplication != null)
        {
            mTangoApplication.Register(this);
        }
    }

    public void SwitchModeButtonClick()
    {
        SwitchPoseMode(!mRunningPoseMode);
    }

    private void SwitchPoseMode(bool isRunningPoseMode)
    {
        if (isRunningPoseMode)
        {
            if (mRunningPoseMode != isRunningPoseMode)
            {
                mNotificationText.text = "";
                mTangoApplication.EnableAreaDescriptions = IsOn("EnableAreaDescriptionToggle");
                mTangoApplication.AreaDescriptionLearningMode = IsOn("AreaDescriptionLearningModeToggle");

                mUsedAreaDescription = null;
                if (IsOn("UseLastAreaDescriptionToggle"))
                {
                    mUsedAreaDescription = GetMostRecentAreaDescription();
                }

                mTangoApplication.RequestPermissions();
            }
            mSwitchModeButtonText.text = StopPoseMode;
        }
        else
        {
            if (mRunningPoseMode != isRunningPoseMode)
            {
                AreaDescription savedAreaDescription = AreaDescription.SaveCurrent();
                StringBuilder builder = new StringBuilder();
                String usedAreaDescriptionUuid = null;
                String savedAreaDescriptionUuid = null;

                if (mUsedAreaDescription == null)
                {
                    usedAreaDescriptionUuid = "null";
                }
                else
                {
                    usedAreaDescriptionUuid = mUsedAreaDescription.m_uuid;
                }
                if(savedAreaDescription == null)
                {
                    savedAreaDescriptionUuid = "null";
                }
                else
                {
                    savedAreaDescriptionUuid = savedAreaDescription.m_uuid;
                }

                builder.Append("Used Area Description: ").Append(usedAreaDescriptionUuid).Append("\n");
                builder.Append("Saved Area Description: ").Append(savedAreaDescriptionUuid).Append("\n");

                mNotificationText.text = builder.ToString();
            }
            mSwitchModeButtonText.text = StartPoseMode;
        }

        mRunningPoseMode = isRunningPoseMode;
    }

    internal AreaDescription GetMostRecentAreaDescription()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);

        AreaDescription mostRecent = null;

        try
        {
            AreaDescription[] list = AreaDescription.GetList();
            AreaDescription.Metadata mostRecentMetadata = null;

            if (list.Length > 0)
            {
                mostRecent = list[0];
                mostRecentMetadata = mostRecent.GetMetadata();
                foreach (AreaDescription areaDescription in list)
                {
                    AreaDescription.Metadata metadata = areaDescription.GetMetadata();
                    mLogger.CategoryLog(LogCategoryMethodTrace, "Area Description Name = " + areaDescription.GetMetadata().m_name);

                    if (metadata.m_dateTime > mostRecentMetadata.m_dateTime)
                    {
                        mostRecent = areaDescription;
                        mostRecentMetadata = metadata;
                    }
                }
            }
        }
        catch (Exception e)
        {
            mLogger.CategoryLog(LogCategoryMethodError, e);
        }

        mLogger.CategoryLog(LogCategoryMethodOut, mostRecent != null ? mostRecent.ToString() : "null");
        return mostRecent;
    }

    public void OnTangoPermissions(bool permissionsGranted)
    {
        if (permissionsGranted)
        {
            mTangoApplication.Startup(mUsedAreaDescription);
        }
        else
        {
            PopCurrentScene();
        }
    }

    public void OnTangoServiceConnected()
    {
        // 処理なし
    }

    public void OnTangoServiceDisconnected()
    {
        // 処理なし
    }

    public void OnTangoPoseAvailable(TangoPoseData poseData)
    {
        mLogger.CategoryLog(LogCategoryMethodTrace, "base=" + poseData.framePair.baseFrame + ", target=" + poseData.framePair.targetFrame);
    }
}
