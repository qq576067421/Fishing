using System;
using System.Collections.Generic;
using ProtoBuf;

namespace GF.Common
{
    public interface IRpcSession
    {
        //---------------------------------------------------------------------
        bool IsConnected { get; }
        string SessionId { get; }

        //---------------------------------------------------------------------
        void send(ushort method_id, byte[] data);

        //---------------------------------------------------------------------
        void close();
    }
}
