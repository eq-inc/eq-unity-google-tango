using System.Diagnostics;
using System.Text;

namespace Eq.Util
{
    public class LogController
    {
        internal const long LogCategoryMethodIn = 0x2;
        internal const long LogCategoryMethodTrace = 0x4;
        internal const long LogCategoryMethodOut = 0x8;
        public long mOutputLogCategories = 0;

        public void AppendOutputLogCategory(long outputLogCategories)
        {
            mOutputLogCategories |= outputLogCategories;
        }

        public void RemoveOutputLogCategory(long outputLogCategories)
        {
            mOutputLogCategories &= (~outputLogCategories);
        }

        public void SetOutputLogCategory(long outputLogCategories)
        {
            mOutputLogCategories = outputLogCategories;
        }

        public long GetOutputLogCategory()
        {
            return mOutputLogCategories;
        }

        public void CategoryLog(long category, params object[] contents)
        {
            //if ((mOutputLogCategories & category) == category)
            {
                switch (category)
                {
                    case LogCategoryMethodIn:
                    case LogCategoryMethodOut:
                        {
                            StringBuilder contentBuilder = new StringBuilder();
                            contentBuilder
                                .Append(new StackTrace().GetFrame(1).GetMethod().Name)
                                .Append("(")
                                .Append(((category == LogCategoryMethodIn) ? "IN" : "OUT") + ")");
                            if (contents != null && contents.Length > 0)
                            {
                                contentBuilder.Append(": ");
                                foreach (object content in contents)
                                {
                                    contentBuilder.Append(content);
                                }
                            }

                            Debug.WriteLine(contentBuilder.ToString());
                        }
                        break;
                    case LogCategoryMethodTrace:
                        {
                            StringBuilder contentBuilder = new StringBuilder();
                            StackFrame lastStackFrame = new StackTrace(true).GetFrame(1);

                            contentBuilder
                                .Append(lastStackFrame.GetMethod())
                                .Append("(")
                                .Append(System.IO.Path.GetFileName(lastStackFrame.GetFileName()))
                                .Append(":")
                                .Append(lastStackFrame.GetFileLineNumber())
                                .Append(")");
                            if (contents != null && contents.Length > 0)
                            {
                                contentBuilder.Append(": ");
                                foreach (object content in contents)
                                {
                                    contentBuilder.Append(content);
                                }
                            }

                            UnityEngine.Debug.Log(contentBuilder.ToString());
                        }
                        break;
                    default:
                        if (contents != null && contents.Length > 0)
                        {
                            StringBuilder contentBuilder = new StringBuilder();
                            foreach (string content in contents)
                            {
                                contentBuilder.Append(content);
                            }
                            Debug.WriteLine(contentBuilder.ToString());
                        }
                        break;
                }
            }
        }
    }
}
