using System;
using System.Collections.Generic;
using System.Text;
using EventDataXML.remotestream;

namespace RemoteStream
{
    public interface IRemoteStream
    {
        System.IO.Stream OpenRead(string path, bool bLock);

        System.IO.Stream OpenWrite(string path);

        void Delete(string path);
    }

    public static class RemoteStream
    {
        public static IRemoteStream DefRemoteStrem = new DirectRemote();
    }
}
