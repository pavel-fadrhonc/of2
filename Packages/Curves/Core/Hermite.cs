
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

namespace of2.Curves
{
    [Serializable]
    public class Hermite
    {
        public ReadOnlyCollection<Vector3> Positions => m_Positions.AsReadOnly();

        public ReadOnlyCollection<Vector3> InTangents => m_InTangents.AsReadOnly();

        public ReadOnlyCollection<Vector3> OutTangents => m_OutTangents.AsReadOnly();

        public ReadOnlyCollection<float> Times => m_Times.AsReadOnly();

        private bool m_Closed;
        public bool Closed
        {
            get => m_Closed;
            set
            {
                m_Closed = value;
                RefreshClosed();
            }
        }

        [SerializeField] protected List<Vector3> m_Positions;
        [SerializeField] protected List<Vector3> m_InTangents;
        [SerializeField] protected List<Vector3> m_OutTangents;
        [SerializeField] protected List<float> m_Times;
        [SerializeField] protected List<float> m_Lengths;
        [SerializeField] protected float m_TotalLength;
        [SerializeField] protected int m_Count;

        private bool m_LengthDirty = true;
        
        private List<Vector3> m_TempPositions;
        private List<Vector3> m_TempInTangents;
        private List<Vector3> m_TempOutTangents;
        private List<float> m_TempTimes;        

        public Hermite()
        {
            m_Positions = new List<Vector3>();
            m_InTangents = new List<Vector3>();
            m_OutTangents = new List<Vector3>();
            m_Times = new List<float>();
        }
        
        /// <summary>
        /// Set up sample points
        /// </summary>
        /// <param name="positions"></param>
        /// <param name="inTangents"></param>
        /// <param name="outTangents"></param>
        /// <param name="times"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public bool Initialize(in Vector3[] positions, in Vector3[] inTangents, in Vector3[] outTangents,
            in float[] times, int count)
        {
            // make sure data is valid
            if (count < 2 || positions == null || times == null || inTangents == null || outTangents == null
                || positions.Length != count || times.Length != count || inTangents.Length != count - 1 || outTangents.Length != count - 1)
                return false;
            
            // set up arrays
            InitInternal(count);
            
            // copy data
            m_Positions.AddRange(positions);
            m_InTangents.AddRange(inTangents);
            m_OutTangents.AddRange(outTangents);
            m_Times.AddRange(times);
                        
            // set up curve segment lengths
            m_LengthDirty = true;
            
            return true;
        }

        private float[] m_A = null;
        /// <summary>
        /// Set up sample points for clamped spline
        /// </summary>
        /// <param name="positions"></param>
        /// <param name="times"></param>
        /// <param name="count"></param>
        /// <param name="inTangent"></param>
        /// <param name="outTangent"></param>
        /// <returns></returns>
        public bool InitializeClamped(in Vector3[] positions, in float[] times, int count, Vector3 inTangent,
            Vector3 outTangent)
        {
            // make sure data is valid
            if (count < 3 || m_Positions == null || m_Times == null || positions.Length != count || times.Length != count)
                return false;
            
            // build A
            int n = count;
            int i;
            if (m_A == null)
                m_A = new float[n * n];
            else
            {
                if (n * n != m_A.Length)
                    m_A = new float[n * n];
                
                for (i = 0; i < n - 1; ++i)
                {
                    for (int j = 0; j < n - 1; ++j)
                    {
                        m_A[i * n + j] = 0;
                    }                    
                }
            }
            
            m_A[0] = 1.0f;
            
            for (i = 1; i < n - 1; ++i)
            {
                m_A[i + n * i - n] = 1.0f;
                m_A[i + n * i] = 4.0f;
                m_A[i + n*i + n] = 1.0f;
            }
            m_A[n*n-1] = 1.0f;
            
            // invert it
            // we'd might get better accuracy if we solve the linear system 3 times,
            // once each for x, y, and z, but this is more efficient
            if (!of2.Math.GaussianElim.InvertMatrix(ref m_A, (uint) n ))
            {
                return false;
            }
            
            // set up arrays
            InitInternal(count);
            
            // handle end conditions
            m_Positions.Add(positions[0]);
            m_Times.Add(times[0]);
            m_OutTangents.Add(outTangent);
            
            // set up the middle
            for ( i = 1; i < count-1; ++i )
            {
                // copy position and time
                m_Positions.Add(positions[i]);
                m_Times.Add(times[i]);

                // multiply b by inverse of A to get x
                m_OutTangents.Add(m_A[i]*outTangent + m_A[i + n*n-n]*inTangent);
                for ( int j = 1; j < n-1; ++j )
                {
                    Vector3 b_j = 3.0f*(positions[j+1]-positions[j-1]);
                    m_OutTangents[i] += m_A[i + n*j]*b_j;
                }

                // in tangent is out tangent of next segment
                m_InTangents.Add(m_OutTangents[i]);
            } 
            
            m_Positions.Add(positions[count-1]);
            m_Times.Add(times[count-1]);
            m_InTangents.Add(inTangent);                 
            
            // set up curve segment lengths
            m_LengthDirty = true;
            
            return true;
        }

        private List<float> m_U = null; // upper diagonal matrix entries
        private List<Vector3> m_z = null; // solution of lower diagonal system Lz = b
        /// <summary>
        /// Set up sample points for natural spline
        /// Uses tridiagonal matrix solver
        /// </summary>
        /// <param name="positions"></param>
        /// <param name="times"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public bool InitializeNatural(in Vector3[] positions, in float[] times, int count)
        {
            // make sure data is valid
            if ( count < 3 || positions == null|| times == null || positions.Length != count || times.Length != count)
                return false;

            // set up arrays
            InitInternal(count);

            // copy positions and times
            m_Positions.AddRange(positions);
            m_Times.AddRange(times);

            // build tangent data
            int n = count;
            float L;                          // current lower diagonal matrix entry
            
            if (m_U == null)
                m_U = new List<float>();
            else
                m_U.Clear();
            if (m_z == null)
                m_z = new List<Vector3>();
            else
                m_z.Clear();

            // solve for upper matrix and partial solution z
            L = 2.0f;
            m_U.Add(0.5f);
            m_z.Add(3.0f*(positions[1] - positions[0])/L);
            m_InTangents.Add(Vector3.zero);
            m_OutTangents.Add(Vector3.zero);
            for ( int i = 1; i < n-1; ++i )
            {
                // init lists (up to n-2 count)
                m_InTangents.Add(Vector3.zero);
                m_OutTangents.Add(Vector3.zero);
                
                // add internal entry to linear system for smooth spline
                L = 4.0f - m_U[i-1];
                m_U.Add(1.0f/L);
                var zi = 3.0f * (positions[i + 1] - positions[i - 1]);
                zi -= m_z[i-1];
                zi /= L;
                m_z.Add(zi);
            }
            L = 2.0f - m_U[n-2];
            m_z.Add(3.0f*(positions[n-1] - positions[n-2]));
            m_z[n-1] -= m_z[n-2];
            m_z[n-1] /= L;

            // solve Ux = z (see Burden and Faires for details)
            m_InTangents[n-2] = m_z[n-1];
            for ( int i = n-2; i > 0; --i )
            {
                m_InTangents[i-1] = m_z[i] - m_U[i]*m_InTangents[i];
                m_OutTangents[i] = m_InTangents[i-1];
            }
            m_OutTangents[0] = m_z[0] - m_U[0]*m_InTangents[0];

            // set up curve segment lengths
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
            Debug.Assert( m_Count >= 3 );
            if ( m_Count < 3 )
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
            Vector3 A = 2.0f*m_Positions[i]
                          - 2.0f*m_Positions[i+1]
                          + m_InTangents[i]
                          + m_OutTangents[i];
            Vector3 B = -3.0f*m_Positions[i]
                          + 3.0f*m_Positions[i+1]
                          - m_InTangents[i]
                          - 2.0f*m_OutTangents[i];
    
            return m_Positions[i] + u*(m_OutTangents[i] + u*(B + u*A));            
        }

        /// <summary>
        /// Evaluate derivative at parameter t
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public Vector3 Velocity(float t)
        {
            // make sure data is valid
            Debug.Assert( m_Count >= 3 );
            if ( m_Count < 3 )
                return UnityEngine.Vector3.zero;

            // handle boundary conditions
            if ( t <= m_Times[0] )
                return m_OutTangents[0];
            else if ( t >= m_Times[m_Count-1] )
                return m_InTangents[m_Count-2];

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
            Vector3 A = 2.0f*m_Positions[i]
                          - 2.0f*m_Positions[i+1]
                          + m_InTangents[i]
                          + m_OutTangents[i];
            Vector3 B = -3.0f*m_Positions[i]
                          + 3.0f*m_Positions[i+1]
                          - m_InTangents[i]
                          - 2.0f*m_OutTangents[i];
    
            return m_OutTangents[i] + u*(2.0f*B + 3.0f*u*A);            
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

            // evaluate
            Vector3 A = 2.0f*m_Positions[i]
                          - 2.0f*m_Positions[i+1]
                          + m_InTangents[i]
                          + m_OutTangents[i];
            Vector3 B = -3.0f*m_Positions[i]
                          + 3.0f*m_Positions[i+1]
                          - m_InTangents[i]
                          - 2.0f*m_OutTangents[i];
    
            return 2.0f*B + 6.0f*u*A;            
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
        /// Tries to make times as uniform as possible setting their value according to lengths between points
        /// </summary>
        public void MakeUniformTimes()
        {
            if (m_LengthDirty)
                RegenerateLengths();

            var cumulativeLength = 0f;
            for (int i = 1; i < m_Times.Count - 1; i++)
            {
                cumulativeLength += m_Lengths[i-1];
                m_Times[i] = cumulativeLength / m_TotalLength;
            }

            m_Times[m_Times.Count - 1] = 1.0f;
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
                if (m_LengthDirty)
                    RegenerateLengths();
                
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
            Vector3 A = 2.0f*m_Positions[i]
                - 2.0f*m_Positions[i+1]
                + m_InTangents[i]
                + m_OutTangents[i];
            Vector3 B = -3.0f*m_Positions[i]
                + 3.0f*m_Positions[i+1]
                - m_InTangents[i]
                - 2.0f*m_OutTangents[i];
            Vector3 C = m_InTangents[i];
                
            for ( int j = 0; j < 5; ++j )
            {
                float u = 0.5f*((u2 - u1)*x[j] + u2 + u1);
                Vector3 derivative = C + u*(2.0f*B + 3.0f*u*A);
                sum += c[j]*derivative.magnitude;
            }
            sum *= 0.5f*(u2-u1);
            
            return sum;            
        }

        // if needed, this is missing generating the time
//        public void AddPosition(Vector3 pos, Vector3 inTangent, Vector3 outTangent)
//        {
//            m_Positions.Add(pos);
//            m_InTangents.Add(inTangent);
//            m_InTangents.Add(outTangent);
//            m_Count++;
//        }

        /// <summary>
        /// Figure out natural in and out tangents.
        /// </summary>
        /// <param name="pos">position to add, in curve local space</param>
        /// <param name="index">zero based index at where to add the position</param>
        public bool AddPositionToIndexNatural(Vector3 pos, int index)
        {
            if (m_Count < 3)
                return false;

            // save and initialize arrays
            InitTemps();    
            m_InTangents.Clear();
            m_OutTangents.Clear();

            if (index == m_Positions.Count)
                m_Positions.Add(pos); // do to addition      
            else
                m_Positions.Insert(index, pos);
            
            m_Count++;
            
            // build tangent data
            int n = m_Count;
            float L;                          // current lower diagonal matrix entry

            if (m_U == null)
                m_U = new List<float>();
            else
                m_U.Clear();
            if (m_z == null)
                m_z = new List<Vector3>();
            else
                m_z.Clear();            
            
            // solve for upper matrix and partial solution z
            L = 2.0f;
            m_U.Add(0.5f);
            m_z.Add(3.0f*(m_Positions[1] - m_Positions[0])/L);
            m_InTangents.Add(Vector3.zero);
            m_OutTangents.Add(Vector3.zero);            
            for ( int i = 1; i < n-1; ++i )
            {
                m_InTangents.Add(Vector3.zero);
                m_OutTangents.Add(Vector3.zero);                            
                
                // add internal entry to linear system for smooth spline
                L = 4.0f - m_U[i-1];
                m_U.Add(1.0f/L);
                var zi = 3.0f * (m_Positions[i + 1] - m_Positions[i - 1]);
                zi -= m_z[i-1];
                zi /= L;
                m_z.Add(zi);
            }
            L = 2.0f - m_U[n-2];
            m_z.Add(3.0f*(m_Positions[n-1] - m_Positions[n-2]));
            m_z[n-1] -= m_z[n-2];
            m_z[n-1] /= L;

            // solve Ux = z (see Burden and Faires for details)
            m_InTangents[n-2] = m_z[n-1];
            for ( int i = n-2; i > 0; --i )
            {
                m_InTangents[i-1] = m_z[i] - m_U[i]*m_InTangents[i];
                m_OutTangents[i] = m_InTangents[i-1];
            }
            m_OutTangents[0] = m_z[0] - m_U[0]*m_InTangents[0];

            // return back the old data
            for (int i = 0; i < m_Positions.Count; i++)
            {
                if (i == index)   
                    continue;

                if (i > 0 && i != index && !(index == 0 && i <= 1))
                {
                    m_InTangents[i - 1] = m_TempInTangents[i < index? i - 1 : i - 2];
                }
                
                if (i < m_TempOutTangents.Count && i != index)
                    m_OutTangents[i] = m_TempOutTangents[i < index ? i : i - 1];
            }

            // determine natural time
            var newMax = 1.0f - (1.0f / m_Count);
            for (int i = 0; i < m_Times.Count; i++)
            {
                m_Times[i] *= newMax;
            }
            m_Times.Add(1.0f);

            m_LengthDirty = true;

            return true;
        }
        
        /// <summary>
        /// Adds position at the end.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns>If operation was successful</returns>
        public bool AddPositionNatural(Vector3 pos)
        {
            return AddPositionToIndexNatural(pos, m_Positions.Count);
        }

        public bool RemovePosition(int index)
        {
            if (index > m_Count - 1 || m_Count <= 3)
                return false;

            InitTemps();
            
            m_TempPositions.RemoveAt(index);
            m_TempInTangents.RemoveAt(Mathf.Max(index-1,0));
            m_TempOutTangents.RemoveAt(Mathf.Max(index-1,0));
            m_TempTimes.RemoveAt(index);
            m_Count--;

            return Initialize(m_TempPositions.ToArray(), m_TempInTangents.ToArray(), m_TempOutTangents.ToArray(), 
                m_TempTimes.ToArray(), m_Count);
        }

        public bool SetPosition(int index, Vector3 pos)
        {
            if (index < 0 || index > m_Count - 1)
                return false;

            m_Positions[index] = pos;

            m_LengthDirty = true;

            return true;
        }

        public bool SetInTangent(int index, Vector3 tangent)
        {
            if (index > m_Count - 2)
                return false;

            m_InTangents[index] = tangent;

            m_LengthDirty = true;

            return true;
        }
        
        public bool SetOutTangent(int index, Vector3 tangent)
        {
            if (index > m_Count - 2)
                return false;

            m_OutTangents[index] = tangent;

            return true;
        }

        public bool SetTime(int index, float time)
        {
            if (index > m_Count - 1)
                return false;            
            
            m_Times[index] = time;

            m_LengthDirty = true;

            return true;
        }
        
        private void InitInternal(int count)
        {
            m_Positions.Clear();
            m_InTangents.Clear();
            m_OutTangents.Clear();
            m_Times.Clear();
            m_Count = count;            
        }

        private void InitTemps()
        {
            if (m_TempPositions == null)
                m_TempPositions = new List<Vector3>();
            else 
                m_TempPositions.Clear();
            
            if (m_TempInTangents == null)
                m_TempInTangents = new List<Vector3>();
            else 
                m_TempInTangents.Clear();
            
            if (m_TempOutTangents == null)
                m_TempOutTangents = new List<Vector3>();
            else
                m_TempOutTangents.Clear();
            
            if (m_TempTimes == null)
                m_TempTimes = new List<float>();
            else
                m_TempTimes.Clear();     
            
            m_TempPositions.AddRange(m_Positions);
            m_TempInTangents.AddRange(m_InTangents);
            m_TempOutTangents.AddRange(m_OutTangents);
            m_TempTimes.AddRange(m_Times);
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

            m_LengthDirty = false;
        }

        private void RefreshClosed()
        {
            if (Closed)
            { // closing
                if (m_Positions[0] != m_Positions[m_Positions.Count - 1] ||
                    m_OutTangents[0] != m_InTangents[m_InTangents.Count - 1])
                { // refresh closing
                    AddPositionNatural(m_Positions[0]);
                    m_InTangents[m_InTangents.Count - 1] = m_OutTangents[0];
                }
            }
            else
            { 
                if (m_Positions[0] == m_Positions[m_Positions.Count - 1] &&
                    m_OutTangents[0] == m_InTangents[m_OutTangents.Count - 1])
                { // remove the last point
                    RemovePosition(m_Positions.Count - 1);
                }
            }
        }
        
    }
}