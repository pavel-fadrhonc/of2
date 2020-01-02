using System;
using System.Collections.Generic;
using UnityEngine;

namespace of2.Curves
{
    [Serializable]
    public class CatmullRom
    {
        [SerializeField] protected List<Vector3> m_Positions;
        [SerializeField] protected List<float> m_Times;
        [SerializeField] protected List<float> m_Lengths;
        [SerializeField] protected float m_TotalLength;
        [SerializeField] protected int m_Count;     
        
        bool Initialize( in Vector3[] positions, in float[] times, int count)
        {
            // make sure data is valid
            if ( count < 4 || positions == null || times == null || positions.Length != count || times.Length != count)
                return false;

            InitInternal(count);
            
            // copy data
            m_Positions.AddRange(positions);
            m_Times.AddRange(times);

            // set up curve segment lengths
            RegenerateLengths();

            return true;
        }
        
        public Vector3 Evaluate( float t )
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
            int i;  // segment #
            for ( i = 0; i < m_Count-1; ++i )
            {
                if ( t <= m_Times[i+1] )
                {
                    break;
                }
            }
            Debug.Assert( i >= 0 && i < m_Count );

            float t0 = m_Times[i];
            float t1 = m_Times[i+1];
            float u = (t - t0)/(t1 - t0);

            // quadratic Catmull-Rom for Q_0
            if (i == 0)
            {
                Vector3 A = m_Positions[0] - 2.0f*m_Positions[1] + m_Positions[2];
                Vector3 B = 4.0f*m_Positions[1] - 3.0f*m_Positions[0] - m_Positions[2];
        
                return m_Positions[0] + (0.5f*u)*(B + u*A);
            }
            // quadratic Catmull-Rom for Q_n-1
            else if (i >= m_Count-2)
            {
                i = m_Count-2;
                Vector3 A = m_Positions[i-1] - 2.0f*m_Positions[i] + m_Positions[i+1];
                Vector3 B = m_Positions[i+1] - m_Positions[i-1];
        
                return m_Positions[i] + (0.5f*u)*(B + u*A);
            }
            // cubic Catmull-Rom for interior segments
            else
            {
                // evaluate
                Vector3 A = 3.0f*m_Positions[i]
                              - m_Positions[i-1]
                              - 3.0f*m_Positions[i+1]
                              + m_Positions[i+2];
                Vector3 B = 2.0f*m_Positions[i-1]
                              - 5.0f*m_Positions[i]
                              + 4.0f*m_Positions[i+1]
                              - m_Positions[i+2];
                Vector3 C = m_Positions[i+1] - m_Positions[i-1];
    
                return m_Positions[i] + (0.5f*u)*(C + u*(B + u*A));
            }
        }         
        
        /// <summary>
        /// Evaluate derivative at parameter t 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public Vector3 Velocity( float t )
        {
            // make sure data is valid
            Debug.Assert( m_Count >= 2 );
            if ( m_Count < 2 )
                return Vector3.zero;

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

            // evaluate
            // quadratic Catmull-Rom for Q_0
            if (i == 0)
            {
                Vector3 A = m_Positions[0] - 2.0f*m_Positions[1] + m_Positions[2];
                Vector3 B = 4.0f*m_Positions[1] - 3.0f*m_Positions[0] - m_Positions[2];
        
                return 0.5f*B + u*A;
            }
            // quadratic Catmull-Rom for Q_n-1
            else if (i >= m_Count-2)
            {
                i = m_Count-2;
                Vector3 A = m_Positions[i-1] - 2.0f*m_Positions[i] + m_Positions[i+1];
                Vector3 B = m_Positions[i+1] - m_Positions[i-1];
        
                return 0.5f*B + u*A;
            }
            // cubic Catmull-Rom for interior segments
            else
            {
                // evaluate
                Vector3 A = 3.0f*m_Positions[i]
                              - m_Positions[i-1]
                              - 3.0f*m_Positions[i+1]
                              + m_Positions[i+2];
                Vector3 B = 2.0f*m_Positions[i-1]
                              - 5.0f*m_Positions[i]
                              + 4.0f*m_Positions[i+1]
                              - m_Positions[i+2];
                Vector3 C = m_Positions[i+1] - m_Positions[i-1];
    
                return 0.5f*C + u*(B + 1.5f*u*A);
            }
        }        
        
        /// <summary>
        /// Evaluate second derivative at parameter t
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public Vector3 Acceleration( float t )
        {
            // make sure data is valid
            Debug.Assert( m_Count >= 2 );
            if ( m_Count < 2 )
                return Vector3.zero;
    
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
    
            // evaluate
            // quadratic Catmull-Rom for Q_0
            if (i == 0)
            {
                return m_Positions[0] - 2.0f*m_Positions[1] + m_Positions[2];
            }
            // quadratic Catmull-Rom for Q_n-1
            else if (i >= m_Count-2)
            {
                i = m_Count-2;
                return m_Positions[i-1] - 2.0f*m_Positions[i] + m_Positions[i+1];
            }
            // cubic Catmull-Rom for interior segments
            else
            {
                // evaluate
                Vector3 A = 3.0f*m_Positions[i]
                              - m_Positions[i-1]
                              - 3.0f*m_Positions[i+1]
                              + m_Positions[i+2];
                Vector3 B = 2.0f*m_Positions[i-1]
                              - 5.0f*m_Positions[i]
                              + 4.0f*m_Positions[i+1]
                              - m_Positions[i+2];
    
                return B + (3.0f*u)*A;
            }
        }    
        
        /// <summary>
        /// Find parameter s distance in arc length from Q(t1)
        /// Returns max float if can't find it
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public float FindParameterByDistance( float t1, float s )
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
            float p = t1 + s*(m_Times[m_Count-1]-m_Times[0]) / m_TotalLength;

            // iterate and look for zeros
            for ( UInt32 i = 0; i < 32; ++i )
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
        public float ArcLength( float t1, float t2 )
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
                if ( t1 <= m_Times[seg1+1] )
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

        public void AddPosition(Vector3 pos)
        {
            m_Positions.Add(pos);
            
            // determine natural time
            var newMax = 1.0f - (1.0f / m_Count);
            for (int i = 0; i < m_Times.Count; i++)
            {
                m_Times[i] *= newMax;
            }
            m_Times.Add(1.0f);    
            
            RegenerateLengths();
        }

        public void RemovePosition(int index)
        {           
            m_Positions.RemoveAt(index);
            m_Times.RemoveAt(index);
            
            if (index == 0)
                m_Times[0] = 0f;
            else if (index == m_Positions.Count) // because we already removed the element
                m_Times[m_Times.Count - 1] = 1f;                        
            
            RegenerateLengths();
        }

        public void SetPosition(int index, Vector3 pos)
        {
            m_Positions[index] = pos;

            RegenerateLengths();
        }

        public void SetTime(int index, float time)
        {
            m_Times[index] = time;
        }
        
        readonly float[] x =
        {
            0.0000000000f, 0.5384693101f, -0.5384693101f, 0.9061798459f, -0.9061798459f 
        };
        readonly float[] c =
        {
            0.5688888889f, 0.4786286705f, 0.4786286705f, 0.2369268850f, 0.2369268850f
        };

        /// <summary>
        /// Find length of curve segment between parameters u1 and u2
        /// </summary>
        /// <param name="i"></param>
        /// <param name="u1"></param>
        /// <param name="u2"></param>
        /// <returns></returns>
        private float SegmentArcLength(int i, float u1, float u2)
        {
            Debug.Assert(i >= 0 && i < m_Count - 1);

            if (u2 <= u1)
                return 0.0f;

            if (u1 < 0.0f)
                u1 = 0.0f;

            if (u2 > 1.0f)
                u2 = 1.0f;

            // use Gaussian quadrature
            float sum = 0.0f;
            Vector3 A, B, C = Vector3.zero;
            if (i == 0)
            {
                A = m_Positions[0] - 2.0f * m_Positions[1] + m_Positions[2];
                B = 4.0f * m_Positions[1] - 3.0f * m_Positions[0] - m_Positions[2];
            }
            else if (i >= m_Count - 2)
            { // quadratic Catmull-Rom for Q_n-1
                i = m_Count - 2;
                A = m_Positions[i - 1] - 2.0f * m_Positions[i] + m_Positions[i + 1];
                B = m_Positions[i + 1] - m_Positions[i - 1];
            }
            else
            { // cubic Catmull-Rom for interior segments
                A = 3.0f * m_Positions[i]
                    - m_Positions[i - 1]
                    - 3.0f * m_Positions[i + 1]
                    + m_Positions[i + 2];
                B = 2.0f * m_Positions[i - 1]
                    - 5.0f * m_Positions[i]
                    + 4.0f * m_Positions[i + 1]
                    - m_Positions[i + 2];
                C = m_Positions[i + 1] - m_Positions[i - 1];
            }
            for (UInt32 j = 0; j < 5; ++j)
            {
                float u = 0.5f * ((u2 - u1) * x[j] + u2 + u1);
                Vector3 derivative;
                if (i == 0 || i >= m_Count - 2)
                    derivative = 0.5f * B + u * A;
                else
                    derivative = 0.5f * C + u * (B + 1.5f * u * A);
                sum += c[j] * derivative.magnitude;
            }

            sum *= 0.5f * (u2 - u1);

            return sum;
        }        
        
        private void RegenerateLengths()
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
        }        
        
        private void InitInternal(int count)
        {
            m_Positions.Clear();
            m_Times.Clear();
            m_Count = count;            
        }
        
    }
}