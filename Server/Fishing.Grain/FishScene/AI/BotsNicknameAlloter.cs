using System;
using System.IO;
using System.Collections.Generic;
using GF.Common;

namespace Ps
{
    public class BotsNicknameAlloter
    {
        //---------------------------------------------------------------------
        Dictionary<int, string> mDicNicknames = new Dictionary<int, string>();

        //---------------------------------------------------------------------
        public void create(string saved_file_name)
        {
            using (StreamReader reader = new StreamReader(saved_file_name))
            {
                for (int id = 0; !reader.EndOfStream; id++)
                {
                    string name = reader.ReadLine().Trim();
                    if (string.IsNullOrEmpty(name))
                    {
                        continue;
                    }
                    mDicNicknames.Add(id, name);
                }
                EbLog.Note("Nickname count is " + mDicNicknames.Count);
            }
        }

        //---------------------------------------------------------------------
        public string getNicknameById(int id)
        {
            if (mDicNicknames.ContainsKey(id))
            {
                return mDicNicknames[id];
            }
            return string.Empty;
        }
    }
}
