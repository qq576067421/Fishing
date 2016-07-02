using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public class CRenderLevelStateNormal : EbState, IDisposable
    {
        //---------------------------------------------------------------------
        CRenderScene mScene = null;
        CRenderLevel mLevel = null;
        float mfCurSecond = 0.0f;// 关卡当前运行总秒数

        //---------------------------------------------------------------------
        public CRenderLevelStateNormal(CRenderScene render_scene, CRenderLevel render_level)
        {
            _defState("CRenderLevelStateNormal", "EbFsm", 0, true);
            _bindAction("update", new EbAction(this.evUpdate));
            _bindAction("setState", new EbAction(this.evSetState));

            mScene = render_scene;
            mLevel = render_level;

            // 因为初始化的时候首先是这个状态先出来，所以在这里加载一下地图
            mLevel.getSpriteLevel().switchBackgroundMap(mLevel.CurMapVibId, 0);
        }

        //---------------------------------------------------------------------
        ~CRenderLevelStateNormal()
        {
            this.Dispose(false);
        }

        //-----------------------------------------------------------------------------
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        //-----------------------------------------------------------------------------
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        //---------------------------------------------------------------------
        public override void enter()
        {
            IRenderListener listener = mScene.getListener();
            if (listener != null)
            {
                //listener.onSceneShowMessageBox("进入新关卡", false, "", 2, (int)_eMessageBoxLayer.EnterNewLevel);
            }
        }

        //---------------------------------------------------------------------
        public override void exit()
        {
        }

        //-----------------------------------------------------------------------------
        public string evUpdate(IEbEvent ev)
        {
            EbEvent1<float> e = (EbEvent1<float>)ev;
            float elapsed_tm = e.param1;
            mfCurSecond += elapsed_tm;

            return "";
        }

        //-----------------------------------------------------------------------------
        public string evSetState(IEbEvent ev)
        {
            EbEvent1<_eLevelState> e = (EbEvent1<_eLevelState>)ev;
            _eLevelState level_state = e.param1;

            if (level_state == _eLevelState.Switch)
            {
                return "CRenderLevelStateSwitch";
            }

            return "";
        }

        //-----------------------------------------------------------------------------
        public float getCurSecond()
        {
            return mfCurSecond;
        }
    }
}
