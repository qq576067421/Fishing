using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    // 角色MirrorAi
    public class DefActorMirrorAi : ComponentDef
    {
        //---------------------------------------------------------------------
        [PropAttrEditor("行为树名字")]
        [PropAttrDistribution((byte)NodeType.Cell, false)]
        public Prop<string> mPropBtName;

        [PropAttrDistribution((byte)NodeType.Cell, false)]
        public Prop<bool> mPropIsDie;// 是否死亡

        //---------------------------------------------------------------------
        public override void defAllProp(Dictionary<string, string> map_param)
        {
            mPropBtName = defProp<string>(map_param, "BtName", "");
            mPropIsDie = defProp<bool>(map_param, "IsDie", false);
        }
    }
}
