using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public class ClientActorMirror<TDef> : Component<TDef> where TDef : DefActorMirror, new()
    {
        //-------------------------------------------------------------------------
        public ClientPlayer<DefPlayer> CoPlayer { get; private set; }
        //public UiMbPlayerInfo UiPlayerInfo { get; private set; }
        public bool IsMe { get { return Def.mPropActorId.get() == CoPlayer.CoActor.Def.mPropActorId.get(); } }// 是否是本人

        //-------------------------------------------------------------------------
        public override void init()
        {
            CoPlayer = EntityMgr.findFirstEntityByType<EtPlayer>().getComponent<ClientPlayer<DefPlayer>>();

            if (IsMe)
            {
                CoPlayer.CoPlayerDesktop.MeMirror = Entity;
            }

            if (!string.IsNullOrEmpty(CoPlayer.CoPlayerDesktop.DesktopConfigData.desktop_etguid))
            {
                //UiMbPlayDesktop ui_desk = UiMgr.Instance.getCurrentUi<UiMbPlayDesktop>();
                //if (ui_desk != null)
                //{
                //    ui_desk.playerEnterDesk(this);
                //}
            }
        }

        //-------------------------------------------------------------------------
        public override void release()
        {
            //if (UiPlayerInfo != null)
            //{
            //    UiPlayerInfo.player_leave();
            //}

            //UiMbPlayDesktop ui_desk = UiMgr.Instance.getCurrentUi<UiMbPlayDesktop>();
            //ui_desk.playerLeaveDesk(this);

            //UiPlayerInfo = null;
        }

        //-------------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
        }

        //-------------------------------------------------------------------------
        public void initActor(int actor_table_id, Action can_set_actor_prop)
        {
        }

        //-------------------------------------------------------------------------
        //public void setUiPlayerInfo(UiMbPlayerInfo player_info)
        //{
        //    UiPlayerInfo = player_info;
        //}

        //-------------------------------------------------------------------------
        //public void gameEnd()
        //{
        //    if (UiPlayerInfo != null)
        //    {
        //        UiPlayerInfo.gameEnd();
        //    }
        //}

        //-------------------------------------------------------------------------
        public void playerOb()
        {
            //Def.mPropDesktopPlayerState.set(DesktopPlayerState.Ob);
        }

        //-------------------------------------------------------------------------
        public void playerSitdown(byte seat_index, int stack, DesktopPlayerState state)
        {
            //Def.mPropSeatIndex.set(seat_index);
            //Def.mPropStack.set(stack);
            //Def.mPropDesktopPlayerState.set(state);
            //Def.mPropPlayerActionType.set(action);

            //UiMbPlayDesktop play_desk = UiMgr.Instance.getCurrentUi<UiMbPlayDesktop>();
            //if (play_desk != null)
            //{
            //    play_desk.playerEnterDesk(this);
            //}
        }

        //-------------------------------------------------------------------------
        public void playerWaitWhile(float wait_while_tm)
        {
            Def.mPropWaitWhileTime.set(wait_while_tm);
            //Def.mPropDesktopPlayerState.set(DesktopPlayerState.WaitWhile);
        }

        //-------------------------------------------------------------------------
        public void playerReturn(int stack, DesktopPlayerState state)
        {
            //Def.mPropStack.set(stack);
            //Def.mPropDesktopPlayerState.set(state);
            //Def.mPropPlayerActionType.set(action);
        }

        //-------------------------------------------------------------------------
        // 桌子内聊天广播
        public void desktopChat(ChatMsgRecv msg_rev)
        {
            //ChatInfo chat_info
            //if (SceneActorObj != null)
            //{
            //    SceneActorObj.setChatInfo(chat_info);
            //}
        }

        //-------------------------------------------------------------------------
        public void updateMirrorActorInfo(string actor_name, string actor_ico)
        {
        }

        //-------------------------------------------------------------------------
        void _onPropStackChanged(IProp prop, object param)
        {
            //UiMbPlayDesk play_desk = UiMgr.Instance.getCurrentUi<UiMbPlayDesk>();
            //if (play_desk != null)
            //{
            //    play_desk.playerStackChange(this);
            //}

            //if (UiPlayerInfo != null)
            //{
            //    UiPlayerInfo.playerStackChange();
            //}
        }
    }
}
