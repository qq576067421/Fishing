using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public class TbDataRoom : EbData
    {
        //-------------------------------------------------------------------------
        public enum DataState
        {
            Default = -1,
            ACTIVE = 0,
            DEACTIVE = 1
        }

        public enum _eRoomType
        {
            Default = -1,
            RT_None = 0,
            RT_Tenfold = 1,// 十倍炮房
            RT_Hundredfold = 2,// 百倍炮房
            RT_Thousandfold = 3,// 千倍炮房
            RT_Times = 4,// 万倍炮房
            RT_BillionTimes = 5,// 亿倍炮房
        }

        //-------------------------------------------------------------------------
        public string Name { get; private set; }
        public DataState State { get; private set; }
        public _eRoomType RoomType;
        public int RoomNum;// 房间数量
        public int RoomEnterGoldLimit;// 进入房间限制 - 金币限制
        public int RoomSingleFireGoldLimit;// 单炮限制
        public string RoomRate;// 房间倍率
        public int RoomDesktopNum;// 桌子数量

        //-------------------------------------------------------------------------
        public override void load(EbPropSet prop_set)
        {
            Name = prop_set.getPropString("T_Name").get();
            var prop_state = prop_set.getPropInt("I_State");
            State = prop_state == null ? DataState.Default : (DataState)prop_state.get();
            RoomType = (_eRoomType)prop_set.getPropInt("I_RoomType").get();
            RoomNum = prop_set.getPropInt("I_RoomNum").get();
            RoomEnterGoldLimit = prop_set.getPropInt("I_RoomEnterGoldLimit").get();
            RoomSingleFireGoldLimit = prop_set.getPropInt("I_RoomSingleFireGoldLimit").get();
            RoomRate = prop_set.getPropString("T_RoomRate").get();
            RoomDesktopNum = prop_set.getPropInt("I_RoomDesktopNum").get();
        }
    }
}
