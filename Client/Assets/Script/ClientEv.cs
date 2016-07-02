using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GF.Common;
using Ps;

//-----------------------------------------------------------------------------
// Entity消息，FixedUpdate
public class EvEntityFixedUpdate : EntityEvent
{
    public EvEntityFixedUpdate() : base() { }
    public float tm;
}

//-----------------------------------------------------------------------------
// Entity消息，通知添加好友
public class EvEntityNotifyAddFriend : EntityEvent
{
    public EvEntityNotifyAddFriend() : base() { }
    public PlayerInfo friend_item;
}

//-----------------------------------------------------------------------------
// Entity消息，通知删除好友
public class EvEntityNotifyDeleteFriend : EntityEvent
{
    public EvEntityNotifyDeleteFriend() : base() { }
    public string friend_etguid;
}

//-----------------------------------------------------------------------------
// Entity消息，通知查找好友
public class EvEntityFindFriend : EntityEvent
{
    public EvEntityFindFriend() : base() { }
    public List<PlayerInfo> list_friend_item;
}

//-----------------------------------------------------------------------------
// Entity消息，金币更改
public class EvEntityGoldCoinChanged : EntityEvent
{
    public EvEntityGoldCoinChanged() : base() { }
}

//-----------------------------------------------------------------------------
// Entity消息，筹码更改
public class EvEntityChipChanged : EntityEvent
{
    public EvEntityChipChanged() : base() { }
}

//-----------------------------------------------------------------------------
// Entity消息，推荐好友
public class EvEntityRecommendPlayerList : EntityEvent
{
    public EvEntityRecommendPlayerList() : base() { }
    public List<PlayerInfo> list_recommend;
}

//-----------------------------------------------------------------------------
// Entity消息，推荐好友
public class EvEntityPlayerInfoChanged : EntityEvent
{
    public EvEntityPlayerInfoChanged() : base() { }
}

//-----------------------------------------------------------------------------
// Entity消息，获取在线人数
public class EvEntitySetOnLinePlayerNum : EntityEvent
{
    public EvEntitySetOnLinePlayerNum() : base() { }
    public int online_num;
}

//-----------------------------------------------------------------------------
// Entity消息，获取好友列表
public class EvEntitySetPlayerList : EntityEvent
{
    public EvEntitySetPlayerList() : base() { }
    public Dictionary<string, PlayerInfo> map_friend;
}

//-----------------------------------------------------------------------------
// Entity消息，获取大厅桌子列表
public class EvEntityGetLobbyDeskList : EntityEvent
{
    public EvEntityGetLobbyDeskList() : base() { }
    public List<DesktopInfo> list_desktop;
}

//-----------------------------------------------------------------------------
// Entity消息，获取好友所在桌子列表
public class EvEntitySearchDesktopFollowFriend : EntityEvent
{
    public EvEntitySearchDesktopFollowFriend() : base() { }
    public List<DesktopInfo> list_desktop;
}

//-----------------------------------------------------------------------------
// Entity消息，获取筹码排行
public class EvEntityGetRankingChip : EntityEvent
{
    public EvEntityGetRankingChip() : base() { }
    public List<RankingChip> list_rankingchip;
}

//-----------------------------------------------------------------------------
// Entity消息，获取积分排行
public class EvEntityGetRankingVIPPoint : EntityEvent
{
    public EvEntityGetRankingVIPPoint() : base() { }
    public List<RankingVIPPoint> list_rankingvippoint;
}

//-----------------------------------------------------------------------------
// Entity消息，获取到商店筹码列表
//public class EvEntityResponseGetShopChipList : EntityEvent
//{
//    public EvEntityResponseGetShopChipList() : base() { }
//    public List<_tShopChipInfo> list_shopchip;
//}

//-----------------------------------------------------------------------------
// Entity消息，获取到商店金币列表
//public class EvEntityResponseGetShopCoinList : EntityEvent
//{
//    public EvEntityResponseGetShopCoinList() : base() { }
//    public List<_tShopCoinInfo> list_shopcoin;
//}

//-----------------------------------------------------------------------------
// Entity消息，购买VIP
public class EvEntityBuyVIP : EntityEvent
{
    public EvEntityBuyVIP() : base() { }
    public int buy_id;
}

//-----------------------------------------------------------------------------
// Entity消息，好友详细信息返回
public class EvEntityResponsePlayerInfoFriend : EntityEvent
{
    public EvEntityResponsePlayerInfoFriend() : base() { }
    public PlayerInfoFriend player_info;
}

//-----------------------------------------------------------------------------
// Entity消息，玩家详细信息返回
public class EvEntityResponsePlayerInfoOther : EntityEvent
{
    public EvEntityResponsePlayerInfoOther() : base() { }
    public PlayerInfoOther player_info;
}

//-----------------------------------------------------------------------------
// Entity消息，角色属性更新
public class EvEntityActorPropUpdate : EntityEvent
{
    public EvEntityActorPropUpdate() : base() { }
    public string et_guid;
}

//-----------------------------------------------------------------------------
// Entity消息，角色施放技能
public class EvEntityActorPerformSkill : EntityEvent
{
    public EvEntityActorPerformSkill() : base() { }
    public int item_id;
    //public string et_target_guid;
}

//-----------------------------------------------------------------------------
// Entity消息，使用快捷技能
public class EvEntitySkillPerformSkill : EntityEvent
{
    public EvEntitySkillPerformSkill() : base() { }
    public Slot item;
}

//-----------------------------------------------------------------------------
// Entity消息，血量更改
public class EvEntityActorCurHealthChange : EntityEvent
{
    public EvEntityActorCurHealthChange() : base() { }
    public int change_value;
    public string et_guid;
}

//-----------------------------------------------------------------------------
// Entity消息，添加道具
public class EvEntityBagAddItem : EntityEvent
{
    public EvEntityBagAddItem() : base() { }
    public Item item;
}

//-----------------------------------------------------------------------------
// Entity消息，更新道具
public class EvEntityBagUpdateItem : EntityEvent
{
    public EvEntityBagUpdateItem() : base() { }
    public Item item;
}

//-----------------------------------------------------------------------------
// Entity消息，操作道具
public class EvEntityBagOperateItem : EntityEvent
{
    public EvEntityBagOperateItem() : base() { }
    public Item item;
}

//-----------------------------------------------------------------------------
// Entity消息，删除道具
public class EvEntityBagDeleteItem : EntityEvent
{
    public EvEntityBagDeleteItem() : base() { }
    public string item_objid;
}

//-----------------------------------------------------------------------------
// Entity消息，穿装备
public class EvEntityEquipTakeonEquip : EntityEvent
{
    public EvEntityEquipTakeonEquip() : base() { }
    public Item item;
}

//-----------------------------------------------------------------------------
// Entity消息，脱装备
public class EvEntityEquipTakeoffEquip : EntityEvent
{
    public EvEntityEquipTakeoffEquip() : base() { }
    public Item item;
}

//-----------------------------------------------------------------------------
// Entity消息，获取好友
public class EvEntityFriendGetFriends : EntityEvent
{
    public EvEntityFriendGetFriends() : base() { }
    public Dictionary<string, PlayerInfo> map_item;
}

//-----------------------------------------------------------------------------
// Entity消息，新好友
public class EvEntityFriendHaveNewFriend : EntityEvent
{
    public EvEntityFriendHaveNewFriend() : base() { }
    public Dictionary<string, PlayerInfo> map_item;
}

//-----------------------------------------------------------------------------
// Entity消息，加好友
public class EvEntityFriendAddAFriend : EntityEvent
{
    public EvEntityFriendAddAFriend() : base() { }
    public PlayerInfo friend_item;
}

//-----------------------------------------------------------------------------
// Entity消息，删除好友
public class EvEntityFriendDeleteFriend : EntityEvent
{
    public EvEntityFriendDeleteFriend() : base() { }
    public PlayerInfo friend_item;
}

//-----------------------------------------------------------------------------
// Entity消息，获取推荐好友
public class EvEntityFriendGetRecommandFriend : EntityEvent
{
    public EvEntityFriendGetRecommandFriend() : base() { }
    public List<PlayerInfo> list_item;
}

//-----------------------------------------------------------------------------
// Entity消息，获取好友信息
public class EvEntityFriendGetFriendInfo : EntityEvent
{
    public EvEntityFriendGetFriendInfo() : base() { }
    //public Dictionary<ToolTipPlayerOperate, string> map_operate;
    public PlayerInfo player_info;
}

//-----------------------------------------------------------------------------
// Entity消息，帮派解散
public class EvEntityFactionDissolve : EntityEvent
{
    public EvEntityFactionDissolve() : base() { }
}

//-----------------------------------------------------------------------------
// Entity消息，删邮件
public class EvEntityMailDelete : EntityEvent
{
    public EvEntityMailDelete() : base() { }
    public int id;
    public bool deleted;
}

//-----------------------------------------------------------------------------
// Entity消息，收邮件
public class EvEntityMailRecv : EntityEvent
{
    public EvEntityMailRecv() : base() { }
    public List<MailData> list_mail;
}

//-----------------------------------------------------------------------------
// Main消息，OnFingerUp
public class EvMainOnFingerUp : EntityEvent
{
    public EvMainOnFingerUp() : base() { }
    public Vector2 screen_position;
}

//-----------------------------------------------------------------------------
// Entity消息，设置是否挂机
public class EvEntitySetAFKOrNot : EntityEvent
{
    public EvEntitySetAFKOrNot() : base() { }
}

//-----------------------------------------------------------------------------
// Entity消息，创建Status
public class EvEntityStatusCreateStatus : EntityEvent
{
    public EvEntityStatusCreateStatus() : base() { }
    public string et_guid;
    public Item item;
}

//-----------------------------------------------------------------------------
// Entity消息，销毁Status
public class EvEntityStatusDestroyStatus : EntityEvent
{
    public EvEntityStatusDestroyStatus() : base() { }
    public string et_guid;
    public int item_id;
}

//-----------------------------------------------------------------------------
// Entity消息，获取Buffer
public class EvEntityStatusInit : EntityEvent
{
    public EvEntityStatusInit() : base() { }
    public string et_guid;
    public Dictionary<int, Item> map_item;
}

//-----------------------------------------------------------------------------
// Entity消息,刷新任务
public class EvEntityRefreshTask : EntityEvent
{
    public EvEntityRefreshTask() : base() { }
}

//-----------------------------------------------------------------------------
// Entity消息,小秘书邮件信息更改
public class EvEntityMailInfoChange : EntityEvent
{
    public EvEntityMailInfoChange() : base() { }
}

//-----------------------------------------------------------------------------
// Entity消息,小秘书新邮件
public class EvEntityGetNewMail : EntityEvent
{
    public EvEntityGetNewMail() : base() { }
}

//-----------------------------------------------------------------------------
// Entity消息,小秘书系统信息更新
public class EvEntitySystemInfoChange : EntityEvent
{
    public EvEntitySystemInfoChange() : base() { }
}

//-----------------------------------------------------------------------------
// Entity消息,小秘书弹提示
public class EvEntitySecretarySendBubble : EntityEvent
{
    public EvEntitySecretarySendBubble() : base() { }
    public string text;
}

//-----------------------------------------------------------------------------
// Entity消息,任务InitDone
public class EvEntityPlayerTaskInitDone : EntityEvent
{
    public EvEntityPlayerTaskInitDone() : base() { }
}

//-----------------------------------------------------------------------------
// Entity消息，注册账号成功
public class EvEntityRegisterAccountSuccess : EntityEvent
{
    public EvEntityRegisterAccountSuccess() : base() { }
    public string acc;
    public string pwd;
}
