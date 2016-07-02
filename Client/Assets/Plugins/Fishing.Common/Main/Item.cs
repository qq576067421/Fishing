using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using GF.Common;

namespace Ps
{
    //-------------------------------------------------------------------------
    // Item网络传输的数据
    [Serializable]
    [ProtoContract]
    public class ItemData
    {
        [ProtoMember(1)]
        public string item_objid;
        [ProtoMember(2)]
        public int item_id;// item_id
        [ProtoMember(3)]
        public byte n;// 叠加数量，1~99
        [ProtoMember(4)]
        public Dictionary<byte, string> map_unit_data;
    }

    public class Item
    {
        //-------------------------------------------------------------------------
        public Entity EtSrc { get; private set; }
        public ItemData ItemData { get; private set; }
        public TbDataItem TbDataItem { get; private set; }
        public Unit UnitLink { get; private set; }

        //-------------------------------------------------------------------------
        public Item(Entity et_src, ItemData item_data)
        {
            _setup(et_src, item_data.item_id);

            ItemData = item_data;

            IUnitFactory unit_factory = UnitSys.getUnitFactory(TbDataItem.UnitType);
            if (unit_factory != null) UnitLink = unit_factory.createUnit(this, EtSrc, ItemData.map_unit_data);
        }

        //-------------------------------------------------------------------------
        public Item(Entity et_src, int item_id, byte overlap_num)
        {
            _setup(et_src, item_id);

            ItemData = new ItemData();
            ItemData.item_objid = Guid.NewGuid().ToString();
            ItemData.item_id = item_id;
            ItemData.n = overlap_num;
            ItemData.map_unit_data = null;

            IUnitFactory unit_factory = UnitSys.getUnitFactory(TbDataItem.UnitType);
            if (unit_factory != null) UnitLink = unit_factory.createUnit(this, EtSrc, ItemData.map_unit_data);
        }

        //-------------------------------------------------------------------------
        public object operate(string operate_id)
        {
            if (operate_id == TbDataItem.MainOperateInfo.OperateId)
            {
                var def_actor = EtSrc.getComponentDef<DefActor>();
                var effect_context = def_actor.EffectMgr.genEffectContext();
                effect_context.caller = EffectCaller.UseItem;
                effect_context.item = this;
                effect_context.EtActor = EtSrc;

                TbDataEffect data_effect = EbDataMgr.Instance.getData<TbDataEffect>(
                                TbDataItem.MainOperateInfo.EffectData.EffectId);
                return def_actor.EffectMgr.doEffect(effect_context, data_effect.ScriptName,
                    data_effect.PredefineParamList, TbDataItem.MainOperateInfo.EffectData.ListParam);
            }

            return null;
        }

        //-------------------------------------------------------------------------
        void _setup(Entity et_src, int item_id)
        {
            EtSrc = et_src;
            UnitLink = null;
            TbDataItem = EbDataMgr.Instance.getData<TbDataItem>(item_id);
        }
    }
}
