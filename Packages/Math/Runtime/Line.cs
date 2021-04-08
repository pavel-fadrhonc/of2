using UnityEngine;

namespace of2.Math
{
    public struct Line
    {
        public Vector3 Origin { get; private set; }
        public Vector3 Direction { get; private set; }

        public Line(Vector3 origin, Vector3 direction)
        {
            Origin = origin;
            Direction = direction;
        }
        
    #region comparison operations
        
        public override bool Equals(object obj) 
        {
            return obj is Line && this == (Line)obj;
        }
        public override int GetHashCode() 
        {
            return Origin.GetHashCode() ^ Direction.GetHashCode();
        }
        public static bool operator ==(Line l1, Line l2) 
        {
            return l1.Origin == l2.Origin && l1.Direction == l2.Direction;
        }
        public static bool operator !=(Line ls1, Line ls2) 
        {
            return !(ls1 == ls2);
        }
        
    #endregion        
    
        public override string ToString()
        {
            return $"<{Origin}, {Direction})";
        }

        /// <summary>
        /// Returns the distance between lines.
        /// </summary>
        /// <param name="line0">First line</param>
        /// <param name="line1">Second line</param>
        /// <param name="s_c">Output parameter that denotes where on the first line the closest point is. 0 is Origin, 1 is Origin + Direction.</param>
        /// <param name="t_c">Output parameter that denotes where on the second line the closest point is. 0 is Origin, 1 is Origin + Direction.</param>
        /// <returns>The distance squared between lines.</returns>        
        public static float Distance(in Line line0, in Line line1, out float s_c, out float t_c)
        {
            return Mathf.Sqrt(DistanceSquared(line0, line1, out s_c, out t_c));
        }
        
        /// <summary>
        /// Returns the distance squared between lines.
        /// </summary>
        /// <param name="line0">First line</param>
        /// <param name="line1">Second line</param>
        /// <param name="s_c">Output parameter that denotes where on the first line the closest point is. 0 is Origin, 1 is Origin + Direction.</param>
        /// <param name="t_c">Output parameter that denotes where on the second line the closest point is. 0 is Origin, 1 is Origin + Direction.</param>
        /// <returns>The distance squared between lines.</returns>
        public static float DistanceSquared( in Line line0, in Line line1, out float s_c, out float t_c )
        {
            Vector3 w0 = line0.Origin - line1.Origin;
            float a = Vector3.Dot(line0.Direction, line0.Direction );
            float b = Vector3.Dot(line0.Direction, line1.Direction );
            float c = Vector3.Dot(line1.Direction, line1.Direction );
            float d = Vector3.Dot(line0.Direction, w0 );
            float e = Vector3.Dot(line1.Direction, w0 );
            float denom = a*c - b*b;
            if ( Mathf.Approximately(denom, 0) )
            {
                s_c = 0.0f;
                t_c = e/c;
                Vector3 wc = w0 - t_c*line1.Direction;
                return Vector3.Dot(wc,wc);
            }
            else
            {
                s_c = ((b*e - c*d)/denom);
                t_c = ((a*e - b*d)/denom);
                Vector3 wc = w0 + s_c*line0.Direction
                               - t_c*line1.Direction;
                return Vector3.Dot(wc, wc);
            }

        }

        /// <summary>
        /// Returns the distance between line and a point.
        /// </summary>
        /// <param name="line">line</param>
        /// <param name="point">point</param>
        /// <param name="t_c">Output parameter that denotes where on the line the closest point is. 0 is Origin, 1 is Origin + Direction.</param>
        /// <returns></returns>        
        public static float Distance(in Line line, Vector3 point, out float t_c)
        {
            return Mathf.Sqrt(DistanceSquared(line, point, out t_c));
        }
        
        /// <summary>
        /// Returns the distance squared between line and a point.
        /// </summary>
        /// <param name="line">line</param>
        /// <param name="point">point</param>
        /// <param name="t_c">Output parameter that denotes where on the line the closest point is. 0 is Origin, 1 is Origin + Direction.</param>
        /// <returns></returns>
        public static float DistanceSquared( in Line line, Vector3 point, out float t_c )
        {
            Vector3 w = point - line.Origin;
            float vsq = Vector3.Dot(line.Direction, line.Direction);
            float proj = Vector3.Dot(w, line.Direction);
            t_c = proj/vsq; 

            return Vector3.Dot( w,w) - t_c*proj;
        }   
        
        /// <summary>
        /// Returns the closest points between two lines
        /// </summary>
        /// <param name="line0">First line</param>
        /// <param name="line1">Second line</param>
        /// <param name="point0">Output point, closest on first line.</param>
        /// <param name="point1">Output point, closest on second line.</param>
        public static void ClosestPoints( in Line line0, in Line line1, out Vector3 point0, out Vector3 point1 )
        {
            // compute intermediate parameters
            Vector3 w0 = line0.Origin - line1.Origin;
            float a = Vector3.Dot(line0.Direction, line0.Direction );
            float b = Vector3.Dot(line0.Direction, line1.Direction );
            float c = Vector3.Dot(line1.Direction, line1.Direction );
            float d = Vector3.Dot(line0.Direction, w0 );
            float e = Vector3.Dot(line1.Direction, w0 );

            float denom = a*c - b*b;

            if ( Mathf.Approximately(denom, 0) )
            {
                point0 = line0.Origin;
                point1 = line1.Origin + (e/c)*line1.Direction;
            }
            else
            {
                point0 = line0.Origin + ((b * e - c * d) / denom) * line0.Direction;
                point1 = line1.Origin + ((a * e - b * d) / denom) * line1.Direction;
            }
        }
        
        /// <summary>
        /// Returns the closest points to the point on line
        /// </summary>
        /// <param name="line">line</param>
        /// <param name="point">point</param>
        /// <returns>The closest points to the point on line</returns>
        public static Vector3 ClosestPoint(in Line line, in Vector3 point) 
        {
            Vector3 w = point - line.Origin;
            float vsq = Vector3.Dot(line.Direction, line.Direction);
            float proj = Vector3.Dot(w, line.Direction);

            return line.Origin + (proj/vsq)* line.Direction;
        }         
    }
}