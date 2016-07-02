using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public class ClientActor<TDef> : Component<TDef> where TDef : DefActor, new()
    {
        //-------------------------------------------------------------------------
        public EffectMgr EffectMgr { get; private set; }
        int LastLevel { get; set; }

        //-------------------------------------------------------------------------
        public override void init()
        {
            EffectMgr = new EffectMgr();

            LastLevel = Def.mPropLevel.get();
            Def.mPropExperience.OnChanged = _onPropExperienceChanged;
            Def.mPropLevel.OnChanged = _onPropLevelChanged;
            Def.mPropIsAFK.OnChanged = _onPropIsAFKChanged;
            Def.PropGold.OnChanged = _onPropGoldChanged;
            Def.mPropIpAddress.OnChanged = _onPropIpAddressChanged;
            Def.mPropNickName.OnChanged = _onPropNickNameChanged;
            Def.mPropIndividualSignature.OnChanged = _onPropIndividualSignatureChanged;

            _showExperience();
        }

        //-------------------------------------------------------------------------
        public override void release()
        {
        }

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
        }

        //-------------------------------------------------------------------------
        public override void handleEvent(object sender, EntityEvent e)
        {
        }

        //-------------------------------------------------------------------------
        void _onPropExperienceChanged(IProp prop, object param)
        {
            _showExperience();
        }

        //-------------------------------------------------------------------------
        void _onPropLevelChanged(IProp prop, object param)
        {
            if (LastLevel != Def.mPropLevel.get())
            {
                LastLevel = Def.mPropLevel.get();

                //FloatMsgInfo f_info;
                //f_info.msg = string.Format("您升级到{0}级！", Def.mPropLevel.get());
                //f_info.color = Color.green;
                //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);
            }
        }

        //-------------------------------------------------------------------------
        void _onPropIsAFKChanged(IProp prop, object param)
        {
            //UiMgr.Instance.getEventPublisherEntityToUi().genEvent<EvEntitySetAFKOrNot>().send(null);

            //string msg = "";
            //UnityEngine.Color color;
            //if (Def.mPropIsAFK.get())
            //{
            //    msg = "切换到挂机模式!";
            //    color = UnityEngine.Color.green;
            //}
            //else
            //{
            //    msg = "切换到手动模式!";
            //    color = UnityEngine.Color.red;
            //}

            //FloatMsgInfo f_info;
            //f_info.msg = msg;
            //f_info.color = color;
            //UiMgr.Instance.FloatMsgMgr.createFloatMsg(f_info);
        }

        //-------------------------------------------------------------------------
        void _onPropGoldChanged(IProp prop, object param)
        {
            EntityMgr.getDefaultEventPublisher().genEvent<EvEntityGoldCoinChanged>().send(null);
        }

        //-------------------------------------------------------------------------
        void _onPropNickNameChanged(IProp prop, object param)
        {
            //UiMgr.Instance.destroyCurrentUi<UiMbWaiting>();

            //UiMbPlayerProfile ui_profile = UiMgr.Instance.getCurrentUi<UiMbPlayerProfile>();
            //if (ui_profile != null)
            //{
            //    ui_profile.playerInfoChanged();
            //}
            //EntityMgr.getDefaultEventPublisher().genEvent<EvEntityPlayerInfoChanged>().send(null);
        }

        //-------------------------------------------------------------------------
        void _onPropIndividualSignatureChanged(IProp prop, object param)
        {
            //UiMgr.Instance.destroyCurrentUi<UiMbWaiting>();

            //UiMbPlayerProfile ui_profile = UiMgr.Instance.getCurrentUi<UiMbPlayerProfile>();
            //if (ui_profile != null)
            //{
            //    ui_profile.playerInfoChanged();
            //}
        }

        //-------------------------------------------------------------------------
        void _onPropIpAddressChanged(IProp prop, object param)
        {
            //UiMbPlayerProfile ui_profile = UiMgr.Instance.getCurrentUi<UiMbPlayerProfile>();
            //if (ui_profile != null)
            //{
            //    ui_profile.playerInfoChanged();
            //}
        }

        //-------------------------------------------------------------------------
        void _showExperience()
        {
            // 经验变化
            //UiMbMain main = UiMgr.Instance.getCurrentUi<UiMbMain>();
            //int level_cur = Def.mPropLevel.get();
            //int level_next = level_cur + 1;
            //int exp_cur = Def.mPropExperience.get();

            //var tb_actorlevel_cur = EbDataMgr.Instance.getData<TbDataActorLevel>(level_cur);
            //var tb_actorlevel_next = EbDataMgr.Instance.getData<TbDataActorLevel>(level_next);
            //if (tb_actorlevel_next == null)
            //{
            //    // 已经升到最高级
            //    if (main != null)
            //    {
            //        main.setExp(exp_cur, exp_cur);
            //    }
            //    return;
            //}

            //int exp_total = tb_actorlevel_next.Experience - tb_actorlevel_cur.Experience;
            //if (exp_total <= 0)
            //{
            //    EbLog.Error("CellActor._onPropExperienceChanged() Error: exp_total<=0 level_cur=" + level_cur);
            //    return;
            //}

            //if (main != null)
            //{
            //    main.setExp(exp_cur, exp_total);
            //}
        }
    }
}
