using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using GF.Common;

//-----------------------------------------------------------------------------
[Serializable]
public enum _eNodeState : byte
{
    Init = 0,
    Start,
    Run,
    Stop,
    Release
}

//-----------------------------------------------------------------------------
[Serializable]
public enum _eNodeParam : byte
{
    PreNodeId = 0,
    AllExit,
    ExitId
}

//-----------------------------------------------------------------------------
[Serializable]
public enum _eNodeOp : byte
{
    CreateNode = 0,
    DestroyNode,
    EnterState
}

//-----------------------------------------------------------------------------
[Serializable]
[ProtoContract]
public struct _tNodeParamPair
{
    [ProtoMember(1)]
    public byte k;
    [ProtoMember(2, DynamicType = true)]
    public object v;
}

//-----------------------------------------------------------------------------
[Serializable]
[ProtoContract]
public class _tNodeInfo
{
    [ProtoMember(1)]
    public int id = 0;
    [ProtoMember(2)]
    public byte state = (byte)_eNodeState.Init;
    [ProtoMember(3)]
    public List<_tNodeParamPair> list_param;
    [ProtoMember(4)]
    public List<_tNodeInfo> list_child;
}

//-----------------------------------------------------------------------------
[Serializable]
[ProtoContract]
public struct _tNodeOp
{
    [ProtoMember(1)]
    public byte op;//_eNodeOp
    [ProtoMember(2)]
    public int id;
    [ProtoMember(3)]
    public byte state;//_eNodeState
    [ProtoMember(4)]
    public List<_tNodeParamPair> list_param;
}

//-----------------------------------------------------------------------------
public enum _eNodeMsg : byte
{
    NodeEnterStart = 0,
    NodeEnterRun,
    NodeLeaveStop
}

//-----------------------------------------------------------------------------
public struct _tMsgInfo
{
    public int msg_type;
    public int msg_id;
}
