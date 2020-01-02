using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace of2.Curves
{
    public class BSpline
    {
        public ReadOnlyCollection<Vector3> Positions => m_Positions.AsReadOnly();
        public ReadOnlyCollection<float> Times => m_Times.AsReadOnly();

        [SerializeField] protected List<Vector3> m_Positions;
        [SerializeField] protected List<float> m_Times;

        [SerializeField] protected List<float> m_Lengths;
        [SerializeField] protected float m_TotalLength;
        [SerializeField] protected int m_Count;

        private bool m_LengthDirty = true;

        public BSpline()
        {
            m_Positions = new List<Vector3>();
            m_Times = new List<float>();
        }

        public bool Initialize(in Vector3[] positions, int count)
        {
            if (count < 2 || positions == null || positions.Length != count)
                return false;

            // set up arrays
            m_Count = count + 4;

            // copy position data
            // triplicate start and end points so that curve passes through them
            m_Positions.Add(positions[0]);
            m_Positions.Add(positions[0]);

            for (var i = 0; i < count; ++i)
            {
                m_Positions.Add(positions[i]);
            }

            m_Positions.Add(positions[count - 1]);
            m_Positions.Add(positions[count - 1]);

            // now set up times
            // we subdivide interval to get arrival times at each knot location
            float dt = 1f / (float) (count + 1);
            m_Times.Add(0);
            for (var i = 0; i < count; ++i)
            {
                m_Times.Add(m_Times[i] + dt);
            }

            m_Times.Add(1.0f);

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
            Debug.Assert(m_Count >= 6);

            if (m_Count < 6)
                return Vector3.zero;

            // handle boundary conditions
            if (t <= m_Times[0])
                return m_Positions[0];
            else if (t >= m_Times[m_Count - 3])
                return m_Positions[m_Count - 3];

            // find segment and parameter
            int i;
            for (i = 0; i < m_Count - 1; ++i)
            {
                if (t < m_Times[i + 1])
                {
                    break;
                }
            }

            float t0 = m_Times[i];
            float t1 = m_Times[i + 1];
            float u = (t - t0) / (t1 - t0);

            // match segment index to standard B-spline terminology
            i += 3;

            // evaluate
            Vector3 A = m_Positions[i]
                          - 3.0f * m_Positions[i - 1]
                          + 3.0f * m_Positions[i - 2]
                          - m_Positions[i - 3];
            Vector3 B = 3.0f * m_Positions[i - 1]
                          - 6.0f * m_Positions[i - 2]
                          + 3.0f * m_Positions[i - 3];
            Vector3 C = 3.0f * m_Positions[i - 1] - 3.0f * m_Positions[i - 3];
            Vector3 D = m_Positions[i - 1]
                          + 4.0f * m_Positions[i - 2]
                          + m_Positions[i - 3];

            return (D + u * (C + u * (B + u * A))) / 6.0f;
        }

        /// <summary>
        /// Evaluate derivative at parameter t
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public Vector3 Velocity(float t)
        {
            // make sure data is valid
            Debug.Assert(m_Count >= 2);
            if (m_Count < 2)
                return UnityEngine.Vector3.zero;

            // handle boundary conditions
            if (t <= m_Times[0])
                t = m_Times[0];
            else if (t >= m_Times[m_Count - 1])
                t = m_Times[m_Count - 2];

            // find segment and parameter
            int i;
            for (i = 0; i < m_Count - 1; ++i)
            {
                if (t < m_Times[i + 1])
                {
                    break;
                }
            }

            float t0 = m_Times[i];
            float t1 = m_Times[i + 1];
            float u = (t - t0) / (t1 - t0);

            // match segment index to standard B-spline terminology
            i += 3;

            // evaluate
            Vector3 A = m_Positions[i]
                          - 3.0f * m_Positions[i - 1]
                          + 3.0f * m_Positions[i - 2]
                          - m_Positions[i - 3];
            Vector3 B = 3.0f * m_Positions[i - 1]
                          - 6.0f * m_Positions[i - 2]
                          + 3.0f * m_Positions[i - 3];
            Vector3 C = 3.0f * m_Positions[i - 1] - 3.0f * m_Positions[i - 3];

            Vector3 result = (C + u * (2.0f * B + 3.0f * u * A)) / 6.0f;

            return result;
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
                t = 0.0f;
            else if ( t > m_Times[m_Count-1] )
                t = m_Times[m_Count-1];

            // find segment and parameter
            int i;
            for ( i = 0; i < m_Count-1; ++i )
            {
                if ( t <= m_Times[i+1] )
                {
                    break;
                }
            }
            float t0 = m_Times[i];
            float t1 = m_Times[i+1];
            float u = (t - t0)/(t1 - t0);
                
            
            // match segment index to standard B-spline terminology
            i += 3;

            // evaluate
            Vector3 A = m_Positions[i]
                          - 3.0f*m_Positions[i-1]
                          + 3.0f*m_Positions[i-2]
                          - m_Positions[i-3];
            Vector3 B = 3.0f*m_Positions[i-1]
                          - 6.0f*m_Positions[i-2]
                          + 3.0f*m_Positions[i-3];
    
            return 1.0f/3.0f*B + u*A;   
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

            if (m_LengthDirty)
                RegenerateLengths();
            
            // make first guess
            float p = t1 + s*(m_Times[m_Count-1]-m_Times[0])/m_TotalLength;

            // iterate and look for zeros
            for ( int i = 0; i < 32; ++i )
            {
                // compute function value and test against zero
                float func = ArcLength(t1, p) - s;
                if ( System.Math.Abs(func) < 1.0e-03f )
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
                for ( int i = seg1+1; i < seg2; ++i )
                    result += m_Lengths[i];
                result += SegmentArcLength( seg2, 0.0f, u2 );
            }

            return result;            
        }       
        
        /// <summary>
        /// Find length of curve segment between parameters u1 and u2
        /// </summary>
        /// <param name="i"></param>
        /// <param name="u1"></param>
        /// <param name="u2"></param>
        /// <returns></returns>
        readonly float[] x =
        {
            0.0000000000f, 0.5384693101f, -0.5384693101f, 0.9061798459f, -0.9061798459f 
        };
        readonly float[] c =
        {
            0.5688888889f, 0.4786286705f, 0.4786286705f, 0.2369268850f, 0.2369268850f
        };
        
        public float SegmentArcLength(int i, float u1, float u2)
        {
            Debug.Assert(i >= 0 && i < m_Count-1);
            
            if ( u2 <= u1 )
                return 0.0f;
            
            if ( u1 < 0.0f )
                u1 = 0.0f;
            
            if ( u2 > 1.0f )
                u2 = 1.0f;
            
            // use Gaussian quadrature
            float sum = 0.0f;
            
            // set up for computation of Hermite derivative
            Vector3 A = m_Positions[i]
                        - 3.0f*m_Positions[i-1]
                        + 3.0f*m_Positions[i-2]
                        - m_Positions[i-3];
            Vector3 B = 3.0f*m_Positions[i-1]
                        - 6.0f*m_Positions[i-2]
                        + 3.0f*m_Positions[i-3];
            Vector3 C = 3.0f*m_Positions[i-1] - 3.0f*m_Positions[i-3];
                
            for ( int j = 0; j < 5; ++j )
            {
                float u = 0.5f*((u2 - u1)*x[j] + u2 + u1);
                Vector3 derivative = (C + u*(2.0f*B + 3.0f*u*A))/6.0f;
                sum += c[j]*derivative.magnitude;
            }
            sum *= 0.5f*(u2-u1);
            
            return sum;            
        }

        private void RegenerateLengths()
        {
            if (m_Lengths == null)
                m_Lengths = new List<float>();
            else
                m_Lengths.Clear();
            
            m_TotalLength = 0.0f;
            for ( int i = 0; i < m_Count-3; ++i )
            {
                var l = SegmentArcLength(i, 0.0f, 1.0f);
                m_Lengths.Add(l);
                m_TotalLength += l;
            }

            m_LengthDirty = false;            
        }

        private void InitInternal(int count)
        {
            m_Positions.Clear();
            m_Times.Clear();
            m_Count = count;            
        }
        
    }
}