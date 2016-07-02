using System;
using System.Collections.Generic;
using System.IO;
using GF.Common;

public class EntityRpcSessionSuperSocketC : IRpcSession
{
    //---------------------------------------------------------------------
    EntityMgr mEntityMgr;
    SuperSocketClient mSocket;

    //---------------------------------------------------------------------
    public OnSocketConnected OnSocketConnected { get; set; }
    public OnSocketClosed OnSocketClosed { get; set; }
    public OnSocketError OnSocketError { get; set; }
    public bool IsConnected { get { return mSocket.IsConnected; } }
    public string SessionId { get { return ""; } }

    //---------------------------------------------------------------------
    public EntityRpcSessionSuperSocketC(EntityMgr entity_mgr)
    {
        mEntityMgr = entity_mgr;
        mSocket = new SuperSocketClient();
        mSocket.OnSocketReceive += _onSocketReceive;
        mSocket.OnSocketConnected += _onSocketConnected;
        mSocket.OnSocketClosed += _onSocketClosed;
        mSocket.OnSocketError += _onSocketError;
    }

    //---------------------------------------------------------------------
    public void send(ushort method_id, byte[] data)
    {
        if (mSocket != null)
        {
            mSocket.send(method_id, data);
        }
    }

    //---------------------------------------------------------------------
    public void close()
    {
        if (mSocket != null) mSocket.close();
    }

    //---------------------------------------------------------------------
    public void connect(string ip, int port)
    {
        if (mSocket != null) mSocket.connect(ip, port);
    }

    //---------------------------------------------------------------------
    public void update(float elapsed_tm)
    {
        if (mSocket != null) mSocket.update(elapsed_tm);
    }

    //---------------------------------------------------------------------
    void _onSocketReceive(byte[] data, int len)
    {
        mEntityMgr.LastRpcSession = this;

        ushort method_id = BitConverter.ToUInt16(data, 0);

        byte[] buf = null;
        if (data.Length > sizeof(ushort))
        {
            ushort data_len = (ushort)(data.Length - sizeof(ushort));
            buf = new byte[data_len];
            Array.Copy(data, sizeof(ushort), buf, 0, data_len);
        }
        else buf = new byte[0];

        mEntityMgr.onRecvRpcData(this, method_id, buf);
    }

    //---------------------------------------------------------------------
    void _onSocketError(object rec, SocketErrorEventArgs args)
    {
        mSocket = null;

        if (OnSocketError != null)
        {
            OnSocketError(this, args);
        }
    }

    //---------------------------------------------------------------------
    void _onSocketConnected(object client, EventArgs args)
    {
        if (OnSocketConnected != null)
        {
            OnSocketConnected(this, args);
        }
    }

    //---------------------------------------------------------------------
    void _onSocketClosed(object client, EventArgs args)
    {
        mSocket = null;

        if (OnSocketClosed != null)
        {
            OnSocketClosed(this, args);
        }
    }
}
