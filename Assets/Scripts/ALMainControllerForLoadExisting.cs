using System;
using Tango;
using UnityEngine;

public class ALMainControllerForLoadExisting : BaseALMainController
{
    internal override bool StartTangoService()
    {
        CategoryLog(LogCategoryMethodIn);
        bool ret = true;
        AreaDescription mostRecentAreaDescription = GetMostRecentAreaDescription();

        if (mostRecentAreaDescription != null)
        {
            mTangoApplication.Startup(mostRecentAreaDescription);
        }
        else
        {
            Debug.LogError("none area description file");
            ret = false;
        }

        CategoryLog(LogCategoryMethodOut, "ret = " + ret);
        return ret;
    }

    internal override bool StopTangoService()
    {
        mTangoApplication.Shutdown();
        return true;
    }
}
