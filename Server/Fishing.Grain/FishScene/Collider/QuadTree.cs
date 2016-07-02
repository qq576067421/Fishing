using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class QuadTree<A> : IEnumerable<QuadNode<A>> where A : IArea
    {
        //---------------------------------------------------------------------
        public int MaxDepth { get { return mMaxDepth; } }
        QuadNode<A> mRootNode = null;
        List<QuadNode<A>> mNodes = new List<QuadNode<A>>();
        int mMaxDepth;

        //---------------------------------------------------------------------
        public void create(int depth, A area, QuadNodeObjectFactory<A> node_object_factory)
        {
            mMaxDepth = depth;
            mRootNode = new QuadNode<A>();
            mRootNode.create(0, this, area, node_object_factory);
        }

        //---------------------------------------------------------------------
        public void insert(IPoint<A> obj)
        {
            mRootNode.insert(obj);
        }

        //---------------------------------------------------------------------
        public void addNode(QuadNode<A> node)
        {
            mNodes.Add(node);
        }

        //---------------------------------------------------------------------
        public void removeNode(QuadNode<A> node)
        {
            mNodes.Remove(node);
        }

        //---------------------------------------------------------------------
        public void clear()
        {
            foreach (var it in mNodes)
            {
                it.clear();
            }
        }

        //---------------------------------------------------------------------
        public IEnumerator<QuadNode<A>> GetEnumerator()
        {
            return mNodes.GetEnumerator();
        }

        //---------------------------------------------------------------------
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (QuadNode<A> node in mNodes)
            {
                yield return node;
            }
        }
    }
}
