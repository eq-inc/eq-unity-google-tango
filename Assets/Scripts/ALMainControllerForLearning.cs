using System.Collections;
using Tango;

public class ALMainControllerForLearning : BaseALMainController
{
    private bool mLearningArea = false;

    delegate AreaDescription SaveAreaDescription();
    private IEnumerator SaveCurrentAreaDescription()
    {
        yield return InnerSaveCurrentAreaDescription();
    }

    internal AreaDescription InnerSaveCurrentAreaDescription()
    {
        CategoryLog(LogCategoryMethodIn);
        AreaDescription ret = AreaDescription.SaveCurrent();
        mTangoApplication.Shutdown();
        CategoryLog(LogCategoryMethodOut);

        return ret;
    }

    internal override bool StartTangoService()
    {
        CategoryLog(LogCategoryMethodIn);
        bool ret = true;

        if (!mLearningArea)
        {
            AreaDescription mostRecentAreaDescription = GetMostRecentAreaDescription();

            if (mostRecentAreaDescription != null)
            {
                mTangoApplication.Startup(mostRecentAreaDescription);
            }
            else
            {
                mTangoApplication.Startup(null);
            }

            mLearningArea = true;
        }
        else
        {
            ret = false;
        }

        CategoryLog(LogCategoryMethodOut, "ret = " + ret);
        return ret;
    }

    internal override bool StopTangoService()
    {
        CategoryLog(LogCategoryMethodIn);
        bool ret = true;

        if (mLearningArea)
        {
            // 停止したときに、領域学習の保存を実施
            StartCoroutine(SaveCurrentAreaDescription());
            mLearningArea = false;
        }
        else
        {
            ret = false;
        }

        CategoryLog(LogCategoryMethodOut, "ret = " + ret);
        return ret;
    }
}
