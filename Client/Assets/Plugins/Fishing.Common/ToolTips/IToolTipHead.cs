using System;
using System.Collections.Generic;

namespace Ps
{
    public abstract class IToolTipHead
    {
        public string ItemIco { get; set; }
        public string ItemName { get; set; }
        public int ItemTypeId { get; set; }
    }

    public class ToolTip
    {
        public IToolTipHead ToolTipHead { get; set; }
        public IToolTipDetail ToolTipDetail { get; set; }
        public ToolTipEnd ToolTipEnd { get; set; }
    }

    //-----------------------------------------------------------------------------
    public enum _eUiItemParent
    {
        Bag,
        Storage,
        Chat,
        CompareItemWithSelf,
        TaskReward,
        Equip,
        Mail,
        Help,
        Skill,
        CompoundGemOperate,
        CompoundEquip,
        CompoundSkill,
        CompoundMainItem,
        CompoundParent,
        Trade,
    }
}
