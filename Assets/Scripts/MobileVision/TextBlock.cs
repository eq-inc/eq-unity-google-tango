using UnityEngine;

namespace com.google.android.gms.vision.text
{
    public class TextBlock
    {
        private AndroidJavaObject mAndroidJO;

        internal AndroidJavaObject AndroidJO
        {
            get
            {
                return mAndroidJO;
            }
        }

        public TextBlock(AndroidJavaObject androidJO)
        {
            mAndroidJO = androidJO;
        }
    }
}
