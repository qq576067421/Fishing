using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

namespace Ps
{
    public enum UnitType
    {
        None = 0,// 无效单位
        Consumables,// 消耗品
        Equip,// 装备
        Material,// 材料
        Gem,// 宝石
        Skill,// 技能
        SkillBook,// 技能书
        Status,// 状态
    }

    public interface Unit
    {
        //-------------------------------------------------------------------------
        Entity EtSrc { get; set; }
        UnitType UnitType { get; }// 单位类型
        Item Item { get; set; }// 关联的道具
        bool IsClient { get; set; }// 是否是客户端
        string MadeBy { get; set; }//由谁制造

        //-------------------------------------------------------------------------
        void create(Entity et_src, bool is_client, Dictionary<byte, string> map_unit_data);

        //-------------------------------------------------------------------------
        void destroy();

        //-------------------------------------------------------------------------
        ToolTip getToolTip(_eUiItemParent item_from);
    }

    public abstract class IUnitFactory
    {
        //-------------------------------------------------------------------------
        public abstract Unit createUnit(Item item, Entity et_src, Dictionary<byte, string> map_unit_data);
    }

    public class TUnitFactory<T> : IUnitFactory where T : Unit, new()
    {
        //-------------------------------------------------------------------------
        bool mIsClient = false;

        //-------------------------------------------------------------------------
        public TUnitFactory(bool is_client)
        {
            mIsClient = is_client;
        }

        //-------------------------------------------------------------------------
        public override Unit createUnit(Item item, Entity et_src, Dictionary<byte, string> map_unit_data)
        {
            Unit unit = new T();

            unit.Item = item;
            unit.create(et_src, mIsClient, map_unit_data);

            return unit;
        }
    }

    public static class UnitSys
    {
        //-------------------------------------------------------------------------
        static Dictionary<string, IUnitFactory> mMapUnitFactory = new Dictionary<string, IUnitFactory>();

        //-------------------------------------------------------------------------
        public static void setup(bool is_client)
        {
            _regUnitFactroy(UnitType.Consumables.ToString(), new UnitConsumablesFactory<UnitConsumables>(is_client));
        }

        //-------------------------------------------------------------------------
        public static IUnitFactory getUnitFactory(string unit_type)
        {
            IUnitFactory factroy = null;
            mMapUnitFactory.TryGetValue(unit_type, out factroy);
            return factroy;
        }

        //-------------------------------------------------------------------------
        public static void setUnitValue()
        {
        }

        //-------------------------------------------------------------------------
        static void _regUnitFactroy(string unit_type, IUnitFactory factory)
        {
            mMapUnitFactory[unit_type] = factory;
        }
    }
}
