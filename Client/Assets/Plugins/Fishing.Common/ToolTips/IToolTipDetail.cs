using System;
using System.Collections.Generic;

namespace Ps
{
    public abstract class IToolTipDetail
    {
        public List<_ToolTipContentDetailInfo> list_detail;//效果详细
        public string MadeBy { get; set; }//打造人
    }

    //-------------------------------------------------------------------------
    public class _ToolTipContentDetailInfo
    {
        public _eToolTipContentDetailType detail_type;
        public object param;
    }

    //-------------------------------------------------------------------------
    public enum _eToolTipContentDetailType
    {
        NormalLable,
        Ico,
        Mix,
    }
}
