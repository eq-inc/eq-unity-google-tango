using UnityEngine;

namespace com.google.android.gms.vision.face
{
    public class Face
    {
        private AndroidJavaObject mAndroidJO;

        internal AndroidJavaObject AndroidJO
        {
            get
            {
                return mAndroidJO;
            }
        }

        public Face(AndroidJavaObject androidJO)
        {
            mAndroidJO = androidJO;
        }
    }
}
