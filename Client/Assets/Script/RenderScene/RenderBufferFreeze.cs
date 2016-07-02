using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public class CRenderBufferFreeze : CRenderBuffer
    {
        //-------------------------------------------------------------------------
        public CRenderBufferFreeze(CRenderScene scene, CRenderTurret turret, string name, List<object> param, string prefab_name)
            : base(scene, turret, name, param, prefab_name)
        {
            //mScene.getListener().onSceneShowInfo("创建冻结buffer");
        }

        //-------------------------------------------------------------------------
        public override void destroy()
        {
            base.destroy();

            //mScene.getListener().onSceneShowInfo("销毁冻结buffer");
        }

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
            if (canDestroy()) return;
        }

        //-------------------------------------------------------------------------
        public override string getName()
        {
            return "BufFreeze";
        }
    }
}
