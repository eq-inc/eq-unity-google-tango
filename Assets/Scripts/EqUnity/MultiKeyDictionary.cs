using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eq.Unity
{
    public class MultiKeyDictionary<V>
    {
        internal Type[] mKeyTypes = null;
        internal Dictionary<MultiKeyValue<V>, V> mDictionary = null;

        public MultiKeyDictionary(params Type[] keyTypes)
        {
            if (keyTypes == null || keyTypes.Length == 0)
            {
                throw new ArgumentNullException("keyTypes == null || keyTypes.Length == 0: " + keyTypes);
            }

            mKeyTypes = keyTypes;
            mDictionary = new Dictionary<MultiKeyValue<V>, V>();
        }

        public int Add(object[] keys, V value)
        {
            int errorCount = 0;

            if (keys.Length == mKeyTypes.Length)
            {
                for (int i = 0, size = mKeyTypes.Length; i < size; i++)
                {
                    UnityEngine.Debug.Log("keys[" + i + "].GetType() = " + keys[i].GetType() + ", mKeyTypes[" + i + "] = " + mKeyTypes[i]);
                    if (!keys[i].GetType().Equals(mKeyTypes[i]))
                    {
                        errorCount++;
                    }
                }

                if (errorCount == 0)
                {
                    mDictionary[new MultiKeyValue<V>(this, keys)] = value;
                }
            }

            return -errorCount;
        }

        public Dictionary<object[], V> Get(int keyIndex, object key)
        {
            Dictionary<object[], V> ret = new Dictionary<object[], V>();
            MultiKeyValue<V> tempKey = new MultiKeyValue<V>(this, keyIndex, key);

            foreach(KeyValuePair<MultiKeyValue<V>, V> tempKeyValuePair in mDictionary)
            {
                MultiKeyValue<V> targetKey = tempKeyValuePair.Key;
                if (targetKey.Equals(tempKey))
                {
                    ret.Add(targetKey.mKeys, tempKeyValuePair.Value);
                }
            }

            return ret;
        }

        public bool ContainsKey(int keyIndex, object key)
        {
            //MultiKeyValue<V> searchKey = new MultiKeyValue<V>(this, keyIndex, key);
            //return mDictionary.ContainsKey(searchKey);
            MultiKeyValue<V> searchKey = new MultiKeyValue<V>(this, keyIndex, key);
            bool ret = false;

            foreach(MultiKeyValue<V> tempKey in mDictionary.Keys)
            {
                if (tempKey.Equals(searchKey))
                {
                    ret = true;
                    break;
                }
            }

            return ret;
        }

        public int Remove(int keyIndex, object key)
        {
            int ret = 0;

            foreach (KeyValuePair<MultiKeyValue<V>, V> keyPairValue in mDictionary.ToArray<KeyValuePair<MultiKeyValue<V>, V>>())
            {
                MultiKeyValue<V> multiKey = keyPairValue.Key;
                V value = keyPairValue.Value;

                if (multiKey.mKeys[keyIndex].Equals(key))
                {
                    mDictionary.Remove(multiKey);
                    ret++;
                }
            }

            return ret;
        }

        public bool Remove(object[] keys)
        {
            MultiKeyValue<V> key = new MultiKeyValue<V>(this, keys);
            return mDictionary.Remove(key);
        }

        public KeyValuePair<object[], V> ElementAt(int index)
        {
            KeyValuePair<MultiKeyValue<V>, V> tempKeyValuePair = mDictionary.ElementAt(index);
            return new KeyValuePair<object[], V>(tempKeyValuePair.Key.mKeys, tempKeyValuePair.Value);
        }

        public int Count
        {
            get { return mDictionary.Count; }
        }
    }

    internal class MultiKeyValue<V>
    {
        internal enum Mode
        {
            KeyValue,
            SearchSingleKey,
        };
        internal const int MaxSupportCount = byte.MaxValue;
        internal MultiKeyDictionary<V> mParentDictionary;
        internal Mode mMode = Mode.KeyValue;
        internal object[] mKeys;

        public MultiKeyValue(MultiKeyDictionary<V> parentDictionary, int keyIndex, object key)
        {
            if (parentDictionary == null)
            {
                throw new ArgumentNullException("parentDictionary == null");
            }
            else if (key == null)
            {
                throw new ArgumentNullException("key == null");
            }

            mParentDictionary = parentDictionary;
            mMode = Mode.SearchSingleKey;
            mKeys = new object[keyIndex + 1];
            mKeys[keyIndex] = key;
        }

        public MultiKeyValue(MultiKeyDictionary<V> parentDictionary, params object[] keys)
        {
            if(parentDictionary == null)
            {
                throw new ArgumentNullException("parentDictionary == null");
            }
            else if (keys == null)
            {
                throw new ArgumentNullException("keys == null");
            }
            else if (keys.Length > MaxSupportCount)
            {
                throw new ArgumentOutOfRangeException("keys.Length(" + keys.Length + ") > " + MaxSupportCount);
            }

            mParentDictionary = parentDictionary;
            mKeys = keys;
        }

        public override bool Equals(object obj)
        {
            bool ret = false;
            MultiKeyValue<V> tempObj = obj as MultiKeyValue<V>;

            if ((obj != null) && (tempObj != null))
            {
                if((tempObj.mMode == mMode) && (mMode == Mode.KeyValue))
                {
                    if(mKeys.Length == tempObj.mKeys.Length)
                    {
                        ret = true;
                        for (int i = 0, size = mKeys.Length; i < size; i++)
                        {
                            if (!mKeys[i].Equals(tempObj.mKeys[i]))
                            {
                                ret = false;
                                break;
                            }
                        }
                    }
                }
                else if((tempObj.mMode == mMode) && (mMode == Mode.SearchSingleKey))
                {
                    int keyIndex = tempObj.mKeys.Length - 1;
                    object key = tempObj.mKeys[keyIndex];

                    if(keyIndex < mKeys.Length)
                    {
                        ret = key.Equals(mKeys[keyIndex]);
                    }
                }
                else if(tempObj.mMode != mMode)
                {
                    if((mMode == Mode.KeyValue) && (tempObj.mMode == Mode.SearchSingleKey))
                    {
                        int keyIndex = tempObj.mKeys.Length - 1;
                        object key = tempObj.mKeys[keyIndex];

                        if (keyIndex < mKeys.Length)
                        {
                            ret = key.Equals(mKeys[keyIndex]);
                        }
                    }
                    else
                    {
                        int keyIndex = mKeys.Length - 1;
                        object key = mKeys[keyIndex];

                        if (keyIndex < mKeys.Length)
                        {
                            ret = key.Equals(tempObj.mKeys[keyIndex]);
                        }
                    }
                }
            }

            return ret;
        }

        public override int GetHashCode()
        {
            int hashCode = 0;

            if (mMode == Mode.KeyValue)
            {
                int maxOneHashCode = int.MaxValue / MaxSupportCount / mKeys.Length;

                foreach (object key in mKeys)
                {
                    hashCode += (key.GetHashCode() % maxOneHashCode);
                }

                hashCode |= (mKeys.Length << 24);
            }
            else if(mMode == Mode.SearchSingleKey)
            {
                foreach (KeyValuePair<MultiKeyValue<V>, V> tempKeyValuePair in mParentDictionary.mDictionary)
                {
                    if (tempKeyValuePair.Key.Equals(this))
                    {
                        hashCode = tempKeyValuePair.Key.GetHashCode();
                        break;
                    }
                }
            }

            return hashCode;
        }
    }
}
