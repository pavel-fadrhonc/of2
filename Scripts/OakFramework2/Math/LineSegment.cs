using UnityEngine;

namespace of2.Math
{
    public struct LineSegment
    {
        public Vector3 Origin { get; private set; }
        public Vector3 Endpoint { get; private set; }
        
        public Vector3 Direction { get; private set; }
        public Vector3 Center { get { return Origin + 0.5f * Direction; } }
        public float Length { get { return Direction.magnitude; } }
        public float LengthSquared { get { return Direction.sqrMagnitude; } }

        public LineSegment(Vector3 origin, Vector3 endpoint)
        {
            this.Origin = origin;
            this.Endpoint = endpoint;

            Direction = endpoint - origin;
        }
        
    #region comparison operations
        
        public override bool Equals(object obj) 
        {
            return obj is LineSegment && this == (LineSegment)obj;
        }
        public override int GetHashCode() 
        {
            return Origin.GetHashCode() ^ Endpoint.GetHashCode();
        }
        public static bool operator ==(LineSegment ls1, LineSegment ls2) 
        {
            return ls1.Origin == ls2.Origin && ls1.Endpoint == ls2.Endpoint;
        }
        public static bool operator !=(LineSegment ls1, LineSegment ls2) 
        {
            return !(ls1 == ls2);
        }
        
    #endregion

        public override string ToString()
        {
            return $"<{Origin}, {Endpoint}>";
        }

        /// <summary>
        /// Returns the distance between two line segments.
        /// </summary>
        /// <param name="segment0">first line segment</param>
        /// <param name="segment1">second line segment</param>
        /// <param name="s_c">output parameter between [0,1] that denotes where in the first line the closest point is.</param>
        /// <param name="t_c">output parameter between [0,1] that denotes where in the second line the closest point is.</param>
        /// <returns>The distance squared between two line segments</returns>        
        public static float Distance(in LineSegment segment0, in LineSegment segment1, out float s_c, out float t_c)
        {
            return Mathf.Sqrt(DistanceSquared(segment0, segment1, out s_c, out t_c));
        }
        
        /// <summary>
        /// Returns the distance squared between two line segments.
        /// </summary>
        /// <param name="segment0">first line segment</param>
        /// <param name="segment1">second line segment</param>
        /// <param name="s_c">output parameter between [0,1] that denotes where in the first line the closest point is.</param>
        /// <param name="t_c">output parameter between [0,1] that denotes where in the second line the closest point is.</param>
        /// <returns>The distance squared between two line segments</returns>
        public static float DistanceSquared( in LineSegment segment0, in LineSegment segment1, out float s_c, out float t_c )
        {
            // compute intermediate parameters
            Vector3 w0 = segment0.Origin - segment1.Endpoint;
            float a = Vector3.Dot(segment0.Direction, segment0.Direction );
            float b = Vector3.Dot(segment0.Direction, segment1.Direction );
            float c = Vector3.Dot(segment1.Direction, segment1.Direction );
            float d = Vector3.Dot(segment0.Direction, w0 );
            float e = Vector3.Dot(segment1.Direction, w0 );
        
            float denom = a*c - b*b;
            // parameters to compute s_c, t_c
            float sn, sd, tn, td;
        
            // if denom is zero, try finding closest point on segment1 to origin0
            if ( Mathf.Approximately(denom, 0) )
            {
                // clamp s_c to 0
                sd = td = c;
                sn = 0.0f;
                tn = e;
            }
            else
            {
                // clamp s_c within [0,1]
                sd = td = denom;
                sn = b*e - c*d;
                tn = a*e - b*d;
          
                // clamp s_c to 0
                if (sn < 0.0f)
                {
                    sn = 0.0f;
                    tn = e;
                    td = c;
                }
                // clamp s_c to 1
                else if (sn > sd)
                {
                    sn = sd;
                    tn = e + b;
                    td = c;
                }
            }
        
            // clamp t_c within [0,1]
            // clamp t_c to 0
            if (tn < 0.0f)
            {
                t_c = 0.0f;
                // clamp s_c to 0
                if ( -d < 0.0f )
                {
                    s_c = 0.0f;
                }
                // clamp s_c to 1
                else if ( -d > a )
                {
                    s_c = 1.0f;
                }
                else
                {
                    s_c = -d/a;
                }
            }
            // clamp t_c to 1
            else if (tn > td)
            {
                t_c = 1.0f;
                // clamp s_c to 0
                if ( (-d+b) < 0.0f )
                {
                    s_c = 0.0f;
                }
                // clamp s_c to 1
                else if ( (-d+b) > a )
                {
                    s_c = 1.0f;
                }
                else
                {
                    s_c = (-d+b)/a;
                }
            }
            else
            {
                t_c = tn/td;
                s_c = sn/sd;
            }
        
            // compute difference vector and distance squared
            Vector3 wc = w0 + s_c*segment0.Direction - t_c*segment1.Direction;
            return Vector3.Dot(wc, wc);
        }

        /// <summary>
        /// Returns the distance between line segment and ray.
        /// </summary>
        /// <param name="segment0">first line segment</param>
        /// <param name="segment1">second line segment</param>
        /// <param name="s_c">output parameter between [0,1] that denotes where in the first line the closest point is.</param>
        /// <param name="t_c">output parameter that denotes where on the ray the closest point is. 0 is origin, 1 is origin + direction</param>
        /// <returns>The distance squared between line segment and ray</returns>                
        public static float Distance(in LineSegment segment, in Ray ray, out float s_c, out float t_c)
        {
            return Mathf.Sqrt(DistanceSquared(segment, ray, out s_c, out t_c));
        }
        
        /// <summary>
        /// Returns the distance squared between line segment and ray.
        /// </summary>
        /// <param name="segment0">first line segment</param>
        /// <param name="segment1">second line segment</param>
        /// <param name="s_c">output parameter between [0,1] that denotes where in the first line the closest point is.</param>
        /// <param name="t_c">output parameter that denotes where on the ray the closest point is. 0 is origin, 1 is origin + direction</param>
        /// <returns>The distance squared between line segment and ray</returns>        
        public static float DistanceSquared( in LineSegment segment, in Ray ray, out float s_c, out float t_c )
        {
            // compute intermediate parameters
            Vector3 w0 = segment.Origin - ray.origin;
            float a = Vector3.Dot(segment.Direction, segment.Direction);
            float b = Vector3.Dot(segment.Direction, ray.direction );
            float c = Vector3.Dot(ray.direction, ray.direction);
            float d = Vector3.Dot(segment.Direction, w0 );
            float e = Vector3.Dot(ray.direction, w0 );

            float denom = a*c - b*b;
            // parameters to compute s_c, t_c
            float sn, sd, tn, td;

            // if denom is zero, try finding closest point on segment1 to origin0
            if ( Mathf.Approximately(denom, 0) )
            {
                // clamp s_c to 0
                sd = td = c;
                sn = 0.0f;
                tn = e;
            }
            else
            {
                // clamp s_c within [0,1]
                sd = td = denom;
                sn = b*e - c*d;
                tn = a*e - b*d;
  
                // clamp s_c to 0
                if (sn < 0.0f)
                {
                    sn = 0.0f;
                    tn = e;
                    td = c;
                }
                // clamp s_c to 1
                else if (sn > sd)
                {
                    sn = sd;
                    tn = e + b;
                    td = c;
                }
            }

            // clamp t_c within [0,+inf]
            // clamp t_c to 0
            if (tn < 0.0f)
            {
                t_c = 0.0f;
                // clamp s_c to 0
                if ( -d < 0.0f )
                {
                    s_c = 0.0f;
                }
                // clamp s_c to 1
                else if ( -d > a )
                {
                    s_c = 1.0f;
                }
                else
                {
                    s_c = -d/a;
                }
            }
            else
            {
                t_c = tn/td;
                s_c = sn/sd;
            }

            // compute difference vector and distance squared
            Vector3 wc = w0 + s_c*segment.Direction - t_c*ray.direction;
            return Vector3.Dot(wc,wc);

        }

        /// <summary>
        /// Returns the distance between line segment and line.
        /// </summary>
        /// <param name="segment">line segment</param>
        /// <param name="line">line</param>
        /// <param name="s_c">output parameter between [0,1] that denotes where in the first line the closest point is.</param>
        /// <param name="t_c">output parameter that denotes where on the line the closest point is. 0 is origin, 1 is origin + direction</param>
        /// <returns>The distance squared between line segment and line</returns>
        public static float Distance(in LineSegment segment, in Line line, out float s_c, out float t_c)
        {
            return Mathf.Sqrt(DistanceSquared(segment, line, out s_c, out t_c));
        }
        
        /// <summary>
        /// Returns the distance squared between line segment and line.
        /// </summary>
        /// <param name="segment">line segment</param>
        /// <param name="line">line</param>
        /// <param name="s_c">output parameter between [0,1] that denotes where in the first line the closest point is.</param>
        /// <param name="t_c">output parameter that denotes where on the line the closest point is. 0 is origin, 1 is origin + direction</param>
        /// <returns>The distance squared between line segment and line</returns>
        public static float DistanceSquared( in LineSegment segment, in Line line, out float s_c, out float t_c )
        {
            // compute intermediate parameters
            Vector3 w0 = segment.Origin - line.Origin;
            float a = Vector3.Dot(segment.Direction, segment.Direction );
            float b = Vector3.Dot(segment.Direction, line.Direction );
            float c = Vector3.Dot(line.Direction, line.Direction );
            float d = Vector3.Dot(segment.Direction, w0 );
            float e = Vector3.Dot(line.Direction, w0 );

            float denom = a*c - b*b;

            // if denom is zero, try finding closest point on segment1 to origin0
            if ( Mathf.Approximately(denom, 0) )
            {
                s_c = 0.0f;
                t_c = e/c;
                // compute difference vector and distance squared
                Vector3 wc = w0 - t_c*line.Direction;
                return Vector3.Dot(wc, wc);
            }
            else
            {
                // parameters to compute s_c, t_c
                float sn;

                // clamp s_c within [0,1]
                sn = b*e - c*d;
  
                // clamp s_c to 0
                if (sn < 0.0f)
                {
                    s_c = 0.0f;
                    t_c = e/c;
                }
                // clamp s_c to 1
                else if (sn > denom)
                {
                    s_c = 1.0f;
                    t_c = (e+b)/c;
                }
                else
                {
                    s_c = sn/denom;
                    t_c = (a*e - b*d)/denom;
                }

                // compute difference vector and distance squared
                Vector3 wc = w0 + s_c*segment.Direction - t_c*line.Direction;
                return Vector3.Dot(wc,wc);
            }

        }

        /// <summary>
        /// Returns the distance between line segment and line.
        /// </summary>
        /// <param name="segment">line segment</param>
        /// <param name="point">point</param>
        /// <param name="t_c">output parameter between [0,1] that denotes where in the first line the closest point is.</param>
        /// <returns>The distance squared between line segment and line</returns>
        public static float Distance(in LineSegment segment, in Vector3 point, out float t_c)
        {
            return Mathf.Sqrt(DistanceSquared(segment, point, out t_c));
        }
        
        /// <summary>
        /// Returns the distance squared between line segment and line.
        /// </summary>
        /// <param name="segment">line segment</param>
        /// <param name="point">point</param>
        /// <param name="t_c">output parameter between [0,1] that denotes where in the first line the closest point is.</param>
        /// <returns>The distance squared between line segment and line</returns>
        public static float DistanceSquared( in LineSegment segment, in Vector3 point, out float t_c ) 
        {
            Vector3 w = point - segment.Origin;
            float proj = Vector3.Dot(w, segment.Direction);
            // endpoint 0 is closest point
            if ( proj <= 0 )
            {
                t_c = 0.0f;
                return Vector3.Dot(w, w);
            }
            else
            {
                float vsq = Vector3.Dot(segment.Direction, segment.Direction);
                // endpoint 1 is closest point
                if ( proj >= vsq )
                {
                    t_c = 1.0f;
                    return Vector3.Dot(w, w) - 2.0f*proj + vsq;
                }
                // otherwise somewhere else in segment
                else
                {
                    t_c = proj/vsq;
                    return Vector3.Dot(w, w) - t_c*proj;
                }
            }
        }         
        
        /// <summary>
        /// Returns the closest points between two line segments.
        /// </summary>
        /// <param name="segment0">first line segment</param>
        /// <param name="segment1">second line segment</param>
        /// <param name="point0">Output point, closest on first line segment/</param>
        /// <param name="point1">Output point, closest on second line segment/</param>
        public static void ClosestPoints(in LineSegment segment0, in LineSegment segment1, out Vector3 point0, out Vector3 point1 )
        {
            // compute intermediate parameters
            Vector3 w0 = segment0.Origin - segment1.Origin;
            float a = Vector3.Dot(segment0.Direction, segment0.Direction );
            float b = Vector3.Dot(segment0.Direction, segment1.Direction );
            float c = Vector3.Dot(segment1.Direction, segment1.Direction );
            float d = Vector3.Dot(segment0.Direction, w0 );
            float e = Vector3.Dot(segment1.Direction, w0 );
        
            float denom = a*c - b*b;
            // parameters to compute s_c, t_c
            float s_c, t_c;
            float sn, sd, tn, td;
        
            // if denom is zero, try finding closest point on segment1 to origin0
            if ( Mathf.Approximately(denom, 0) )
            {
                // clamp s_c to 0
                sd = td = c;
                sn = 0.0f;
                tn = e;
            }
            else
            {
                // clamp s_c within [0,1]
                sd = td = denom;
                sn = b*e - c*d;
                tn = a*e - b*d;
          
                // clamp s_c to 0
                if (sn < 0.0f)
                {
                    sn = 0.0f;
                    tn = e;
                    td = c;
                }
                // clamp s_c to 1
                else if (sn > sd)
                {
                    sn = sd;
                    tn = e + b;
                    td = c;
                }
            }
        
            // clamp t_c within [0,1]
            // clamp t_c to 0
            if (tn < 0.0f)
            {
                t_c = 0.0f;
                // clamp s_c to 0
                if ( -d < 0.0f )
                {
                    s_c = 0.0f;
                }
                // clamp s_c to 1
                else if ( -d > a )
                {
                    s_c = 1.0f;
                }
                else
                {
                    s_c = -d/a;
                }
            }
            // clamp t_c to 1
            else if (tn > td)
            {
                t_c = 1.0f;
                // clamp s_c to 0
                if ( (-d+b) < 0.0f )
                {
                    s_c = 0.0f;
                }
                // clamp s_c to 1
                else if ( (-d+b) > a )
                {
                    s_c = 1.0f;
                }
                else
                {
                    s_c = (-d+b)/a;
                }
            }
            else
            {
                t_c = tn/td;
                s_c = sn/sd;
            }
        
            // compute closest points
            point0 = segment0.Origin + s_c*segment0.Direction;
            point1 = segment1.Origin + t_c*segment1.Direction;
        }

        /// <summary>
        /// Returns the closest points between segments and ray.
        /// </summary>
        /// <param name="segment0">line segment</param>
        /// <param name="segment1">ray</param>
        /// <param name="point0">Output point, closest on the line segment/</param>
        /// <param name="point1">Output point, closest on the ray</param>        
        public static void ClosestPoints(in LineSegment segment, in Ray ray, out Vector3 point0, out Vector3 point1 )
        {
            // compute intermediate parameters
            Vector3 w0 =  segment.Origin - ray.origin;
            float a = Vector3.Dot(segment.Direction, segment.Direction);
            float b = Vector3.Dot(segment.Direction, ray.direction);
            float c = Vector3.Dot(ray.direction, ray.direction);
            float d = Vector3.Dot(segment.Direction, w0 );
            float e = Vector3.Dot(ray.direction,  w0 );

            float denom = a*c - b*b;
            // parameters to compute s_c, t_c
            float s_c, t_c;
            float sn, sd, tn, td;

            // if denom is zero, try finding closest point on segment1 to origin0
            if ( Mathf.Approximately(denom, 0) )
            {
                // clamp s_c to 0
                sd = td = c;
                sn = 0.0f;
                tn = e;
            }
            else
            {
                // clamp s_c within [0,1]
                sd = td = denom;
                sn = b*e - c*d;
                tn = a*e - b*d;
  
                // clamp s_c to 0
                if (sn < 0.0f)
                {
                    sn = 0.0f;
                    tn = e;
                    td = c;
                }
                // clamp s_c to 1
                else if (sn > sd)
                {
                    sn = sd;
                    tn = e + b;
                    td = c;
                }
            }

            // clamp t_c within [0,+inf]
            // clamp t_c to 0
            if (tn < 0.0f)
            {
                t_c = 0.0f;
                // clamp s_c to 0
                if ( -d < 0.0f )
                {
                    s_c = 0.0f;
                }
                // clamp s_c to 1
                else if ( -d > a )
                {
                    s_c = 1.0f;
                }
                else
                {
                    s_c = -d/a;
                }
            }
            else
            {
                t_c = tn/td;
                s_c = sn/sd;
            }

            // compute closest points
            point0 = segment.Origin + s_c*segment.Direction;
            point1 = ray.origin + t_c*ray.direction;
        }

        /// <summary>
        /// Returns the closest points between segments and line.
        /// </summary>
        /// <param name="segment0">line segment</param>
        /// <param name="segment1">ray</param>
        /// <param name="point0">Output point, closest on the line segment/</param>
        /// <param name="point1">Output point, closest on the line</param>        
        public static void ClosestPoints( in LineSegment segment, in Line line, out Vector3 point0, out Vector3 point1 )
        {
            // compute intermediate parameters
            Vector3 w0 = segment.Origin - line.Origin;
            float a = Vector3.Dot(segment.Direction, segment.Direction) ;
            float b = Vector3.Dot(segment.Direction, line.Direction );
            float c = Vector3.Dot(line.Direction, line.Direction );
            float d = Vector3.Dot(segment.Direction, w0 );
            float e = Vector3.Dot(line.Direction, w0 );

            float denom = a*c - b*b;

            // if denom is zero, try finding closest point on line to segment origin
            if ( Mathf.Approximately(denom, 0) )
            {
                // compute closest points
                point0 = segment.Origin;
                point1 = line.Origin + (e/c)*line.Direction;
            }
            else
            {
                // parameters to compute s_c, t_c
                float s_c, t_c;
                float sn;

                // clamp s_c within [0,1]
                sn = b*e - c*d;
  
                // clamp s_c to 0
                if (sn < 0.0f)
                {
                    s_c = 0.0f;
                    t_c = e/c;
                }
                // clamp s_c to 1
                else if (sn > denom)
                {
                    s_c = 1.0f;
                    t_c = (e+b)/c;
                }
                else
                {
                    s_c = sn/denom;
                    t_c = (a*e - b*d)/denom;
                }

                // compute closest points
                point0 = segment.Origin + s_c*segment.Direction;
                point1 = line.Origin + t_c*line.Direction;
            }

        }

        /// <summary>
        /// Returns the closest point to the point on the line segment.
        /// </summary>
        /// <param name="segment0">line segment</param>
        /// <param name="segment1">point</param>
        /// <returns>The closest point to the point on the line segment</returns> 
        public static Vector3 ClosestPoint( in LineSegment segment, Vector3 point )
        {
            Vector3 w = point - segment.Origin;
            float proj = Vector3.Dot(w,segment.Direction);
            // endpoint 0 is closest point
            if ( proj <= 0.0f )
                return segment.Origin;
            else
            {
                float vsq = Vector3.Dot(segment.Direction, segment.Direction);
                // endpoint 1 is closest point
                if ( proj >= vsq )
                    return segment.Origin + segment.Direction;
                // else somewhere else in segment
                else
                    return segment.Origin + (proj/vsq)*segment.Direction;
            }
        }
        
    }
}