using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using of2.Math;
using UnityEngine;

namespace of2.Curves
{
    public class Bezier
    {
        public ReadOnlyCollection<Vector3> Positions => m_Positions.AsReadOnly();

        public ReadOnlyCollection<Vector3> Controls => m_Controls.AsReadOnly();

        public ReadOnlyCollection<float> Times => m_Times.AsReadOnly();        
        
        [SerializeField] protected List<Vector3> m_Positions;
        [SerializeField] protected List<Vector3> m_Controls;
        [SerializeField] protected List<float> m_Times;
        [SerializeField] protected List<float> m_Lengths;
        [SerializeField] protected float m_TotalLength;
        [SerializeField] protected int m_Count;
        
        private bool m_LengthDirty = true;

        public Bezier()
        {
            m_Positions = new List<Vector3>();
            m_Controls = new List<Vector3>();
            m_Times = new List<float>();
            m_Lengths = new List<float>();
        }

        public bool Initialize(in Vector3[] positions, in Vector3[] controls, in float[] times, int count)
        {
            // make sure data is valid
            if (count < 2 || positions == null || times == null || controls == null || positions.Length != count || times.Length != count || controls.Length != 2*(count - 1) )
                return false;

            InitInternal(count);
            
            m_Positions.AddRange(positions);
            m_Controls.AddRange(controls);
            m_Times.AddRange(times);

            m_LengthDirty = true;

            return true;
        }

        public bool Initialize(in Vector3[] positions, in float[] times, int count)
        {
            // make sure data is valid
            if (count < 2 || positions == null || times == null || positions.Length != count || times.Length != count )
                return false;

            InitInternal(count);
            
            m_Positions.AddRange(positions);
            m_Times.AddRange(times);
            
            // create approximating control points
            for (var i = 0; i < 2 * count - 3; i++)
            { // need to first init whole list to 0 because control points are not generated sequentially
                m_Controls.Add(Vector3.zero);
            }
            for (var i = 0; i < count-1; ++i )
            {
                if ( i > 0 )
                    m_Controls[2*i] = m_Positions[i] + (m_Positions[i+1]-m_Positions[i-1])/3.0f;
                if ( i < count-2 )
                    m_Controls[2*i+1] = m_Positions[i+1] - (m_Positions[i+2]-m_Positions[i])/3.0f;
            }
            m_Controls[0] = m_Controls[1] - (m_Positions[1] - m_Positions[0])/3.0f;
            m_Controls[2*count-3] = m_Controls[2*count-4] + (m_Positions[count-1] - m_Positions[count-2])/3.0f;            
            
            m_LengthDirty = true;

            return true;            
        }

        /// <summary>
        /// Evaluate spline
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public Vector3 Evaluate(float t)
        {
            // make sure data is valid
            Debug.Assert( m_Count >= 2 );
            if ( m_Count < 2 )
                return Vector3.zero;

            // handle boundary conditions
            if ( t <= m_Times[0] )
                return m_Positions[0];
            else if ( t >= m_Times[m_Count-1] )
                return m_Positions[m_Count-1];

            // find segment and parameter
            int i;
            for ( i = 0; i < m_Count-1; ++i )
            {
                if ( t < m_Times[i+1] )
                {
                    break;
                }
            }
            float t0 = m_Times[i];
            float t1 = m_Times[i+1];
            float u = (t - t0)/(t1 - t0);

            // evaluate
            Vector3 A = m_Positions[i+1]
                          - 3.0f*m_Controls[2*i+1]
                          + 3.0f*m_Controls[2*i]
                          - m_Positions[i];
            Vector3 B = 3.0f*m_Controls[2*i+1]
                          - 6.0f*m_Controls[2*i]
                          + 3.0f*m_Positions[i];
            Vector3 C = 3.0f*m_Controls[2*i]
                          - 3.0f*m_Positions[i];
    
            return m_Positions[i] + u*(C + u*(B + u*A));            
        }

        /// <summary>
        /// Evaluate derivative at parameter t
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public Vector3 Velocity(float t)
        {
            // make sure data is valid
            Debug.Assert( m_Count >= 2 );
            if ( m_Count < 2 )
                return UnityEngine.Vector3.zero;

            // handle boundary conditions
            if ( t <= m_Times[0] )
                return m_Positions[0];
            else if ( t >= m_Times[m_Count-1] )
                return m_Positions[m_Count-1];

            // find segment and parameter
            int i;
            for ( i = 0; i < m_Count-1; ++i )
            {
                if ( t < m_Times[i+1] )
                {
                    break;
                }
            }
            float t0 = m_Times[i];
            float t1 = m_Times[i+1];
            float u = (t - t0)/(t1 - t0);

            // evaluate
            Vector3 A = m_Positions[i+1]
                          - 3.0f*m_Controls[2*i+1]
                          + 3.0f*m_Controls[2*i]
                          - m_Positions[i];
            Vector3 B = 6.0f*m_Controls[2*i+1]
                          - 12.0f*m_Controls[2*i]
                          + 6.0f*m_Positions[i];
            Vector3 C = 3.0f*m_Controls[2*i]
                          - 3.0f*m_Positions[i];
    
            return C + u*(B + 3.0f*u*A);            
        }

        /// <summary>
        /// Evaluate second derivative at parameter t
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public Vector3 Acceleration(float t)
        {
            // make sure data is valid
            Debug.Assert( m_Count >= 2 );
            if ( m_Count < 2 )
                return UnityEngine.Vector3.zero;

            // handle boundary conditions
            if ( t <= m_Times[0] )
                return m_Positions[0];
            else if ( t >= m_Times[m_Count-1] )
                return m_Positions[m_Count-1];

            // find segment and parameter
            int i;
            for ( i = 0; i < m_Count-1; ++i )
            {
                if ( t < m_Times[i+1] )
                {
                    break;
                }
            }
            float t0 = m_Times[i];
            float t1 = m_Times[i+1];
            float u = (t - t0)/(t1 - t0);

            // evaluate
            Vector3 A = m_Positions[i+1]
                          - 3.0f*m_Controls[2*i+1]
                          + 3.0f*m_Controls[2*i]
                          - m_Positions[i];
            Vector3 B = 6.0f*m_Controls[2*i+1]
                          - 12.0f*m_Controls[2*i]
                          + 6.0f*m_Positions[i];
    
            return B + 6.0f*u*A;            
        }

        /// <summary>
        /// Find parameter s distance in arc length from Q(t1)
        /// Returns max float if can't find it
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public float FindParameterByDistance(float t1, float s)
        {
            // initialize bisection endpoints
            float a = t1;
            float b = m_Times[m_Count-1];

            // ensure that we remain within valid parameter space
            if ( s >= ArcLength(t1, b) )
                return b;
            if ( s <= 0.0f )
                return a;

            // make first guess
            float p = t1 + s*(m_Times[m_Count-1]-m_Times[0])/m_TotalLength;

            // iterate and look for zeros
            for ( int i = 0; i < 32; ++i )
            {
                // compute function value and test against zero
                float func = ArcLength(t1, p) - s;
                if ( Mathf.Abs(func) < 1.0e-03f )
                {
                    return p;
                }

                // update bisection endpoints
                if ( func < 0.0f )
                {
                    a = p;
                }
                else
                {
                    b = p;
                }

                // get speed along curve
                float speed = Velocity(p).magnitude;

                // if result will lie outside [a,b] 
                if ( ((p-a)*speed - func)*((p-b)*speed - func) > -1.0e-3f )
                {
                    // do bisection
                    p = 0.5f*(a+b);
                }    
                else
                {
                    // otherwise Newton-Raphson
                    p -= func/speed;
                }
            }

            // done iterating, return failure case
            return float.MaxValue;
        }

        /// <summary>
        /// Find length of curve between parameters t1 and t2
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public float ArcLength(float t1, float t2)
        {
            if ( t2 <= t1 )
                return 0.0f;

            if ( t1 < m_Times[0] )
                t1 = m_Times[0];

            if ( t2 > m_Times[m_Count-1] )
                t2 = m_Times[m_Count-1];

            // find segment and parameter
            int seg1;
            for ( seg1 = 0; seg1 < m_Count-1; ++seg1 )
            {
                if ( t1 < m_Times[seg1+1] )
                {
                    break;
                }
            }
            float u1 = (t1 - m_Times[seg1])/(m_Times[seg1+1] - m_Times[seg1]);
    
            // find segment and parameter
            int seg2;
            for ( seg2 = 0; seg2 < m_Count-1; ++seg2 )
            {
                if ( t2 <= m_Times[seg2+1] )
                {
                    break;
                }
            }
            float u2 = (t2 - m_Times[seg2])/(m_Times[seg2+1] - m_Times[seg2]);
    
            float result;
            // both parameters lie in one segment
            if ( seg1 == seg2 )
            {
                result = SegmentArcLength( seg1, u1, u2 );
            }
            // parameters cross segments
            else
            {
                result = SegmentArcLength( seg1, u1, 1.0f );
                for ( var i = seg1+1; i < seg2; ++i )
                    result += m_Lengths[i];
                result += SegmentArcLength( seg2, 0.0f, u2 );
            }

            return result;            
        }

        /// <summary>
        /// Adds point to index and generates controls points
        /// </summary>
        public void AddPointToIndex(Vector3 point, int index)
        {
            m_Positions.Insert(index, point);
            
            // TODO: do the control point generating (regenerate all control points? at lest the ones for point before and after the added one...)
            
            // regenerate all control points
            for (var i = 0; i < m_Count-1; ++i )
            {
                if ( i > 0 )
                    m_Controls[2*i] = m_Positions[i] + (m_Positions[i+1]-m_Positions[i-1])/3.0f;
                if ( i < m_Count-2 )
                    m_Controls[2*i+1] = m_Positions[i+1] - (m_Positions[i+2]-m_Positions[i])/3.0f;
            }
            m_Controls[0] = m_Controls[1] - (m_Positions[1] - m_Positions[0])/3.0f;
            m_Controls[2*m_Count-3] = m_Controls[2*m_Count-4] + (m_Positions[m_Count-1] - m_Positions[m_Count-2])/3.0f;
        }

        public void RemovePosition(int index)
        {
            m_Positions.RemoveAt(index);

            // TODO: do the control point generating (regenerate all control points? at lest the ones for point before and after the added one...)            
        }

        public void SetPosition(int index, Vector3 pos)
        {
            if (index < 0 || index > m_Positions.Count - 1)
                return;
            
            m_Positions[index] = pos;
            
            // TODO: do the control point generating (regenerate all control points? at lest the ones for point before and after the added one...)            
        }

        public void SetControlPointPos(int index, Vector3 pos)
        {
            if (index < 0 || index > m_Controls.Count - 1)
                return; 
            
            m_Controls[index] = pos;
            
            if (index == 1)
                m_Controls[0] = m_Controls[1] - (m_Positions[1] - m_Positions[0])/3.0f;
            else if (index == 2*m_Count-4)
                m_Controls[2*m_Count-3] = m_Controls[2*m_Count-4] + (m_Positions[m_Count-1] - m_Positions[2*m_Count-4-2])/3.0f;            
        }

        public bool SetTime(int index, float time)
        {
            if (index > m_Count - 1)
                return false;

            m_Times[index] = time;

            m_LengthDirty = true;

            return true;
        }

        private float SegmentArcLength(int i, float u1, float u2)
        {
            Debug.Assert(i >= 0 && i < m_Count - 1);
            
            if ( u2 <= u1 )
                return 0.0f;

            if ( u1 < 0.0f )
                u1 = 0.0f;

            if ( u2 > 1.0f )
                u2 = 1.0f;

            Vector3 P0 = m_Positions[i];
            Vector3 P1 = m_Controls[2*i];
            Vector3 P2 = m_Controls[2*i+1];
            Vector3 P3 = m_Positions[i+1];

            // get control points for subcurve from 0.0 to u2 (de Casteljau's method)
            float minus_u2 = (1.0f - u2);
            Vector3 L1 = minus_u2*P0 + u2*P1;
            Vector3 H = minus_u2*P1 + u2*P2;
            Vector3 L2 = minus_u2*L1 + u2*H;
            Vector3 L3 = minus_u2*L2 + u2*(minus_u2*H + u2*(minus_u2*P2 + u2*P3));

            // resubdivide to get control points for subcurve between u1 and u2
            float minus_u1 = (1.0f - u1);
            H = minus_u1*L1 + u1*L2;
            Vector3 R3 = L3;
            Vector3 R2 = minus_u1*L2 + u1*L3;
            Vector3 R1 = minus_u1*H + u1*R2;
            Vector3 R0 = minus_u1*(minus_u1*(minus_u1*P0 + u1*L1) + u1*H) + u1*R1;

            // get length through subdivision
            return SubdivideLength( R0, R1, R2, R3 );            
        }
        
        /// <summary>
        /// Get length of Bezier curve using midpoint subdivision
        /// </summary>
        /// <param name="P0"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="P3"></param>
        /// <returns></returns>
        private float SubdivideLength( in Vector3 P0, in Vector3 P1, in Vector3 P2, in Vector3 P3 )
        {
            // check to see if basically straight
            float Lmin = Vector3.Magnitude(P0 - P3);
            float Lmax = Vector3.Magnitude(P0 - P1) + Vector3.Magnitude(P1 - P2) + Vector3.Magnitude(P2 - P3);
            float diff = Lmin - Lmax;

            if ( diff*diff < 1.0e-3f )
                return 0.5f*(Lmin + Lmax);

            // otherwise get control points for subdivision
            Vector3 L1 = (P0 + P1) * 0.5f;
            Vector3 H = (P1 + P2) * 0.5f;
            Vector3 L2 = (L1 + H) * 0.5f;
            Vector3 R2 = (P2 + P3) * 0.5f;
            Vector3 R1 = (H + R2) * 0.5f;
            Vector3 mid = (L2 + R1) * 0.5f;

            // subdivide
            return SubdivideLength( P0, L1, L2, mid ) + SubdivideLength( mid, R1, R2, P3 );
        }
        
        private int CountSubdivideVerts( in Vector3 P0, in Vector3 P1, in Vector3 P2, in Vector3 P3 )
        {
            // check to see if straight
            LineSegment segment = new LineSegment( P0, P3 );
            float t;
            if ( LineSegment.DistanceSquared( segment, P1, out t ) < 1.0e-6f &&
                 LineSegment.DistanceSquared( segment, P2, out t ) < 1.0e-6f )
                return 1;

            // otherwise get control points for subdivision
            Vector3 L1 = (P0 + P1) * 0.5f;
            Vector3 H = (P1 + P2) * 0.5f;
            Vector3 L2 = (L1 + H) * 0.5f;
            Vector3 R2 = (P2 + P3) * 0.5f;
            Vector3 R1 = (H + R2) * 0.5f;
            Vector3 mid = (L2 + R1) * 0.5f;

            // subdivide
            return 1 + CountSubdivideVerts( P0, L1, L2, mid ) + CountSubdivideVerts( mid, R1, R2, P3 );            
        }        

        private void InitInternal(int count)
        {
            m_Positions.Clear();
            m_Controls.Clear();
            m_Times.Clear();
            m_Count = count;            
        }

        private void RegenerateLenghts()
        {

            if (m_Lengths == null)
                m_Lengths = new List<float>();
            else
                m_Lengths.Clear();
            
            m_TotalLength = 0.0f;
            for ( int i = 0; i < m_Count-1; ++i )
            {
                var l = SegmentArcLength(i, 0.0f, 1.0f);
                m_Lengths.Add(l);
                m_TotalLength += l;
            }

            m_LengthDirty = false;
        }
    }
}