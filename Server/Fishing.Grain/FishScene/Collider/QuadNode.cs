using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class QuadNode<A> where A : IArea
    {
        //---------------------------------------------------------------------
        public QuadNodeObject<A> QuadNodeObject { get { return mQuadNodeObject; } }
        A mArea;
        QuadTree<A> mQuadTree = null;
        QuadNode<A>[] mChildren = null;
        QuadNodeObject<A> mQuadNodeObject = null;
        int mDepth;

        //---------------------------------------------------------------------
        public void create(int depth, QuadTree<A> tree, A area, QuadNodeObjectFactory<A> node_object_factory)
        {
            mDepth = depth;
            mQuadTree = tree;
            mArea = area;

            if (mDepth < mQuadTree.MaxDepth)
            {
                mChildren = new QuadNode<A>[4];
                mChildren[(int)_eQuadTreeLeaf.UR] = newQuadNode(_eQuadTreeLeaf.UR, node_object_factory);
                mChildren[(int)_eQuadTreeLeaf.UL] = newQuadNode(_eQuadTreeLeaf.UL, node_object_factory);
                mChildren[(int)_eQuadTreeLeaf.LL] = newQuadNode(_eQuadTreeLeaf.LL, node_object_factory);
                mChildren[(int)_eQuadTreeLeaf.LR] = newQuadNode(_eQuadTreeLeaf.LR, node_object_factory);
            }
            else
            {
                mQuadNodeObject = node_object_factory.build();
                mQuadTree.addNode(this);
            }
        }

        //---------------------------------------------------------------------
        public void insert(IPoint<A> new_obj)
        {
            if (!new_obj.isInArea(mArea)) return;

            if (mChildren == null)
            {
                mQuadNodeObject.add(new_obj);
            }
            else
            {
                foreach (var it in mChildren)
                {
                    it.insert(new_obj);
                }
            }
        }

        //---------------------------------------------------------------------
        public void clear()
        {
            if (mQuadNodeObject == null) return;
            mQuadNodeObject.clear();
        }

        //---------------------------------------------------------------------
        QuadNode<A> newQuadNode(_eQuadTreeLeaf leaf, QuadNodeObjectFactory<A> node_object_factory)
        {
            QuadNode<A> node = new QuadNode<A>();
            node.create(mDepth + 1, mQuadTree, (A)mArea.getArea(leaf), node_object_factory);
            return node;
        }
    }
}
