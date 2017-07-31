using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Eq.Unity
{
    class AsyncTaskComponent : BaseAndroidBehaviour
    {
        // 処理なし
    }

    abstract public class AsyncTask<Param, Progress, Result>
    {
        private static GameObject sGlobalGameObject;
        private static AsyncTaskComponent sGlobalComponent;
        private static System.Object sMutex = new System.Object();

        private Thread mWorkThread;
        private Param[] mParameters;
        private Result mResult;
        private bool mCanceled = false;
        private bool mFinished = false;
        private ManualResetEvent mEvent = new ManualResetEvent(false);
        private Queue<Progress[]> mProgressValueQueue;
        internal LogController mLogger = new LogController();

        abstract internal Result DoInBackground(params Param[] parameters);

        public void EnableDebugLog(bool enableDebugLog)
        {
            mLogger.SetOutputLogCategory(enableDebugLog ? (LogController.LogCategoryMethodIn | LogController.LogCategoryMethodOut | LogController.LogCategoryMethodTrace) : 0);
        }

        public void Execute(params Param[] parameters)
        {
            mLogger.CategoryLog(LogController.LogCategoryMethodIn);

            if (mWorkThread == null)
            {
                mLogger.CategoryLog(LogController.LogCategoryMethodTrace, "call OnPreExecute");
                OnPreExecute();
                mWorkThread = new Thread(this.ParameterizedThreadStart);
                mLogger.CategoryLog(LogController.LogCategoryMethodTrace, "start work thread");
                mWorkThread.Start(parameters);

                StartCoroutine(Looper());
            }

            mLogger.CategoryLog(LogController.LogCategoryMethodOut);
        }

        public void Cancel()
        {
            mLogger.CategoryLog(LogController.LogCategoryMethodIn);
            mCanceled = true;

            if (!mFinished)
            {
                mWorkThread.Abort();
            }
            mLogger.CategoryLog(LogController.LogCategoryMethodOut);
        }

        public Result Get()
        {
            mLogger.CategoryLog(LogController.LogCategoryMethodIn);
            mEvent.Reset();
            if (!mFinished)
            {
                mEvent.WaitOne();
            }
            mEvent.Set();

            mLogger.CategoryLog(LogController.LogCategoryMethodOut, mResult);
            return mResult;
        }

        public bool IsCanceled()
        {
            mLogger.CategoryLog(LogController.LogCategoryMethodIn);
            mLogger.CategoryLog(LogController.LogCategoryMethodOut, mCanceled);
            return mCanceled;
        }

        private void StartCoroutine(IEnumerator enumerator)
        {
            lock (sMutex)
            {
                if(sGlobalGameObject == null)
                {
                    sGlobalGameObject = new GameObject();
                    sGlobalGameObject.name = "GlobalGameObject_for_AsyncTask";
                    sGlobalComponent = sGlobalGameObject.AddComponent<AsyncTaskComponent>();
                }
                sGlobalComponent.StartCoroutine(enumerator);
            }
        }

        private void DestroyComponent()
        {
            lock (sMutex)
            {
                if(sGlobalGameObject != null)
                {
                    UnityEngine.Object.Destroy(sGlobalGameObject);
                    sGlobalGameObject = null;
                    sGlobalComponent = null;
                }
            }
        }

        private void ParameterizedThreadStart(object obj)
        {
            mLogger.CategoryLog(LogController.LogCategoryMethodIn);
            mEvent.Reset();
            mParameters = (Param[])obj;
            mResult = DoInBackground(mParameters);
            mFinished = true;
            mEvent.Set();
            mLogger.CategoryLog(LogController.LogCategoryMethodOut);
        }

        internal virtual void OnCancelled()
        {

        }

        internal virtual void OnCancelled(Result result, params Param[] parameters)
        {

        }

        internal virtual void OnPreExecute()
        {

        }

        internal virtual void OnProgressUpdate(params Progress[] values)
        {

        }

        internal virtual void OnPostExecute(Result result, params Param[] parameters)
        {

        }

        internal void PublishProgress(params Progress[] values)
        {
            if (mProgressValueQueue == null)
            {
                mProgressValueQueue = new Queue<Progress[]>();
            }

            mProgressValueQueue.Enqueue(values);
        }

        private IEnumerator Looper()
        {
            mLogger.CategoryLog(LogController.LogCategoryMethodIn);
            while (!mFinished)
            {
                yield return OnProgressUpdateForCoroutine();
                yield return new WaitForEndOfFrame();
            }

            if (mCanceled)
            {
                if (mFinished)
                {
                    yield return OnCancelledForCoroutine(mResult, mParameters);
                }
                else
                {
                    yield return OnCancelledForCoroutine();
                }
            }
            else
            {
                yield return OnPostExecuteForCoroutine(mResult, mParameters);
            }

            mLogger.CategoryLog(LogController.LogCategoryMethodOut);
            yield return DestroyComponentForCoroutine();
        }

        private System.Object OnProgressUpdateForCoroutine()
        {
            if(mProgressValueQueue != null)
            {
                while (mProgressValueQueue.Count > 0)
                {
                    OnProgressUpdate(mProgressValueQueue.Dequeue());
                }
            }
            return new System.Object();
        }

        private System.Object OnCancelledForCoroutine()
        {
            mLogger.CategoryLog(LogController.LogCategoryMethodIn);
            OnCancelled();
            mLogger.CategoryLog(LogController.LogCategoryMethodOut);
            return new System.Object();
        }

        private System.Object OnCancelledForCoroutine(Result result, params Param[] parameters)
        {
            mLogger.CategoryLog(LogController.LogCategoryMethodIn);
            OnCancelled(result, parameters);
            mLogger.CategoryLog(LogController.LogCategoryMethodOut);
            return new System.Object();
        }

        private System.Object OnPostExecuteForCoroutine(Result result, params Param[] parameters)
        {
            mLogger.CategoryLog(LogController.LogCategoryMethodIn);
            OnPostExecute(result, parameters);
            mLogger.CategoryLog(LogController.LogCategoryMethodOut);
            return new System.Object();
        }

        private System.Object DestroyComponentForCoroutine()
        {
            mLogger.CategoryLog(LogController.LogCategoryMethodIn);
            DestroyComponent();
            mLogger.CategoryLog(LogController.LogCategoryMethodOut);
            return new System.Object();
        }
    }

    public class CallbackAsncTask<Param, Progress, Result> : AsyncTask<Param, Progress, Result>
    {
        private ICallback mCallback;

        public CallbackAsncTask(ICallback callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException();
            }
            mCallback = callback;
        }

        internal override void OnCancelled()
        {
            ICancelCallback callback = mCallback as ICancelCallback;
            if(callback != null)
            {
                callback.OnCancelled();
            }
        }

        internal override void OnCancelled(Result result, params Param[] parameters)
        {
            ICancelCallback callback = mCallback as ICancelCallback;
            if (callback != null)
            {
                callback.OnCancelled(result, parameters);
            }
        }

        internal override void OnPreExecute()
        {
            IResultCallback callback = mCallback as IResultCallback;
            if (callback != null)
            {
                callback.OnPreExecute();
            }
        }

        internal override void OnProgressUpdate(params Progress[] values)
        {
            IResultCallback callback = mCallback as IResultCallback;
            if (callback != null)
            {
                callback.OnProgressUpdate();
            }
        }

        internal override Result DoInBackground(params Param[] parameters)
        {
            return mCallback.DoInBackground(parameters);
        }

        internal override void OnPostExecute(Result result, params Param[] parameters)
        {
            IResultCallback callback = mCallback as IResultCallback;
            if (callback != null)
            {
                callback.OnPostExecute(result, parameters);
            }
        }

        public interface ICallback
        {
            Result DoInBackground(params Param[] parameters);
        }

        public interface IResultCallback : ICallback
        {
            void OnPreExecute();
            void OnProgressUpdate(params Progress[] values);
            void OnPostExecute(Result result, params Param[] parameters);
        }

        public interface ICancelCallback : ICallback
        {
            void OnCancelled();
            void OnCancelled(Result result, params Param[] parameters);
        }

        public interface IFullCallback : ICallback, IResultCallback, ICancelCallback
        {
        }
    }
}
