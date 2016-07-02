using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

class CNodeEffectMgr
{
    //-------------------------------------------------------------------------
    Dictionary<string, INodeEffect> mMapNodeEffect = new Dictionary<string, INodeEffect>();

    //-------------------------------------------------------------------------
    public void create()
    {
    }

    //-------------------------------------------------------------------------
    public void destroy()
    {
        mMapNodeEffect.Clear();
    }

    //-------------------------------------------------------------------------
    public void regNodeEffect(INodeEffect effect)
    {
        mMapNodeEffect[effect.getId()] = effect;
    }

    //-------------------------------------------------------------------------
    public INodeEffect getNodeEffect(string effect_id)
    {
        foreach (var i in mMapNodeEffect.Keys)
        {
            if (effect_id.Contains(i))
            {
                return mMapNodeEffect[i];
            }
        }

        return null;
    }
}