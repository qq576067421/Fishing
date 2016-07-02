using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public class ClientPlayerMirror<TDef> : Component<TDef> where TDef : DefPlayerMirror, new()
    {
        //-------------------------------------------------------------------------
        public ClientActorMirror<DefActorMirror> CoActorMirror { get; private set; }
        public TbDataPlayer TbDataPlayer { get; private set; }
        public int LastHealthPointCur { get; private set; }
        Bt Bt { get; set; }

        //-------------------------------------------------------------------------
        public override void init()
        {
            TbDataPlayer = EbDataMgr.Instance.getData<TbDataPlayer>(Def.mPropPlayerTableId.get());

            Def.mPropLevel.OnChanged = _onPropLevelChanged;
            Def.mPropNickName.OnChanged = _onPropNickNameChanged;
            Def.mPropIcon.OnChanged = _onPropIcoChanged;

            CoActorMirror = Entity.getComponent<ClientActorMirror<DefActorMirror>>();
            CoActorMirror.initActor(Def.mPropPlayerTableId.get(), () =>
            {
            //CoActorMirror.SceneActorObj.setIcon(Def.mPropIcon.get());
            //CoActorMirror.SceneActorObj.setNickName(Def.mPropNickName.get(), Color.green);
        });
            CoActorMirror.updateMirrorActorInfo(Def.mPropNickName.get(), Def.mPropIcon.get());

            if (CoActorMirror.IsMe)
            {
                Entity et_app = EntityMgr.findFirstEntityByType<EtApp>();
                var co_app = et_app.getComponent<ClientApp<DefApp>>();
                Bt = co_app.createBt("BtPlayerMirror", Entity);
                //writeBlackboardPlayerOperateType(PlayerOperateType.None);
                writeBlackboardPosMoveTo(-10000, -10000);
                writeBlackboardLastPosMoveTo(-10000, 10000);
                writeBlackboardSelectEntity(null);
                writeBlackboardSkillItemId(0);
                writeBlackboardTaskId(0);
                writeBlackboardTaskAutoInfo(null);

                // 恢复任务自动执行
                //if (CoActorMirror.CoScene.TaskAutoInfo != null)
                //{
                //    writeBlackboardPlayerOperateType(PlayerOperateType.DoTask);
                //    writeBlackboardTaskAutoInfo(CoActorMirror.CoScene.TaskAutoInfo);
                //}
            }
        }

        //-------------------------------------------------------------------------
        public override void release()
        {
            if (Bt != null)
            {
                Bt.close();
                Bt = null;
            }
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
        // 往黑板上写入要移动到的位置
        public void writeBlackboardPosMoveTo(float x, float y)
        {
            Bt.Blackboard.setData("PosMoveToX", x);
            Bt.Blackboard.setData("PosMoveToY", y);
        }

        //-------------------------------------------------------------------------
        // 从黑板上读出要移动到的位置
        public void readBlackboardPosMoveTo(out float x, out float y)
        {
            x = (float)Bt.Blackboard.getData("PosMoveToX");
            y = (float)Bt.Blackboard.getData("PosMoveToY");
        }

        //-------------------------------------------------------------------------
        // 往黑板上写入上次要移动到的位置
        public void writeBlackboardLastPosMoveTo(float x, float y)
        {
            Bt.Blackboard.setData("LastPosMoveToX", x);
            Bt.Blackboard.setData("LastPosMoveToY", y);
        }

        //-------------------------------------------------------------------------
        // 从黑板上读出上次要移动到的位置
        public void readBlackboardLastPosMoveTo(out float x, out float y)
        {
            x = (float)Bt.Blackboard.getData("LastPosMoveToX");
            y = (float)Bt.Blackboard.getData("LastPosMoveToY");
        }

        //-------------------------------------------------------------------------
        // 往黑板上写入要施放的技能Id
        public void writeBlackboardSkillItemId(int skill_itemid)
        {
            Bt.Blackboard.setData("SkillItemId", skill_itemid);
        }

        //-------------------------------------------------------------------------
        // 往黑板上读出要施放的技能Id
        public int readBlackboardSkillItemId()
        {
            return (int)Bt.Blackboard.getData("SkillItemId");
        }

        //-------------------------------------------------------------------------
        // 往黑板上写入选中目标Entity
        public void writeBlackboardSelectEntity(Entity select_et)
        {
            Bt.Blackboard.setData("SelectEntity", select_et);
        }

        //-------------------------------------------------------------------------
        // 往黑板上读出选中目标Entity
        public Entity readBlackboardSelectEntity()
        {
            return (Entity)Bt.Blackboard.getData("SelectEntity");
        }

        //-------------------------------------------------------------------------
        // 往黑板上写入TaskId
        public void writeBlackboardTaskId(int task_id)
        {
            Bt.Blackboard.setData("TaskId", task_id);
        }

        //-------------------------------------------------------------------------
        // 往黑板上读出TaskId
        public int readBlackboardTaskId()
        {
            return (int)Bt.Blackboard.getData("TaskId");
        }

        //-------------------------------------------------------------------------
        // 往黑板上写入任务自动执行信息
        public void writeBlackboardTaskAutoInfo(TaskAutoInfo task_autoinfo)
        {
            Bt.Blackboard.setData("TaskAutoInfo", task_autoinfo);
        }

        //-------------------------------------------------------------------------
        // 往黑板上读出任务自动执行信息
        public TaskAutoInfo readBlackboardTaskAutoInfo()
        {
            return (TaskAutoInfo)Bt.Blackboard.getData("TaskAutoInfo");
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
            //if (o == null) return PlayerOperateType.None;
            PlayerOperateType player_operate_type = (PlayerOperateType)o;
            return player_operate_type;
        }

        //-------------------------------------------------------------------------
        void _onPropLevelChanged(IProp prop, object param)
        {
            // 播放升级特效
            //CoActorMirror.SceneActorObj.actorLevelUp();

            // 是自己时，浮动文字提示
            if (CoActorMirror.IsMe)
            {
                //FloatMsgInfo f_info;
                //f_info.msg = "您升级了!";
                //f_info.color = Color.green;
                //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);
            }
        }

        //-------------------------------------------------------------------------
        void _onPropNickNameChanged(IProp prop, object param)
        {
            CoActorMirror.updateMirrorActorInfo(Def.mPropNickName.get(), Def.mPropIcon.get());
        }

        //-------------------------------------------------------------------------
        void _onPropIcoChanged(IProp prop, object param)
        {
            CoActorMirror.updateMirrorActorInfo(Def.mPropNickName.get(), Def.mPropIcon.get());
        }
    }
}
