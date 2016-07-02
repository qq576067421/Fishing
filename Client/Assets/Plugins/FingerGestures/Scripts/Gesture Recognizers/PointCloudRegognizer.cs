using UnityEngine;
using System.Collections.Generic;

// Implementation of the $P Gesture Recognizer (Point Cloud)
//  http://depts.washington.edu/aimgroup/proj/dollar/pdollar.pdf

public class PointCloudGesture : DiscreteGesture
{
    public List<PointCloudRegognizer.Point> RawPoints = new List<PointCloudRegognizer.Point>( 64 );
    public List<PointCloudRegognizer.Point> NormalizedPoints = new List<PointCloudRegognizer.Point>( 64 );
    public PointCloudGestureTemplate RecognizedTemplate = null;
    public float MatchDistance = 0; // absolute match distance
    public float MatchScore = 0;    // 0% -> 100%
}

/*
public class PointCloudGestureTemplate
{
    public string Name = "CustomGesture";
    public List<PointCloudRegognizer.Point> Points = new List<PointCloudRegognizer.Point>();
}*/
[AddComponentMenu( "FingerGestures/Gestures/PointCloud Recognizer" )]
public class PointCloudRegognizer : DiscreteGestureRecognizer<PointCloudGesture>
{
    /// <summary>
    /// Minimum distance between two consecutive sample points (used while recording the user's current gesture)
    ///  in pixels
    /// </summary>
    public float MinDistanceBetweenSamples = 5;

    /// <summary>
    /// Maximum gesture-to-template distance (difference) allowed to register as a valid gesture match
    /// </summary>
    public float MaxMatchDistance = 3.5f;

    /// <summary>
    /// List of gesture templates to match the user's gesture against
    /// </summary>
    public List<PointCloudGestureTemplate> Templates;

    #region Runtime Gesture Processing

    [System.Serializable]
    public struct Point
    {
        public Point( int strokeId, Vector2 pos )
        {
            this.StrokeId = strokeId;
            this.Position = pos;
        }

        public Point( int strokeId, float x, float y )
        {
            this.StrokeId = strokeId;
            this.Position = new Vector2( x, y );
        }

        public int StrokeId;
        public Vector2 Position;
    }

    class NormalizedTemplate
    {
        public PointCloudGestureTemplate Source;
        public List<Point> Points; // normalized points
    }

    // utility class to normalize a set of sampled points without doing too much superfluous memory allocations
    // as this is potentially called on each frame a PointCloud gesture is being recognized
    class GestureNormalizer
    {
        List<Point> normalizedPoints;
        List<Point> pointBuffer;

        public GestureNormalizer()
        {
            normalizedPoints = new List<Point>();
            pointBuffer = new List<Point>();
        }

        public List<Point> Apply( List<Point> inputPoints, int normalizedPointsCount )
        {
            normalizedPoints = Resample( inputPoints, normalizedPointsCount );
            Scale( normalizedPoints );
            TranslateToOrigin( normalizedPoints );
            return normalizedPoints;
        }

        // X points => normalizedPointsCount points
        List<Point> Resample( List<Point> points, int normalizedPointsCount )
        {
            float intervalLength = PathLength( points ) / ( normalizedPointsCount - 1 );
            float D = 0;
            Point q = new Point();

            normalizedPoints.Clear();
            normalizedPoints.Add( points[0] );

            pointBuffer.Clear();
            pointBuffer.AddRange( points );

            for( int i = 1; i < pointBuffer.Count; ++i )
            {
                Point a = pointBuffer[i - 1];
                Point b = pointBuffer[i];

                if( a.StrokeId == b.StrokeId )
                {
                    float d = Vector2.Distance( a.Position, b.Position );

                    if( ( D + d ) > intervalLength )
                    {
                        q.Position = Vector2.Lerp( a.Position, b.Position, ( intervalLength - D ) / d );
                        q.StrokeId = a.StrokeId;

                        normalizedPoints.Add( q );
                        pointBuffer.Insert( i, q ); // q becomes the next "b" (point[i])

                        D = 0;
                    }
                    else
                    {
                        D += d;
                    }
                }
            }

            // sometimes we fall a rounding-error short of adding the last point, so add it if so
            if( normalizedPoints.Count == normalizedPointsCount - 1 )
                normalizedPoints.Add( pointBuffer[pointBuffer.Count - 1] );

            return normalizedPoints;
        }

        // compute total length of the set of strokes
        static float PathLength( List<Point> points )
        {
            float d = 0;

            for( int i = 1; i < points.Count; ++i )
            {
                if( points[i].StrokeId == points[i - 1].StrokeId )
                    d += Vector2.Distance( points[i - 1].Position, points[i].Position );
            }

            return d;
        }

        // Rescale "points" in place with shape preservation so that the resulting bounding box will be within [0..1] range
        static void Scale( List<Point> points )
        {
            Vector2 min = new Vector2( float.PositiveInfinity, float.PositiveInfinity );
            Vector2 max = new Vector2( float.NegativeInfinity, float.NegativeInfinity );

            for( int i = 0; i < points.Count; ++i )
            {
                Point p = points[i];
                min.x = Mathf.Min( min.x, p.Position.x );
                min.y = Mathf.Min( min.y, p.Position.y );
                max.x = Mathf.Max( max.x, p.Position.x );
                max.y = Mathf.Max( max.y, p.Position.y );
            }

            float size = Mathf.Max( max.x - min.x, max.y - min.y );
            float invSize = 1.0f / size;

            for( int i = 0; i < points.Count; ++i )
            {
                Point p = points[i];
                p.Position = ( p.Position - min ) * invSize;
                points[i] = p;
            }
        }

        // translate points in place so the cloud is centered at the (0,0) origin
        static void TranslateToOrigin( List<Point> points )
        {
            Vector2 c = Centroid( points );

            for( int i = 0; i < points.Count; ++i )
            {
                Point p = points[i];
                p.Position -= c;
                points[i] = p;
            }
        }

        // compute the center of the points cloud
        static Vector2 Centroid( List<Point> points )
        {
            Vector2 c = Vector2.zero;

            for( int i = 0; i < points.Count; ++i )
                c += points[i].Position;

            c /= points.Count;
            return c;
        }
    }

    const int NormalizedPointCount = 32;
    GestureNormalizer normalizer;
    List<NormalizedTemplate> normalizedTemplates;

    #endregion

    protected override void Awake()
    {
        base.Awake();
        normalizer = new GestureNormalizer();
        normalizedTemplates = new List<NormalizedTemplate>();

        foreach( PointCloudGestureTemplate template in Templates )
            AddTemplate( template );
    }

    NormalizedTemplate FindNormalizedTemplate( PointCloudGestureTemplate template )
    {
        return normalizedTemplates.Find( t => ( t.Source == template ) );
    }

    List<Point> Normalize( List<Point> points )
    {
        return new List<Point>( normalizer.Apply( points, NormalizedPointCount ) );
    }

    public bool AddTemplate( PointCloudGestureTemplate template )
    {
        if( FindNormalizedTemplate( template ) != null )
        {
            Debug.LogWarning( "The PointCloud template " + template.name + " is already present in the list" );
            return false;
        }

        // convert template point entry to recognizer point
        List<Point> points = new List<Point>();
        for( int i = 0; i < template.PointCount; ++i )
            points.Add( new Point( template.GetStrokeId( i ), template.GetPosition( i ) ) );
        
        NormalizedTemplate nt = new NormalizedTemplate();
        nt.Source = template;
        nt.Points = Normalize( points );

        normalizedTemplates.Add( nt );
        return true;
    }

    protected override void OnBegin( PointCloudGesture gesture, FingerGestures.IFingerList touches )
    {
        gesture.StartPosition = touches.GetAverageStartPosition();
        gesture.Position = touches.GetAveragePosition();

        gesture.RawPoints.Clear();
        gesture.RawPoints.Add( new Point( 0, gesture.Position ) );
    }

    bool RecognizePointCloud( PointCloudGesture gesture )
    {
        debugLastGesture = gesture;

        gesture.MatchDistance = 0;
        gesture.MatchScore = 0;
        gesture.RecognizedTemplate = null;
        gesture.NormalizedPoints.Clear();

        if( gesture.RawPoints.Count < 2 )
            return false;
        
        gesture.NormalizedPoints.AddRange( normalizer.Apply( gesture.RawPoints, NormalizedPointCount ) );

        float bestDist = float.PositiveInfinity;

        for( int i = 0; i < normalizedTemplates.Count; ++i )
        {
            NormalizedTemplate template = normalizedTemplates[i];
            float d = GreedyCloudMatch( gesture.NormalizedPoints, template.Points );
            
            if( d < bestDist )
            {
                bestDist = d;
                gesture.RecognizedTemplate = template.Source;
                debugLastMatchedTemplate = template;
            }
        }
        
        if( gesture.RecognizedTemplate != null )
        {
            gesture.MatchDistance = bestDist;
            gesture.MatchScore = Mathf.Max( ( MaxMatchDistance - bestDist ) / MaxMatchDistance, 0.0f );
            //Debug.Log( "Matched: " + recognizedTemplateOut + " dist: " + bestDist + " Score: " + matchScoreOut );
        }

        return gesture.MatchScore > 0;
    }

    // Match two clouds (points and template) by performing repeated alignments between their points (each new alignment starts with a different starting point index i). 
    // Returns the minimum alignment cost.
    float GreedyCloudMatch( List<Point> points, List<Point> refPoints )
    {
        float e = 0.5f; // [0..1] controls the number of of tested alignments
        int step = Mathf.FloorToInt( Mathf.Pow( points.Count, 1 - e ) );
        float min = float.PositiveInfinity;

        for( int i = 0; i < points.Count; i += step )
        {
            float d1 = CloudDistance( points, refPoints, i );
            float d2 = CloudDistance( refPoints, points, i );
            min = Mathf.Min( min, d1, d2 );
        }

        return min;
    }

    // Distance between two point clouds
    // Compute the minimum-cost alignment between points1 and points2 starting with point #startIndex. 
    // Assign decreasing confidence weights [0::1] to point matchings.
    static float CloudDistance( List<Point> points1, List<Point> points2, int startIndex )
    {
        int numPoints = points1.Count;
        ResetMatched( numPoints );

    #if UNITY_EDITOR
        if( points1.Count != points2.Count )
        {
            Debug.LogError( "Points1 != Points2 count: " + points1.Count + " vs " + points2.Count );
            return float.PositiveInfinity;
        }
    #endif

        float sum = 0;
        int i = startIndex;
		
        do
        {
            int index = -1;
            float minDistance = float.PositiveInfinity;

            for( int j = 0; j < numPoints; ++j )
            {
                if( !matched[j] )
                {
                    float distance = Vector2.Distance( points1[i].Position, points2[j].Position ); //OPTIMIZEME: replace by square distance compare

                    if( distance < minDistance )
                    {
                        minDistance = distance;
                        index = j;
                    }
                }
            }
			
            matched[index] = true;

            float weight = 1 - ( ( i - startIndex + points1.Count ) % points1.Count ) / points1.Count;  // [0::1] assign decreasing confidence weights to point matchings.
            sum += weight * minDistance;
		

            i = ( i + 1 ) % points1.Count;

        } while( i != startIndex );

        return sum;
    }

    private static bool[] matched = new bool[NormalizedPointCount];

    static void ResetMatched( int count )
    {
        if( matched.Length < count )
            matched = new bool[count];

        for( int i = 0; i < count; ++i )
            matched[i] = false;
    }

    protected override GestureRecognitionState OnRecognize( PointCloudGesture gesture, FingerGestures.IFingerList touches )
    {
        if( touches.Count != RequiredFingerCount )
        {
            // fingers lifted off?
            if( touches.Count < RequiredFingerCount )
            {
                // recognize
                if( RecognizePointCloud( gesture ) )
                    return GestureRecognitionState.Recognized;

                return GestureRecognitionState.Failed;
            }

            return GestureRecognitionState.Failed;
        }

        // update current gesture position
        gesture.Position = touches.GetAveragePosition();

        Vector2 lastSamplePos = gesture.RawPoints[gesture.RawPoints.Count - 1].Position;

        // check if we should take a new sample
        float dist = Vector2.SqrMagnitude( gesture.Position - lastSamplePos );

        if( dist > MinDistanceBetweenSamples * MinDistanceBetweenSamples )
        {
            int strokeId = 0;   //TODO increment this after each finger up>down
            gesture.RawPoints.Add( new Point( strokeId, gesture.Position ) );
        }

        return GestureRecognitionState.InProgress;
    }

    public override string GetDefaultEventMessageName()
    {
        return "OnCustomGesture";
    }

    #region Gizmos & Debugging

    PointCloudGesture debugLastGesture = null;
    NormalizedTemplate debugLastMatchedTemplate = null;
    const float gizmoSphereRadius = 0.01f;

    public void OnDrawGizmosSelected()
    {
        if( debugLastMatchedTemplate != null )
        {
            Gizmos.color = Color.yellow;
            DrawNormalizedPointCloud( debugLastMatchedTemplate.Points, 15 );
        }

        if( debugLastGesture != null )
        {
            Gizmos.color = Color.green;
            DrawNormalizedPointCloud( debugLastGesture.NormalizedPoints, 15 );
        }
    }

    void DrawNormalizedPointCloud( List<Point> points, float scale )
    {
        if( points.Count > 0 )
        {
            Gizmos.DrawWireSphere( scale * points[0].Position, gizmoSphereRadius );

            for( int i = 1; i < points.Count; ++i )
            {
                Gizmos.DrawLine( scale * points[i - 1].Position, scale * points[i].Position );
                Gizmos.DrawWireSphere( scale * points[i].Position, gizmoSphereRadius );
            }
        }
    }

    #endregion
}
