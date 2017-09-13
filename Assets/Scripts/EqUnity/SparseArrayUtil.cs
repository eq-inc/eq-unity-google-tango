using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Eq.Unity
{
    public class SparseArrayUtil<T>
    {
        public delegate T WrapAndroidJavaObject(AndroidJavaObject sourceInstanceJO);

        public static List<T> ExchangeToList(WrapAndroidJavaObject callback, AndroidJavaObject sparseArrayJO, List<T> dstList=null)
        {
            if(callback != null)
            {
                if (dstList == null)
                {
                    dstList = new List<T>();
                }

                try
                {
                    int sparseArraySize = sparseArrayJO.Call<int>("size");
                    for (int i = 0; i < sparseArraySize; i++)
                    {
                        AndroidJavaObject listItem = sparseArrayJO.Call<AndroidJavaObject>("valueAt", i);
                        dstList.Add(callback(listItem));
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(e);
                }
            }

            return dstList;
        }

        public static implicit operator SparseArrayUtil<T>(List<T> v)
        {
            throw new NotImplementedException();
        }
    }
}
