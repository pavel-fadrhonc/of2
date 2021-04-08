using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using of2.Curves;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

[ExecuteInEditMode]
public class HermiteCurveOld : MonoBehaviour
{

    [SerializeField] //[HideInInspector]
    private Hermite m_Hermite;

    public Hermite Hermite => m_Hermite;
    
#if UNITY_EDITOR
    [SerializeField]
    private Color m_CurveColor = Color.red;
    
    [SerializeField]
    private Color m_PointColor = Color.yellow;    
    public Color PointColor => m_PointColor;
    
    [SerializeField]
    private Color m_InTangColor = Color.blue;
    public Color InTangColor => m_InTangColor;
    
    [SerializeField]
    private Color m_OutTangColor = Color.magenta;
    public Color OutTangColor => m_OutTangColor;

    [SerializeField]
    private int m_ResolutionVisual = 20;

    [HideInInspector]
    public List<Vector3> pointsForEditorAdd;
    
    [SerializeField]
    private List<Transform> m_Points = new List<Transform>();
    [SerializeField]
    private List<Transform> m_InTangents = new List<Transform>();
    [SerializeField]
    private List<Transform> m_OutTangents = new List<Transform>();
    [SerializeField]
    private List<float> m_Times = new List<float>();

    public ReadOnlyCollection<Transform> Points => m_Points.AsReadOnly();
    public ReadOnlyCollection<float> Times => m_Times.AsReadOnly();
    public ReadOnlyCollection<Transform> InTangents => m_InTangents.AsReadOnly();
    public ReadOnlyCollection<Transform> OutTangents => m_OutTangents.AsReadOnly();
    
    public Transform LastPoint => m_Points[m_Points.Count - 1];

    private void Awake()
    {
        if (m_Points.Count < 3 || m_InTangents.Count != m_Points.Count - 1 ||
            m_OutTangents.Count != m_Points.Count - 1)
        { // (re)initialize
            m_Points.Clear();
            m_InTangents.Clear();
            m_OutTangents.Clear();
            m_Times.Clear();            
        
            AddNewPointInternal();
            LastPoint.position = transform.position;
            AddNewPointInternal();
            LastPoint.position = transform.position + Vector3.forward; // TODO: make own monobehaviour with cached
            AddNewPointInternal();
            LastPoint.position = transform.position + 2 * Vector3.forward + Vector3.right; // TODO: make own monobehaviour with cached
            
            m_Times.Add(0.0f);
            m_Times.Add(0.5f);
            m_Times.Add(1.0f);

            m_Hermite = new Hermite();
            m_Hermite.InitializeNatural(m_Points.Select(p => p.position).ToArray(), m_Times.ToArray(), m_Points.Count);

            AddNewIntagentInternal(1);
            AddNewIntagentInternal(2);
            AddNewOutTangentInternal(0);
            AddNewOutTangentInternal(1);
            
//            // create intangents
//            for (int i = 0; i < m_Hermite.InTangents.Count; i++)
//            {
//                var inTangent = new GameObject(m_Points[i+1].name + "_inTangent");
//                inTangent.transform.SetParent(m_Points[i+1].transform);
//                m_InTangents.Add(inTangent.transform);
//                inTangent.transform.position = m_Hermite.InTangents[i];
//            }
//            
//            // create outtangents
//            for (int i = 0; i < m_Hermite.OutTangents.Count; i++)
//            {
//                var outTangent = new GameObject(m_Points[i].name + "_outTangent");
//                outTangent.transform.SetParent(m_Points[i].transform);
//                m_OutTangents.Add(outTangent.transform);
//                outTangent.transform.position = m_Hermite.OutTangents[i];
//            }                    
        }
    }

    private void Update()
    {
//        m_Hermite = new Hermite();
//        m_Hermite.Initialize(m_Points.Select(tr => tr.position).ToArray(),
//            m_InTangents.Select(tr => tr.forward).ToArray(),
//            m_OutTangents.Select(tr => tr.forward).ToArray(),
//            m_Times.ToArray(),
//            m_Points.Count);
//
//        // draw curve
//        float step = 1f / m_ResolutionVisual;
//        for (int i = 1; i < m_ResolutionVisual; i++)
//        {
//            var previousPoint = m_Hermite.Evaluate((i - 1) * step);
//            var thisPoint = m_Hermite.Evaluate((i) * step);
//            Debug.DrawLine(previousPoint, thisPoint, m_CurveColor);
//        }

        for (int i = 0; i < m_InTangents.Count; i++)
        {
            m_Hermite.SetInTangent(i, m_InTangents[i].position);
        }
        for (int i = 0; i < m_OutTangents.Count; i++)
        {
            m_Hermite.SetOutTangent(i, m_OutTangents[i].position);
        }
        for (int i = 0; i < m_Points.Count; i++)
        {
            m_Hermite.SetPosition(i, m_Points[i].position);
        }
        for (int i = 0; i < m_Times.Count; i++)
        {
            m_Hermite.SetTime(i, m_Times[i]);
        }
        
//        // draw curve
//        float step = 1f / m_ResolutionVisual;
//        for (int i = 1; i < m_ResolutionVisual; i++)
//        {
//            var previousPoint = m_Hermite.Evaluate((i - 1) * step);
//            var thisPoint = m_Hermite.Evaluate((i) * step);
//            Debug.DrawLine(previousPoint, thisPoint, m_CurveColor);
//        }        
        
        // draw tangents
        for (int i = 0; i < m_Points.Count; i++)
        {
            if (i > 0)
            {
                Debug.DrawLine(m_Points[i].position, m_Points[i].position + m_InTangents[i-1].position, m_InTangColor);
            }

            if (i < m_Points.Count - 1)
            {
                Debug.DrawLine(m_Points[i].position, m_Points[i].position + m_OutTangents[i].position, m_OutTangColor);                
            }
        }
    }

    public GameObject AddNewPoint()
    {
        if (m_Points.Count < 3)
            Debug.LogWarning("Can't add point when less than two points.");

        Vector3 newPos = LastPoint.position +
                         (LastPoint.position - m_Points[m_Points.Count - 2].position);

        if (m_Hermite.AddPositionNatural(newPos))
        {
            AddNewPointInternal();   
            
            AddNewIntagentInternal(m_Hermite.Positions.Count - 1);
            AddNewOutTangentInternal(m_Hermite.Positions.Count - 2);
            
            return LastPoint.gameObject;
        }

        return null;
    }

    public void RemovePoint(int index)
    {
        if (m_Points.Count <= 3)
        {
            Debug.LogWarning("Can't remove any more points.");
            return;
        }
        
        //TODO:finish removing, remove also tangents and times
        m_Hermite.RemovePosition(index);
    }

    private bool VerifyIntegrity(bool fix = false, bool report = false)
    {
        var posFail = m_Points.Count != m_Hermite.Positions.Count;
        var outTangFail = m_OutTangents.Count != m_Hermite.OutTangents.Count;
        var inTangFail = m_InTangents.Count != m_Hermite.InTangents.Count;
        var timesFail = m_Times.Count != m_Hermite.Times.Count;

//        if (posFail && fix && m_Points.Count == m_Hermite.Positions.Count + 1)
//        { // we assume addition from editor via SerializedProperty has happened
//            // find the index
//            for (int i = 0; i < m_Hermite.Positions.Count)
//            {
//                if (m_Hermite.Positions[i] != m_Points[i].position)
//                {
//                    
//                }
//            }
//        }

        return true;
    }

//    public Vector3 Evaluate(float t)
//    {
//        
//    }
    
    private void AddNewPointInternal()
    {
        var newPosGO = new GameObject(GetNewName());
        newPosGO.hideFlags = HideFlags.HideInHierarchy;
        newPosGO.transform.SetParent(transform);
            
        m_Points.Add(newPosGO.transform);     
    }

    /// <summary>
    /// Assumes that point internal has been added before
    /// </summary>
    /// <param name="pointIndex">Corresponding point index</param>
    private void AddNewIntagentInternal(int pointIndex)
    {
        if (pointIndex == 0)
            return;

        var correspondingPoint = m_Points[pointIndex];
        var inTangent = new GameObject(correspondingPoint.name + "_inTangent");
        inTangent.transform.SetParent(correspondingPoint.transform);
        m_InTangents.Add(inTangent.transform);
        inTangent.transform.position = m_Hermite.InTangents[pointIndex - 1];        
    }
    
    /// <summary>
    /// Assumes that point internal has been added before
    /// </summary>
    /// <param name="pointIndex">Corresponding point index</param>
    private void AddNewOutTangentInternal(int pointIndex)
    {
        if (pointIndex >= m_Points.Count - 1)
            return;

        var correspondingPoint = m_Points[pointIndex];
        var outTangent = new GameObject(correspondingPoint.name + "_outTangent");
        outTangent.transform.SetParent(correspondingPoint.transform);
        m_OutTangents.Add(outTangent.transform);
        outTangent.transform.position = m_Hermite.OutTangents[pointIndex];        
    }    
    
    private string GetNewName()
    {
        if (m_Points.Count == 0)
            return "A";
        
        var lastPoint = m_Points[m_Points.Count - 1];
        var newName = lastPoint.name;
        newName = newName.Substring(0, newName.Length - 1) + (char) (newName[newName.Length - 1] + 1);
        if (lastPoint.name[lastPoint.name.Length - 1] == 'Z')
        {
            newName = newName.Substring(0, newName.Length - 1) + 'A' + 'A';
        }

        return newName;
    }
    
#endif
}
