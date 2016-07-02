using System;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class ToolTipHelper
    {
        static ToolTipHelper mToolTipHelper;

        //-------------------------------------------------------------------------
        private ToolTipHelper()
        {
        }

        //-------------------------------------------------------------------------
        public static ToolTipHelper Instant
        {
            get
            {
                if (mToolTipHelper == null)
                {
                    mToolTipHelper = new ToolTipHelper();
                }
                return mToolTipHelper;
            }
        }

        //-------------------------------------------------------------------------
        public List<_ToolTipContentDetailInfo> getToolTipNormalContentDetailText(List<EffectData> list_effect)
        {
            List<_ToolTipContentDetailInfo> list_detail = new List<_ToolTipContentDetailInfo>();
            foreach (var i in list_effect)
            {
                if (i.ListParam == null || i.ListParam.Length <= 0)
                {
                    continue;
                }

                TbDataEffect tb_effect = EbDataMgr.Instance.getData<TbDataEffect>(i.EffectId);
                Ps.PropOperate op = (Ps.PropOperate)byte.Parse(i.ListParam[0]);
                string value = tb_effect.FormatDesc.Replace("%s", "");
                value += _getOperateStr(op);
                value += ("  " + i.ListParam[1]);

                _ToolTipContentDetailInfo content_detail = new _ToolTipContentDetailInfo();
                content_detail.detail_type = _eToolTipContentDetailType.NormalLable;
                content_detail.param = value;
                list_detail.Add(content_detail);
            }

            return list_detail;
        }

        //-------------------------------------------------------------------------
        public List<_ToolTipContentDetailInfo> getTbToolTipEquipContentDetailText(List<EffectData> list_effect)
        {
            List<_ToolTipContentDetailInfo> list_detail = new List<_ToolTipContentDetailInfo>();
            foreach (var i in list_effect)
            {
                if (i.ListParam == null || i.ListParam.Length <= 0)
                {
                    continue;
                }

                TbDataEffect tb_effect = EbDataMgr.Instance.getData<TbDataEffect>(i.EffectId);
                Ps.PropOperate op = (Ps.PropOperate)byte.Parse(i.ListParam[0]);
                string value = tb_effect.FormatDesc.Replace("%s", "");
                value += _getOperateStr(op);
                value += (" " + i.ListParam[1] + " ~ " + i.ListParam[4]);

                _ToolTipContentDetailInfo content_detail = new _ToolTipContentDetailInfo();
                content_detail.detail_type = _eToolTipContentDetailType.NormalLable;
                content_detail.param = value;
                list_detail.Add(content_detail);
            }

            return list_detail;
        }

        //-------------------------------------------------------------------------
        public List<_ToolTipContentDetailInfo> getToolTipEquipContentDetailText(List<EffectData> list_effect, Dictionary<string, IProp> map_prop)
        {
            List<_ToolTipContentDetailInfo> list_detail = new List<_ToolTipContentDetailInfo>();
            foreach (var i in list_effect)
            {
                if (i.ListParam == null || i.ListParam.Length <= 0)
                {
                    continue;
                }

                TbDataEffect tb_effect = EbDataMgr.Instance.getData<TbDataEffect>(i.EffectId);
                Ps.PropOperate op = (Ps.PropOperate)byte.Parse(i.ListParam[0]);

                IProp p = null;
                map_prop.TryGetValue(tb_effect.SelfDefine1, out p);
                if (p == null) IProp.setProp<int>(map_prop, tb_effect.SelfDefine1, 0);
                Prop<int> prop = (Prop<int>)map_prop[tb_effect.SelfDefine1];
                string value = tb_effect.FormatDesc.Replace("%s", "");
                value += _getOperateStr(op);
                value += (" " + prop.get().ToString());
                _ToolTipContentDetailInfo content_detail = new _ToolTipContentDetailInfo();
                content_detail.detail_type = _eToolTipContentDetailType.NormalLable;
                content_detail.param = value;
                list_detail.Add(content_detail);
            }

            return list_detail;
        }

        //-------------------------------------------------------------------------
        public List<_ToolTipContentDetailInfo> getTbToolTipSkillContentDetailText(List<EffectData> list_effect)
        {
            List<_ToolTipContentDetailInfo> list_detail = new List<_ToolTipContentDetailInfo>();
            foreach (var i in list_effect)
            {
                if (i.ListParam == null || i.ListParam.Length <= 0)
                {
                    continue;
                }

                TbDataEffect tb_effect = EbDataMgr.Instance.getData<TbDataEffect>(i.EffectId);
                Ps.PropOperate op = (Ps.PropOperate)byte.Parse(i.ListParam[0]);
                string value = "资质: " + tb_effect.FormatDesc.Replace("%s", "");
                value += _getOperateStr(op);
                value += (" " + i.ListParam[1] + " ~ " + i.ListParam[6]);

                _ToolTipContentDetailInfo content_damage_detail = new _ToolTipContentDetailInfo();
                content_damage_detail.detail_type = _eToolTipContentDetailType.NormalLable;
                content_damage_detail.param = value;

                value = "成长: " + i.ListParam[3] + " ~ " + i.ListParam[8];
                _ToolTipContentDetailInfo content_grow_detail = new _ToolTipContentDetailInfo();
                content_grow_detail.detail_type = _eToolTipContentDetailType.NormalLable;
                content_grow_detail.param = value;
                list_detail.Add(content_damage_detail);
                list_detail.Add(content_grow_detail);
            }

            return list_detail;
        }

        //-------------------------------------------------------------------------
        public List<_ToolTipContentDetailInfo> getToolTipSkillContentDetailText(List<EffectData> list_effect, Dictionary<string, IProp> map_final_prop,
            Dictionary<string, IProp> map_first_prop, Dictionary<string, IProp> map_grow_prop)
        {
            List<_ToolTipContentDetailInfo> list_detail = new List<_ToolTipContentDetailInfo>();
            foreach (var i in list_effect)
            {
                if (i.ListParam == null || i.ListParam.Length <= 0)
                {
                    continue;
                }

                TbDataEffect tb_effect = EbDataMgr.Instance.getData<TbDataEffect>(i.EffectId);
                Ps.PropOperate op = (Ps.PropOperate)byte.Parse(i.ListParam[0]);
                IProp p = null;
                map_final_prop.TryGetValue(tb_effect.SelfDefine1, out p);
                if (p == null) IProp.setProp<int>(map_final_prop, tb_effect.SelfDefine1, 0);
                Prop<int> prop = (Prop<int>)map_final_prop[tb_effect.SelfDefine1];
                string value = tb_effect.FormatDesc.Replace("%s", "");
                value += _getOperateStr(op);
                value += (" " + prop.get());
                _ToolTipContentDetailInfo content_detail = new _ToolTipContentDetailInfo();
                content_detail.detail_type = _eToolTipContentDetailType.NormalLable;
                content_detail.param = value;
                list_detail.Add(content_detail);

                IProp p_first = null;
                map_first_prop.TryGetValue(tb_effect.SelfDefine1, out p_first);
                if (p_first == null) IProp.setProp<int>(map_first_prop, tb_effect.SelfDefine1, 0);
                Prop<int> prop_first = (Prop<int>)map_first_prop[tb_effect.SelfDefine1];
                string value_first = "资质:" + prop_first.get();
                _ToolTipContentDetailInfo value_first_detail = new _ToolTipContentDetailInfo();
                value_first_detail.detail_type = _eToolTipContentDetailType.NormalLable;
                value_first_detail.param = value_first;
                list_detail.Add(value_first_detail);

                IProp p_grow = null;
                map_grow_prop.TryGetValue(tb_effect.SelfDefine1, out p_grow);
                if (p_grow == null) IProp.setProp<int>(map_grow_prop, tb_effect.SelfDefine1, 0);
                Prop<int> prop_grow = (Prop<int>)map_grow_prop[tb_effect.SelfDefine1];
                string value_grow = "成长:" + prop_grow.get();
                _ToolTipContentDetailInfo value_grow_detail = new _ToolTipContentDetailInfo();
                value_grow_detail.detail_type = _eToolTipContentDetailType.NormalLable;
                value_grow_detail.param = value_grow;
                list_detail.Add(value_grow_detail);
            }

            return list_detail;
        }

        //-------------------------------------------------------------------------
        public List<ItemOperateInfo> getItemOperate(List<ItemOperateInfo> list_operate_info, TbDataItem tb_item)
        {
            return list_operate_info;
        }

        //-------------------------------------------------------------------------
        string _getOperateStr(Ps.PropOperate op)
        {
            string value = "";
            switch (op)
            {
                case Ps.PropOperate.Add:
                    value = "+";
                    break;
                case Ps.PropOperate.Subtract:
                    value = "-";
                    break;
                case Ps.PropOperate.Multiply:
                    value = "*";
                    break;
                case Ps.PropOperate.Divide:
                    value = "/";
                    break;
            }

            return value;
        }
    }
}
