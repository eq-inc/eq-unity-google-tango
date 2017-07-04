using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SelectObject : BaseAndroidMainController
{
    public void MainMenuClicked(BaseEventData eventData)
    {
        string nextSceneName = gameObject.name;
        PushNextScene(nextSceneName);
    }
}
