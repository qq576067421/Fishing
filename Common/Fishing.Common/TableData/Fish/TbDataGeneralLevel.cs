using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class TbDataGeneralLevel : EbData
    {
        //-------------------------------------------------------------------------
        public enum DataState
        {
            Default = -1,
            ACTIVE = 0,
            DEACTIVE = 1
        }
        public enum _ePlayType
        {
            Default = -1,
            Score = 0,
            Fish = 1,
            ScoreFish = 2,
            ScoreCd = 3,
            FishCd = 4,
            ScoreFishCd = 5,
            Robot = 6,
            ScoreRobot = 7,
            FishRobot = 8,
            RobotCd = 9,
            ScoreRobotCd = 10,
            FishRobotCd = 11,
            ScoreFishRobotCd = 12
        }

        //-------------------------------------------------------------------------
        public struct _sPreLevelProp
        {
            public enum _eIsActive
            {
                Default = -1,
                PropDisabled = 0,
                PropActive = 1
            }

            public TbDataSinglePreLevelProp PropVariety;
            public int amount;
            public _eIsActive IsActive;
        }

        public struct _sDuringLevelProp
        {
            public enum _eIsActive
            {
                Default = -1,
                PropDisabled = 0,
                PropActive = 1
            }

            public TbDataSingleDuringLevelProp PropVariety;
            public int amount;
            public _eIsActive IsActive;
        }
        //-------------------------------------------------------------------------
        public string Name { get; private set; }
        public DataState State { get; private set; }
        public int LevelNo { get; private set; }       // 关卡No
        public _ePlayType PlayType { get; private set; }  // 玩法类型
        public int BulletNum { get; private set; }        // 子弹
        public int Ratio { get; private set; }                 // 倍率
        public int LowScore { get; private set; }           // 低级分数
        public int MidScore { get; private set; }           // 中级分数
        public int HighScore { get; private set; }           // 高级分数
        public int FishRequireId { get; private set; }     // 需要捕的鱼Id
        public int FishRequireNum { get; private set; }             //  需要捕的鱼数量
        public int CdMinute { get; private set; }             // 倒计时 - 分
        public int CdSecond { get; private set; }              // 倒计时 - 秒
        public string LevelDesn { get; private set; }        // 关卡过关提示

        public List<_sPreLevelProp> PreLevelProps; // 关卡前道具
        public List<_sDuringLevelProp> DuringLevelProps;   // 关卡中道具

        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            Name = prop_set.getPropString("T_Name").get();
            var prop_state = prop_set.getPropInt("I_State");
            State = prop_state == null ? DataState.Default : (DataState)prop_state.get();
            LevelNo = prop_set.getPropInt("I_LevelNo").get();
            var prop_playtype = prop_set.getPropInt("I_PlayType");
            PlayType = prop_playtype == null ? _ePlayType.Default : (_ePlayType)prop_set.getPropInt("I_PlayType").get();
            BulletNum = prop_set.getPropInt("I_BulletNum").get();
            Ratio = prop_set.getPropInt("I_Ratio").get();
            LowScore = prop_set.getPropInt("I_LowScore").get();
            MidScore = prop_set.getPropInt("I_MidScore").get();
            HighScore = prop_set.getPropInt("I_HighScore").get();
            FishRequireId = prop_set.getPropInt("I_FishRequireId").get();
            FishRequireNum = prop_set.getPropInt("I_FishRequireNum").get();
            CdMinute = prop_set.getPropInt("I_CdMinute").get();
            CdSecond = prop_set.getPropInt("I_CdSecond").get();
            LevelDesn = prop_set.getPropString("T_LevelDesn").get();

            PreLevelProps = new List<_sPreLevelProp>();
            for (int i = 1; i <= 3; ++i)
            {
                string strPreLevelProps = prop_set.getPropString("T_PreLevelProps" + i.ToString()).get();
                string[] arrayStrPreLevelProps = strPreLevelProps.Split(';');
                _sPreLevelProp preLevelProp = new _sPreLevelProp();
                preLevelProp.PropVariety = EbDataMgr.Instance.getData<TbDataSinglePreLevelProp>(int.Parse(arrayStrPreLevelProps[0]));
                preLevelProp.amount = int.Parse(arrayStrPreLevelProps[1]);
                preLevelProp.IsActive = string.IsNullOrEmpty(arrayStrPreLevelProps[2]) ? _sPreLevelProp._eIsActive.Default : (_sPreLevelProp._eIsActive)int.Parse(arrayStrPreLevelProps[2]);
            }

            DuringLevelProps = new List<_sDuringLevelProp>();
            for (int i = 1; i <= 3; ++i)
            {
                string strPreLevelProps = prop_set.getPropString("T_PreLevelProps" + i.ToString()).get();
                string[] arrayStrPreLevelProps = strPreLevelProps.Split(';');
                _sDuringLevelProp duringLevelProp = new _sDuringLevelProp();
                duringLevelProp.PropVariety = EbDataMgr.Instance.getData<TbDataSingleDuringLevelProp>(int.Parse(arrayStrPreLevelProps[0]));
                duringLevelProp.amount = int.Parse(arrayStrPreLevelProps[1]);
                duringLevelProp.IsActive = string.IsNullOrEmpty(arrayStrPreLevelProps[2]) ? _sDuringLevelProp._eIsActive.Default : (_sDuringLevelProp._eIsActive)int.Parse(arrayStrPreLevelProps[2]);
            }
        }
    }
}
