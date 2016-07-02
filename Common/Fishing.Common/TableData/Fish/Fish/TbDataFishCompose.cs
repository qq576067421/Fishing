using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class TbDataFishCompose : EbData
    {
        //-------------------------------------------------------------------------
        public enum DataState
        {
            Default = -1,
            ACTIVE = 0,
            DEACTIVE = 1
        }

        //-------------------------------------------------------------------------
        public string Name { get; private set; }
        public DataState State { get; private set; }
        public string BackGroundAniName { get; private set; }
        public int BackGroundAnimationScale { get; private set; }//放大100倍保持2位小数精度
        public List<TbDataFishEachCompose> FishComposes { get; private set; }

        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            Name = prop_set.getPropString("T_Name").get();
            var prop_state = prop_set.getPropInt("I_State");
            State = prop_state == null ? DataState.Default : (DataState)prop_state.get();
            BackGroundAniName = prop_set.getPropString("T_BackGroundAniName").get();
            BackGroundAnimationScale = prop_set.getPropInt("I_BackGroundAnimationScale").get();
            FishComposes = new List<TbDataFishEachCompose>();
            string strFishComposes = prop_set.getPropString("T_FishComposes").get();
            string[] arrayFishcomposes = strFishComposes.Split(';');
            foreach (string fishCompose in arrayFishcomposes)
            {
                FishComposes.Add(EbDataMgr.Instance.getData<TbDataFishEachCompose>(int.Parse(fishCompose)));
            }
        }
    }
}
