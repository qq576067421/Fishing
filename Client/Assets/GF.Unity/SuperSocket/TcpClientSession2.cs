using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using GF.Common;

//public delegate void OnSocketReceive(byte[] data, int len);
//public delegate void OnSocketConnected(object client, EventArgs args);
//public delegate void OnSocketClosed(object client, EventArgs args);
//public delegate void OnSocketError(object rec, SocketErrorEventArgs args);

//public class TcpStateObj
//{
//    public Socket socket;
//    public byte[] buffer;
//}

//public class DataEventArgs : EventArgs
//{
//    public byte[] Data { get; set; }
//    public int Offset { get; set; }
//    public int Length { get; set; }
//}

//public class SocketErrorEventArgs : EventArgs
//{
//    public Exception Exception { get; private set; }

//    public SocketErrorEventArgs(Exception exception)
//    {
//        Exception = exception;
//    }
//}

public class TcpClientSession2 : IDisposable
{
    //-------------------------------------------------------------------------
    readonly int MAX_RECEIVE_LEN = 8192;
    Socket mSocket;
    EndPoint mRemoteEndPoint;
    byte[] m_KeepAliveOptionValues;
    byte[] m_KeepAliveOptionOutValues;
    volatile bool mDisposed;
    volatile bool mSending;
    Queue<byte[]> mQueSending = new Queue<byte[]>();
    object mLockQueSending = new object();

    //-------------------------------------------------------------------------
    public bool IsConnected { get { return (mSocket == null || mDisposed) ? false : mSocket.Connected; } }
    public OnSocketReceive DataReceived { get; set; }
    public OnSocketConnected Connected { get; set; }
    public OnSocketClosed Closed { get; set; }
    public OnSocketError Error { get; set; }

    //-------------------------------------------------------------------------
    public TcpClientSession2(EndPoint remote_endpoint)
    {
        mRemoteEndPoint = remote_endpoint;

        m_KeepAliveOptionValues = new byte[sizeof(uint) * 3];
        m_KeepAliveOptionOutValues = new byte[m_KeepAliveOptionValues.Length];
        // whether enable KeepAlive
        BitConverter.GetBytes((uint)1).CopyTo(m_KeepAliveOptionValues, 0);
        // how long will start first keep alive
        BitConverter.GetBytes((uint)(30 * 1000)).CopyTo(m_KeepAliveOptionValues, sizeof(uint));
        // keep alive interval
        BitConverter.GetBytes((uint)(300 * 1000)).CopyTo(m_KeepAliveOptionValues, sizeof(uint) * 2);
    }

    //-------------------------------------------------------------------------
    ~TcpClientSession2()
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
    public void connect()
    {
        try
        {
            if (mDisposed) return;

            mSending = false;
            mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            mSocket.SendBufferSize = MAX_RECEIVE_LEN;
            mSocket.ReceiveBufferSize = MAX_RECEIVE_LEN;
            mSocket.NoDelay = true;
#if !SILVERLIGHT
            // Set keep alive
            mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
#endif
            mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, m_KeepAliveOptionValues);

            var ip_endpoint = mRemoteEndPoint as IPEndPoint;
            mSocket.BeginConnect(ip_endpoint.Address, ip_endpoint.Port, new AsyncCallback(_onConnect), mSocket);
        }
        catch (Exception ex)
        {
            _raiseError(ex);
            _raiseClosed();
            return;
        }
    }

    //-------------------------------------------------------------------------
    public void send(byte[] buf)
    {
        if (mSending)
        {
            lock (mLockQueSending)
            {
                mQueSending.Enqueue(buf);
            }
        }
        else
        {
            _write(buf);
        }
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
                _raiseConnected();
            }
            else
            {
                _raiseClosed();
            }
        }
        catch (Exception ex)
        {
            _raiseError(ex);
            _raiseClosed();
            return;
        }

        _read();
    }

    //-------------------------------------------------------------------------
    void _read()
    {
        try
        {
            if (mDisposed) return;

            TcpStateObj obj = new TcpStateObj();
            obj.socket = mSocket;
            obj.buffer = new byte[MAX_RECEIVE_LEN];
            mSocket.BeginReceive(obj.buffer, 0, MAX_RECEIVE_LEN, SocketFlags.None, new AsyncCallback(_onRead), obj);
        }
        catch (Exception ex)
        {
            _raiseError(ex);
            _raiseClosed();
            return;
        }
    }

    //-------------------------------------------------------------------------
    void _onRead(IAsyncResult result)
    {
        TcpStateObj obj = null;
        int num = 0;
        try
        {
            obj = (TcpStateObj)result.AsyncState;
            num = obj.socket.EndReceive(result);
        }
        catch (Exception ex)
        {
            _raiseError(ex);
            _raiseClosed();
            return;
        }

        if (num <= 0 || obj == null)
        {
            _raiseClosed();
            return;
        }

        byte[] recv_buf = new byte[num];
        Array.Copy(obj.buffer, 0, recv_buf, 0, recv_buf.Length);
        _raiseDataReceived(recv_buf, recv_buf.Length);

        _read();
    }

    //-------------------------------------------------------------------------
    void _write(byte[] buf)
    {
        try
        {
            if (mDisposed) return;

            mSending = true;
            mSocket.BeginSend(buf, 0, buf.Length, SocketFlags.None, new AsyncCallback(_onWrite), mSocket);
        }
        catch (Exception ex)
        {
            _raiseError(ex);
            _raiseClosed();
            return;
        }
    }

    //-------------------------------------------------------------------------
    void _onWrite(IAsyncResult result)
    {
        try
        {
            Socket socket = (Socket)result.AsyncState;
            socket.EndSend(result);
        }
        catch (Exception ex)
        {
            _raiseError(ex);
            _raiseClosed();
            return;
        }

        byte[] buf = null;
        lock (mLockQueSending)
        {
            if (mQueSending.Count > 0) buf = mQueSending.Dequeue();
        }
        if (buf != null)
        {
            _write(buf);
        }
        else
        {
            mSending = false;
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
