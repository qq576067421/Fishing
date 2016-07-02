using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GF.Common;

namespace Ps
{
    public class ClientEffectComponent : CEffect
    {
        //-------------------------------------------------------------------------
        protected CRenderScene mScene = null;
        protected EbVector3 mSourcePosition;
        protected EbVector3 mDestPosition;

        //-------------------------------------------------------------------------
        public override void create(Dictionary<string, object> param)
        {
            mMapParam = param;
            mScene = mMapParam["RenderScene"] as CRenderScene;

            if (mMapParam.ContainsKey("SourcePosition"))
            {
                mSourcePosition = (EbVector3)mMapParam["SourcePosition"];
            }

            if (mMapParam.ContainsKey("DestPosition"))
            {
                mDestPosition = (EbVector3)mMapParam["DestPosition"];
            }
        }

        //-------------------------------------------------------------------------
        public override void start()
        {
            base.start();
        }

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
        }

        //-------------------------------------------------------------------------
        public override void destroy()
        {
        }
    }
}
