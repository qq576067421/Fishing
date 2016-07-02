using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using org.apache.zookeeper;
using org.apache.zookeeper.data;
using Orleans;
using GF.Common;
using GF.Server;
using Ps;

class CellZkWatcher : Watcher
{
    //-------------------------------------------------------------------------
    public CellZkWatcher()
    {
    }

    //-------------------------------------------------------------------------
    public override Task process(org.apache.zookeeper.WatchedEvent @event)
    {
        return TaskDone.Done;
    }
}

public class DbClientZk
{
    //-------------------------------------------------------------------------
    public static DbClientZk Instance { get; private set; }
    const int ZOOKEEPER_CONNECTION_TIMEOUT = 2000;
    string ZkConnectionString { get; set; }
    CellZkWatcher Watcher { get; set; }
    ZooKeeper ZkClient { get; set; }


    //-------------------------------------------------------------------------
    public DbClientZk()
    {
        Instance = this;
        ZkConnectionString = ConfigurationManager.AppSettings["ZkConnectionStr"];
        Watcher = new CellZkWatcher();
        ZkClient = new ZooKeeper(ZkConnectionString, ZOOKEEPER_CONNECTION_TIMEOUT, Watcher);
    }

    //-------------------------------------------------------------------------
    public async Task<bool> TryTransaction(Func<Transaction, Transaction> transaction_func)
    {
        try
        {
            await UsingZk(zk => transaction_func(zk.transaction()).commitAsync());
            return true;
        }
        catch (KeeperException e)
        {
            // these exceptions are thrown when the transaction fails to commit due to semantical reasons
            if (e is KeeperException.NodeExistsException || e is KeeperException.NoNodeException ||
                e is KeeperException.BadVersionException)
            {
                return false;
            }
            throw;
        }
    }

    //-------------------------------------------------------------------------
    public Task UsingZk(Func<ZooKeeper, Task> zk_method)
    {
        return ZooKeeper.Using(ZkConnectionString, ZOOKEEPER_CONNECTION_TIMEOUT, Watcher, zk_method);
    }

    //-------------------------------------------------------------------------
    public Task UsingZk(string connect_string, Func<ZooKeeper, Task> zk_method)
    {
        return ZooKeeper.Using(connect_string, ZOOKEEPER_CONNECTION_TIMEOUT, Watcher, zk_method);
    }

    //-------------------------------------------------------------------------
    //Task<T> UsingZookeeper<T>(Func<ZooKeeper, Task<T>> zkMethod)
    //{
    //    return ZooKeeper.Using(deploymentConnectionString, ZOOKEEPER_CONNECTION_TIMEOUT, watcher, zkMethod);
    //}
}
