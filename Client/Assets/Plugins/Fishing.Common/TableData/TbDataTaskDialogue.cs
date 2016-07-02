using System;
using System.Collections.Generic;
using System.Text;
using GF.Common;

public struct OneDialogue
{
    public bool is_me;
    public string dialogue;
}

public class TbDataTaskDialogue : EbData
{
    //-------------------------------------------------------------------------
    public int NpcId { get; private set; }// 对白的NpcId
    public List<OneDialogue> ListDialogue { get; private set; }// 对白列表

    //-------------------------------------------------------------------------
    public override void load(EbPropSet prop_set)
    {
        NpcId = prop_set.getPropInt("I_NpcId").get();

        ListDialogue = new List<OneDialogue>();

        Prop<string> p = null;
        p = prop_set.getPropString("T_Dialogue1");
        if (p != null && !string.IsNullOrEmpty(p.get()))
        {
            OneDialogue d;
            d.is_me = prop_set.getPropInt("I_WhoSay1").get() == 0 ? true : false;
            d.dialogue = p.get();
            ListDialogue.Add(d);
        }

        p = prop_set.getPropString("T_Dialogue2");
        if (p != null && !string.IsNullOrEmpty(p.get()))
        {
            OneDialogue d;
            d.is_me = prop_set.getPropInt("I_WhoSay2").get() == 0 ? true : false;
            d.dialogue = p.get();
            ListDialogue.Add(d);
        }

        p = prop_set.getPropString("T_Dialogue3");
        if (p != null && !string.IsNullOrEmpty(p.get()))
        {
            OneDialogue d;
            d.is_me = prop_set.getPropInt("I_WhoSay3").get() == 0 ? true : false;
            d.dialogue = p.get();
            ListDialogue.Add(d);
        }

        p = prop_set.getPropString("T_Dialogue4");
        if (p != null && !string.IsNullOrEmpty(p.get()))
        {
            OneDialogue d;
            d.is_me = prop_set.getPropInt("I_WhoSay4").get() == 0 ? true : false;
            d.dialogue = p.get();
            ListDialogue.Add(d);
        }

        p = prop_set.getPropString("T_Dialogue5");
        if (p != null && !string.IsNullOrEmpty(p.get()))
        {
            OneDialogue d;
            d.is_me = prop_set.getPropInt("I_WhoSay5").get() == 0 ? true : false;
            d.dialogue = p.get();
            ListDialogue.Add(d);
        }

        p = prop_set.getPropString("T_Dialogue6");
        if (p != null && !string.IsNullOrEmpty(p.get()))
        {
            OneDialogue d;
            d.is_me = prop_set.getPropInt("I_WhoSay6").get() == 0 ? true : false;
            d.dialogue = p.get();
            ListDialogue.Add(d);
        }
    }
}
