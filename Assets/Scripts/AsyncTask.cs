using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts
{
    abstract public class AsyncTask<Param, Progress, Result> : BaseAndroidBehaviour
    {
        private Thread mWorkThread;
        private Param[] mParameters;
        private Result mResult;
        private bool mCanceled = false;
        private bool mFinished = false;
        private ManualResetEvent mEvent = new ManualResetEvent(false);
        private Queue<Progress[]> mProgressValueQueue;

        abstract internal Result DoInBackground(params Param[] parameters);

        public void Execute(params Param[] parameters)
        {
            if (mWorkThread == null)
            {
                OnPreExecute();
                mWorkThread = new Thread(this.ParameterizedThreadStart);
                mWorkThread.Start(parameters);

                StartCoroutine(Looper());
            }
        }

        public void Cancel()
        {
            mCanceled = true;

            if (!mFinished)
            {
                mWorkThread.Abort();
            }
        }

        public Result Get()
        {
            mEvent.Reset();
            if (!mFinished)
            {
                mEvent.WaitOne();
            }
            mEvent.Set();

            return mResult;
        }

        public bool IsCanceled()
        {
            return mCanceled;
        }

        private void ParameterizedThreadStart(object obj)
        {
            mEvent.Reset();
            mParameters = (Param[])obj;
            mResult = DoInBackground(mParameters);
            mFinished = true;
            mEvent.Set();
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

            yield break;
        }

        private System.Object OnProgressUpdateForCoroutine()
        {
            while (mProgressValueQueue.Count > 0)
            {
                OnProgressUpdate(mProgressValueQueue.Dequeue());
            }
            return new System.Object();
        }

        private System.Object OnCancelledForCoroutine()
        {
            OnCancelled();
            return new System.Object();
        }

        private System.Object OnCancelledForCoroutine(Result result, params Param[] parameters)
        {
            OnCancelled(result, parameters);
            return new System.Object();
        }

        private System.Object OnPostExecuteForCoroutine(Result result, params Param[] parameters)
        {
            OnPostExecute(result, parameters);
            return new System.Object();
        }
    }

    public class CallbackAsncTask<Param, Progress, Result> : AsyncTask<Param, Progress, Result>
    {
        private ICallback mCallback;

        public CallbackAsncTask(ICallback callback)
        {
            if (mCallback == null)
            {
                throw new System.ArgumentNullException();
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
