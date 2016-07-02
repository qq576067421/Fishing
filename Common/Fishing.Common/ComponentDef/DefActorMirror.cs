using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    // 角色Mirror
    public class DefActorMirror : ComponentDef
    {
        //---------------------------------------------------------------------
        [PropAttrDistribution((byte)NodeType.Cell, false, (byte)NodeType.Client)]
        public Prop<byte> PropActorIdInDesktop;// 玩家在桌内的临时Id

        [PropAttrDistribution((byte)NodeType.Cell, false, (byte)NodeType.Client)]
        public Prop<ulong> mPropActorId;

        [PropAttrDistribution((byte)NodeType.Cell, true)]
        public Prop<bool> mPropIsBot;// 是否是机器人

        [PropAttrDistribution((byte)NodeType.Cell, false, (byte)NodeType.Client)]
        public Prop<string> mPropNickName;// 昵称

        [PropAttrDistribution((byte)NodeType.Cell, false, (byte)NodeType.Client)]
        public Prop<string> mPropIcon;// 头像

        [PropAttrDistribution((byte)NodeType.Cell, true, (byte)NodeType.Client)]
        public Prop<string> mPropIpAddress;// Ip所在地

        [PropAttrDistribution((byte)NodeType.Cell, false, (byte)NodeType.Client)]
        public Prop<int> PropGold;// 金币

        [PropAttrDistribution((byte)NodeType.Cell, false, (byte)NodeType.Client)]
        public Prop<string> mPropCurrentGiftEtGuid;// 当前礼物EtGuid

        [PropAttrDistribution((byte)NodeType.Cell, false, (byte)NodeType.Client)]
        public Prop<float> mPropWaitWhileTime;// // 暂离等待时间（单位：秒）

        //---------------------------------------------------------------------
        public override void defAllProp(Dictionary<string, string> map_param)
        {
            PropActorIdInDesktop = defProp<byte>(map_param, "PropActorIdInDesktop", 0, false);
            mPropActorId = defProp<ulong>(map_param, "ActorId", 0, false);
            mPropIsBot = defProp<bool>(map_param, "IsBot", false);
            mPropNickName = defProp<string>(map_param, "NickName", "");
            mPropIcon = defProp<string>(map_param, "Icon", "");
            mPropIpAddress = defProp<string>(map_param, "IpAddress", "");
            PropGold = defProp<int>(map_param, "Gold", 0);
            mPropCurrentGiftEtGuid = defProp<string>(map_param, "CurrentGiftEtGuid", "");
            mPropWaitWhileTime = defProp<float>(map_param, "WaitWhileTime", 60f);
        }
    }
}
