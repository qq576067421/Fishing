using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using GF.Common;
using GF.Server;
using Ps;

public class CellConfig
{
    //-------------------------------------------------------------------------
    SharpConfig.Configuration Cfg { get; set; }
    // 全局配置
    public int GlobalAFKLevelLimit { get; private set; }// 挂机等级限制
    // 新手村配置信息
    public int BornSceneId { get; private set; }
    public float BornPosMin { get; private set; }
    public float BornPosMax { get; private set; }
    // 地府配置信息
    public int ReviveSceneId { get; private set; }
    public float RevivePosMin { get; private set; }
    public float RevivePosMax { get; private set; }
    // 任务配置信息
    public List<int> ListTaskStoryStartTaskId { get; private set; }

    //-------------------------------------------------------------------------
    public CellConfig()
    {
        SharpConfig.Configuration.ValidCommentChars = new char[] { '#', ';' };
        string cfg_file = ServerPath.getPathMediaRoot() + "Fishing\\Config\\CellConfig.cfg";
        Cfg = SharpConfig.Configuration.LoadFromFile(cfg_file);

        //Cfg.Save(cfg_file);

        //foreach (var section in Cfg)
        //{
        //    EbLog.Note("SectionName=" + section.Name);

        //    foreach (var setting in section)
        //    {
        //        EbLog.Note("SettingName=" + setting.Name);
        //    }
        //}

        _parse();
    }

    //-------------------------------------------------------------------------
    void _parse()
    {
        // 全局配置
        var global = Cfg["Global"];
        GlobalAFKLevelLimit = global["GlobalAFKLevelLimit"].IntValue;

        // 新手村场景配置
        var born_scene = Cfg["BornScene"];
        BornSceneId = born_scene["BornSceneId"].IntValue;
        BornPosMin = (float)born_scene["BornPosMin"].DoubleValue;
        BornPosMax = (float)born_scene["BornPosMax"].DoubleValue;

        // 地府场景配置
        var revive_scene = Cfg["ReviveScene"];
        ReviveSceneId = revive_scene["ReviveSceneId"].IntValue;
        RevivePosMin = (float)revive_scene["RevivePosMin"].DoubleValue;
        RevivePosMax = (float)revive_scene["RevivePosMax"].DoubleValue;

        // 任务配置信息
        var task = Cfg["Task"];

        ListTaskStoryStartTaskId = new List<int>();
        string[] tasks = task["ListTaskStoryStartTaskId"].StringValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var i in tasks)
        {
            ListTaskStoryStartTaskId.Add(int.Parse(i));
        }
    }
}
