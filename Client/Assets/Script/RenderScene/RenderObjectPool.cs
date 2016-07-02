using System.Collections.Generic;
using System.Linq;
using System.Text;
using GF.Common;
using UnityEngine;

namespace Ps
{
    public class CRenderObjectPool : ILoadable
    {
        //-------------------------------------------------------------------------
        CRenderStillSpritePool<FishStillSprite> mFishSpritePool = new CRenderStillSpritePool<FishStillSprite>();
        CRenderStillSpritePool<StillSprite> mStillSpritePool = new CRenderStillSpritePool<StillSprite>();

        //-------------------------------------------------------------------------
        public CRenderObjectPool(CRenderScene scene)
        {
            mFishSpritePool.create(scene);
            mStillSpritePool.create(scene);
        }

        //-------------------------------------------------------------------------
        public void update()
        {
        }

        //-------------------------------------------------------------------------
        public FishStillSprite newFishStillSprite()
        {
            return mFishSpritePool.newStillSprite("Game/fishObjectPrefab");
        }

        //-------------------------------------------------------------------------
        public void freeFishStillSprite(FishStillSprite still_sprite)
        {
#if UNITY_EDITOR
            if (still_sprite != null && still_sprite.GetType().ToString() == "StillSprite")
            {
                EbLog.Error("freeFishStillSprite::error");
            }
#endif
            still_sprite.setISpriteFishNull();
            still_sprite.detachChildren();
            mFishSpritePool.freeStillSprite(still_sprite);
        }

        //-------------------------------------------------------------------------
        public StillSprite newStillSprite()
        {
            return mStillSpritePool.newStillSprite("Game/gameObjectPrefab");
        }

        //-------------------------------------------------------------------------
        public void freeStillSprite(StillSprite still_sprite)
        {
#if UNITY_EDITOR
            if (still_sprite != null && still_sprite.GetType().ToString() == "FishStillSprite")
            {
                EbLog.Error("freeStillSprite::error");
            }
#endif

            mStillSpritePool.freeStillSprite(still_sprite);
        }

        //-------------------------------------------------------------------------
        public void destroy()
        {
            mFishSpritePool.destroy();
            mStillSpritePool.destroy();
        }

        #region ILoadable
        public float Progress { get { return mProgress; } }
        public float TotalTime { get { return (float)mPreLoadCount / mLoadCountPerSecond; } }
        public bool Loaded { get { return mLoaded; } }
        public string LoadingInfo { get { return "正在加载图片资源。。。"; } }

        int mPreLoadCount = 300;
        float mLoadCountPerSecond = 3f;
        bool mLoaded = false;
        float mProgress = 0;

        List<StillSprite> mListPreloadStillSprite = new List<StillSprite>();
        List<FishStillSprite> mListPreloadFishStillSprite = new List<FishStillSprite>();

        public void load(float elapsed_tm)
        {
            if (mLoaded) return;

            for (int i = 0; i < 3 * mLoadCountPerSecond; i++)
            {
                StillSprite still_sprite = newStillSprite();
                mListPreloadStillSprite.Add(still_sprite);

#if UNITY_EDITOR
                still_sprite.gameObject.name = "TkSprite_Loading";
#endif
            }

            for (int i = 0; i < 1 * mLoadCountPerSecond; i++)
            {
                FishStillSprite fish_still_sprite = newFishStillSprite();
                mListPreloadFishStillSprite.Add(fish_still_sprite);

#if UNITY_EDITOR
                fish_still_sprite.gameObject.name = "TkFish_Loading";
#endif
            }

            mProgress = (float)mListPreloadFishStillSprite.Count / (float)mPreLoadCount;
            if (mListPreloadFishStillSprite.Count >= mPreLoadCount)
            {
                foreach (var it in mListPreloadStillSprite)
                {
                    freeStillSprite(it);
                }
                mListPreloadStillSprite.Clear();

                foreach (var it in mListPreloadFishStillSprite)
                {
                    freeFishStillSprite(it);
                }
                mListPreloadFishStillSprite.Clear();

                mProgress = 1;
                mLoaded = true;
            }
        }
        #endregion
    }

    public class DebugStatistics
    {
        //-------------------------------------------------------------------------
        static DebugStatistics mInstance = null;
        Dictionary<string, int> mIncreaseNumber = new Dictionary<string, int>();

        //-------------------------------------------------------------------------
        public static DebugStatistics Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new DebugStatistics();
                }
                return mInstance;
            }
        }

        //-------------------------------------------------------------------------
        DebugStatistics()
        {
        }

        //-------------------------------------------------------------------------
        public void increase(string indicate_name)
        {
            if (!mIncreaseNumber.ContainsKey(indicate_name))
            {
                mIncreaseNumber[indicate_name] = 0;
            }
            mIncreaseNumber[indicate_name]++;
        }

        //-------------------------------------------------------------------------
        public void recordMaxValue(string indicate_name, int v)
        {
            if (!mIncreaseNumber.ContainsKey(indicate_name))
            {
                mIncreaseNumber.Add(indicate_name, v);
                return;
            }
            if (mIncreaseNumber[indicate_name] < v)
            {
                mIncreaseNumber[indicate_name] = v;
            }
        }

        //-------------------------------------------------------------------------
        public void setvalue(string indicate_name, int v)
        {
            mIncreaseNumber[indicate_name] = v;
        }

        //-------------------------------------------------------------------------
        public void display()
        {
            //string str = "DebugStatistics : \n";

            //foreach (var it in mIncreaseNumber)
            //{
            //    str += it.Key + " -> " + it.Value + "\n";
            //}

            //UnityEngine.Debug.LogWarning(str);
        }
    }
}
