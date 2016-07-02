using System;
using ProtoBuf;
using GF.Common;

namespace Ps
{
    //-------------------------------------------------------------------------
    public enum NodeType : byte
    {
        Dummy = 0,
        Client,
        Base,
        Cell
    }

    //-------------------------------------------------------------------------
    public enum MethodType : short
    {
        None = 0,// 无效

        // Account
        c2sAccountRequest,
        s2cAccountResponse,
        s2cAccountNotify,

        // CoBag
        c2sBagRequest = 100,// c->s, 背包请求
        s2cBagResponse,// s->c, 背包响应
        s2cBagNotify,// s->c, 背包通知

        // CoEquip
        c2sEquipRequest = 200,// c->s, 装备请求
        s2cEquipResponse,// s->c, 装备响应
        s2cEquipNotify,// s->c, 装备通知

        // CoStatus
        c2sStatusRequest = 400,// c->s, 技能请求
        s2cStatusResponse,// s->c，技能响应
        s2cStatusNotify,// s->c，技能通知

        // CoPlayer
        c2sPlayerRequest = 600,// c->s，玩家请求
        s2cPlayerResponse,// s-c，玩家响应
        s2cPlayerNotify,// s->c, 玩家通知
        s2sPlayerRequest,// s->s，服务端之间玩家请求

        // CoPlayerMailBox
        c2sPlayerMailBoxRequest = 700,// c->s, 小秘书请求
        s2cPlayerMailBoxResponse,// s->c, 小秘书响应
        s2cPlayerMailBoxNotify,// s->c, 小秘书通知

        // CoPlayerChat
        c2sPlayerChatRequest = 800,// c->s, 聊天请求
        s2cPlayerChatResponse,// s-c，聊天响应
        s2cPlayerChatNotify,// s->c, 聊天通知

        // CoPlayerFriend
        c2sPlayerFriendRequest = 900,// c->s, 好友请求
        s2cPlayerFriendResponse,// s-c，好友响应
        s2cPlayerFriendNotify,// s->c, 好友通知

        // CoPlayerTask
        c2sPlayerTaskRequest = 1000,// c->s, 小秘书请求
        s2cPlayerTaskResponse,// s->c, 小秘书响应
        s2cPlayerTaskNotify,// s->c, 小秘书通知

        // CoPlayerTrade
        c2sPlayerTradeRequest = 1300,// c->s, 请求交易
        s2cPlayerTradeResponse,// s->c, 交易响应
        s2cPlayerTradeNotify,// s->c, 交易通知

        // CoPlayerDesktop
        c2sPlayerDesktopRequest = 1400,// c->s, 桌子请求
        s2cPlayerDesktopResponse,// s->c, 桌子响应
        s2cPlayerDesktopNotify,// s->c, 桌子通知

        // CoPlayerLobby
        c2sPlayerLobbyRequest = 1500,// c->s, 大厅请求
        s2cPlayerLobbyResponse,// s->c, 大厅响应
        s2cPlayerLobbyNotify,// s->c, 大厅通知

        // CoPlayerRanking
        c2sPlayerRankingRequest = 1600,// c->s, 排行榜请求
        s2cPlayerRankingResponse,// s->c, 排行榜响应
        s2cPlayerRankingNotify,// s->c, 排行榜通知
    }

    //-------------------------------------------------------------------------
    public class MathHelper
    {
        public static float Angle(EbVector3 from, EbVector3 to)
        {
            return (EbMath.Acos(EbMath.Clamp(EbVector3.dot(from.normalized, to.normalized), -1f, 1f)));
        }
    }

    //-------------------------------------------------------------------------
    public enum ProtocolResult : short
    {
        Success = 0,// 通用，成功
        Failed,// 失败
        Exist,// 已存在
        Timeout,// 超时
        DbError,// 通用，数据库内部错误
        LogoutNewLogin,// 重复登录，踢出前一帐号
        EnterWorldAccountVerifyFailed,// 角色进入游戏，帐号验证失败
        EnterWorldTokenError,// 角色进入游戏，Token错误
        EnterWorldTokenExpire,// 角色进入游戏，Token过期
        EnterWorldNotExistPlayer,// 角色进入游戏，角色不存在
        BagFull,// 背包满
        FriendExistFriend,// 好友已存在，不可以重复添加
        FriendIsMe,// 不可以添加自己为好友
    }

    //-------------------------------------------------------------------------
    [Serializable]
    public class VersionInfo
    {
        public ulong version_bundlecode;// 程序版本编号
        public string version_bundle;// 程序版本
        public string version_data;// 数据版本
        public string version_protocol;// 通信协议版本
    }

    //-------------------------------------------------------------------------
    public enum SlotType : byte
    {
        None = 0,// 无效
        Bag,// 背包槽
        Warehouse,// 仓库槽
        Equip,// 装备槽
        SkillInnate,// 技能槽（先天）
        SkillAcquired,// 技能槽（后天）
        SkillLife,//生活技能
        SkillLink,// 技能快捷栏槽
    }

    //-------------------------------------------------------------------------
    [Serializable]
    [ProtoContract]
    public class SlotData
    {
        [ProtoMember(1)]
        public SlotType slot_type;
        [ProtoMember(2)]
        public byte slot_id;
        [ProtoMember(3)]
        public ItemData item_data;
    }

    //-------------------------------------------------------------------------
    public class Slot
    {
        public SlotType slot_type;
        public byte slot_id;
        public Item item;

        public SlotData getData()
        {
            SlotData slot_data = new SlotData();
            slot_data.slot_type = slot_type;
            slot_data.slot_id = slot_id;
            if (item != null) slot_data.item_data = item.ItemData;
            return slot_data;
        }
    }

    //-------------------------------------------------------------------------
    // Status叠加方式
    public enum StatusOverlapType : byte
    {
        None = 0,
    }

    //-----------------------------------------------------------------------------
    // 属性操作
    public enum PropOperate : byte
    {
        Add = 0,// 加
        Subtract,// 减
        Multiply,// 乘
        Divide,// 除
    }

    //-----------------------------------------------------------------------------
    // 技能类型
    public enum SkillType : byte
    {
        None = 0,// 无效
        Initiative = 1,// 主动
        passivity = 2,// 被动
        Life = 3,// 生活    
    }

    //-----------------------------------------------------------------------------
    // 性别
    public enum SexType : byte
    {
        None = 0,// 无效
        Male,// 男
        Female,// 女
        Other,// 其他
    }

    //-----------------------------------------------------------------------------
    // 任务类型
    public enum TaskType : byte
    {
        None = 0,// 无效任务
        Dialogue = 1,// 对白任务
        KillMonster = 2,// 打怪任务，可计数
        CollectItem = 3,// 寻物任务，可计数
    }

    //-----------------------------------------------------------------------------
    // 任务分类
    public enum TaskCategory : byte
    {
        None = 0,// 无效分类
        TaskStory = 1,// 主线剧情
    }

    //-----------------------------------------------------------------------------
    public enum TipsType : byte
    {
        Loading = 1,//加载Tips
        QuitGame,//退出游戏Tips
    }
}
