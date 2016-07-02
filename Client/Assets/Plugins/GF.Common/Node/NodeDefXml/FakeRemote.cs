using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteStream
{
    // 稍微模拟了一下
    class FakeRemoteRead : System.IO.MemoryStream
    {
        public System.IO.Stream lockedStream = null;

        public FakeRemoteRead()
        {
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }
        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new Exception("can't");
        }
        public void RealWrite(byte[] buffer, int offset, int count)
        {
            base.Write(buffer, offset, count);
        }
        public override void WriteByte(byte value)
        {
            throw new Exception("can't");
        }
        public override void WriteTo(System.IO.Stream stream)
        {
            throw new Exception("can't");
        }
        public override void Close()
        {
            base.Close();
            if (lockedStream != null)
            {
                lockedStream.Close();
            }
        }
    }

    class FakeRemoteWrite : System.IO.MemoryStream
    {
        public System.IO.Stream lockedStream = null;

        public FakeRemoteWrite()
        {
        }

        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new Exception("can't");
        }

        public override int ReadByte()
        {
            throw new Exception("can't");
        }

        public override void Close()
        {
            byte[] buf = new byte[this.Length];
            this.Seek(0, System.IO.SeekOrigin.Begin);
            base.Read(buf, 0, (int)this.Length);
            base.Close();
            if (lockedStream != null)
            {
                lockedStream.Write(buf, 0, (int)this.Length);
                lockedStream.Close();
            }
        }
    }

    public class FakeRemote : IRemoteStream
    {
        public System.IO.Stream OpenRead(string path, bool bLock)
        {
            if (System.IO.File.Exists(path) == false)
            {
                return null;
            }
            System.IO.Stream fs = System.IO.File.Open(path, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            try
            {
                fs.Seek(0, System.IO.SeekOrigin.End);
                int ilen = (int)fs.Position;
                fs.Seek(0, System.IO.SeekOrigin.Begin);
                if (ilen == 0)
                {
                    try
                    {
                        fs.Close();
                    }
                    catch
                    {

                    }
                    return null;
                }
                FakeRemoteRead rs = new FakeRemoteRead();
                byte[] buf = new byte[ilen];
                fs.Read(buf, 0, ilen);
                rs.RealWrite(buf, 0, ilen);
                rs.Seek(0, System.IO.SeekOrigin.Begin);
                if (bLock)
                {
                    rs.lockedStream = fs;
                }
                else
                {
                    fs.Close();
                }
                return rs;
            }
            catch
            {
                try
                {
                    fs.Close();
                }
                catch
                {

                }
                return null;
            }
        }

        public System.IO.Stream OpenWrite(string path)
        {
            System.IO.Stream fs = System.IO.File.Open(path, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            FakeRemoteWrite fw = new FakeRemoteWrite();
            fw.lockedStream = fs;
            return fs;
        }

        public void Delete(string path)
        {
            System.IO.File.Delete(path);
        }
    }
}
