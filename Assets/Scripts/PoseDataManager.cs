using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Eq.Unity
{
    class PoseDataManager
    {
        public const string TypeAreaLearning = "al";
        private const String cTemporaryPoseDataManagerUuid = "###_TemporaryUuid_###";
        private static Dictionary<string, PoseDataManager> sPoseDataManagerDictionary = new Dictionary<string, PoseDataManager>();

        public static PoseDataManager GetInstance(string uuid)
        {
            PoseDataManager ret = null;

            lock (sPoseDataManagerDictionary)
            {
                if (String.IsNullOrEmpty(uuid))
                {
                    uuid = cTemporaryPoseDataManagerUuid;
                }

                if (sPoseDataManagerDictionary.TryGetValue(uuid, out ret) == false)
                {
                    ret = new PoseDataManager(uuid);
                    sPoseDataManagerDictionary[uuid] = ret;
                }
            }

            return ret;
        }

        public static bool RemoveInstance(String uuid)
        {
            bool ret = false;

            lock (sPoseDataManagerDictionary)
            {
                if (uuid.CompareTo(cTemporaryPoseDataManagerUuid) != 0)
                {
                    sPoseDataManagerDictionary.Remove(uuid);

                    // フォルダを削除
                    StringBuilder pathBuilder = new StringBuilder(GetRootPath()).Append(Path.DirectorySeparatorChar).Append(uuid);
                    String path = pathBuilder.ToString();
                    try
                    {
                        if (Directory.Exists(path))
                        {
                            Directory.Delete(path, true);
                        }
                    }
                    catch (IOException e)
                    {
                        // 処理なし
                    }
                }
            }

            return ret;
        }

        private static String GetRootPath()
        {
#if UNITY_EDITOR
            return Directory.GetCurrentDirectory();
#else
            return Application.persistentDataPath;
#endif
        }

        private const string cJsonFileNamePrefix = "mt_";
        private const string cJsonFileNameSuffix = ".json";
        private const string cJsonFileNameRegEx = cJsonFileNamePrefix + "([a-zA-Z0-9\\-_\\+]*)\\" + cJsonFileNameSuffix;
        //private const string cJsonFileNameRegEx = cJsonFileNamePrefix + "(?<type>.*?)\\" + cJsonFileNameSuffix;
        private const string cJsonFileNameFmt = cJsonFileNamePrefix + "{0}" + cJsonFileNameSuffix;
        private string mUuid;
        private bool mTemporary = false;
        private Dictionary<string, List<PoseData>> mPoseDataListPerType = new Dictionary<string, List<PoseData>>();
        private Unity.LogController mLogger = new Unity.LogController();

        private PoseDataManager(string uuid)
        {
            if (String.IsNullOrEmpty(uuid))
            {
                throw new ArgumentException("uuid == null or empty");
            }
            else if (cTemporaryPoseDataManagerUuid.CompareTo(uuid) == 0)
            {
                mTemporary = true;
            }
            mUuid = uuid;
        }

        public void EnableDebugLog(bool enableDebugLog)
        {
            if (enableDebugLog)
            {
                mLogger.SetOutputLogCategory(Eq.Unity.LogController.LogCategoryMethodIn | Eq.Unity.LogController.LogCategoryMethodOut | Eq.Unity.LogController.LogCategoryMethodTrace);
            }
            else
            {
                mLogger.SetOutputLogCategory(0);
            }
        }

        public bool IsTemporary()
        {
            mLogger.CategoryLog(Eq.Unity.LogController.LogCategoryMethodIn);
            mLogger.CategoryLog(Eq.Unity.LogController.LogCategoryMethodOut, "mTemporary = " + mTemporary);
            return mTemporary;
        }

        public void Add(String type, PoseData poseData)
        {
            List<PoseData> poseDataList = null;

            lock (mPoseDataListPerType)
            {
                if (mPoseDataListPerType.TryGetValue(type, out poseDataList) == false)
                {
                    poseDataList = new List<PoseData>();
                    mPoseDataListPerType[type] = poseDataList;
                }
            }

            poseDataList.Add(poseData);
        }

        public void Remove(String type)
        {
            mPoseDataListPerType.Remove(type);
        }

        public int Size(String type)
        {
            int ret = 0;
            List<PoseData> tempPoseDataList = null;

            if (mPoseDataListPerType.TryGetValue(type, out tempPoseDataList))
            {
                ret = tempPoseDataList.Count;
            }

            return ret;
        }

        public int SizeAll()
        {
            int ret = 0;

            foreach (List<PoseData> tempPoseDataList in mPoseDataListPerType.Values)
            {
                ret += tempPoseDataList.Count;
            }

            return ret;
        }

        public List<PoseData>.Enumerator GetEnumerator(String type)
        {
            List<PoseData>.Enumerator ret;
            List<PoseData> tempPoseDataList = null;

            if (mPoseDataListPerType.TryGetValue(type, out tempPoseDataList))
            {
                ret = tempPoseDataList.GetEnumerator();
            }
            else
            {
                throw new ArgumentException("type: " + type + " is not in");
            }

            return ret;
        }

        public bool SetUuid(String uuid)
        {
            mLogger.CategoryLog(Eq.Unity.LogController.LogCategoryMethodIn, "mTemporary = " + mTemporary + ", next uuid = " + uuid);

            bool ret = false;

            if (mTemporary && cTemporaryPoseDataManagerUuid.CompareTo(uuid) != 0)
            {
                ret = true;
                mTemporary = false;
                lock (sPoseDataManagerDictionary)
                {
                    sPoseDataManagerDictionary.Remove(cTemporaryPoseDataManagerUuid);
                    sPoseDataManagerDictionary[uuid] = this;
                }

                mUuid = uuid;
            }

            mLogger.CategoryLog(Eq.Unity.LogController.LogCategoryMethodOut, "ret = " + ret);
            return ret;
        }

        public List<PoseData> Load(string type)
        {
            mLogger.CategoryLog(Eq.Unity.LogController.LogCategoryMethodIn, "mTemporary = " + mTemporary);
            List<PoseData> ret = null;

            lock (mPoseDataListPerType)
            {
                if (mPoseDataListPerType.TryGetValue(type, out ret) == false)
                {
                    if (!mTemporary)
                    {
                        StringBuilder pathBuilder = new StringBuilder(GetRootPath()).Append(Path.DirectorySeparatorChar).Append(mUuid);
                        String directoryPath = pathBuilder.ToString();
                        if (Directory.Exists(directoryPath))
                        {
                            string[] filesInUuidDir = Directory.GetFiles(directoryPath);
                            if (filesInUuidDir != null && filesInUuidDir.Count() > 0)
                            {
                                foreach (string fileInUuidDir in filesInUuidDir)
                                {
                                    FileInfo tempFile = new FileInfo(fileInUuidDir);
                                    Match typeText = Regex.Match(tempFile.Name, cJsonFileNameRegEx);
                                    mLogger.CategoryLog(Unity.LogController.LogCategoryMethodTrace, "Regex.Match(" + tempFile.Name + ", " + cJsonFileNameRegEx + ") = " + typeText.Success);
                                    if (typeText.Success)
                                    {
                                        mLogger.CategoryLog(Unity.LogController.LogCategoryMethodTrace, "typeText.Groups[1].Value = " + typeText.Groups[1].Value + ", type = " + type);
                                        if (typeText.Groups[1].Value.CompareTo(type) == 0)
                                        {
                                            PoseDataWrapper poseDataListWrapper = new PoseDataWrapper();
                                            poseDataListWrapper = FromJson<PoseDataWrapper>(tempFile.FullName);
                                            if (poseDataListWrapper.poseDataList != null)
                                            {
                                                mPoseDataListPerType[type] = ret = poseDataListWrapper.poseDataList;
                                                mLogger.CategoryLog(Unity.LogController.LogCategoryMethodTrace, "type: " + type + ", load " + ret.Count + " pose data");
                                            }
                                            else
                                            {
                                                mLogger.CategoryLog(Unity.LogController.LogCategoryMethodError, "fail to parse JSON: " + tempFile.FullName);
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            mLogger.CategoryLog(Unity.LogController.LogCategoryMethodError, "not exist directory: " + directoryPath);
                        }
                    }

                    if (ret == null)
                    {
                        // 一時インスタンスのため、ファイル保存していないため、そのままインスタンス管理のみ実施
                        mPoseDataListPerType[type] = ret = new List<PoseData>();
                    }
                }
            }

            mLogger.CategoryLog(Eq.Unity.LogController.LogCategoryMethodOut, "ret = " + ret);
            return ret;
        }

        public bool Save(string type)
        {
            mLogger.CategoryLog(Eq.Unity.LogController.LogCategoryMethodIn, "mTemporary = " + mTemporary);
            bool ret = false;

            if (!mTemporary)
            {
                StringBuilder pathBuilder = new StringBuilder(GetRootPath()).Append(Path.DirectorySeparatorChar).Append(mUuid);
                String directoryPath = pathBuilder.ToString();

                if (!Directory.Exists(directoryPath))
                {
                    mLogger.CategoryLog(Unity.LogController.LogCategoryMethodTrace, "create directory: " + directoryPath);
                    Directory.CreateDirectory(directoryPath);
                }

                if (Directory.Exists(directoryPath))
                {
                    List<PoseData> poseDataList = null;
                    if (mPoseDataListPerType.TryGetValue(type, out poseDataList))
                    {
                        mLogger.CategoryLog(Unity.LogController.LogCategoryMethodTrace, "type: " + type + ", save " + poseDataList.Count + " pose data");

                        pathBuilder.Append(Path.DirectorySeparatorChar).Append(String.Format(cJsonFileNameFmt, type));
                        PoseDataWrapper poseDataListWrapper = new PoseDataWrapper(poseDataList);
                        ToJson<PoseDataWrapper>(pathBuilder.ToString(), poseDataListWrapper);
                        ret = true;
                    }
                    else
                    {
                        mLogger.CategoryLog(Unity.LogController.LogCategoryMethodError, "type: " + type + " is not in list");
                    }
                }
                else
                {
                    mLogger.CategoryLog(Unity.LogController.LogCategoryMethodError, "fail to create directory: " + directoryPath);
                }
            }
            else
            {
                mLogger.CategoryLog(Unity.LogController.LogCategoryMethodError, "cannot save, because this pose data manager is still temporary. call again after it sets uuid");
            }

            mLogger.CategoryLog(Eq.Unity.LogController.LogCategoryMethodOut, "ret = " + ret);
            return ret;
        }

        public Dictionary<string, bool> SaveAll()
        {
            mLogger.CategoryLog(Eq.Unity.LogController.LogCategoryMethodIn);
            Dictionary<string, bool> ret = new Dictionary<string, bool>();

            foreach (string type in mPoseDataListPerType.Keys)
            {
                ret[type] = Save(type);
            }

            mLogger.CategoryLog(Eq.Unity.LogController.LogCategoryMethodOut);
            return ret;
        }

        internal T FromJson<T>(String filePath)
        {
            mLogger.CategoryLog(Eq.Unity.LogController.LogCategoryMethodIn);
            StreamReader reader = new StreamReader(filePath);
            StringBuilder jsonBuilder = new StringBuilder();
            char[] readBuffer = new char[8 * 1024];

            while (true)
            {
                int readSize = reader.Read(readBuffer, 0, 1);
                if (readSize > 0)
                {
                    jsonBuilder.Append(readBuffer, 0, readSize);
                }
                else
                {
                    break;
                }
            }

            mLogger.CategoryLog(Eq.Unity.LogController.LogCategoryMethodOut, "json = " + jsonBuilder.ToString());
            return JsonUtility.FromJson<T>(jsonBuilder.ToString());
        }

        internal bool ToJson<T>(String filePath, T targetObject)
        {
            mLogger.CategoryLog(Eq.Unity.LogController.LogCategoryMethodIn);
            bool ret = true;
            StreamWriter writer = null;

            try
            {
                writer = new StreamWriter(filePath);
                String jsonText = JsonUtility.ToJson(targetObject);
                mLogger.CategoryLog(Eq.Unity.LogController.LogCategoryMethodTrace, "json = " + jsonText);
                writer.Write(jsonText);
            }
            catch (Exception e)
            {
                ret = false;
                mLogger.CategoryLog(Eq.Unity.LogController.LogCategoryMethodError, e);
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
            }

            mLogger.CategoryLog(Eq.Unity.LogController.LogCategoryMethodOut);
            return ret;
        }
    }

    [Serializable]
    public class PoseDataWrapper
    {
        public List<PoseData> poseDataList;

        public PoseDataWrapper()
        {
        }

        public PoseDataWrapper(List<PoseData> poseDataList)
        {
            this.poseDataList = poseDataList;
        }
    }
}
