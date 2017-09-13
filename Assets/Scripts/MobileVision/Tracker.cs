using UnityEngine;

namespace com.google.android.gms.vision
{
    public class Tracker : AndroidJavaProxy
    {
        public Tracker() : base("com.google.android.gms.vision.Tracker")
        {
            // 処理なし
        }

        public void onDone(AndroidJavaObject self)
        {
            // 処理なし
            UnityEngine.Debug.Log("onDone");
        }

        public void onMissing(AndroidJavaObject self, AndroidJavaObject detectionsJO)
        {
            // 処理なし
            UnityEngine.Debug.Log("onMissing");
        }

        public void onNewItem(AndroidJavaObject self, int id, AndroidJavaObject itemJO)
        {
            // 処理なし
            UnityEngine.Debug.Log("onNewItem");
        }

        public void onUpdate(AndroidJavaObject self, AndroidJavaObject detectionsJO, AndroidJavaObject itemJO)
        {
            // 処理なし
            UnityEngine.Debug.Log("onUpdate");
        }
    }
}
