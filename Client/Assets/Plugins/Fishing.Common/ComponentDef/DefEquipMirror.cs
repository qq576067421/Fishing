using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using GF.Common;

namespace Ps
{
    // 装备Mirror
    public class DefEquipMirror : ComponentDef
    {
        //---------------------------------------------------------------------
        [PropAttrDistribution((byte)NodeType.Cell, false, (byte)NodeType.Client)]
        public Prop<int> mPropWeaponItemId;

        //---------------------------------------------------------------------
        public override void defAllProp(Dictionary<string, string> map_param)
        {
            mPropWeaponItemId = defProp<int>(map_param, "WeaponItemId", 0);
        }
    }
}
