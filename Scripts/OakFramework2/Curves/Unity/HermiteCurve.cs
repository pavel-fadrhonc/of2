using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using of2.Curves;
using UnityEditor.SceneManagement;
using UnityEditorInternal.VR.iOS;
using UnityEngine;
using UnityEngine.Serialization;

namespace OakFramework2.Curves.Unity
{
    [ExecuteInEditMode]
    public class HermiteCurve : MonoBehaviour, ICurve
    {
        public int ControlPointCount { get; }

        public List<Vector3> points = new List<Vector3>();
        public List<Vector3> inTangents = new List<Vector3>();
        public List<Vector3> outTangents = new List<Vector3>();
        public List<float> times = new List<float>();
        public bool closed;
        public float? length;
        
        [SerializeField] [HideInInspector] private Hermite m_Hermite;
        public Hermite Hermite
        {
            get { return m_Hermite; }
        }
        
        private bool m_ClosedInternal;

        /// <summary>
        /// Index is used to provide uniform point searching.
        /// </summary>
        HermiteCurveIndex uniformIndex;

        HermiteCurveIndex Index
        {
            get
            {
                if (uniformIndex == null) uniformIndex = new HermiteCurveIndex(this);
                return uniformIndex;
            }
        }

        #region UNITY FUNCTIONS

        private void Awake()
        {
            Init();
        }

        #endregion

        void OnValidate()
        {
            if (uniformIndex != null)
            {
                ValidateData();
                uniformIndex.ReIndex();
            }
        }

        private Dictionary<int, Vector3> m_DifferencesVect = new Dictionary<int, Vector3>();
        private Dictionary<int, float> m_DifferencesFloat = new Dictionary<int, float>();

        public void ValidateData()
        {
            var posFail = points.Count != m_Hermite.Positions.Count;
            var outTangFail = outTangents.Count != m_Hermite.OutTangents.Count;
            var inTangFail = inTangents.Count != m_Hermite.InTangents.Count;
            var timesFail = times.Count != m_Hermite.Times.Count;

            if (posFail)
            {
                ///////////////////
                /// ADDITION
                ///////////////////
                if (points.Count == m_Hermite.Positions.Count + 1)
                {
                    // we assume addition from editor via SerializedProperty has happened
                    var differentIndex = CheckForDifference<Vector3>(points, m_Hermite.Positions);
                    m_Hermite.AddPositionToIndexNatural(points[differentIndex], differentIndex);

                    inTangents.Clear();
                    inTangents.AddRange(m_Hermite.InTangents);
                    outTangents.Clear();
                    outTangents.AddRange(m_Hermite.OutTangents);
                    times.Clear();
                    times.AddRange(m_Hermite.Times);
                }
//                if (inTangents.Count + 1 == m_Hermite.InTangents.Count)
//                { // we assume another tangent has been generated because we've added another point
//                    var differentIndex = CheckForDifference<Vector3>(inTangents, m_Hermite.InTangents);
//                    inTangents.Insert(differentIndex, m_Hermite.InTangents[differentIndex]);
//                }
//                if (outTangents.Count + 1 == m_Hermite.OutTangents.Count)
//                { // we assume another tangent has been generated because we've added another point
//                    var differentIndex = CheckForDifference<Vector3>(outTangents, m_Hermite.OutTangents);
//                    outTangents.Insert(differentIndex, m_Hermite.OutTangents[differentIndex]);                    
//                }
//                if (times.Count + 1 == m_Hermite.Times.Count)
//                { // we assume another time has been generated because we've added another point
//                    var differentIndex = CheckForDifference<float>(times, m_Hermite.Times);
//                    times.Insert(differentIndex, m_Hermite.Times[differentIndex]);                    
//                }

                ///////////////////
                /// REMOVAL
                ///////////////////
                if (points.Count == m_Hermite.Positions.Count - 1)
                {
                    // we assume it's removal
                    var differentIndex = CheckForDifference<Vector3>(points, m_Hermite.Positions);
                    m_Hermite.RemovePosition(differentIndex);
                }

                if (inTangents.Count - 1 == m_Hermite.InTangents.Count)
                {
                    // we assume tangent has been removed internally because of point removal
                    var differentIndex = CheckForDifference<Vector3>(inTangents, m_Hermite.InTangents);
                    inTangents.RemoveAt(differentIndex);
                }

                if (outTangents.Count - 1 == m_Hermite.OutTangents.Count)
                {
                    // we assume tangent has been removed internally because of point removal
                    var differentIndex = CheckForDifference<Vector3>(outTangents, m_Hermite.OutTangents);
                    outTangents.RemoveAt(differentIndex);
                }

                if (times.Count - 1 == m_Hermite.Times.Count)
                {
                    // we assume time has been removed because we've remove a point
                    var differentIndex = CheckForDifference<float>(times, m_Hermite.Times);
                    times.RemoveAt(differentIndex);
                }
            }

            // EVALUATE THE CHANGES - they can happen by manipulating the field via editor SerializedProperty writing
            GetTheDifferences<Vector3>(m_Hermite.Positions, points, m_DifferencesVect);
            if (m_DifferencesVect.Count > 0)
            {
                foreach (var difVect in m_DifferencesVect)
                {
                    m_Hermite.SetPosition(difVect.Key, difVect.Value);
                }
            }

            GetTheDifferences<Vector3>(m_Hermite.InTangents, inTangents, m_DifferencesVect);
            if (m_DifferencesVect.Count > 0)
            {
                foreach (var difVect in m_DifferencesVect)
                {
                    m_Hermite.SetInTangent(difVect.Key, difVect.Value);
                }
            }

            GetTheDifferences<Vector3>(m_Hermite.OutTangents, outTangents, m_DifferencesVect);
            if (m_DifferencesVect.Count > 0)
            {
                foreach (var difVect in m_DifferencesVect)
                {
                    m_Hermite.SetOutTangent(difVect.Key, difVect.Value);
                }
            }

            GetTheDifferences<float>(m_Hermite.Times, times, m_DifferencesFloat);
            if (m_DifferencesFloat.Count > 0)
            {
                foreach (var difFloat in m_DifferencesFloat)
                {
                    m_Hermite.SetTime(difFloat.Key, difFloat.Value);
                }
            }
        }

        public void ValidateClosed()
        {
            if (closed != m_ClosedInternal)
            {
                if (closed && m_Hermite.Positions[0] != m_Hermite.Positions[m_Hermite.Positions.Count - 1])
                {
                    var newPos = m_Hermite.Positions[0];

                    m_Hermite.AddPositionNatural(newPos);
                    m_Hermite.MakeUniformTimes();
                    m_Hermite.SetInTangent(m_Hermite.Positions.Count - 2, m_Hermite.OutTangents[0]);

                    points.Clear();
                    points.AddRange(m_Hermite.Positions);
                    inTangents.Clear();
                    inTangents.AddRange(m_Hermite.InTangents);
                    outTangents.Clear();
                    outTangents.AddRange(m_Hermite.OutTangents);
                    times.Clear();
                    times.AddRange(m_Hermite.Times);
                }
                else if (m_Hermite.Positions[0] == m_Hermite.Positions[m_Hermite.Positions.Count - 1])
                {
                    m_Hermite.RemovePosition(m_Hermite.Positions.Count - 1);

                    points.Clear();
                    points.AddRange(m_Hermite.Positions);
                    inTangents.Clear();
                    inTangents.AddRange(m_Hermite.InTangents);
                    outTangents.Clear();
                    outTangents.AddRange(m_Hermite.OutTangents);
                    m_Hermite.MakeUniformTimes();
                    times.Clear();
                    times.AddRange(m_Hermite.Times);
                }

                m_ClosedInternal = closed;
            }
        }

        /// <summary>
        /// Returns index of first element that's different or -1 if not found.
        /// If one array is larger and no element was different when traversing the smaller
        /// array it returns index of sizeOfSmallerArray+1
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private int CheckForDifference<T>(IReadOnlyList<T> array1, IReadOnlyList<T> array2) where T : IEquatable<T>
        {
            var numComps = Mathf.Min(array1.Count, array2.Count);

            for (int i = 0; i < numComps; i++)
            {
                if (!array1[i].Equals(array2[i]))
                    return i;
            }

            if (array1.Count != array2.Count)
            {
                return array1.Count > array2.Count ? array2.Count : array1.Count;
            }

            return -1;
        }

        /// <summary>
        /// Fills the differences IDictionary with <index,Value> records of elements that are different in array2 against array1.
        /// Also all elements from array2 with index larger that array1.Count are added.
        /// If array1 has elements with index larger that array2.Count they are ignored.
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <param name="differences"></param>
        /// <typeparam name="T"></typeparam>
        private void GetTheDifferences<T>(IReadOnlyList<T> array1, IReadOnlyList<T> array2,
            IDictionary<int, T> differences) where T : IEquatable<T>
        {
            differences.Clear();

            for (int i = 0; i < array2.Count; i++)
            {
                if (i < array1.Count)
                {
                    if (!array1[i].Equals(array2[i]))
                        differences.Add(i, array2[i]);
                }
                else
                {
                    differences.Add(i, array2[i]);
                }
            }
        }

        void Reset()
        {
            Init();

            points.Clear();

            points.Add(Vector3.forward * 3);
            points.Add(Vector3.forward * 6);
            points.Add(Vector3.forward * 9);

            times.Clear();
            times.Add(0f);
            times.Add(0.5f);
            times.Add(1.0f);

            m_Hermite.InitializeNatural(points.ToArray(), times.ToArray(), 3);

            inTangents.Clear();
            inTangents.AddRange(m_Hermite.InTangents);

            outTangents.Clear();
            outTangents.AddRange(m_Hermite.OutTangents);
        }

        public void ResetIndex()
        {
            uniformIndex = null;
            length = null;
        }

        /// <summary>
        /// Returns world space position of point evaluated on t.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public Vector3 GetNonUniformPoint(float t)
        {
            return transform.TransformPoint(m_Hermite.Evaluate(t));
        }

        public Vector3 GetPoint(float t) => Index.GetPoint(t);

        public Vector3 GetLeft(float t) => -GetRight(t);

        public Vector3 GetDown(float t) => -GetUp(t);

        public Vector3 GetBackward(float t) => -GetForward(t);

        public Vector3 GetRight(float t)
        {
            var A = GetPoint(t - 0.001f);
            var B = GetPoint(t + 0.001f);
            var delta = (B - A);
            return new Vector3(-delta.z, 0, delta.x).normalized;

        }

        public Vector3 GetUp(float t)
        {
            var A = GetPoint(t - 0.001f);
            var B = GetPoint(t + 0.001f);
            var delta = (B - A).normalized;
            return Vector3.Cross(delta, GetRight(t));
        }

        public Vector3 GetForward(float t)
        {
            var A = GetPoint(t - 0.001f);
            var B = GetPoint(t + 0.001f);
            return (B - A).normalized;

        }

        public float GetLength()
        {
            return m_Hermite.ArcLength(0f, 1f);
        }

        public Vector3 GetControlPoint(int index)
        {
            return m_Hermite.Positions[index];
        }

        public void SetControlPoint(int index, Vector3 position)
        {
            m_Hermite.SetPosition(index, position);
        }

        public void InsertControlPoint(int index, Vector3 position)
        {
            ResetIndex();

            m_Hermite.AddPositionToIndexNatural(position, index);
        }

        public void RemoveControlPoint(int index)
        {
            ResetIndex();
            m_Hermite.RemovePosition(index);
        }

        public Vector3 GetDistance(float distance)
        {
            if (length == null) length = GetLength();
            return uniformIndex.GetPoint(distance / length.Value);
        }

        Vector3 GetPointByIndex(int i)
        {
            if (i < 0) i += points.Count;
            return points[i % points.Count];
        }

        public Vector3 FindClosest(Vector3 worldPoint)
        {
            var smallestDelta = float.MaxValue;
            var step = 1f / 1024;
            var closestPoint = Vector3.zero;
            for (var i = 0; i <= 1024; i++)
            {
                var p = GetPoint(i * step);
                var delta = (worldPoint - p).sqrMagnitude;
                if (delta < smallestDelta)
                {
                    closestPoint = p;
                    smallestDelta = delta;
                }
            }

            return closestPoint;
        }

        private void Init()
        {
            if (m_Hermite == null)
                m_Hermite = new Hermite();
        }

        #region EDITOR ONLY STUFF

#if UNITY_EDITOR

        public enum eCurveMode
        {
            PointsAndTangents,
            Times
        }
        
        public enum eTangentEditMode
        {
            free,
            in_out,
            in_neg_out,
            in_dir_out,
            in_neg_dir_out,
            out_in,
            out_neg_in,
            out_dir_in,
            out_neg_dir_in
        }

        public enum eEditFilter
        {
            selected,
            all
        }

        [Flags]
        public enum eTangentViewOptions
        {
            none =                  0, 
            selectedPoint =         1,
            closePoint =            2,
            selectedCurve =         4,
            always =             selectedPoint | closePoint | selectedCurve
        }

        public enum eTangentFlatteningOptions
        {
            Keep,
            Move,
            Flatten
        }

        public eCurveMode curveMode;
        [FormerlySerializedAs("editMode")] [HideInInspector][SerializeField]
        public eTangentEditMode tangentEditMode;
        [HideInInspector][SerializeField]
        public eEditFilter editFilter;
        [HideInInspector][SerializeField]
        public int tangentViewOptions;
        [HideInInspector][SerializeField]
        public eTangentFlatteningOptions tangentFlattenOptions;

        public bool tangentLengthControlApply = false;
        public float tangentLengthControlLength;

        public GameObject moveObject;
        public float moveObjectNormPos;
        public float timeEditPointSize = 0.05f;
        
#endif

        #endregion
    }
}
