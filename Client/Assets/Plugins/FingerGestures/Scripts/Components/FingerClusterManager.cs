using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu( "FingerGestures/Components/Finger Cluster Manager" )]
public class FingerClusterManager : MonoBehaviour 
{
    [System.Serializable]
    public class Cluster
    {
        public int Id = 0;
        public float StartTime = 0;
        public FingerGestures.FingerList Fingers = new FingerGestures.FingerList();

        public void Reset()
        {
            Fingers.Clear();
        }
    }

    public DistanceUnit DistanceUnit = DistanceUnit.Pixels;
    public float ClusterRadius = 250.0f; // spatial grouping
    public float TimeTolerance = 0.5f;  // temporal grouping

    int lastUpdateFrame = -1;
    int nextClusterId = 1;
    List<Cluster> clusters; // active clusters
    List<Cluster> clusterPool;
    FingerGestures.FingerList fingersAdded;
    FingerGestures.FingerList fingersRemoved;

    public FingerGestures.IFingerList FingersAdded
    {
        get { return fingersAdded; }
    }
    
    public FingerGestures.IFingerList FingersRemoved
    {
        get { return fingersRemoved; }
    }

    public List<Cluster> Clusters
    {
        get { return clusters; }
    }

    public List<Cluster> GetClustersPool() { return clusterPool; }
    
    void Awake()
    {
        clusters = new List<Cluster>();
        clusterPool = new List<Cluster>();
        fingersAdded = new FingerGestures.FingerList();
        fingersRemoved = new FingerGestures.FingerList();
    }

	public void Update()
    {
        // already updated this frame, skip
        if( lastUpdateFrame == Time.frameCount )
            return;

        lastUpdateFrame = Time.frameCount;

        fingersAdded.Clear();
        fingersRemoved.Clear();

        for( int i = 0; i < FingerGestures.Instance.MaxFingers; ++i )
        {
            FingerGestures.Finger finger = FingerGestures.GetFinger( i );

            if( finger.IsDown )
            {
                // new touch?
                if( !finger.WasDown )
                {
                    //Debug.Log( "ADDED " + finger );
                    fingersAdded.Add( finger );
                }
            }
            else
            {
                // lifted off finger
                if( finger.WasDown )
                {
                    //Debug.Log( "REMOVED " + finger );
                    fingersRemoved.Add( finger );
                }
            }
        }

        // remove fingers from clusters
        for( int i = 0; i < fingersRemoved.Count; ++i )
        {
            FingerGestures.Finger finger = fingersRemoved[i];

            // update active clusters
            for( int clusterIndex = clusters.Count - 1; clusterIndex >= 0; --clusterIndex )
            {
                Cluster cluster = clusters[clusterIndex];

                if( cluster.Fingers.Remove( finger ) )
                {
                    // retire clusters that no longer have any fingers left
                    if( cluster.Fingers.Count == 0 )
                    {
                        //Debug.Log( "Recycling cluster " + cluster.Id );

                        // remove from active clusters list
                        clusters.RemoveAt( clusterIndex );

                        // move back to pool
                        clusterPool.Add( cluster );
                    }
                }
            }
        }

        // add new fingers
        for( int i = 0; i < fingersAdded.Count; ++i )
        {
            FingerGestures.Finger finger = fingersAdded[i];

            // try to add finger to existing cluster
            Cluster cluster = FindExistingCluster( finger );

            // no valid active cluster found for that finger, create a new cluster
            if( cluster == null )
            {
                cluster = NewCluster();
                cluster.StartTime = finger.StarTime;
                //Debug.Log( "Created NEW cluster " + cluster.Id + " for " + finger );                
            }
            else
            {
                //Debug.Log( "Found existing cluster " + cluster.Id + " for " + finger );
            }

            // add finger to selected cluster
            cluster.Fingers.Add( finger );
        }
    }

    public Cluster FindClusterById( int clusterId )
    {
        return clusters.Find( c => c.Id == clusterId );
    }

    Cluster NewCluster()
    {
        Cluster cluster = null;

        if( clusterPool.Count == 0 )
        {
            cluster = new Cluster();
        }
        else
        {
            int lastIdx = clusterPool.Count - 1;
            cluster = clusterPool[lastIdx];
            cluster.Reset();
            clusterPool.RemoveAt( lastIdx );
        }

        // assign a new ID
        cluster.Id = nextClusterId++;

        // add to active clusters
        clusters.Add( cluster );    // add cluster to active clusters list

        //Debug.Log( "Created new finger cluster #" + cluster.Id );
        return cluster;
    }

    // Find closest cluster within radius
    Cluster FindExistingCluster( FingerGestures.Finger finger )
    {
        Cluster best = null;
        float bestSqrDist = float.MaxValue;

        float sqrClusterRadiusInPixels = FingerGestures.Convert( ClusterRadius * ClusterRadius, DistanceUnit, DistanceUnit.Pixels );

        for( int i = 0; i < clusters.Count; ++i )
        {
            Cluster cluster = clusters[i];
            float elapsedTime = finger.StarTime - cluster.StartTime;

            // temporal grouping criteria
            if( elapsedTime > TimeTolerance )
                continue;

            Vector2 centroid = cluster.Fingers.GetAveragePosition();
            float sqrDist = Vector2.SqrMagnitude( finger.Position - centroid );

            if( sqrDist < bestSqrDist && sqrDist < sqrClusterRadiusInPixels )
            {
                best = cluster;
                bestSqrDist = sqrDist;
            }
        }

        return best;
    }
}
