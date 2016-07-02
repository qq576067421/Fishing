using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using GF.Common;
using GF.Server;
using Ps;

public class CellApp
{
    //-------------------------------------------------------------------------
    Dictionary<string, BtFactory> mMapBtFactory = new Dictionary<string, BtFactory>();

    //-------------------------------------------------------------------------
    public static CellApp Instance { get; private set; }
    public CellConfig Cfg { get; private set; }
    public ThreadSafeRandom Rd { get; private set; }
    public JsonConfig jsonCfg { get; private set; }
    //-------------------------------------------------------------------------
    public CellApp()
    {
        Instance = this;

        Cfg = new CellConfig();
        Rd = new ThreadSafeRandom();
        jsonCfg = new JsonConfig();
        // 注册BtFactory
        _regBtFactory(new BtFactoryBot());
        _regBtFactory(new BtFactoryPlayer());
    }

    //-------------------------------------------------------------------------
    public Bt createBt(string bt_name, Entity self)
    {
        BtFactory bt_factory = null;
        mMapBtFactory.TryGetValue(bt_name, out bt_factory);

        if (bt_factory == null) return null;
        else return bt_factory.createBt(self);
    }

    //-------------------------------------------------------------------------
    void _regBtFactory(BtFactory bt_factory)
    {
        mMapBtFactory[bt_factory.getName()] = bt_factory;
    }
}
