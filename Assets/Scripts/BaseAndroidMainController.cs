using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class BaseAndroidMainController : BaseAndroidBehaviour
{
    public class ScreenTimeout
    {
        private int mValue = SleepTimeout.SystemSetting;
        internal ScreenTimeout(int value)
        {
            mValue = value;
        }

        public int GetValue()
        {
            return mValue;
        }
    }
    public static ScreenTimeout NeverSleep = new ScreenTimeout(SleepTimeout.NeverSleep);
    public static ScreenTimeout SystemSetting = new ScreenTimeout(SleepTimeout.SystemSetting);
    internal static Stack<string> SceneStack = new Stack<string>();

    internal bool IsOn(string toggleName)
    {
        bool ret = false;
        GameObject toggleGameObject = GameObject.Find(toggleName);

        if (toggleGameObject != null)
        {
            ret = IsOn(toggleGameObject);
        }

        return ret;
    }

    internal bool IsOn(GameObject toggleGameObject)
    {
        bool ret = false;

        UnityEngine.UI.Toggle toggle = toggleGameObject.GetComponent<UnityEngine.UI.Toggle>();
        if (toggle != null)
        {
            ret = toggle.isOn;
        }

        return ret;
    }

    public ScreenTimeout GetScreenTimeout(int value)
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        ScreenTimeout ret = null;

        switch (value)
        {
            case SleepTimeout.NeverSleep:
                ret = NeverSleep;
                break;
            case SleepTimeout.SystemSetting:
                ret = SystemSetting;
                break;
            default:
                ret = new ScreenTimeout(value);
                break;
        }

        mLogger.CategoryLog(LogCategoryMethodOut, ret);
        return ret;
    }

    public ScreenTimeout GetCurrentScreenTimeout()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);

        ScreenTimeout ret = GetScreenTimeout(Screen.sleepTimeout);

        mLogger.CategoryLog(LogCategoryMethodOut, ret);
        return ret;
    }

    public void SetScreenTimeout(int value)
    {
        mLogger.CategoryLog(LogCategoryMethodIn, "value = " + value);

        ScreenTimeout nextScreenTimeout = GetScreenTimeout(value);
        if (nextScreenTimeout.GetValue() != Screen.sleepTimeout)
        {
            Screen.sleepTimeout = value;
        }

        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    public void SetScreenTimeout(ScreenTimeout nextScreenTimeout)
    {
        mLogger.CategoryLog(LogCategoryMethodIn, "nextScreenTimeout = " + nextScreenTimeout);

        if (nextScreenTimeout.GetValue() != Screen.sleepTimeout)
        {
            Screen.sleepTimeout = nextScreenTimeout.GetValue();
        }

        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    public void SetScreenOrientation(ScreenOrientation nextScreenOrientation)
    {
        mLogger.CategoryLog(LogCategoryMethodIn, "nextScreenOrientation = " + nextScreenOrientation);

        if (Screen.orientation != nextScreenOrientation)
        {
            Screen.orientation = nextScreenOrientation;
        }

        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    public void PushNextScene(string nextScene)
    {
        mLogger.CategoryLog(LogCategoryMethodIn, "nextScene = " + nextScene);
        lock (SceneStack)
        {
            if (SceneStack.Count == 0)
            {
                // 「最初のシーンからの別のシーン起動」が初めて発生したケースなので、このときに最初のシーンをstackに登録
                mLogger.CategoryLog(LogCategoryMethodTrace, "push first scene: " + SceneManager.GetActiveScene().name);
                SceneStack.Push(SceneManager.GetActiveScene().name);
            }

            SceneStack.Push(nextScene);
            SceneManager.LoadSceneAsync(nextScene);
        }
        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    public void PopCurrentScene()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        bool quitApplication = false;

        lock (SceneStack)
        {
            if (SceneStack.Count > 0)
            {
                string currentSceneName = SceneStack.Pop();
                if (currentSceneName != null)
                {
                    SceneManager.UnloadSceneAsync(currentSceneName);

                    if (SceneStack.Count == 0)
                    {
                        mLogger.CategoryLog(LogCategoryMethodTrace);
                        quitApplication = true;
                    }
                    else
                    {
                        string prevSceneName = SceneStack.Peek();
                        if (prevSceneName != null)
                        {
                            Scene prevScene = SceneManager.GetSceneByName(prevSceneName);
                            if (prevScene.isLoaded)
                            {
                                SceneManager.SetActiveScene(prevScene);
                            }
                            else
                            {
                                SceneManager.LoadSceneAsync(prevSceneName);
                            }

                            mLogger.CategoryLog(LogCategoryMethodTrace, "return to " + prevSceneName);
                        }
                        else
                        {
                            mLogger.CategoryLog(LogCategoryMethodTrace);
                            quitApplication = true;
                        }
                    }
                }
                else
                {
                    mLogger.CategoryLog(LogCategoryMethodTrace);
                    quitApplication = true;
                }
            }
            else
            {
                mLogger.CategoryLog(LogCategoryMethodTrace);
                quitApplication = true;
            }
        }

        if (quitApplication)
        {
            mLogger.CategoryLog(LogCategoryMethodTrace, "BaseAndroidMainController.PopCurrentScene: call Application.Quit()");
            Application.Quit();
        }

        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    private ScreenTimeout mBeforeScreenTimeout = null;
    private ScreenOrientation mBeforeScreenOrientation;

    internal virtual void Start()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    // Update is called once per frame
    virtual internal void Update()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            // エスケープキー取得
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!Back())
                {
                    mLogger.CategoryLog(LogCategoryMethodTrace, "call Application.Quit");
                    Application.Quit();
                    return;
                }
            }
        }
    }

    override internal void OnEnable()
    {
        base.OnEnable();
        mLogger.CategoryLog(LogCategoryMethodIn);

        mBeforeScreenTimeout = GetScreenTimeout(Screen.sleepTimeout);
        mBeforeScreenOrientation = Screen.orientation;

        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    virtual internal void OnDisable()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);

        // 元の設定に戻す
        Screen.sleepTimeout = mBeforeScreenTimeout.GetValue();
        Screen.orientation = mBeforeScreenOrientation;

        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    virtual internal void OnDestroy()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    virtual internal bool Back()
    {
        bool ret = false;

        // 必要に応じてoverrideすること
        if (SceneStack.Count > 0)
        {
            PopCurrentScene();
            ret = true;
        }

        return ret;
    }
}
