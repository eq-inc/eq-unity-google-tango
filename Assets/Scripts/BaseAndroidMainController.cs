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

    public ScreenTimeout GetScreenTimeout(int value)
    {
        CategoryLog(LogCategoryMethodIn);
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

        CategoryLog(LogCategoryMethodOut, ret);
        return ret;
    }

    public ScreenTimeout GetCurrentScreenTimeout()
    {
        CategoryLog(LogCategoryMethodIn);

        ScreenTimeout ret = GetScreenTimeout(Screen.sleepTimeout);

        CategoryLog(LogCategoryMethodOut, ret);
        return ret;
    }

    public void SetScreenTimeout(int value)
    {
        CategoryLog(LogCategoryMethodIn);

        ScreenTimeout nextScreenTimeout = GetScreenTimeout(value);
        if (nextScreenTimeout.GetValue() != Screen.sleepTimeout)
        {
            Screen.sleepTimeout = value;
        }

        CategoryLog(LogCategoryMethodOut);
    }

    public void SetScreenTimeout(ScreenTimeout nextScreenTimeout)
    {
        CategoryLog(LogCategoryMethodIn);

        if (nextScreenTimeout.GetValue() != Screen.sleepTimeout)
        {
            Screen.sleepTimeout = nextScreenTimeout.GetValue();
        }

        CategoryLog(LogCategoryMethodOut);
    }

    public void SetScreenOrientation(ScreenOrientation nextScreenOrientation)
    {
        CategoryLog(LogCategoryMethodIn);

        if (Screen.orientation != nextScreenOrientation)
        {
            Screen.orientation = nextScreenOrientation;
        }

        CategoryLog(LogCategoryMethodOut);
    }

    public void PushNextScene(string nextScene)
    {
        CategoryLog(LogCategoryMethodIn);
        lock (SceneStack)
        {
            if (SceneStack.Count == 0)
            {
                // 「最初のシーンからの別のシーン起動」が初めて発生したケースなので、このときに最初のシーンをstackに登録
                CategoryLog(LogCategoryMethodTrace, "push first scene: " + SceneManager.GetActiveScene().name);
                SceneStack.Push(SceneManager.GetActiveScene().name);
            }

            SceneStack.Push(nextScene);
            SceneManager.LoadSceneAsync(nextScene);
        }
        CategoryLog(LogCategoryMethodOut);
    }

    public void PopCurrentScene()
    {
        CategoryLog(LogCategoryMethodIn);
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
                        CategoryLog(LogCategoryMethodTrace);
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
                        }
                        else
                        {
                            CategoryLog(LogCategoryMethodTrace);
                            quitApplication = true;
                        }
                    }
                }
                else
                {
                    CategoryLog(LogCategoryMethodTrace);
                    quitApplication = true;
                }
            }
            else
            {
                CategoryLog(LogCategoryMethodTrace);
                quitApplication = true;
            }
        }

        if (quitApplication)
        {
            CategoryLog(LogCategoryMethodTrace, "BaseAndroidMainController.PopCurrentScene: call Application.Quit()");
            Application.Quit();
        }

        CategoryLog(LogCategoryMethodOut);
    }

    private ScreenTimeout mBeforeScreenTimeout = null;
    private ScreenOrientation mBeforeScreenOrientation;

    internal virtual void Start()
    {
        CategoryLog(LogCategoryMethodIn);
        CategoryLog(LogCategoryMethodOut);
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
                    CategoryLog(LogCategoryMethodTrace, "call Application.Quit");
                    Application.Quit();
                    return;
                }
            }
        }
    }

    virtual internal void OnEnable()
    {
        CategoryLog(LogCategoryMethodIn);

        mBeforeScreenTimeout = GetScreenTimeout(Screen.sleepTimeout);
        mBeforeScreenOrientation = Screen.orientation;

        CategoryLog(LogCategoryMethodOut);
    }

    virtual internal void OnDisable()
    {
        CategoryLog(LogCategoryMethodIn);

        // 元の設定に戻す
        Screen.sleepTimeout = mBeforeScreenTimeout.GetValue();
        Screen.orientation = mBeforeScreenOrientation;

        CategoryLog(LogCategoryMethodOut);
    }

    virtual internal void OnDestroy()
    {
        CategoryLog(LogCategoryMethodIn);
        CategoryLog(LogCategoryMethodOut);
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
