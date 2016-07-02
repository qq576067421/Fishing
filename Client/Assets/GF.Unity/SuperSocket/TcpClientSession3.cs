using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using GF.Common;

public delegate void OnSocketReceive(byte[] data, int len);
public delegate void OnSocketConnected(object client, EventArgs args);
public delegate void OnSocketClosed(object client, EventArgs args);
public delegate void OnSocketError(object rec, SocketErrorEventArgs args);

public class TcpStateObj
{
    public Socket socket;
    public byte[] buffer;
}

public class DataEventArgs : EventArgs
{
    public byte[] Data { get; set; }
    public int Offset { get; set; }
    public int Length { get; set; }
}

public class SocketErrorEventArgs : EventArgs
{
    public Exception Exception { get; private set; }

    public SocketErrorEventArgs(Exception exception)
    {
        Exception = exception;
    }
}

public class TcpClientSession3 : IDisposable
{
    //-------------------------------------------------------------------------
    readonly int MAX_RECEIVE_LEN = 8192;
    Socket mSocket;
    EndPoint mRemoteEndPoint;
    byte[] m_KeepAliveOptionValues;
    byte[] m_KeepAliveOptionOutValues;
    volatile bool mDisposed;
    Queue<byte[]> mQueSending = new Queue<byte[]>();
    int mSendLength = 0;
    volatile bool mConnected;

    //-------------------------------------------------------------------------
    public bool IsConnected { get { return (mSocket == null || mDisposed) ? false : mConnected; } }
    public OnSocketReceive DataReceived { get; set; }
    public OnSocketConnected Connected { get; set; }
    public OnSocketClosed Closed { get; set; }
    public OnSocketError Error { get; set; }

    //-------------------------------------------------------------------------
    public TcpClientSession3(EndPoint remote_endpoint)
    {
        mRemoteEndPoint = remote_endpoint;

        m_KeepAliveOptionValues = new byte[sizeof(uint) * 3];
        m_KeepAliveOptionOutValues = new byte[m_KeepAliveOptionValues.Length];
        // whether enable KeepAlive
        BitConverter.GetBytes((uint)1).CopyTo(m_KeepAliveOptionValues, 0);
        // how long will start first keep alive
        BitConverter.GetBytes((uint)(3 * 1000)).CopyTo(m_KeepAliveOptionValues, sizeof(uint));
        // keep alive interval
        BitConverter.GetBytes((uint)(1 * 1000)).CopyTo(m_KeepAliveOptionValues, sizeof(uint) * 2);
    }

    //-------------------------------------------------------------------------
    ~TcpClientSession3()
    {
        Dispose(false);
    }

    //-------------------------------------------------------------------------
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    //-------------------------------------------------------------------------
    protected void Dispose(bool disposing)
    {
        if (!mDisposed)
        {
            if (disposing)
            {
                try
                {
                    if (mSocket != null)
                    {
                        mSocket.Shutdown(SocketShutdown.Both);
                    }
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    try
                    {
                        if (mSocket != null)
                        {
                            mSocket.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }

                _raiseClosed();
            }

            mDisposed = true;
        }
    }

    //-------------------------------------------------------------------------
    public void update()
    {
        try
        {
            if (mDisposed || !mConnected) return;

            if (mSocket.Poll(0, SelectMode.SelectError))
            {
                _raiseError(new Exception("SocketError"));
                Dispose();
                return;
            }

            if (mSocket.Poll(0, SelectMode.SelectRead))
            {
                byte[] recv_buf = new byte[MAX_RECEIVE_LEN];
                int num = mSocket.Receive(recv_buf, MAX_RECEIVE_LEN, SocketFlags.None);
                if (num > 0) _raiseDataReceived(recv_buf, num);
            }

            if (mSocket.Poll(0, SelectMode.SelectWrite))
            {
                if (mQueSending.Count > 0)
                {
                    byte[] buf = mQueSending.Peek();
                    int send_num = mSocket.Send(buf, mSendLength, buf.Length - mSendLength, SocketFlags.None);
                    if (send_num > 0)
                    {
                        mSendLength += send_num;
                        if (mSendLength >= buf.Length)
                        {
                            mQueSending.Dequeue();
                            mSendLength = 0;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _raiseError(ex);
            Dispose();
        }
    }

    //-------------------------------------------------------------------------
    public void connect()
    {
        try
        {
            if (mDisposed) return;

            mSendLength = 0;
            mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);

            mConnected = false;

            var ip_endpoint = mRemoteEndPoint as IPEndPoint;
            mSocket.BeginConnect(ip_endpoint.Address, ip_endpoint.Port, new AsyncCallback(_onConnect), mSocket);
        }
        catch (Exception ex)
        {
            _raiseError(ex);
            Dispose();
        }
    }

    //-------------------------------------------------------------------------
    public void send(byte[] buf)
    {
        mQueSending.Enqueue(buf);
    }

    //-------------------------------------------------------------------------
    void _onConnect(IAsyncResult result)
    {
        try
        {
            Socket socket = (Socket)result.AsyncState;
            socket.EndConnect(result);

            if (socket.Connected)
            {
                bool SupportSocketIOControlByCodeEnum;
                try
                {
                    mSocket.IOControl(IOControlCode.KeepAliveValues, null, null);
                    SupportSocketIOControlByCodeEnum = true;
                }
                catch (NotSupportedException)
                {
                    SupportSocketIOControlByCodeEnum = false;
                }
                catch (NotImplementedException)
                {
                    SupportSocketIOControlByCodeEnum = false;
                }
                catch (Exception)
                {
                    SupportSocketIOControlByCodeEnum = true;
                }

                if (!SupportSocketIOControlByCodeEnum)
                {
                    mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, m_KeepAliveOptionValues);
                }
                else
                {
                    mSocket.IOControl(IOControlCode.KeepAliveValues, m_KeepAliveOptionValues, m_KeepAliveOptionOutValues);
                }

#if !__IOS__
                mSocket.NoDelay = true;
#endif
                mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);

                mConnected = true;
                _raiseConnected();
            }
            else
            {
                mConnected = false;
                _raiseClosed();
            }
        }
        catch (Exception ex)
        {
            _raiseError(ex);
            _raiseClosed();
            return;
        }
    }

    //---------------------------------------------------------------------
    void _raiseDataReceived(byte[] data, int len)
    {
        if (DataReceived != null)
        {
            DataReceived(data, len);
        }
    }

    //---------------------------------------------------------------------
    void _raiseConnected()
    {
        if (Connected != null)
        {
            Connected(null, EventArgs.Empty);
        }
    }

    //---------------------------------------------------------------------
    void _raiseClosed()
    {
        if (Closed != null)
        {
            Closed(null, EventArgs.Empty);
        }
    }

    //---------------------------------------------------------------------
    void _raiseError(Exception e)
    {
        if (Error != null)
        {
            Error(null, new SocketErrorEventArgs(e));
        }
    }
}
