using System.Diagnostics;
using System.Text;

namespace Eq.Unity
{
    public class LogController
    {
        public const System.Int64 LogCategoryMethodIn = 0x0000000000000001 << 1;
        public const System.Int64 LogCategoryMethodTrace = 0x0000000000000001 << 2;
        public const System.Int64 LogCategoryMethodOut = 0x0000000000000001 << 3;
        public const System.Int64 LogCategoryMethodError = 0x0000000000000001 << 63;
        public System.Int64 mOutputLogCategories = 0;

        public void AppendOutputLogCategory(System.Int64 outputLogCategories)
        {
            mOutputLogCategories |= outputLogCategories;
        }

        public void RemoveOutputLogCategory(System.Int64 outputLogCategories)
        {
            mOutputLogCategories &= (~outputLogCategories);
        }

        public void SetOutputLogCategory(System.Int64 outputLogCategories)
        {
            mOutputLogCategories = outputLogCategories;
        }

        public System.Int64 GetOutputLogCategory()
        {
            return mOutputLogCategories;
        }

        public void CategoryLog(System.Int64 category, params object[] contents)
        {
            if (category == LogCategoryMethodError)
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
            else if ((mOutputLogCategories & category) == category)
            {
                switch (category)
                {
                    case LogCategoryMethodIn:
                    case LogCategoryMethodOut:
                        {
                            StringBuilder contentBuilder = new StringBuilder();
                            StackFrame lastStackFrame = new StackTrace(true).GetFrame(1);
                            contentBuilder.Append(lastStackFrame.GetMethod());
                            if (!string.IsNullOrEmpty(lastStackFrame.GetFileName()))
                            {
                                contentBuilder.Append("(")
                                                .Append(lastStackFrame.GetFileName())
                                                .Append(":")
                                                .Append(lastStackFrame.GetFileLineNumber())
                                                .Append(")");
                            }
                            contentBuilder.Append((category == LogCategoryMethodIn) ? "(IN)" : "(OUT)");

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
                    case LogCategoryMethodTrace:
                        {
                            StringBuilder contentBuilder = new StringBuilder();
                            StackFrame lastStackFrame = new StackTrace(true).GetFrame(1);

                            contentBuilder
                                .Append(lastStackFrame.GetMethod());
                            if (!string.IsNullOrEmpty(lastStackFrame.GetFileName()))
                            {
                                contentBuilder.Append("(")
                                .Append(System.IO.Path.GetFileName(lastStackFrame.GetFileName()))
                                .Append(":")
                                .Append(lastStackFrame.GetFileLineNumber())
                                .Append(")");

                            }

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
                            UnityEngine.Debug.Log(contentBuilder.ToString());
                        }
                        break;
                }
            }
        }
    }
}
