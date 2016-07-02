using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GF.Common;

namespace Ps
{
    public class CRenderLevelStateSwitch : EbState, IDisposable
    {
        //---------------------------------------------------------------------
        CRenderScene mScene = null;
        CRenderLevel mLevel = null;
        float mfCurSecond = 0.0f;// 关卡当前运行总秒数

        //---------------------------------------------------------------------
        public CRenderLevelStateSwitch(CRenderScene render_scene, CRenderLevel render_level)
        {
            _defState("CRenderLevelStateSwitch", "EbFsm", 0, false);
            _bindAction("update", new EbAction(this.evUpdate));
            _bindAction("setState", new EbAction(this.evSetState));

            mScene = render_scene;
            mLevel = render_level;
        }

        //---------------------------------------------------------------------
        ~CRenderLevelStateSwitch()
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
            // 关闭所有子弹的碰撞
            mLevel.closeBulletCollision();
            mLevel.setTimeFactor(1);
            mLevel.getSpriteLevel().switchBackgroundMap(mLevel.NextMapVibId, mLevel.LevelCurRunSecond);
        }

        //---------------------------------------------------------------------
        public override void exit()
        {
            // 开启所有子弹的碰撞
            mLevel.openBulletCollision();
        }

        //-----------------------------------------------------------------------------
        public string evUpdate(IEbEvent ev)
        {
            EbEvent1<float> e = (EbEvent1<float>)ev;
            float elapsed_tm = e.param1;
            mfCurSecond += elapsed_tm;

            IRenderListener listener = mScene.getListener();
            if (listener != null)
            {
                listener.onSceneShowMessageBox("鱼潮来临！！！", false, "", 1, (int)0, false, false);//_eMessageBoxLayer.SwitchLevel
            }

            return "";
        }

        //-----------------------------------------------------------------------------
        public string evSetState(IEbEvent ev)
        {
            EbEvent1<_eLevelState> e = (EbEvent1<_eLevelState>)ev;
            _eLevelState level_state = e.param1;

            if (level_state == _eLevelState.Normal)
            {
                return "CRenderLevelStateNormal";
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
