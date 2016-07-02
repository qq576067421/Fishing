using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;
using Ps;

public enum PlayerOperateType
{
    None = 0,
    MoveTo,
    PerformSkill,
}

public class ActorAiConfig
{
    public float PatrolRadius = 10f;// 巡逻半径
}

public class CellActorMirrorAi<TDef> : Component<TDef> where TDef : DefActorMirrorAi, new()
{
    //-------------------------------------------------------------------------
    public CellActorMirror<DefActorMirror> CoActorMirror { get; private set; }
    public ActorAiConfig ActorAiConfig { get; private set; }// Ai配置信息，只读
    Bt Bt { get; set; }

    //-------------------------------------------------------------------------
    public override void init()
    {
        EnableSave2Db = false;
        EnableNetSync = false;

        // 初始化Ai属性
        Def.mPropBtName.OnChanged = _onPropBtNameChanged;
        CoActorMirror = Entity.getComponent<CellActorMirror<DefActorMirror>>();
        ActorAiConfig = new ActorAiConfig();
    }

    //-------------------------------------------------------------------------
    public override void release()
    {
        if (Bt != null)
        {
            Bt.close();
            Bt = null;
        }

        CoActorMirror = null;
        ActorAiConfig = null;
    }

    //-------------------------------------------------------------------------
    public override void update(float elapsed_tm)
    {
        // 更新行为树
        if (Bt != null)
        {
            Bt.update(elapsed_tm);
        }
    }

    //-------------------------------------------------------------------------
    public override void handleEvent(object sender, EntityEvent e)
    {
    }

    //-------------------------------------------------------------------------
    // 往黑板上写入玩家操作类型
    public void writeBlackboardPlayerOperateType(PlayerOperateType player_operate_type)
    {
        Bt.Blackboard.setData("PlayerOperateType", player_operate_type);
    }

    //-------------------------------------------------------------------------
    // 往黑板上读出玩家操作类型
    public PlayerOperateType readBlackboardPlayerOperateType()
    {
        object o = Bt.Blackboard.getData("PlayerOperateType");
        if (o == null) return PlayerOperateType.None;
        PlayerOperateType player_operate_type = (PlayerOperateType)o;
        return player_operate_type;
    }

    //-------------------------------------------------------------------------
    void _onPropBtNameChanged(IProp prop, object param)
    {
        string bt_name = Def.mPropBtName.get();
        if (!string.IsNullOrEmpty(bt_name))
        {
            Bt = CellApp.Instance.createBt(bt_name, Entity);
        }
    }
}
