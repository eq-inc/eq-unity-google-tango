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

        private const string cJsonFileNamePrefix = "mt_";
        private const string cJsonFileNameSuffix = ".json";
        private const string cJsonFileNameRegEx = cJsonFileNamePrefix + "([a-zA-Z0-9\\-_\\+]*)\\" + cJsonFileNameSuffix;
        private const string cJsonFileNameFmt = cJsonFileNamePrefix + "{0}" + cJsonFileNameSuffix;
        private string mUuid;
        private bool mTemporary = false;
        private Dictionary<string, List<PoseData>> mPoseDataListPerType = new Dictionary<string, List<PoseData>>();
        private Eq.Util.LogController mLogger = new Eq.Util.LogController();

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
                mLogger.SetOutputLogCategory(Eq.Util.LogController.LogCategoryMethodIn | Eq.Util.LogController.LogCategoryMethodOut | Eq.Util.LogController.LogCategoryMethodTrace);
            }
            else
            {
                mLogger.SetOutputLogCategory(0);
            }
        }

        public bool IsTemporary()
        {
            mLogger.CategoryLog(Eq.Util.LogController.LogCategoryMethodIn);
            mLogger.CategoryLog(Eq.Util.LogController.LogCategoryMethodOut, "mTemporary = " + mTemporary);
            return mTemporary;
        }

        public bool SetUuid(String uuid)
        {
            mLogger.CategoryLog(Eq.Util.LogController.LogCategoryMethodIn, "mTemporary = " + mTemporary + ", next uuid = " + uuid);

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

            mLogger.CategoryLog(Eq.Util.LogController.LogCategoryMethodOut, "ret = " + ret);
            return ret;
        }

        public List<PoseData> Load(String rootDataPath, string type)
        {
            mLogger.CategoryLog(Eq.Util.LogController.LogCategoryMethodIn, "mTemporary = " + mTemporary);
            List<PoseData> ret = null;

            lock (mPoseDataListPerType)
            {
                if (mPoseDataListPerType.TryGetValue(type, out ret) == false)
                {
                    if (!mTemporary)
                    {
                        StringBuilder pathBuilder = new StringBuilder(rootDataPath).Append(Path.DirectorySeparatorChar).Append(mUuid);
                        String directoryPath = pathBuilder.ToString();
                        mLogger.CategoryLog(Eq.Util.LogController.LogCategoryMethodTrace, "path = " + directoryPath);
                        if (Directory.Exists(directoryPath))
                        {
                            string[] filesInUuidDir = Directory.GetFiles(directoryPath);
                            if (filesInUuidDir != null && filesInUuidDir.Count() > 0)
                            {
                                foreach (string fileInUuidDir in filesInUuidDir)
                                {
                                    FileInfo tempFile = new FileInfo(fileInUuidDir);
                                    Match typeText = Regex.Match(tempFile.Name, cJsonFileNameRegEx);
                                    if (typeText.Success)
                                    {
                                        if (typeText.Value.CompareTo(type) == 0)
                                        {
                                            ret = FromJson<List<PoseData>>(tempFile.FullName);
                                            if (ret != null)
                                            {
                                                mPoseDataListPerType[type] = ret;
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if(ret == null)
                    {
                        // 一時インスタンスのため、ファイル保存していないため、そのままインスタンス管理のみ実施
                        mPoseDataListPerType[type] = ret = new List<PoseData>();
                    }
                }
            }

            mLogger.CategoryLog(Eq.Util.LogController.LogCategoryMethodOut, "ret = " + ret);
            return ret;
        }

        public bool Save(String rootDataPath, string type)
        {
            mLogger.CategoryLog(Eq.Util.LogController.LogCategoryMethodIn, "mTemporary = " + mTemporary);
            bool ret = false;

            if (!mTemporary)
            {
                StringBuilder pathBuilder = new StringBuilder(rootDataPath).Append(Path.DirectorySeparatorChar).Append(mUuid);
                String directoryPath = pathBuilder.ToString();

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                if (Directory.Exists(directoryPath))
                {
                    List<PoseData> poseDataList = null;
                    if (mPoseDataListPerType.TryGetValue(type, out poseDataList))
                    {
                        pathBuilder.Append(Path.DirectorySeparatorChar).Append(String.Format(cJsonFileNameFmt, type));
                        ToJson<List<PoseData>>(pathBuilder.ToString(), poseDataList);
                    }
                    else
                    {
                        Debug.LogError("type: " + type + " is not in list");
                    }
                }
            }
            else
            {
                Debug.LogError("cannot save, because this pose data manager is still temporary. call again after it sets uuid");
            }

            mLogger.CategoryLog(Eq.Util.LogController.LogCategoryMethodOut, "ret = " + ret);
            return ret;
        }

        public Dictionary<string, bool> SaveAll(String rootDataPath)
        {
            mLogger.CategoryLog(Eq.Util.LogController.LogCategoryMethodIn);
            Dictionary<string, bool> ret = new Dictionary<string, bool>();

            foreach (string type in mPoseDataListPerType.Keys)
            {
                ret[type] = Save(rootDataPath, type);
            }

            mLogger.CategoryLog(Eq.Util.LogController.LogCategoryMethodOut);
            return ret;
        }

        internal T FromJson<T>(String filePath)
        {
            mLogger.CategoryLog(Eq.Util.LogController.LogCategoryMethodIn);
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

            mLogger.CategoryLog(Eq.Util.LogController.LogCategoryMethodOut, "json = " + jsonBuilder.ToString());
            return JsonUtility.FromJson<T>(jsonBuilder.ToString());
        }

        internal bool ToJson<T>(String filePath, T targetObject)
        {
            mLogger.CategoryLog(Eq.Util.LogController.LogCategoryMethodIn);
            bool ret = true;
            StreamWriter writer = null;

            try
            {
                writer = new StreamWriter(filePath);
                writer.Write(JsonUtility.ToJson(targetObject));
            }
            catch (Exception e)
            {
                ret = false;
                Debug.LogError(e);
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
            }

            mLogger.CategoryLog(Eq.Util.LogController.LogCategoryMethodOut);
            return ret;
        }
    }
}
