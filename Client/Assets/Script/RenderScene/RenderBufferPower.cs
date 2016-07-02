using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public class CRenderBufferPower : CRenderBuffer
    {
        //-------------------------------------------------------------------------
        public CRenderBufferPower(CRenderScene scene, CRenderTurret turret, string name, List<object> param, string prefab_name)
            : base(scene, turret, name, param, prefab_name)
        {
            mTurret.setBarrelColor(new Color(1, 0, 0));
        }

        //-------------------------------------------------------------------------
        public override void destroy()
        {
            base.destroy();

            mTurret.setBarrelColor(new Color(1, 1, 1));
        }

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
            if (canDestroy()) return;
        }

        //-------------------------------------------------------------------------
        public override string getName()
        {
            return "BufPower";
        }
    }
}
