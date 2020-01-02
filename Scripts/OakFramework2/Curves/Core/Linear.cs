using UnityEngine;

namespace of2.Curves
{
    public class Linear
    {
        protected Vector3[] m_Positions;
        protected float[] m_Times;
        protected uint m_Count;

        public bool Initialize(Vector3[] samples, float[] times, uint count)
        {
            if (m_Count != 0)
                return false;

            if (m_Count < 2 || samples == null || times == null)
                return false;

            m_Positions = new Vector3[count];
            m_Times = new float[count];
            m_Count = count;

            for (uint i = 0; i < count; i++)
            {
                m_Positions[i] = samples[i];
                m_Times[i] = times[i];
            }

            return true;
        }

        public Vector3 Evaluate(float t)
        {
            // make sure data is valid
            Debug.Assert(m_Count >= 2);
            if (m_Count < 2)
                return Vector3.zero;

            // handle boundary conditions
            if (t <= m_Times[0])
                return m_Positions[0];
            else if (t >= m_Times[m_Count - 1])
                return m_Positions[m_Count - 1];

            // find segment and parameter
            uint i;
            for (i = 0; i < m_Count - 1; ++i)
            {
                if (t < m_Times[i + 1])
                    break;
            }

            float t0 = m_Times[i];
            float t1 = m_Times[i + 1];
            float u = (t - t0) / (t1 - t0);

            // evaluate
            return (1 - u) * m_Positions[i] + u * m_Positions[i + 1];
        }

    }
}