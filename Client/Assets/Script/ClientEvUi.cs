using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GF.Common;
using GF.UCenter.Common.Portable.Models.AppClient;
using Ps;

//-----------------------------------------------------------------------------
//ui消息，点击登录界面注册账号按钮
public class EvUiLoginRegisterAccount : EntityEvent
{
    public EvUiLoginRegisterAccount() : base() { }
    public AccountRegisterInfo register_acc_data = new AccountRegisterInfo();
}

//-----------------------------------------------------------------------------
//ui消息，点击登录界面删除游客账号
public class EvUiLoginDeleteGuest : EntityEvent
{
    public EvUiLoginDeleteGuest() : base() { }
}

//-----------------------------------------------------------------------------
// Ui消息，请求找回密码
public class EvUiRequestGetPwd : EntityEvent
{
    public EvUiRequestGetPwd() : base() { }
    public string super_pwd;
}

//-----------------------------------------------------------------------------
// Ui消息，点击登陆界面登陆按钮
public class EvUiLoginClickBtnLogin : EntityEvent
{
    public EvUiLoginClickBtnLogin() : base() { }
    public string acc = "";
    public string pwd = "";
}

//-----------------------------------------------------------------------------
// Ui消息，点击登陆界面Facebook按钮
public class EvUiLoginClickBtnFacebook : EntityEvent
{
    public EvUiLoginClickBtnFacebook() : base() { }
}

//-----------------------------------------------------------------------------
// Ui消息，点击登陆界面游客按钮
public class EvUiLoginClickBtnVisiter : EntityEvent
{
    public EvUiLoginClickBtnVisiter() : base() { }
}

//-----------------------------------------------------------------------------
// Ui消息，点击主界面商店
public class EvUiClickShop : EntityEvent
{
    public EvUiClickShop() : base() { }
}

//-----------------------------------------------------------------------------
// Ui消息，点击主界面好友
public class EvUiClickFriend : EntityEvent
{
    public EvUiClickFriend() : base() { }
}

//-----------------------------------------------------------------------------
// Ui消息，点击主界面信息
public class EvUiClickMsg : EntityEvent
{
    public EvUiClickMsg() : base() { }
}

//-----------------------------------------------------------------------------
// Ui消息，点击主界面帮助
public class EvUiClickHelp : EntityEvent
{
    public EvUiClickHelp() : base() { }
}

//-----------------------------------------------------------------------------
// Ui消息，点击主界面设置
public class EvUiClickEdit : EntityEvent
{
    public EvUiClickEdit() : base() { }
}

//-----------------------------------------------------------------------------
// Ui消息，点击主界面登录
public class EvUiClickLogin : EntityEvent
{
    public EvUiClickLogin() : base() { }
}

//-----------------------------------------------------------------------------
// Ui消息，点击主界面立刻玩
public class EvUiClickPlayNow : EntityEvent
{
    public EvUiClickPlayNow() : base() { }
}

//-----------------------------------------------------------------------------
// Ui消息，点击主界面大厅
public class EvUiClickSearchDesk : EntityEvent
{
    public EvUiClickSearchDesk() : base() { }
    public DesktopSearchFilter desktop_searchfilter;
}

//-----------------------------------------------------------------------------
// Ui消息，点击锦标赛
public class EvUiClickChampionship : EntityEvent
{
    public EvUiClickChampionship() : base() { }
}

//-----------------------------------------------------------------------------
// Ui消息，点击Vip
public class EvUiClickVip : EntityEvent
{
    public EvUiClickVip() : base() { }
}

//-----------------------------------------------------------------------------
// Ui消息，点击邀请好友
public class EvUiClickInviteFriend : EntityEvent
{
    public EvUiClickInviteFriend() : base() { }
}

//-----------------------------------------------------------------------------
// Ui消息，点击坐下(下注游戏)
public class EvUiClickSeat : EntityEvent
{
    public EvUiClickSeat() : base() { }
    public byte seat_index;
}

//-----------------------------------------------------------------------------
// Ui消息，点击离开桌子
public class EvUiClickExitDesk : EntityEvent
{
    public EvUiClickExitDesk() : base() { }
}

//-----------------------------------------------------------------------------
// Ui消息，点击暂时离开
public class EvUiClickWaitWhile : EntityEvent
{
    public EvUiClickWaitWhile() : base() { }
}

//-----------------------------------------------------------------------------
// Ui消息，点击回到桌子
public class EvUiClickPlayerReturn : EntityEvent
{
    public EvUiClickPlayerReturn() : base() { }
}

//-----------------------------------------------------------------------------
// Ui消息，点击站起
public class EvUiClickOB : EntityEvent
{
    public EvUiClickOB() : base() { }
}

//-----------------------------------------------------------------------------
// Ui消息，点击取消预操作
public class EvUiClickCancelAutoAction : EntityEvent
{
    public EvUiClickCancelAutoAction() : base() { }
}

//-----------------------------------------------------------------------------
// Ui消息，点击查找朋友桌
public class EvUiClickSearchFriendsDesk : EntityEvent
{
    public EvUiClickSearchFriendsDesk() : base() { }
}

//-----------------------------------------------------------------------------
//Ui消息，点击加入牌桌（Play）
public class EvUiClickPlayInDesk : EntityEvent
{
    public EvUiClickPlayInDesk() : base() { }
    public string desk_etguid;
}

//-----------------------------------------------------------------------------
//Ui消息，点击加入牌桌（View）
public class EvUiClickViewInDesk : EntityEvent
{
    public EvUiClickViewInDesk() : base() { }
    public string desk_etguid;
}

//-----------------------------------------------------------------------------
//Ui消息，点击查找合适牌桌
public class EvUiClickFindSuitDeskTop : EntityEvent
{
    public EvUiClickFindSuitDeskTop() : base() { }
}

//-----------------------------------------------------------------------------
//Ui消息，点击离开大厅
public class EvUiClickLeaveLobby : EntityEvent
{
    public EvUiClickLeaveLobby() : base() { }
}

//-----------------------------------------------------------------------------
//Ui消息，点击更改玩家昵称
public class EvUiClickChangePlayerNickName : EntityEvent
{
    public EvUiClickChangePlayerNickName() : base() { }
    public string new_name;
}

//-----------------------------------------------------------------------------
//Ui消息，点击更改玩家签名
public class EvUiClickChangePlayerIndividualSignature : EntityEvent
{
    public EvUiClickChangePlayerIndividualSignature() : base() { }
    public string new_individual_signature;
}

//-----------------------------------------------------------------------------
//Ui消息，点击更改皮肤
public class EvUiClickChangePlayerProfileSkin : EntityEvent
{
    public EvUiClickChangePlayerProfileSkin() : base() { }
    public int skin_id;
}

//-----------------------------------------------------------------------------
//Ui消息，点击刷新IPAddress
public class EvUiClickRefreshIPAddress : EntityEvent
{
    public EvUiClickRefreshIPAddress() : base() { }
}

//-----------------------------------------------------------------------------
//Ui消息，查找好友
public class EvUiFindFriend : EntityEvent
{
    public EvUiFindFriend() : base() { }
    public string find_info;
}

//-----------------------------------------------------------------------------
//Ui消息，添加好友
public class EvUiAddFriend : EntityEvent
{
    public EvUiAddFriend() : base() { }
    public string friend_etguid;
}

//-----------------------------------------------------------------------------
//Ui消息，移除好友
public class EvUiRemoveFriend : EntityEvent
{
    public EvUiRemoveFriend() : base() { }
    public string friend_etguid;
}

//-----------------------------------------------------------------------------
//Ui消息，举报好友
public class EvUiReportFriend : EntityEvent
{
    public EvUiReportFriend() : base() { }
    public string friend_etguid;
    public ReportPlayerType report_type;
}

//-----------------------------------------------------------------------------
// Ui消息，点击发送筹码
public class EvUiClickChipTransaction : EntityEvent
{
    public EvUiClickChipTransaction() : base() { }
    public string send_target_etguid;
}

//-----------------------------------------------------------------------------
// Ui消息，确认发送筹码
public class EvUiClickConfirmChipTransaction : EntityEvent
{
    public EvUiClickConfirmChipTransaction() : base() { }
    public string send_target_etguid;
    public ulong chip;
}

//-----------------------------------------------------------------------------
// Ui消息，点击请求系统事件
public class EvUiRequestShowSystemEvent : EntityEvent
{
    public EvUiRequestShowSystemEvent() : base() { }
}

//-----------------------------------------------------------------------------
// Ui消息，点击同意添加好友
public class EvUiAgreeAddFriend : EntityEvent
{
    public EvUiAgreeAddFriend() : base() { }
    public string from_etguid;
}

//-----------------------------------------------------------------------------
// Ui消息，点击拒绝添加好友
public class EvUiRefuseAddFriend : EntityEvent
{
    public EvUiRefuseAddFriend() : base() { }
    public string from_etguid;
}

//-----------------------------------------------------------------------------
// Ui消息，点击查询该好友所在牌桌
public class EvUiRequestGetCurrentFriendPlayDesk : EntityEvent
{
    public EvUiRequestGetCurrentFriendPlayDesk() : base() { }
    public string desktop_etguid;
}

//-----------------------------------------------------------------------------
// Ui消息，发送消息
public class EvUiSendMsg : EntityEvent
{
    public EvUiSendMsg() : base() { }
    public string target_guid;
    public string msg_content;
    public bool is_text_msg;
    public ChatType chat_type;
}

//-----------------------------------------------------------------------------
// Ui消息，邀请好友一起玩
public class EvUiInviteFriendPlayTogether : EntityEvent
{
    public EvUiInviteFriendPlayTogether() : base() { }
    public string friend_guid;
}

//-----------------------------------------------------------------------------
// Ui消息，创建主界面
public class EvUiCreateMainUi : EntityEvent
{
    public EvUiCreateMainUi() : base() { }
}

//-----------------------------------------------------------------------------
// Ui消息，获取筹码排行
public class EvUiGetRankingChip : EntityEvent
{
    public EvUiGetRankingChip() : base() { }
}

//-----------------------------------------------------------------------------
// Ui消息，获取积分排行
public class EvUiGetRankingVIPPoint : EntityEvent
{
    public EvUiGetRankingVIPPoint() : base() { }
}

//-----------------------------------------------------------------------------
// Ui消息，创建兑换筹码界面
public class EvUiCreateExchangeChip : EntityEvent
{
    public EvUiCreateExchangeChip() : base() { }
}

//-----------------------------------------------------------------------------
// Ui消息，请求获取商店筹码
public class EvUiRequestGetShopChipList : EntityEvent
{
    public EvUiRequestGetShopChipList() : base() { }
}

//-----------------------------------------------------------------------------
// Ui消息，请求获取商店金币
public class EvUiRequestGetShopCoinList : EntityEvent
{
    public EvUiRequestGetShopCoinList() : base() { }
}

//-----------------------------------------------------------------------------
// Ui消息，请求购买筹码
public class EvUiRequestBuyChip : EntityEvent
{
    public EvUiRequestBuyChip() : base() { }
    public int buy_chipid;//所买筹码方案id
}

//-----------------------------------------------------------------------------
// Ui消息，请求购买金币
public class EvUiRequestBuyCoin : EntityEvent
{
    public EvUiRequestBuyCoin() : base() { }
    public int buy_coinid;//所买金币方案id
}

//-----------------------------------------------------------------------------
// Ui消息，请求获取排行玩家信息
public class EvUiRequestGetRankPlayerInfo : EntityEvent
{
    public EvUiRequestGetRankPlayerInfo() : base() { }
    public string player_etguid;
}

//-----------------------------------------------------------------------------
// Ui消息，请求好友详细信息
public class EvUiRequestPlayerInfoFriend : EntityEvent
{
    public EvUiRequestPlayerInfoFriend() : base() { }
    public string player_etguid;
}

//-----------------------------------------------------------------------------
// Ui消息，请求非好友详细信息
public class EvUiRequestPlayerInfoOther : EntityEvent
{
    public EvUiRequestPlayerInfoOther() : base() { }
    public string player_etguid;
}

//-----------------------------------------------------------------------------
// Ui消息，点击购买礼物
public class EvUiBuyGift : EntityEvent
{
    public EvUiBuyGift() : base() { }
    public int gift_id;
    public string to_etguid;
}

//-----------------------------------------------------------------------------
// Ui消息，点击出售礼物
public class EvUiSellGift : EntityEvent
{
    public EvUiSellGift() : base() { }
    public string gift_objid;
}

//-----------------------------------------------------------------------------
// Ui消息，点击移除礼物
public class EvUiRemoveGift : EntityEvent
{
    public EvUiRemoveGift() : base() { }
    public int gift_id;
}

//-----------------------------------------------------------------------------
// Ui消息，请求屏蔽或打开某人消息
public class EvUiRequestLockPlayerChat : EntityEvent
{
    public EvUiRequestLockPlayerChat() : base() { }
    public string player_etguid;
    public bool requestLock;
}

//-----------------------------------------------------------------------------
// Ui消息，请求屏蔽或打开所有桌上玩家
public class EvUiRequestLockAllDesktopPlayer : EntityEvent
{
    public EvUiRequestLockAllDesktopPlayer() : base() { }
    public bool requestLock;
}

//-----------------------------------------------------------------------------
// Ui消息，请求屏蔽或打开所有本桌旁观者
public class EvUiRequestLockAllSpectator : EntityEvent
{
    public EvUiRequestLockAllSpectator() : base() { }
    public bool requestLock;
}

//-----------------------------------------------------------------------------
// Ui消息，请求屏蔽或打开系统消息
public class EvUiRequestLockSystemChat : EntityEvent
{
    public EvUiRequestLockSystemChat() : base() { }
    public bool requestLock;
}

//-----------------------------------------------------------------------------
// Ui消息，请求取钱
public class EvUiRequestBankWithdraw : EntityEvent
{
    public EvUiRequestBankWithdraw() : base() { }
    public ulong withdraw_chip;
}

//-----------------------------------------------------------------------------
// Ui消息，请求存钱
public class EvUiRequestBankDeposit : EntityEvent
{
    public EvUiRequestBankDeposit() : base() { }
    public ulong deposit_chip;
}

//-----------------------------------------------------------------------------
//消息，图片保存结束
public class EvGetPicResultSuccess : EntityEvent
{
    public EvGetPicResultSuccess() : base() { }
    public string str_pic;
}
