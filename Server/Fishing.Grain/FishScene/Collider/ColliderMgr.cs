using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class TagColliderMgr
    {
        //---------------------------------------------------------------------
        QuadTree<RectArea> mColliderQuadTree = null;
        List<TagCollider> mTagColliderList = new List<TagCollider>();
        Queue<TagCollider> mQueTagColliderDestroy = new Queue<TagCollider>();

        //---------------------------------------------------------------------
        public TagColliderMgr()
        {
            mColliderQuadTree = new QuadTree<RectArea>();
            mColliderQuadTree.create(2, new RectArea(new EbVector3(-480, -320, 0), new EbVector3(960, 640, 0)), new QuadNodeObjectFactory());
        }

        //---------------------------------------------------------------------
        public void update()
        {
            _removeDestroyedCollider();

            mColliderQuadTree.clear();

            foreach (var it in mTagColliderList)
            {
                mColliderQuadTree.insert(it);
            }

            TagColliderObject collider_object = null;
            foreach (var it in mColliderQuadTree)
            {
                if (it.QuadNodeObject == null) continue;
                collider_object = (TagColliderObject)it.QuadNodeObject;
                check(collider_object.getFishColliders(), collider_object.getBulletColliders());
            }
        }

        //---------------------------------------------------------------------
        void debug_log()
        {
            string str = "mColliderQuadTree::";
            TagColliderObject collider_object = null;
            foreach (var it in mColliderQuadTree)
            {
                if (it.QuadNodeObject == null) continue;
                collider_object = (TagColliderObject)it.QuadNodeObject;
                str += collider_object.getBulletColliders().Count + "::";
            }
            EbLog.Warning(str);
        }

        //---------------------------------------------------------------------
        void check(List<FishCollider> fish_collider_list, List<BulletCollider> bullet_collider_list)
        {
            foreach (var bullet in bullet_collider_list)
            {
                foreach (var fish in fish_collider_list)
                {
                    if (bullet.isIn(fish))
                    {
                        if (bullet.onCollision == null) break;
                        bullet.onCollision(fish);
                        break;
                    }
                }
            }
        }

        //---------------------------------------------------------------------
        public FishCollider newFishCollider(float center_x, float center_y, float width, float height, CLogicFish fish)
        {
            FishCollider collider = new FishCollider(center_x, center_y, width, height, fish);
            mTagColliderList.Add(collider);
            return collider;
        }

        //---------------------------------------------------------------------
        public BulletCollider newBulletCollider(float center_x, float center_y, float width, float height)
        {
            BulletCollider collider = new BulletCollider(center_x, center_y, width, height);
            mTagColliderList.Add(collider);
            return collider;
        }

        //---------------------------------------------------------------------
        public void removeCollider(TagCollider collider)
        {
            if (collider == null) return;

            mQueTagColliderDestroy.Enqueue(collider);
        }

        //---------------------------------------------------------------------
        public void destroy()
        {
            mColliderQuadTree.clear();
            mQueTagColliderDestroy.Clear();
            mTagColliderList.Clear();
        }

        //---------------------------------------------------------------------
        void _removeDestroyedCollider()
        {
            while (mQueTagColliderDestroy.Count > 0)
            {
                TagCollider c = mQueTagColliderDestroy.Dequeue();
                mTagColliderList.Remove(c);
            }
        }
    }
}
