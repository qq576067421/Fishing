using System;
using System.Collections.Generic;
using System.IO;

namespace GF.Common
{
    public abstract class IComponent
    {
        //---------------------------------------------------------------------
        public Entity Entity { internal set; get; }
        public EntityMgr EntityMgr { internal set; get; }
        public EntityEventPublisher Publisher { get { return Entity.Publisher; } }
        public bool EnableUpdate { set; get; }
        public bool EnableSave2Db { set; get; }
        public bool EnableNetSync { get; set; }
        internal bool _Init { set; get; }

        //---------------------------------------------------------------------
        public IComponent()
        {
            EnableUpdate = true;
            EnableSave2Db = true;
            EnableNetSync = true;
        }

        //---------------------------------------------------------------------
        public abstract void awake();

        //---------------------------------------------------------------------
        public abstract void init();

        //---------------------------------------------------------------------
        public abstract void release();

        //---------------------------------------------------------------------
        public abstract void update(float elapsed_tm);

        //---------------------------------------------------------------------
        public abstract void handleEvent(object sender, EntityEvent e);

        //---------------------------------------------------------------------
        public abstract void onChildInit(Entity child);

        //---------------------------------------------------------------------
        public abstract ComponentDef getDef();

        //---------------------------------------------------------------------
        internal abstract void _genDef(Dictionary<string, string> map_param);
    }

    public class Component<TDef> : IComponent where TDef : ComponentDef, new()
    {
        //---------------------------------------------------------------------
        private TDef mDef = null;

        //---------------------------------------------------------------------
        public TDef Def { private set { mDef = value; } get { return mDef; } }

        //---------------------------------------------------------------------
        public override void awake()
        {
        }

        //---------------------------------------------------------------------
        public override void init()
        {
        }

        //---------------------------------------------------------------------
        public override void release()
        {
        }

        //---------------------------------------------------------------------
        public override void update(float elapsed_tm)
        {
        }

        //---------------------------------------------------------------------
        public override void handleEvent(object sender, EntityEvent e)
        {
        }

        //---------------------------------------------------------------------
        public override void onChildInit(Entity child)
        {
        }

        //---------------------------------------------------------------------
        public override ComponentDef getDef()
        {
            return mDef;
        }

        //---------------------------------------------------------------------
        public void defNodeRpcMethod(ushort method_id, Action action)
        {
            EntityMgr._RpcCallee._defRpcMethod(method_id, action);
        }

        //---------------------------------------------------------------------
        public void defNodeRpcMethod<T1>(ushort method_id, Action<T1> action)
        {
            EntityMgr._RpcCallee._defRpcMethod<T1>(method_id, action);
        }

        //---------------------------------------------------------------------
        public void defNodeRpcMethod<T1, T2>(ushort method_id, Action<T1, T2> action)
        {
            EntityMgr._RpcCallee._defRpcMethod<T1, T2>(method_id, action);
        }

        //---------------------------------------------------------------------
        public void defNodeRpcMethod<T1, T2, T3>(ushort method_id, Action<T1, T2, T3> action)
        {
            EntityMgr._RpcCallee._defRpcMethod<T1, T2, T3>(method_id, action);
        }

        //---------------------------------------------------------------------
        public void defNodeRpcMethod<T1, T2, T3, T4>(ushort method_id, Action<T1, T2, T3, T4> action)
        {
            EntityMgr._RpcCallee._defRpcMethod<T1, T2, T3, T4>(method_id, action);
        }

        //---------------------------------------------------------------------
        public void rpcBySession(IRpcSession session, ushort method_id)
        {
            if (session != null) session.send(method_id, null);
        }

        //---------------------------------------------------------------------
        public void rpcBySession<T1>(IRpcSession session, ushort method_id, T1 obj1)
        {
            byte[] data = null;

            using (MemoryStream s = new MemoryStream())
            {
                try
                {
                    ProtoBuf.Serializer.Serialize<T1>(s, obj1);
                    data = s.ToArray();
                }
                catch (Exception ex)
                {
                    EbLog.Note("Component.rpcBySession<T1>() Serializer Error! MethodId="
                        + method_id + " Exception:" + ex.ToString());
                    EbLog.Note("Type1=" + typeof(T1).Name);
                    return;
                }
            }

            if (session != null) session.send(method_id, data);
        }

        //---------------------------------------------------------------------
        public void rpcBySession<T1, T2>(IRpcSession session, ushort method_id, T1 obj1, T2 obj2)
        {
            byte[] data = null;

            using (MemoryStream s = new MemoryStream())
            {
                try
                {
                    ProtoBuf.Serializer.SerializeWithLengthPrefix<T1>(s, obj1, ProtoBuf.PrefixStyle.Fixed32);
                    ProtoBuf.Serializer.SerializeWithLengthPrefix<T2>(s, obj2, ProtoBuf.PrefixStyle.Fixed32);
                    data = s.ToArray();
                }
                catch (Exception ex)
                {
                    EbLog.Note("Component.rpcBySession<T1,T2>() Serializer Error! MethodId="
                        + method_id + " Exception:" + ex.ToString());
                    EbLog.Note("Type1=" + typeof(T1).Name + " Type2=" + typeof(T2).Name);
                    return;
                }
            }

            if (session != null) session.send(method_id, data);
        }

        //---------------------------------------------------------------------
        public void rpcBySession<T1, T2, T3>(IRpcSession session, ushort method_id, T1 obj1, T2 obj2, T3 obj3)
        {
            byte[] data = null;

            using (MemoryStream s = new MemoryStream())
            {
                try
                {
                    ProtoBuf.Serializer.SerializeWithLengthPrefix<T1>(s, obj1, ProtoBuf.PrefixStyle.Fixed32);
                    ProtoBuf.Serializer.SerializeWithLengthPrefix<T2>(s, obj2, ProtoBuf.PrefixStyle.Fixed32);
                    ProtoBuf.Serializer.SerializeWithLengthPrefix<T3>(s, obj3, ProtoBuf.PrefixStyle.Fixed32);
                    data = s.ToArray();
                }
                catch (Exception ex)
                {
                    EbLog.Error("Component.rpcBySession<T1,T2,T3>() Serializer Error! MethodId="
                        + method_id + " Exception:" + ex.ToString());
                    EbLog.Error("Type1=" + typeof(T1).Name + " Type2=" + typeof(T2).Name + " Type3=" + typeof(T3).Name);
                    return;
                }
            }

            if (session != null) session.send(method_id, data);
        }

        //---------------------------------------------------------------------
        public void rpcBySession<T1, T2, T3, T4>(IRpcSession session, ushort method_id, T1 obj1, T2 obj2, T3 obj3, T4 obj4)
        {
            byte[] data = null;

            using (MemoryStream s = new MemoryStream())
            {
                try
                {
                    ProtoBuf.Serializer.SerializeWithLengthPrefix<T1>(s, obj1, ProtoBuf.PrefixStyle.Fixed32);
                    ProtoBuf.Serializer.SerializeWithLengthPrefix<T2>(s, obj2, ProtoBuf.PrefixStyle.Fixed32);
                    ProtoBuf.Serializer.SerializeWithLengthPrefix<T3>(s, obj3, ProtoBuf.PrefixStyle.Fixed32);
                    ProtoBuf.Serializer.SerializeWithLengthPrefix<T4>(s, obj4, ProtoBuf.PrefixStyle.Fixed32);
                    data = s.ToArray();
                }
                catch (Exception ex)
                {
                    EbLog.Error("Component.rpcBySession<T1,T2,T3,T4>() Serializer Error! MethodId="
                        + method_id + " Exception:" + ex.ToString());
                    EbLog.Error("Type1=" + typeof(T1).Name + " Type2=" + typeof(T2).Name
                        + " Type3=" + typeof(T3).Name + " Type4=" + typeof(T4).Name);
                    return;
                }
            }

            if (session != null) session.send(method_id, data);
        }

        //---------------------------------------------------------------------
        internal override void _genDef(Dictionary<string, string> map_param)
        {
            mDef = new TDef();
            mDef.defAllProp(map_param);
        }
    }
}
