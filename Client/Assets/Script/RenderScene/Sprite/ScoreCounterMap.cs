using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GF.Common;
using UnityEngine;
using System.Text.RegularExpressions;
using Ps;

class ScoreCounterMap
{
    Dictionary<int, int> mDicScoreGap = new Dictionary<int, int>();

    public void create(string score_gap)
    {
        mDicScoreGap.Clear();

        string score_gap_no_space = Regex.Replace(score_gap, @"\s", "");
        string[] each_value = score_gap_no_space.Split(new char[1] { ';' });

        string[] each_entry = null;

        foreach (var it in each_value)
        {
            each_entry = it.Split(new char[1] { ',' });
            mDicScoreGap.Add(int.Parse(each_entry[0]), int.Parse(each_entry[1]));
        }
    }

    public int getNumberByScore(int score)
    {
        foreach (var it in mDicScoreGap)
        {
            if (score < it.Key) { return it.Value; }
        }
        return 1;
    }
}