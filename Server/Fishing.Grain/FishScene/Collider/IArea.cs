using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public enum _eQuadTreeLeaf
    {
        UR = 0,
        UL = 1,
        LL = 2,
        LR = 3,
    }

    public interface IArea
    {
        IArea getArea(_eQuadTreeLeaf leaf);
    }

    public interface IPoint<A> where A : IArea
    {
        bool isInArea(A area);
    }

    public interface QuadNodeObject<A> where A : IArea
    {
        void add(IPoint<A> new_obj);
        void clear();
    }

    public interface QuadNodeObjectFactory<A> where A : IArea
    {
        QuadNodeObject<A> build();
    }
}
