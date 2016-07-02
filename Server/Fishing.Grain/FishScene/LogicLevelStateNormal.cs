using System;
using System.Collections;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class CLogicLevelStateNormal : EbState, IDisposable
    {
        //---------------------------------------------------------------------
        CLogicScene mScene = null;
        CLogicLevel mLevel = null;
        float mfCurSecond = 0.0f;// 关卡当前运行总秒数
        const float mfMaxSecond = 300.0f;// 关卡最大运行总秒数
        bool mIsFistLevel = true;

        //---------------------------------------------------------------------
        public CLogicLevelStateNormal(CLogicScene logic_scene, CLogicLevel logic_level)
        {
            _defState("CLogicLevelStateNormal", "EbFsm", 0, true);
            _bindAction("update", new EbAction(this.evUpdate));

            mScene = logic_scene;
            mLevel = logic_level;
        }

        //---------------------------------------------------------------------   
        ~CLogicLevelStateNormal()
        {
            this.Dispose(false);
        }

        //---------------------------------------------------------------------
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        //---------------------------------------------------------------------
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        //---------------------------------------------------------------------
        public override void enter()
        {
            mfCurSecond = 0.0f;

            // 切换地图
            if (mLevel.CurMapVibId == 0)
            {
                mLevel.CurMapVibId = mLevel.genRandomMap();
            }
            else if (mLevel.NextMapVibId != 0)
            {
                mLevel.CurMapVibId = mLevel.NextMapVibId;
                mLevel.NextMapVibId = 0;
            }

            // 服务端广播关卡更新
            mScene.getProtocol().s2allcLevelUpdate(_eLevelState.Normal, mLevel.getLevelVibId(),
                mLevel.CurMapVibId, mLevel.NextMapVibId, mfCurSecond, mfMaxSecond);

            if (mIsFistLevel)
            {
                mIsFistLevel = false;
            }
            else
            {
                mLevel.outFormation();
            }
        }

        //---------------------------------------------------------------------
        public override void exit()
        {
        }

        //---------------------------------------------------------------------
        public string evUpdate(IEbEvent ev)
        {
            EbEvent1<float> e = (EbEvent1<float>)ev;
            float elapsed_tm = e.param1;

            if (mfCurSecond > mfMaxSecond)
            {
                return "CLogicLevelStateSwitch";
            }
            else
            {
                mfCurSecond += elapsed_tm;
            }

            if (mLevel != null)
            {
                mLevel.updateOutFish(elapsed_tm);
            }

            return "";
        }

        //---------------------------------------------------------------------
        public float getCurSecond()
        {
            return mfCurSecond;
        }

        //---------------------------------------------------------------------
        public float getMaxSecond()
        {
            return mfMaxSecond;
        }
    }
}
