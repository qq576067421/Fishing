using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class EffectEmpty : CEffect
    {
        //---------------------------------------------------------------------
        public override void create(Dictionary<string, object> param)
        {
            throw new NotImplementedException();
        }

        //---------------------------------------------------------------------
        public override void start()
        {
            base.start();
            signDestroy();
        }

        //---------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
        }

        //---------------------------------------------------------------------
        public override void destroy()
        {
        }
    }
}
