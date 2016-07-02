using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class TbDataFormation : EbData
    {
        //-------------------------------------------------------------------------
        public enum DataState
        {
            Default = -1,
            ACTIVE = 0,
            DEACTIVE = 1
        }

        public struct FormationElementStruct
        {
            public TbDataFish Fish;
            public int ColumnNumber;
            public int Amount;//even? 不过过中点 odd？过中点
        }
        //-------------------------------------------------------------------------
        public DataState State { get; private set; }
        public List<FormationElementStruct> FishColumns { get; private set; }

        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            var prop_state = prop_set.getPropInt("I_State");
            State = prop_state == null ? DataState.Default : (DataState)prop_state.get();
            FishColumns = new List<FormationElementStruct>();
            for (int i = 1; i <= 10; ++i)
            {
                string strFishColumns = prop_set.getPropString("T_FishColumns" + i.ToString()).get();
                string[] arrayFishColumns = strFishColumns.Split(';');
                FormationElementStruct formationElementStruct = new FormationElementStruct();
                formationElementStruct.Fish = EbDataMgr.Instance.getData<TbDataFish>(int.Parse(arrayFishColumns[0]));
                formationElementStruct.ColumnNumber = int.Parse(arrayFishColumns[1]);
                formationElementStruct.Amount = int.Parse(arrayFishColumns[2]);
                FishColumns.Add(formationElementStruct);
            }
        }
    }
}
