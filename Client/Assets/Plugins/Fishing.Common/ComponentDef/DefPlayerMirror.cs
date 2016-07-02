using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    // 玩家镜像
    public class DefPlayerMirror : ComponentDef
    {
        //---------------------------------------------------------------------
        [PropAttrDistribution((byte)NodeType.Cell, true, (byte)NodeType.Client)]
        public Prop<int> mPropPlayerTableId;

        [PropAttrDistribution((byte)NodeType.Cell, true, (byte)NodeType.Client)]
        public Prop<string> mPropNickName;

        [PropAttrDistribution((byte)NodeType.Cell, true, (byte)NodeType.Client)]
        public Prop<string> mPropIcon;

        [PropAttrDistribution((byte)NodeType.Cell, true, (byte)NodeType.Client)]
        public Prop<string> mPropPrefab;// Prefab

        [PropAttrDistribution((byte)NodeType.Cell, true, (byte)NodeType.Client)]
        public Prop<int> mPropLevel;

        //---------------------------------------------------------------------
        public override void defAllProp(Dictionary<string, string> map_param)
        {
            mPropPlayerTableId = defProp<int>(map_param, "PlayerTableId", 1);
            mPropNickName = defProp<string>(map_param, "NickName", "");
            mPropIcon = defProp<string>(map_param, "Icon", "");
            mPropPrefab = defProp<string>(map_param, "Prefab", "");
            mPropLevel = defProp<int>(map_param, "Level", 1);
        }
    }
}
