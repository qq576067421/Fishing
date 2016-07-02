using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    // 角色
    public class DefActor : ComponentDef
    {
        //---------------------------------------------------------------------
        [PropAttrDistribution((byte)NodeType.Cell, true)]
        public Prop<bool> mPropIsBot;// 是否是机器人

        [PropAttrDistribution((byte)NodeType.Cell, true, (byte)NodeType.Client)]
        public Prop<string> mPropAccountId;// 帐号Id

        [PropAttrDistribution((byte)NodeType.Cell, true, (byte)NodeType.Client)]
        public Prop<string> mPropAccountName;// 帐号名

        [PropAttrDistribution((byte)NodeType.Cell, true, (byte)NodeType.Client)]
        public Prop<ulong> mPropActorId;// Id（10位）

        [PropAttrDistribution((byte)NodeType.Cell, true, (byte)NodeType.Client)]
        public Prop<string> mPropNickName;// 昵称

        [PropAttrDistribution((byte)NodeType.Cell, true, (byte)NodeType.Client)]
        public Prop<string> mPropIcon;// 头像

        [PropAttrDistribution((byte)NodeType.Cell, true, (byte)NodeType.Client)]
        public Prop<string> mPropIpAddress;// Ip所在地

        [PropAttrDistribution((byte)NodeType.Cell, true, (byte)NodeType.Client)]
        public Prop<bool> mPropGender;// 性别，男=true，女=false

        [PropAttrDistribution((byte)NodeType.Cell, true, (byte)NodeType.Client)]
        public Prop<string> mPropIndividualSignature;// 个性签名

        [PropAttrDistribution((byte)NodeType.Cell, true, (byte)NodeType.Client)]
        public Prop<int> mPropLevel;// 当前等级

        [PropAttrDistribution((byte)NodeType.Cell, true, (byte)NodeType.Client)]
        public Prop<int> mPropExperience;// 当前经验

        [PropAttrDistribution((byte)NodeType.Cell, true, (byte)NodeType.Client)]
        public Prop<int> PropGold;// 金币

        [PropAttrDistribution((byte)NodeType.Cell, true, (byte)NodeType.Client)]
        public Prop<int> mPropProfileSkinTableId;// 个人资料皮肤TableId

        [PropAttrDistribution((byte)NodeType.Cell, true, (byte)NodeType.Client)]
        public Prop<bool> mPropIsVIP;// 是否是VIP

        [PropAttrDistribution((byte)NodeType.Cell, true, (byte)NodeType.Client)]
        public Prop<DateTime> mPropVIPDataTime;// VIP到期时间

        [PropAttrDistribution((byte)NodeType.Cell, true, (byte)NodeType.Client)]
        public Prop<int> mPropVIPPoint;// VIP积分，决定VIP等级

        [PropAttrDistribution((byte)NodeType.Cell, true, (byte)NodeType.Client)]
        public Prop<int> mPropGameTotal;// 完成的游戏局数

        [PropAttrDistribution((byte)NodeType.Cell, true, (byte)NodeType.Client)]
        public Prop<int> mPropGameWin;// 赢的游戏局数（用于统计胜率）

        [PropAttrDistribution((byte)NodeType.Cell, true, (byte)NodeType.Client)]
        public Prop<DateTime> mPropJoinDateTime;// 加入时间

        [PropAttrDistribution((byte)NodeType.Cell, true, (byte)NodeType.Client)]
        public Prop<DateTime> mPropLastOnlineDateTime;// 上次在线时间

        [PropAttrDistribution((byte)NodeType.Cell, false, (byte)NodeType.Client)]
        public Prop<bool> mPropIsAFK;// 是否在挂机中，true=挂机，false=手动

        //---------------------------------------------------------------------
        public EffectMgr EffectMgr { get; private set; }

        //---------------------------------------------------------------------
        public override void defAllProp(Dictionary<string, string> map_param)
        {
            EffectMgr = new EffectMgr();

            mPropIsBot = defProp<bool>(map_param, "IsBot", false);
            mPropAccountId = defProp<string>(map_param, "AccountId", "");
            mPropAccountName = defProp<string>(map_param, "AccountName", "");
            mPropActorId = defProp<ulong>(map_param, "ActorId", 0);
            mPropNickName = defProp<string>(map_param, "NickName", "");
            mPropIcon = defProp<string>(map_param, "Icon", "");
            mPropIpAddress = defProp<string>(map_param, "IpAddress", "");
            mPropGender = defProp<bool>(map_param, "Gender", false);
            mPropIndividualSignature = defProp<string>(map_param, "IndividualSignature", "这家伙很懒，什么也没留下！");
            mPropLevel = defProp<int>(map_param, "Level", 1);
            mPropExperience = defProp<int>(map_param, "Experience", 0);
            PropGold = defProp<int>(map_param, "Gold", 100);
            mPropProfileSkinTableId = defProp<int>(map_param, "ProfileSkinTableId", 1);
            mPropIsVIP = defProp<bool>(map_param, "IsVIP", false);
            mPropVIPDataTime = defProp<DateTime>(map_param, "VIPDataTime", DateTime.Now);
            mPropVIPPoint = defProp<int>(map_param, "VIPPoint", 0);
            mPropGameTotal = defProp<int>(map_param, "GameTotal", 0);
            mPropGameWin = defProp<int>(map_param, "GameWin", 0);
            mPropJoinDateTime = defProp<DateTime>(map_param, "JoinDataTime", DateTime.Now);
            mPropLastOnlineDateTime = defProp<DateTime>(map_param, "LastOnlineDateTime", DateTime.Now);
            mPropIsAFK = defProp<bool>(map_param, "IsAFK", false);
        }
    }
}
