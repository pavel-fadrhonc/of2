using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace of2.Curves
{
    public class Kochanek
    {
        private Hermite m_Hermite;

        public ReadOnlyCollection<Vector3> Positions => m_Hermite.Positions;        
        public ReadOnlyCollection<float> Times => m_Hermite.Times;

        private bool m_CurveDirty;
        
        private float m_Tension;
        public float Tension
        {
            get { return m_Tension; }
            set
            {
                m_Tension = value;
                m_CurveDirty = true;
            }
        }

        private float m_Continuity;
        public float Continuity
        {
            get { return m_Continuity; }
            set
            {
                m_Continuity = value;
                m_CurveDirty = true;
            }
        }

        private float m_Bias;
        public float Bias
        {
            get { return m_Bias; }
            set
            {
                m_Bias = value;
                m_CurveDirty = true;
            }
        }

        private List<Vector3> m_inTangents;
        private List<Vector3> m_outTangents;

        public bool Initialize(in Vector3[] positions, in float[] times, float tension, float continuity, float bias, int count)
        {
            // make sure data is valid
            if (count < 3 || positions == null || times == null || positions.Length != count || times.Length != count)
                return false;            
            
            Tension = tension;
            Continuity = continuity;
            Bias = bias;

            if (!m_Hermite.InitializeNatural(positions, times, count))
                return false;

            RegenerateCurve();

            return true;
        }

        private void GenerateTangents()
        {
            for (int i = 0; i < m_Hermite.Positions.Count - 1; ++i)
            {
                m_inTangents[i] = 0.5f*(1.0f-Tension)*(1.0f-Continuity)*(1.0f+Bias)*(m_Hermite.Positions[i+1]-m_Hermite.Positions[i]);
                
                // standard incoming tangent
                if ( i < m_Hermite.Positions.Count - 2 )
                    m_inTangents[i] += 0.5f*(1.0f-Tension)*(1.0f+Continuity)*(1.0f-Bias)*(m_Hermite.Positions[i+2]-m_Hermite.Positions[i+1]);
                // cyclical incoming tangent
                else
                    m_inTangents[i] += 0.5f*(1.0f-Tension)*(1.0f+Continuity)*(1.0f-Bias)*(m_Hermite.Positions[0]-m_Hermite.Positions[i+1]);
                
                m_outTangents[i] = 0.5f*(1.0f-Tension)*(1.0f-Continuity)*(1.0f-Bias)*(m_Hermite.Positions[i+1]-m_Hermite.Positions[i]);
                
                // standard outgoing tangent
                if ( i > 0 )          
                    m_outTangents[i] += 0.5f*(1.0f-Tension)*(1.0f+Continuity)*(1.0f+Bias)*(m_Hermite.Positions[i]-m_Hermite.Positions[i-1]);
                // cyclical outgoing tangent
                else          
                    m_outTangents[i] += 0.5f*(1.0f-Tension)*(1.0f+Continuity)*(1.0f+Bias)*(m_Hermite.Positions[i]-m_Hermite.Positions[m_Hermite.Positions.Count - 1]);
            }            
        }
        
        private void RegenerateCurve()
        {
            GenerateTangents();

            for (int i = 0; i < m_inTangents.Count; i++)
            {
                m_Hermite.SetInTangent(i, m_inTangents[i]);
                m_Hermite.SetOutTangent(i, m_outTangents[i]);
            }

            m_CurveDirty = false;
        }

        /// <summary>
        /// Evaluate derivative at parameter t
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>        
        public Vector3 Velocity(float t)
        {
            return m_Hermite.Velocity(t);
        }

        /// <summary>
        /// Evaluate second derivative at parameter t
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>        
        public Vector3 Acceleration(float t)
        {
            return m_Hermite.Acceleration(t);
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
            return m_Hermite.FindParameterByDistance(t1, s);
        }

        /// <summary>
        /// Find length of curve between parameters t1 and t2
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public float ArcLength(float t1, float t2)
        {
            return m_Hermite.ArcLength(t1, t2);
        }

        public float SegmentArcLength(int i, float u1, float u2)
        {
            return m_Hermite.SegmentArcLength(i, u1, u2);
        }

        public void AddPositionToIndex(Vector3 pos, int index)
        {
            m_Hermite.AddPositionToIndexNatural(pos, index);

            m_CurveDirty = true;
        }

        public void RemovePosition(int index)
        {
            m_Hermite.RemovePosition(index);

            m_CurveDirty = true;
        }

        public bool SetPosition(int index, Vector3 pos)
        {
            return m_Hermite.SetPosition(index, pos);
        }

        public bool SetTime(int index, float time)
        {
            return m_Hermite.SetTime(index, time);
        }
        
        public Vector3 Evaluate(float t)
        {
            if (m_CurveDirty)
                RegenerateCurve();

            return m_Hermite.Evaluate(t);
        }
    }
}