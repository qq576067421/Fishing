using System;
using System.Collections;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class CLogicLevelStateSwitch : EbState, IDisposable
    {
        //---------------------------------------------------------------------
        CLogicScene mScene = null;
        CLogicLevel mLevel = null;
        float mfCurSecond = 0.0f;// 关卡当前运行总秒数
        const float mfMaxSecond = 3.5f;// 关卡最大运行总秒数

        //---------------------------------------------------------------------
        public CLogicLevelStateSwitch(CLogicScene logic_scene, CLogicLevel logic_level)
        {
            _defState("CLogicLevelStateSwitch", "EbFsm", 0, false);
            _bindAction("update", new EbAction(this.evUpdate));

            mScene = logic_scene;
            mLevel = logic_level;
        }

        //---------------------------------------------------------------------
        ~CLogicLevelStateSwitch()
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
            if (mLevel.NextMapVibId == 0)
            {
                mLevel.NextMapVibId = mLevel.genRandomMap();
            }
            while (mLevel.NextMapVibId == mLevel.CurMapVibId)
            {
                mLevel.NextMapVibId = mLevel.genRandomMap();
            }

            // 清除所有鱼
            mLevel.clearAllFish();

            // 服务端广播关卡更新
            mScene.getProtocol().s2allcLevelUpdate(_eLevelState.Switch, mLevel.getLevelVibId(),
                mLevel.CurMapVibId, mLevel.NextMapVibId, mfCurSecond, mfMaxSecond);
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
                return "CLogicLevelStateNormal";
            }
            else
            {
                mfCurSecond += elapsed_tm;
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
