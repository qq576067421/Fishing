using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class EtPlayer : EntityDef
    {
        //---------------------------------------------------------------------
        public override void declareAllComponent(byte node_type)
        {
            if (node_type != (byte)NodeType.Base)
            {
                declareComponent<DefActor>();
            }
            declareComponent<DefBag>();
            declareComponent<DefEquip>();
            declareComponent<DefStatus>();
            declareComponent<DefPlayer>();
            declareComponent<DefPlayerMailBox>();
            declareComponent<DefPlayerChat>();
            declareComponent<DefPlayerFriend>();
            declareComponent<DefPlayerTask>();
            declareComponent<DefPlayerTrade>();
            declareComponent<DefPlayerLobby>();
            declareComponent<DefPlayerDesktop>();
            declareComponent<DefPlayerRanking>();
        }
    }
}
