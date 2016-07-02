using System.Collections.Generic;
using System.Linq;
using System.Text;
using GF.Common;
using UnityEngine;

namespace Ps
{
    public class CRenderStillSpritePool<T> where T : StillSprite
    {
        //-------------------------------------------------------------------------
        List<T> mAssignedOutSprite = new List<T>();
        Queue<T> mReturnedPoolSprite = new Queue<T>();
        StillSpriteLoader<T> mStillSpriteLoader = new StillSpriteLoader<T>();
        CRenderScene mScene = null;

        //-------------------------------------------------------------------------
        public void create(CRenderScene scene)
        {
            mScene = scene;
        }

        //-------------------------------------------------------------------------
        public T newStillSprite(string prefab_name)
        {
            _loadSpriteFromFileIfPollIsEmpty(prefab_name);
            return _getOutFromPool();
        }

        //-------------------------------------------------------------------------
        public void freeStillSprite(T still_sprite)
        {
            if (still_sprite == null) return;

#if UNITY_EDITOR
            //非常耗时，因此只在debug的时候用来定位多次释放资源的地方。
            //if (mReturnedPoolSprite.Contains(still_sprite))
            //{
            //    ViDebuger.Error("freeStillSprite::repeat free sprite::" + still_sprite.name);
            //    return;
            //}
#endif
            still_sprite.setTag("Untagged");
            //still_sprite.setTrigger(false);
            still_sprite.setScale(1);
            still_sprite.setColor(Color.white);
            still_sprite.setActive(false);

            mAssignedOutSprite.Remove(still_sprite);
            mReturnedPoolSprite.Enqueue(still_sprite);

#if UNITY_EDITOR
            still_sprite.gameObject.name = "TkFishInObjectPool";
#endif
        }

        //-------------------------------------------------------------------------
        public void destroy()
        {
            foreach (var it in mAssignedOutSprite)
            {
                it.destroy();
            }
            mAssignedOutSprite.Clear();

            foreach (var it in mReturnedPoolSprite)
            {
                it.destroy();
            }
            mReturnedPoolSprite.Clear();

            mStillSpriteLoader.destroy();
        }

        //-------------------------------------------------------------------------
        void _loadSpriteFromFileIfPollIsEmpty(string prefab_name)
        {
            if (mReturnedPoolSprite.Count > 0) return;
            mReturnedPoolSprite.Enqueue(mStillSpriteLoader.loadSpriteFromPrefab(prefab_name, mScene));
        }

        //-------------------------------------------------------------------------
        T _getOutFromPool()
        {
            T still_sprite = mReturnedPoolSprite.Dequeue();
            still_sprite.setPosition(new EbVector3(10000, 0, 0));
            mAssignedOutSprite.Add(still_sprite);
            still_sprite.setActive(true);
            return still_sprite;
        }
    }
}
