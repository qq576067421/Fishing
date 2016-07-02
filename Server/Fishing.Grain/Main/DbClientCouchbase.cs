using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.Configuration.Client;
using Couchbase.Configuration.Client.Providers;
using Couchbase.Core;
using Couchbase.IO;
using Couchbase.N1QL;
using Couchbase.Views;
using Newtonsoft.Json;
using Orleans.Storage.Couchbase;
using GF.Common;

public class DbClientCouchbase
{
    //-------------------------------------------------------------------------
    public static DbClientCouchbase Instance { get; private set; }
    public static Cluster Cluster { get; set; }
    public IBucket Bucket { get; set; }

    //-------------------------------------------------------------------------
    public DbClientCouchbase()
    {
        Instance = this;
        Cluster = GrainStateCouchbaseDataManager.Cluster;
        Bucket = GrainStateCouchbaseDataManager.Instance.Bucket;
    }

    //-------------------------------------------------------------------------
    public async Task<string> asyncLoad(string key)
    {
        var op_result = await DbClientCouchbase.Instance.Bucket.GetAsync<string>(key);

        var data = string.Empty;
        if (op_result.Status == ResponseStatus.Success)
        {
            data = op_result.Value;
        }
        else if (op_result.Status == ResponseStatus.KeyNotFound)
        {
            data = string.Empty;
        }
        else
        {
            throw new Exception("Read from Couchbase Error: ", op_result.Exception);
        }

        return data;
    }

    //-------------------------------------------------------------------------
    public Task asyncSave(string key, string data)
    {
        return Bucket.UpsertAsync(key, data);
    }

    //-------------------------------------------------------------------------
    public Task asyncSave(string key, string data, TimeSpan expiration)
    {
        return Bucket.UpsertAsync<string>(key, data, expiration);
    }

    //-------------------------------------------------------------------------
    public Task asyncRemove(string key)
    {
        return Bucket.RemoveAsync(key);
    }

    //-------------------------------------------------------------------------
    public Task<bool> asyncExists(string key)
    {
        return Bucket.ExistsAsync(key);
    }

    //-------------------------------------------------------------------------
    public Task<IOperationResult> asyncTouch(string key, TimeSpan expiration)
    {
        return Bucket.TouchAsync(key, expiration);
    }

    //-------------------------------------------------------------------------
    public async Task<ulong> IncrementAsync(string key)
    {
        var tcs = new TaskCompletionSource<IOperationResult<ulong>>();

        WaitCallback increment_item = (state) =>
        {
            try
            {
                var result = this.Bucket.Increment(key);
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                tcs.SetException(new Exception("Increment from couchbase server error", ex));
            }
        };

        ThreadPool.QueueUserWorkItem(increment_item, null);

        var op_result = await tcs.Task;

        ulong data = 0;

        if (op_result.Status == ResponseStatus.Success)
        {
            data = op_result.Value;
        }
        else if (op_result.Status == ResponseStatus.KeyNotFound)
        {
            data = 0;
        }
        else
        {
            throw new Exception("Increment from couchbase server error", op_result.Exception);
        }

        return data;
    }

    //-------------------------------------------------------------------------
    // 摘要: 
    //     Asynchronously executes a N1QL statement or prepared statement via a Couchbase.N1QL.IQueryRequest
    //     against the Couchbase Cluster.
    // 参数: 
    //   queryRequest:
    //     An Couchbase.N1QL.IQueryRequest object that contains a statement or a prepared
    //     statement and the appropriate properties.
    // 类型参数: 
    //   T:
    //     The Type to deserialze the results to. The dynamic Type works well.
    // 返回结果: 
    //     An instance of an object that implements the Couchbase.N1QL.IQueryResult<T>
    //     interface; the results of the query.
    Task<IQueryResult<T>> QueryAsync<T>(IQueryRequest query_request)
    {
        return this.Bucket.QueryAsync<T>(query_request);
    }

    //-------------------------------------------------------------------------
    // 摘要: 
    //     Asynchronously executes a N1QL query against the Couchbase Cluster.
    // 参数: 
    //   query:
    //     An ad-hoc N1QL query.
    // 类型参数: 
    //   T:
    //     The Type to deserialze the results to. The dynamic Type works well.
    // 返回结果: 
    //     An awaitable System.Threading.Tasks.Task<TResult> with the T a Couchbase.N1QL.IQueryResult<T>
    //     instance.
    // 备注: 
    //     Note this implementation is uncommitted/experimental and subject to change
    //     in future release!
    Task<IQueryResult<T>> QueryAsync<T>(string query)
    {
        return this.Bucket.QueryAsync<T>(query);
    }

    //-------------------------------------------------------------------------
    // 摘要: 
    //     Asynchronously Executes a View query and returns the result.
    // 参数: 
    //   query:
    //     The Couchbase.Views.IViewQuery used to generate the results.
    // 类型参数: 
    //   T:
    //     The Type to deserialze the results to. The dynamic Type works well.
    // 返回结果: 
    //     An awaitable System.Threading.Tasks.Task<TResult> with the T a Couchbase.Views.IViewResult<T>
    //     instance.
    // 备注: 
    //     Note this implementation is experimental and subject to change in future
    //     release!
    //var query = bucket.CreateQuery("beer", "brewery_beers").Limit(10);
    //var result = await bucket.QueryAsync<dynamic>(query);
    Task<IViewResult<T>> QueryAsync<T>(IViewQuery query)
    {
        return this.Bucket.QueryAsync<T>(query);
    }
}
