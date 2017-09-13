using UnityEngine;

namespace com.google.android.gms.vision.barcode
{
    public class Barcode
    {
        private AndroidJavaObject mAndroidJO;

        internal AndroidJavaObject AndroidJO
        {
            get
            {
                return mAndroidJO;
            }
        }

        public Barcode(AndroidJavaObject androidJO)
        {
            mAndroidJO = androidJO;
        }
    }
}
