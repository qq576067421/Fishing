using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class ClientStatusMirror<TDef> : Component<TDef> where TDef : DefStatusMirror, new()
    {
        //-------------------------------------------------------------------------
        public ClientActorMirror<DefActorMirror> CoActorMirror { get; private set; }

        //-------------------------------------------------------------------------
        public override void init()
        {
            CoActorMirror = Entity.getComponent<ClientActorMirror<DefActorMirror>>();
        }

        //-------------------------------------------------------------------------
        public override void release()
        {
        }

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
        }
    }
}
