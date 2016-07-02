using System;
using System.Collections.Generic;
using System.Text;
using RemoteStream;

namespace EventDataXML.remotestream
{
    class DirectRemote : IRemoteStream // 啥都没干的直接Stream
    {
        public System.IO.Stream OpenRead(string path, bool bLock)
        {
            if (bLock)
            {
                return System.IO.File.Open(path, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            }
            else
            {
                return System.IO.File.OpenRead(path);
            }
        }

        public System.IO.Stream OpenWrite(string path)
        {
            return System.IO.File.Open(path, System.IO.FileMode.Create, System.IO.FileAccess.Write);
        }

        public void Delete(string path)
        {
            System.IO.File.Delete(path);
        }
    }
}
