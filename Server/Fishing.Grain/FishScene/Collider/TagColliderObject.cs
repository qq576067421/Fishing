using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class TagColliderObject : QuadNodeObject<RectArea>
    {
        //---------------------------------------------------------------------
        List<FishCollider> mListFishCollider = new List<FishCollider>();
        List<BulletCollider> mListBulletCollider = new List<BulletCollider>();

        //---------------------------------------------------------------------
        public void add(IPoint<RectArea> new_obj)
        {
            TagCollider tag_collider = (TagCollider)new_obj;
            if (tag_collider.Tag == 0)
            {
                mListFishCollider.Add((FishCollider)tag_collider);
            }
            else
            {
                mListBulletCollider.Add((BulletCollider)tag_collider);
            }
        }

        //---------------------------------------------------------------------
        public List<FishCollider> getFishColliders()
        {
            return mListFishCollider;
        }

        //---------------------------------------------------------------------
        public List<BulletCollider> getBulletColliders()
        {
            return mListBulletCollider;
        }

        //---------------------------------------------------------------------
        public void clear()
        {
            mListFishCollider.Clear();
            mListBulletCollider.Clear();
        }
    }

    public class QuadNodeObjectFactory : QuadNodeObjectFactory<RectArea>
    {
        //---------------------------------------------------------------------
        public QuadNodeObject<RectArea> build()
        {
            return new TagColliderObject();
        }
    }
}
