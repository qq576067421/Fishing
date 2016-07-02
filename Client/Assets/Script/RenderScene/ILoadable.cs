using System;
using System.Collections.Generic;
using System.Text;

namespace Ps
{
    public interface ILoadable
    {
        float Progress { get; }
        float TotalTime { get; }
        bool Loaded { get; }
        string LoadingInfo { get; }
        void load(float elapsed_tm);
    }

    public class LoadableManager
    {
        //-------------------------------------------------------------------------
        public bool Loaded
        {
            get
            {
                foreach (var it in mListLoadable)
                {
                    if (!it.Loaded) return false;
                }
                return true;
            }
        }
        public float Progress { get { return mProgress; } }
        public string LoadingInfo { get { return mLoadingInfo; } }

        //-------------------------------------------------------------------------
        float mProgress = 0;
        string mLoadingInfo = string.Empty;
        List<ILoadable> mListLoadable = new List<ILoadable>();
        ILoadable mCurrentLoading = null;
        float mTotalTime = 0f;

        //-------------------------------------------------------------------------
        public LoadableManager() { }

        //-------------------------------------------------------------------------
        public void create(params ILoadable[] loadable_array)
        {
            foreach (var it in loadable_array)
            {
                mListLoadable.Add(it);
                mTotalTime += it.TotalTime;
            }
        }

        //-------------------------------------------------------------------------
        public void destroy()
        {
            if (mListLoadable == null) return;
            mListLoadable.Clear();
            mListLoadable = null;
        }

        //-------------------------------------------------------------------------
        public void update(float elapsed_tm)
        {
            foreach (var it in mListLoadable)
            {
                if (!it.Loaded)
                {
                    it.load(elapsed_tm);
                    mLoadingInfo = it.LoadingInfo;

                    if (mTotalTime != 0)
                    {
                        mProgress = it.TotalTime * it.Progress / mTotalTime;
                    }
                    break;
                }
            }
        }
    }
}
