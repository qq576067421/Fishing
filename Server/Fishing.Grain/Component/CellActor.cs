using System;
using System.Collections.Generic;
using System.Text;
using Orleans;
using Orleans.Concurrency;
using GF.Common;
using Ps;

// CellActor消息，角色属性集变脏
public class EvCellActorPropDirty : EntityEvent
{
    public EvCellActorPropDirty() : base() { }
    public Dictionary<string, string> map_prop_dirty;
}

// CellActor消息，角色升级
public class EvCellActorLevelup : EntityEvent
{
    public EvCellActorLevelup() : base() { }
    public ulong actor_id;
    public int level_new;
}

public class CellActor<TDef> : Component<TDef> where TDef : DefActor, new()
{
    //-------------------------------------------------------------------------
    public int RoleTableId { get; private set; }
    public int ActorTypeId { get; private set; }
    public EffectMgr EffectMgr { get; private set; }
    bool Init { get; set; }
    float SyncPropDirtyElapsedTm { get; set; }

    //-------------------------------------------------------------------------
    public override void init()
    {
        EffectMgr = new EffectMgr();
        Init = false;
        SyncPropDirtyElapsedTm = 1f;

        Def.mPropLevel.OnChanged = _onPropLevelChanged;// 等级属性变更通知
        Def.mPropExperience.OnChanged = _onPropExperienceChanged;
        Def.mPropNickName.OnChanged = _onPropNickNameChanged;
        Def.PropGold.OnChanged = _onPropGoldChanged;
        Def.mPropIsAFK.OnChanged = _onPropIsAFKChanged;
    }

    //-------------------------------------------------------------------------
    public override void release()
    {
        Init = false;
        EffectMgr = null;
    }

    //-------------------------------------------------------------------------
    public override void update(float elapsed_tm)
    {
        if (!Init) return;

        SyncPropDirtyElapsedTm -= elapsed_tm;
        if (SyncPropDirtyElapsedTm < 0f)
        {
            SyncPropDirtyElapsedTm = 1f;

            // CellActor消息，角色属性集变脏
            var map_prop_dirty = Def.getMapPropDirtyThenClear();
            if (map_prop_dirty != null)
            {
                var ev = Publisher.genEvent<EvCellActorPropDirty>();
                ev.map_prop_dirty = map_prop_dirty;
                ev.send(this);
            }
        }
    }

    //-------------------------------------------------------------------------
    public override void handleEvent(object sender, EntityEvent e)
    {
        if (e is EvCellEquipUpdateEquip)
        {
            // CellEquip消息，装备背包更新
            var ev = (EvCellEquipUpdateEquip)e;
        }
    }

    //-------------------------------------------------------------------------
    public void initActor()
    {
        // 当前等级处理
        if (Def.mPropLevel.get() <= 0)
        {
            Def.mPropLevel.set(1);
        }

        Init = true;
    }

    //-------------------------------------------------------------------------
    public void levelup()
    {
        // 经验变化，计算是否会升级
        int level_cur = Def.mPropLevel.get();
        int level_next = level_cur + 1;

        var tb_actorlevel_cur = EbDataMgr.Instance.getData<TbDataActorLevel>(level_cur);
        var tb_actorlevel_next = EbDataMgr.Instance.getData<TbDataActorLevel>(level_next);
        if (tb_actorlevel_next == null)
        {
            // 已经升到最高级
            return;
        }

        int exp_total = tb_actorlevel_next.Experience - tb_actorlevel_cur.Experience;
        if (exp_total <= 0)
        {
            EbLog.Error("CellActor._onPropExperienceChanged() Error: exp_total<=0 level_cur=" + level_cur);
            return;
        }

        int exp_cur = Def.mPropExperience.get();
        if (exp_cur > exp_total)
        {
            int exp_left = exp_cur - exp_total;
            Def.mPropLevel.set(level_next);
            Def.mPropExperience.set(exp_left);
        }
    }

    //-------------------------------------------------------------------------
    // 等级属性改变
    void _onPropLevelChanged(IProp prop, object param)
    {
        if (!Init) return;

        // 广播角色升级Entity内部消息
        var ev = Publisher.genEvent<EvCellActorLevelup>();
        ev.actor_id = Def.mPropActorId.get();
        ev.level_new = Def.mPropLevel.get();
        ev.send(this);
    }

    //-------------------------------------------------------------------------
    // 经验属性改变
    void _onPropExperienceChanged(IProp prop, object param)
    {
        levelup();
    }

    //-------------------------------------------------------------------------
    // 昵称属性改变
    void _onPropNickNameChanged(IProp prop, object param)
    {
    }

    //-------------------------------------------------------------------------
    // 金币属性改变
    void _onPropGoldChanged(IProp prop, object param)
    {
    }

    //-------------------------------------------------------------------------
    // AFK属性改变
    void _onPropIsAFKChanged(IProp prop, object param)
    {
        // 广播玩家是否挂机消息
        var ev = Publisher.genEvent<EvCellPlayerSetAFK>();
        ev.is_afk = Def.mPropIsAFK.get();
        ev.send(this);
    }
}
