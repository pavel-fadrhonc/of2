using System;
using System.Collections;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace OakFramework2.Curves.Unity.Editor
{
    [CustomEditor(typeof(HermiteCurve))]
    public class HermiteCurveEditor : UnityEditor.Editor
    {
        int m_hotIndex = -1;
        int m_removeIndex = -1;
        private bool m_inTangentSelected = false;
        private bool m_outTangentSelected = false;
        private bool m_lastMoveInTangent = false;
        private bool m_ShowCurveInfo = false;
        private bool m_MoveOnCurve;
        private float m_MoveOnCurveSpeed = 1;
        private EditorCoroutine m_MoveOnCurveCR;

        Texture testText;

        private readonly int screenHeightOffset = 39;

        private readonly Color IntangetColor = Color.cyan; 
        private readonly Color OuttangetColor = Color.magenta; 

        private void OnEnable()
        {
            testText = Resources.Load<Texture>("Textures/testText");
        }

        public override void OnInspectorGUI()
        {
            
            
            EditorGUILayout.HelpBox("Hold Shift and click to append and insert curve points. Backspace to delete points.", MessageType.Info);
            var curve = target as HermiteCurve;
            
            DrawCurve(curve, 1024, Color.white, true);            
            
//            if (Event.current.isKey && Event.current.keyCode == KeyCode.T)
//            {
//                m_TimeEditorMode = !m_TimeEditorMode;
//            }
            var curveMode = serializedObject.FindProperty("curveMode");
            var editTimesBool = GUILayout.Toggle(curveMode.enumValueIndex == (int) HermiteCurve.eCurveMode.Times, "Edit Times", "button");
            if (editTimesBool)
            {
                curveMode.enumValueIndex = (int) HermiteCurve.eCurveMode.Times;
                
                GUILayout.BeginHorizontal();
                curve.timeEditPointSize = EditorGUILayout.FloatField("Point size", curve.timeEditPointSize);
                var makeUniform = GUILayout.Button("Make Times Uniform");
                if (makeUniform)
                {  // this is a bit cumbersome as we generate the times directly in Hermite, then push the data to curve serialized property, but it's in order to be compatible with undo/redo
                    curve.Hermite.MakeUniformTimes();
                    for (int i = 0; i < curve.times.Count; i++)
                    {
                        var timesProperty = serializedObject.FindProperty("times");
                        var timeProp = timesProperty.GetArrayElementAtIndex(i);
                        timeProp.floatValue =curve.Hermite.Times[i];
                    }
                }
                
                GUILayout.EndHorizontal();
            }
            else
            {
                curveMode.enumValueIndex = (int) HermiteCurve.eCurveMode.PointsAndTangents;
            }

            GUILayout.BeginHorizontal();
            var closed = GUILayout.Toggle(curve.closed, "Closed", "button");
            if (curve.closed != closed)
            {
                var agreed = EditorUtility.DisplayDialog(curve.closed ? "Close curve" : "Open curve",
                    "This will make times uniform. Continue?", "Ok", "Cancel");
                if (agreed)
                {
                    var closedProp = serializedObject.FindProperty("closed");
                    closedProp.boolValue = closed;

                    serializedObject.ApplyModifiedProperties();
                    curve.ValidateClosed();
                    curve.ResetIndex();
                }
            }
            
//            if (GUILayout.Button("Flatten Y Axis"))
//            {
//                Undo.RecordObject(target, "Flatten Y Axis");
//                //TODO: Flatten(spline.points);
//                spline.ResetIndex();
//            }            
            
//            if (GUILayout.Button("Center around Origin"))
//            {
//                Undo.RecordObject(target, "Center around Origin");
//                //TODO: CenterAroundOrigin(spline.points);
//                spline.ResetIndex();
//            }
            GUILayout.EndHorizontal();

            #region TANGENT CONTROL   
            
            var guiStyle = new GUIStyle();
            guiStyle.fontStyle = FontStyle.Bold;
            
            #region TANGENT LENGTH CONTROL
            
            var outTangents = serializedObject.FindProperty("outTangents");
            var inTangents = serializedObject.FindProperty("inTangents");

            EditorGUILayout.LabelField("Tangent Length Control", guiStyle);
             
            var tangentLengthControlLength = serializedObject.FindProperty("tangentLengthControlLength");
            var tangentLengthControlApply = serializedObject.FindProperty("tangentLengthControlApply");
            
            var previousApply = tangentLengthControlApply.boolValue;
            var previousLength = tangentLengthControlLength.floatValue;

            
            if (m_hotIndex != -1 && (m_inTangentSelected || m_outTangentSelected))
            {
                var point = curve.points[m_hotIndex];
                float intangentLength = 0f;
                float outtangentLength = 0f;
                if (m_inTangentSelected && m_hotIndex > 0)
                {
                    var intangent = inTangents.GetArrayElementAtIndex(m_hotIndex - 1);
                    intangentLength = (intangent.vector3Value - point).magnitude;
                }
                if (m_outTangentSelected && m_hotIndex < curve.points.Count - 1)
                {
                    var outtangent = outTangents.GetArrayElementAtIndex(m_hotIndex);
                    outtangentLength = (outtangent.vector3Value - point).magnitude;
                }

                if (m_lastMoveInTangent)
                    tangentLengthControlLength.floatValue = intangentLength;
                else
                    tangentLengthControlLength.floatValue = outtangentLength;
            }
            
            tangentLengthControlLength.floatValue = Mathf.Max( 0.01f, EditorGUILayout.FloatField("New length:", tangentLengthControlLength.floatValue));
            tangentLengthControlApply.boolValue = EditorGUILayout.Toggle("Apply", tangentLengthControlApply.boolValue);
            
            if (tangentLengthControlApply.boolValue &&
                (previousLength != tangentLengthControlLength.floatValue ||
                 previousApply != tangentLengthControlApply.boolValue))
            {
                if (curve.editFilter == HermiteCurve.eEditFilter.all)
                {
                    for (int pointIdx = 0; pointIdx < curve.points.Count; pointIdx++)
                    {
                        var point = curve.points[pointIdx];

                        if (pointIdx > 0)
                        {
                            var intangent = inTangents.GetArrayElementAtIndex(pointIdx - 1);
                            SetNewTangentPos(intangent, 
                                (intangent.vector3Value - point).normalized * tangentLengthControlLength.floatValue + point, 
                                curve, pointIdx, false);
                        }

                        if (pointIdx < curve.points.Count - 1)
                        {
                            var outtangent = outTangents.GetArrayElementAtIndex(pointIdx);
                            SetNewTangentPos(outtangent, 
                                (outtangent.vector3Value - point).normalized * tangentLengthControlLength.floatValue + point, 
                                curve, pointIdx, false);                        
                        }
                    }
                }
                else if (curve.editFilter == HermiteCurve.eEditFilter.selected && m_hotIndex != -1)
                {
                    var point = curve.points[m_hotIndex];
                    
                    if (m_inTangentSelected)
                    {
                        var intangent = inTangents.GetArrayElementAtIndex(m_hotIndex - 1);
                        SetNewTangentPos(intangent, 
                            (intangent.vector3Value - point).normalized * tangentLengthControlLength.floatValue + point, 
                            curve, m_hotIndex, false);                        
                    }
                    if (m_outTangentSelected)
                    {
                        var outtangent = outTangents.GetArrayElementAtIndex(m_hotIndex);
                        SetNewTangentPos(outtangent, 
                            (outtangent.vector3Value - point).normalized * tangentLengthControlLength.floatValue + point, 
                            curve, m_hotIndex, false);                        
                    }
                }
            }

            #endregion            
            
            EditorGUILayout.LabelField("Tangent Control Mode", guiStyle);
            EditorGUI.indentLevel = 1;          
            
            var buttonStyle = new GUIStyle("button");

            var tangentEditModeProp = serializedObject.FindProperty("tangentEditMode");            
            
            if (GUILayout.Toggle(tangentEditModeProp.enumValueIndex == (int) HermiteCurve.eTangentEditMode.free, "free", "button"))
                tangentEditModeProp.enumValueIndex = (int) HermiteCurve.eTangentEditMode.free;

            var defaultBkgColor = GUI.backgroundColor;

            DrawToggleWithColoredRectangles(IntangetColor, OuttangetColor, "in = out", tangentEditModeProp, (int) HermiteCurve.eTangentEditMode.in_out);
            DrawToggleWithColoredRectangles(IntangetColor, OuttangetColor, "in = -out", tangentEditModeProp, (int) HermiteCurve.eTangentEditMode.in_neg_out);
            DrawToggleWithColoredRectangles(IntangetColor, OuttangetColor, "in = dir(out)", tangentEditModeProp, (int) HermiteCurve.eTangentEditMode.in_dir_out);
            DrawToggleWithColoredRectangles(IntangetColor, OuttangetColor, "in = -dir(out)", tangentEditModeProp, (int) HermiteCurve.eTangentEditMode.in_neg_dir_out);
            
            DrawToggleWithColoredRectangles(OuttangetColor, IntangetColor, "out = in", tangentEditModeProp, (int) HermiteCurve.eTangentEditMode.out_in);
            DrawToggleWithColoredRectangles(OuttangetColor, IntangetColor, "out = -in", tangentEditModeProp, (int) HermiteCurve.eTangentEditMode.out_neg_in);
            DrawToggleWithColoredRectangles(OuttangetColor, IntangetColor, "out = dir(in)", tangentEditModeProp, (int) HermiteCurve.eTangentEditMode.out_dir_in);
            DrawToggleWithColoredRectangles(OuttangetColor, IntangetColor, "out = -dir(in)", tangentEditModeProp, (int) HermiteCurve.eTangentEditMode.out_neg_dir_in);

            GUI.backgroundColor = defaultBkgColor;
            
            #region TANGENT CONTROL FILTER
            
            guiStyle.fontStyle = FontStyle.Bold;
            EditorGUILayout.LabelField("Tangent Control Filter", guiStyle);            
            
            GUILayout.BeginHorizontal();
            
            var editFilterProp = serializedObject.FindProperty("editFilter");
            
            if (GUILayout.Toggle(editFilterProp.enumValueIndex == (int) HermiteCurve.eEditFilter.selected, "selected", "button"))
                editFilterProp.enumValueIndex = (int) HermiteCurve.eEditFilter.selected;
            if (GUILayout.Toggle(editFilterProp.enumValueIndex == (int) HermiteCurve.eEditFilter.all, "all", "button"))
                editFilterProp.enumValueIndex = (int) HermiteCurve.eEditFilter.all;
            
            GUILayout.EndHorizontal();

            switch (curve.editFilter)
            {
                case HermiteCurve.eEditFilter.selected:
                    if (m_hotIndex < 0)
                        break;

                    ApplyEditMode(inTangents, outTangents, curve.tangentEditMode, m_hotIndex, curve);
                    break;
                case HermiteCurve.eEditFilter.all:
                    for (int i = 0; i < curve.points.Count; i++)
                    {
                        ApplyEditMode(inTangents, outTangents, curve.tangentEditMode, i, curve);
                    }

                    break;
            }
            
            #endregion

            #endregion
            
            #region TANGENT VIEW OPTION
            
            EditorGUILayout.LabelField("Tangent View Option", guiStyle);
            var tangentViewProp = serializedObject.FindProperty("tangentViewOptions");
            
            if (GUILayout.Toggle((tangentViewProp.intValue & (int) HermiteCurve.eTangentViewOptions.selectedPoint) != 0, "selected point", "button"))
                tangentViewProp.intValue |= (int) HermiteCurve.eTangentViewOptions.selectedPoint;
            else
                tangentViewProp.intValue &= ~((int) HermiteCurve.eTangentViewOptions.selectedPoint);
            
            if (GUILayout.Toggle((tangentViewProp.intValue & (int) HermiteCurve.eTangentViewOptions.closePoint) != 0 , "close to point", "button"))
                tangentViewProp.intValue |= (int) HermiteCurve.eTangentViewOptions.closePoint;
            else
                tangentViewProp.intValue &= ~((int) HermiteCurve.eTangentViewOptions.closePoint);
            
            if (GUILayout.Toggle((tangentViewProp.intValue & (int) HermiteCurve.eTangentViewOptions.selectedCurve) != 0, "selected curve", "button"))
                tangentViewProp.intValue |= (int) HermiteCurve.eTangentViewOptions.selectedCurve;
            else
                tangentViewProp.intValue &= ~((int) HermiteCurve.eTangentViewOptions.selectedCurve);            
            
            if (GUILayout.Toggle(tangentViewProp.intValue ==  (int) HermiteCurve.eTangentViewOptions.always, "always", "button"))
                tangentViewProp.intValue = (int) HermiteCurve.eTangentViewOptions.always;

            
            #endregion
            
            #region POINT FLATTENING OPTION
            
            var pointsProp = serializedObject.FindProperty("points");
            EditorGUILayout.LabelField("Point Flattening", guiStyle);
            if (GUILayout.Button("Flatten X"))
            {
                for (int i = 0; i < curve.points.Count; i++)
                {
                    var point = pointsProp.GetArrayElementAtIndex(i);
                    var localXPos = curve.transform.InverseTransformPoint(curve.transform.position).x;
                    var pointDif = localXPos - point.vector3Value.x;                    
                    point.vector3Value = new Vector3(point.vector3Value.x + pointDif, point.vector3Value.y, point.vector3Value.z);
                    
                    if (curve.tangentFlattenOptions == HermiteCurve.eTangentFlatteningOptions.Move ||
                        curve.tangentFlattenOptions == HermiteCurve.eTangentFlatteningOptions.Flatten)
                    {
                        if (i > 0)
                        {
                            var inTangent = inTangents.GetArrayElementAtIndex(i - 1);
                            if (curve.tangentFlattenOptions == HermiteCurve.eTangentFlatteningOptions.Move)
                                inTangent.vector3Value = new Vector3(inTangent.vector3Value.x + pointDif, inTangent.vector3Value.y, inTangent.vector3Value.z);
                            else
                                inTangent.vector3Value = new Vector3(localXPos, inTangent.vector3Value.y, inTangent.vector3Value.z);
                        }
                        if (i < curve.points.Count - 1)
                        {   
                            var outTangent = outTangents.GetArrayElementAtIndex(i);
                            if (curve.tangentFlattenOptions == HermiteCurve.eTangentFlatteningOptions.Move)
                                outTangent.vector3Value = new Vector3(outTangent.vector3Value.x + pointDif, outTangent.vector3Value.y, outTangent.vector3Value.z);
                            else
                                outTangent.vector3Value = new Vector3(localXPos, outTangent.vector3Value.y, outTangent.vector3Value.z);
                        }  
                    }                    
                }
            }
            if (GUILayout.Button("Flatten Y"))
            {
                for (int i = 0; i < curve.points.Count; i++)
                {
                    var point = pointsProp.GetArrayElementAtIndex(i);
                    var localYPos = curve.transform.InverseTransformPoint(curve.transform.position).y;
                    var pointDif = localYPos - point.vector3Value.y;                    
                    point.vector3Value = new Vector3(point.vector3Value.x , point.vector3Value.y+ pointDif, point.vector3Value.z);
                    
                    if (curve.tangentFlattenOptions == HermiteCurve.eTangentFlatteningOptions.Move ||
                        curve.tangentFlattenOptions == HermiteCurve.eTangentFlatteningOptions.Flatten)
                    {
                        if (i > 0)
                        {
                            var inTangent = inTangents.GetArrayElementAtIndex(i - 1);
                            if (curve.tangentFlattenOptions == HermiteCurve.eTangentFlatteningOptions.Move)
                                inTangent.vector3Value = new Vector3(inTangent.vector3Value.x, inTangent.vector3Value.y + pointDif, inTangent.vector3Value.z);
                            else
                                inTangent.vector3Value = new Vector3(inTangent.vector3Value.x, localYPos, inTangent.vector3Value.z);
                        }
                        if (i < curve.points.Count - 1)
                        {   
                            var outTangent = outTangents.GetArrayElementAtIndex(i);
                            if (curve.tangentFlattenOptions == HermiteCurve.eTangentFlatteningOptions.Move)
                                outTangent.vector3Value = new Vector3(outTangent.vector3Value.x, outTangent.vector3Value.y + pointDif, outTangent.vector3Value.z);
                            else
                                outTangent.vector3Value = new Vector3(outTangent.vector3Value.x, localYPos, outTangent.vector3Value.z);
                        }  
                    }
                }                
            }
            if (GUILayout.Button("Flatten Z"))
            {
                for (int i = 0; i < curve.points.Count; i++)
                {
                    var point = pointsProp.GetArrayElementAtIndex(i);
                    var localZPos = curve.transform.InverseTransformPoint(curve.transform.position).z;
                    var pointDif = localZPos - point.vector3Value.z;
                    point.vector3Value = new Vector3(point.vector3Value.x , point.vector3Value.y, point.vector3Value.z+ pointDif);

                    if (curve.tangentFlattenOptions == HermiteCurve.eTangentFlatteningOptions.Move ||
                        curve.tangentFlattenOptions == HermiteCurve.eTangentFlatteningOptions.Flatten)
                    {
                        if (i > 0)
                        {
                            var inTangent = inTangents.GetArrayElementAtIndex(i - 1);
                            if (curve.tangentFlattenOptions == HermiteCurve.eTangentFlatteningOptions.Move)
                                inTangent.vector3Value = new Vector3(inTangent.vector3Value.x , inTangent.vector3Value.y, inTangent.vector3Value.z+ pointDif);
                            else
                                inTangent.vector3Value = new Vector3(inTangent.vector3Value.x , inTangent.vector3Value.y, localZPos);
                        }
                        if (i < curve.points.Count - 1)
                        {   
                            var outTangent = outTangents.GetArrayElementAtIndex(i);
                            if (curve.tangentFlattenOptions == HermiteCurve.eTangentFlatteningOptions.Move)
                                outTangent.vector3Value = new Vector3(outTangent.vector3Value.x , outTangent.vector3Value.y, outTangent.vector3Value.z+ pointDif);
                            else
                                outTangent.vector3Value = new Vector3(outTangent.vector3Value.x , outTangent.vector3Value.y, localZPos);
                        }                        
                    }
                }                
            }
            
            var tangentFlattenOption = serializedObject.FindProperty("tangentFlattenOptions");
            
            EditorGUILayout.LabelField("Tangent Flattening with points.", guiStyle);            
            
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Toggle(tangentFlattenOption.enumValueIndex == (int) HermiteCurve.eTangentFlatteningOptions.Keep, "Keep","button"))
            {
                tangentFlattenOption.enumValueIndex = (int) HermiteCurve.eTangentFlatteningOptions.Keep;
            }
            if (GUILayout.Toggle(tangentFlattenOption.enumValueIndex == (int) HermiteCurve.eTangentFlatteningOptions.Move, "Move","button"))
            {
                tangentFlattenOption.enumValueIndex = (int) HermiteCurve.eTangentFlatteningOptions.Move;
            }
            if (GUILayout.Toggle(tangentFlattenOption.enumValueIndex == (int) HermiteCurve.eTangentFlatteningOptions.Flatten, "Flatten","button"))
            {
                tangentFlattenOption.enumValueIndex = (int) HermiteCurve.eTangentFlatteningOptions.Flatten;
            }            
            
            GUILayout.EndHorizontal();
            
            #endregion

            #region CURVE INFO

            m_ShowCurveInfo = EditorGUILayout.Foldout(m_ShowCurveInfo, "Curve Info");
            if (m_ShowCurveInfo)
            {
                string curveInfo = "";
                int maxVectorLength = 30; 
                
                for (int i = 0; i < curve.points.Count; i++)
                {
                    var padding = new String(' ', maxVectorLength - curve.points[i].ToString().Length); 
                    curveInfo += $"Point {i} pos:          local - {curve.points[i]}{padding}world - {curve.transform.TransformPoint(curve.points[i])}\n";
                    if (i > 0)
                    {
                        padding = new String(' ', maxVectorLength - curve.inTangents[i - 1].ToString().Length);
                        curveInfo += $"<color=#{ColorUtility.ToHtmlStringRGB(IntangetColor)}><b>InTangent</b></color> pos:   local - {curve.inTangents[i - 1]}{padding}" + 
                                     $"world - {curve.transform.TransformPoint(curve.inTangents[i - 1])}\n";
                    }
                    if (i < curve.points.Count - 1)
                    {
                        padding = new String(' ', maxVectorLength - curve.outTangents[i].ToString().Length);
                        curveInfo += $"<color=#{ColorUtility.ToHtmlStringRGB(OuttangetColor)}><b>OutTangent</b></color> pos: local - {curve.outTangents[i]}{padding}" + 
                                     $"world - {curve.transform.TransformPoint(curve.outTangents[i])}\n";

                        curveInfo += $"Length of segment between {i} and {i + 1} is {curve.Hermite.ArcLength(curve.times[i], curve.times[i + 1])}\n";

                        curveInfo += Environment.NewLine;
                    }
                }

                curveInfo += $"Length of whole curve is {curve.Hermite.ArcLength(0f, 1f)}";
                
                var style = new GUIStyle();
                style.richText = true;
                var bckgText = new Texture2D(1,1);
                bckgText.SetPixel(0,0, Color.white);
                style.normal.background = bckgText; 
                style.margin = new RectOffset(15,5,5,5);
                //style.border = new RectOffset(10,10,10,10);
                style.padding = new RectOffset(10,10,10,10);
                    
                GUILayout.Label(curveInfo, style);
                
                //EditorGUILayout.HelpBox(curveInfo, MessageType.None);
            }

            #endregion

            #region MOVE ON CURVE

            m_MoveOnCurve = EditorGUILayout.Foldout(m_MoveOnCurve, "Move on curve");
            if (m_MoveOnCurve)
            {
                var moveObject = serializedObject.FindProperty("moveObject");
                EditorGUILayout.ObjectField(moveObject, typeof(GameObject));
                if (moveObject.objectReferenceValue != null)
                {
                    var moveObjectPos = serializedObject.FindProperty("moveObjectNormPos");
                    moveObjectPos.floatValue = GUILayout.HorizontalSlider(moveObjectPos.floatValue, 0f, 1f);

                    GUILayout.BeginHorizontal();
                    var shouldMove = GUILayout.Toggle(m_MoveOnCurveCR != null, "Play", "button");
                    if (shouldMove && m_MoveOnCurveCR == null)
                        EditorCoroutineUtility.StartCoroutine(MoveOnTheCurve(), this);
                    else if (!shouldMove && m_MoveOnCurveCR != null)
                    {
                        EditorCoroutineUtility.StopCoroutine(m_MoveOnCurveCR);
                        m_MoveOnCurveCR = null;
                    }
                    
                    m_MoveOnCurveSpeed = EditorGUILayout.FloatField("Speed", m_MoveOnCurveSpeed);
                    GUILayout.EndHorizontal();
                    
                    var go = moveObject.objectReferenceValue as GameObject;
                    go.transform.position = curve.GetNonUniformPoint(moveObjectPos.floatValue);
                }
            }

            #endregion
            
            curve.ValidateData();
            serializedObject.ApplyModifiedProperties();
            SceneView.RepaintAll();
        }

        private IEnumerator MoveOnTheCurve()
        {
            while (true)
            {
                var moveObjectPos = serializedObject.FindProperty("moveObjectNormPos");
                moveObjectPos.floatValue = Mathf.Repeat(moveObjectPos.floatValue + 0.01f * m_MoveOnCurveSpeed, 1f);
                yield return null;
            }
        }
        
        private void SetNewTangentPos(SerializedProperty tangent, Vector3 newPos, HermiteCurve curve, int pointIdx, bool applyFilter = true)
        {
            if (applyFilter && curve.tangentLengthControlApply
                    && (curve.editFilter == HermiteCurve.eEditFilter.all ||
                        (curve.editFilter == HermiteCurve.eEditFilter.selected))
                )
            {
                var point = curve.points[pointIdx];
                tangent.vector3Value = (newPos - point).normalized *  curve.tangentLengthControlLength + point;
            }   
            else if (!applyFilter || !curve.tangentLengthControlApply)
            {
                tangent.vector3Value = newPos;
            }
        }

        private void DrawToggleWithColoredRectangles(Color rect1Col, Color rect2Col, string toggleText, SerializedProperty showedProperty, int enumValue)
        {
            GUILayout.BeginHorizontal();
            var rect = EditorGUILayout.GetControlRect(GUILayout.Width(15));
            rect.width = 15;
            rect.height = 15;
            EditorGUI.DrawRect(rect, rect1Col);
            if (GUILayout.Toggle(showedProperty.enumValueIndex == enumValue, toggleText, "button", GUILayout.ExpandWidth(true)))
                showedProperty.enumValueIndex = enumValue;
            
            rect = EditorGUILayout.GetControlRect(GUILayout.Width(15));
            rect.width = 15;
            rect.height = 15;
            EditorGUI.DrawRect(rect, rect2Col);
            GUILayout.EndHorizontal();
        }

        private void ApplyEditMode(SerializedProperty inTangents, SerializedProperty outTangents, 
            HermiteCurve.eTangentEditMode tangentEditMode, int pointIndex, HermiteCurve curve)
        {
            SerializedProperty ot = null;   
            SerializedProperty it = null;
            
            if (pointIndex < curve.points.Count - 1)
                ot = outTangents.GetArrayElementAtIndex(pointIndex);
            if (pointIndex > 0)
                it = inTangents.GetArrayElementAtIndex(pointIndex - 1);

            if (ot == null || it == null)
                return;

            var point = curve.points[pointIndex];
            var inPos = it.vector3Value;
            var outPos = ot.vector3Value;
            Vector3 newIntangPos = Vector3.zero;
            Vector3 newOuttangPos = Vector3.zero;
            
            switch (tangentEditMode)
            {
                case HermiteCurve.eTangentEditMode.in_out:
                    newIntangPos = outPos;
                    break;
                case HermiteCurve.eTangentEditMode.in_neg_out:
                    newIntangPos = point -(outPos - point);
                    break;
                case HermiteCurve.eTangentEditMode.in_dir_out:
                    if (Vector3.Dot(inPos - point, outPos - point) < 0)
                    {
                        newIntangPos = point + (outPos - point) * 0.1f;
                    }
                    else
                    {
                        newIntangPos = point + Vector3.Project(inPos - point, outPos - point);
                    }
                    break;
                case HermiteCurve.eTangentEditMode.in_neg_dir_out:
                    if (Vector3.Dot(inPos - point, outPos - point) > 0)
                    {
                        newIntangPos = point + (outPos - point) * -0.1f;
                    }
                    else
                    {
                        newIntangPos = point + Vector3.Project(inPos - point, -(outPos - point));
                    }
                    break;
                case HermiteCurve.eTangentEditMode.out_in:
                    newOuttangPos = inPos;
                    break;
                case HermiteCurve.eTangentEditMode.out_neg_in:
                    newOuttangPos = point-(inPos - point);
                    break;
                case HermiteCurve.eTangentEditMode.out_dir_in:
                    if (Vector3.Dot(outPos- point, inPos- point) < 0)
                    {
                        newOuttangPos = point + (inPos - point) * 0.1f;
                    }
                    else
                    {
                        newOuttangPos = point + Vector3.Project(outPos - point, inPos - point);
                    }
                    break;
                case HermiteCurve.eTangentEditMode.out_neg_dir_in:
                    if (Vector3.Dot(outPos- point, inPos- point) > 0)
                    {
                        newOuttangPos = point + (inPos - point) * -0.1f;
                    }
                    else
                    {
                        newOuttangPos = point + Vector3.Project(outPos - point, -(inPos - point));
                    }
                    break;                    
            }

            if (newIntangPos != Vector3.zero)
            {
                SetNewTangentPos(it, newIntangPos, curve, pointIndex);
            }
            if (newOuttangPos != Vector3.zero)
            {
                SetNewTangentPos(ot, newOuttangPos, curve, pointIndex);
            }            
        }

        protected virtual void OnSceneGUI()
        {            
            if (Event.current != null &&
                Event.current.type == EventType.KeyDown &&
                Event.current.keyCode == KeyCode.Tab)
            {
               Event.current.Use();
            }            
            
            var curve = target as HermiteCurve;
            
            DrawCurve(curve, 1024, Color.white, true);            
            
            var points = serializedObject.FindProperty("points");

            var e = Event.current;
            GUIUtility.GetControlID(FocusType.Passive);
            
            var mousePos = (Vector2) Event.current.mousePosition;
            var view = SceneView.currentDrawingSceneView.camera.ScreenToViewportPoint(Event.current.mousePosition);
            var mouseIsOutside = view.x < 0 || view.x > 1 || view.y < 0 || view.y > 1;
            if (mouseIsOutside)
            {
                if (curve.tangentViewOptions == (int) HermiteCurve.eTangentViewOptions.always)
                {
                    for (int pointIdx = 0; pointIdx < curve.points.Count; pointIdx++)
                    {
                        DrawTangents(pointIdx, curve, true);    
                    }
                }
                else if (m_hotIndex != -1)
                {
                    DrawTangents(m_hotIndex, curve, true);
                }

                if (curve.curveMode == HermiteCurve.eCurveMode.Times)
                {
                    for (int pointIdx = 0; pointIdx < curve.points.Count; pointIdx++)
                    {
                        var prop = points.GetArrayElementAtIndex(pointIdx);
                        var point = prop.vector3Value;
                        var wp = curve.transform.TransformPoint(point);
                        
                        DrawTimeControl(curve, pointIdx, wp);    
                    }
                }
                
                return;
            }
                
            var outTangents = serializedObject.FindProperty("outTangents");
            var inTangents = serializedObject.FindProperty("inTangents");
            
            // in case of closed curve we don't draw the last point we just make sure we move it and it's intangent in case
            // first point is selected
            for (int i = 0; i < (curve.closed ? curve.points.Count - 1 : curve.points.Count); i++)
            {
                var prop = points.GetArrayElementAtIndex(i);
                var point = prop.vector3Value;
                var wp = curve.transform.TransformPoint(point);

                bool mouseIsCloseToPoint = false;
                var pointScreenPoint = SceneView.currentDrawingSceneView.camera.WorldToScreenPoint(wp);
                pointScreenPoint.y = Screen.height - pointScreenPoint.y - screenHeightOffset;
                if (Vector2.Distance(pointScreenPoint, mousePos) < 20)
                    mouseIsCloseToPoint = true;
                
                // test to see where the actual point in screenspace is
//                Handles.BeginGUI();
//                GUILayout.BeginArea(new Rect(pointScreenPoint, new Vector2(100, 100)));
//                GUILayout.Label(testText);
//                GUILayout.EndArea();
//                Handles.EndGUI();

                if (curve.curveMode == HermiteCurve.eCurveMode.PointsAndTangents)
                {
                    if (m_hotIndex == i)
                    {
                        var newWp = Handles.PositionHandle(wp, Tools.pivotRotation == PivotRotation.Global ? Quaternion.identity : curve.transform.rotation);
                        var delta = curve.transform.InverseTransformDirection(newWp - wp);
                        if (delta.sqrMagnitude > 0)
                        {
                            prop.vector3Value = point + delta;
                            if (curve.closed && i == 0)
                            { // adjust also last point and last intangent
                                var lastPoint = points.GetArrayElementAtIndex(curve.points.Count - 1);
                                lastPoint.vector3Value = point + delta;

                                var lastInTangent = inTangents.GetArrayElementAtIndex(curve.inTangents.Count - 1);
                                SetNewTangentPos(lastInTangent, lastInTangent.vector3Value + delta, curve, i);
                            }
                        
                            // adjust also tangent position, so the curvature is kept
                            if (m_hotIndex > 0)
                            {
                                var inTangent = inTangents.GetArrayElementAtIndex(m_hotIndex - 1);
                                SetNewTangentPos(inTangent, inTangent.vector3Value + delta, curve, i);                                
                            }
                            if (m_hotIndex < curve.points.Count - 1)
                            {   
                                var outTangent = outTangents.GetArrayElementAtIndex(m_hotIndex);
                                SetNewTangentPos(outTangent, outTangent.vector3Value + delta, curve, i);                                
                            }
                        
                            curve.ResetIndex();
                        }
                        HandleCommands(wp);
                    }                    
                }
                if (curve.closed)
                    Handles.color = Color.white;
                else
                    Handles.color = i == 0 | i == curve.points.Count - 1 ? Color.red : Color.white;

                var buttonSize = HandleUtility.GetHandleSize(wp) * 0.1f;
                if (Handles.Button(wp, Quaternion.identity, buttonSize, buttonSize, Handles.SphereHandleCap))
                {
                    SetHotIndex(i);
                }

                if (curve.curveMode == HermiteCurve.eCurveMode.Times)
                    DrawTimeControl(curve, i, wp);
                
                // if user is close to handle or the handle is selected, draw the tangents
                if ((m_hotIndex == i && (curve.tangentViewOptions & (int) HermiteCurve.eTangentViewOptions.selectedPoint) != 0) || 
                    (mouseIsCloseToPoint && (curve.tangentViewOptions & (int) HermiteCurve.eTangentViewOptions.closePoint) != 0) ||
                    ((curve.tangentViewOptions & (int) HermiteCurve.eTangentViewOptions.selectedCurve) != 0))
                {
                    if (curve.curveMode == HermiteCurve.eCurveMode.PointsAndTangents)
                    {
                        if (curve.closed && i == 0)
                            DrawTangents(curve.points.Count - 1, curve);
                        
//                        if (curve.tangentLengthControlApply && (curve.tangentViewOptions & (int) HermiteCurve.eTangentViewOptions.selectedCurve) != 0)
//                        {
//                            if (i > 0)
//                            {
//                                var inTangent = inTangents.GetArrayElementAtIndex(i - 1);
//                                SetNewTangentPos(inTangent, inTangent.vector3Value, curve, i);                                
//                            }
//                            if (i < curve.points.Count - 1)
//                            {   
//                                var outTangent = outTangents.GetArrayElementAtIndex(i);
//                                SetNewTangentPos(outTangent, outTangent.vector3Value, curve, i);                                
//                            }
//                        }
                        
                        DrawTangents(i, curve);
                    }
                    
                    if (m_hotIndex >= 0)
                        ApplyEditMode(inTangents, outTangents, curve.tangentEditMode, m_hotIndex, curve);
                    if ((curve.tangentViewOptions & (int) HermiteCurve.eTangentViewOptions.selectedCurve) != 0 && i > 0 && i != m_hotIndex)
                        ApplyEditMode(inTangents, outTangents, curve.tangentEditMode, i, curve);
                }
                
                var v = SceneView.currentDrawingSceneView.camera.transform.InverseTransformPoint(wp);
                var labelIsOutside = v.z < 0;
                if (!labelIsOutside)
                {
                    var pointStyle = new GUIStyle();
                    pointStyle.fontSize = 13;
                    pointStyle.fontStyle = FontStyle.Bold;
                    Handles.Label(wp, i.ToString(), pointStyle);
                }
            }
            
            if (Event.current.shift)
            {
                if (curve.closed)
                    ShowClosestPointOnClosedCurve(points);
                else
                    ShowClosestPointOnOpenCurve(points);
            }
            
            if (m_removeIndex >= 0 && points.arraySize > 4)
            {
                if (curve.closed && m_removeIndex == 0)
                {
                    var lastPoint = points.GetArrayElementAtIndex(curve.points.Count - 1);
                    lastPoint.vector3Value = curve.points[1];            
                    
                    var lastInTangent = inTangents.GetArrayElementAtIndex(curve.inTangents.Count - 1);
                    SetNewTangentPos(lastInTangent, curve.outTangents[1], curve, 1);
                }                
                
                points.DeleteArrayElementAtIndex(m_removeIndex);
                curve.ResetIndex();
            }
            
            m_removeIndex = -1;
            serializedObject.ApplyModifiedProperties();
            curve.ValidateData();
        }

        void DrawTimeControl(HermiteCurve curve, int pointIndex, Vector3 pointWorldPos)
        {
            var times = serializedObject.FindProperty("times");
            DrawUniformPointsOnCurve(curve, 100);
            
            Handles.BeginGUI();

            var timesElemScreenPos = SceneView.currentDrawingSceneView.camera.WorldToScreenPoint(pointWorldPos);
            timesElemScreenPos.y = Screen.height - timesElemScreenPos.y;

            if (pointIndex == 0 || pointIndex == curve.points.Count - 1)
            {
                timesElemScreenPos.y -= 50;
                GUILayout.BeginArea(new Rect(timesElemScreenPos, new Vector2(50, 30)));
                GUILayout.Label(curve.times[pointIndex].ToString());
                GUILayout.EndArea();
            }
            else
            {
                var timesTextAreaHeight = 25;
                var timesTextAreaOffset = 35;
                timesElemScreenPos.y +=  -timesTextAreaHeight - timesTextAreaOffset;  
                
                var areaStyle = new GUIStyle();
                areaStyle.richText = true;
                var bckgText = new Texture2D(1,1);
                bckgText.SetPixel(0,0, Color.white);
                areaStyle.normal.background = bckgText;                
                
                GUILayout.BeginArea(new Rect(timesElemScreenPos, new Vector2(100, timesTextAreaHeight)), areaStyle);
                GUILayout.BeginHorizontal();
                
                var incTime = GUILayout.Button("+");
                
                GUILayout.Label("0.");

                var timeProperty = times.GetArrayElementAtIndex(pointIndex);

                var timeStyle = new GUIStyle();
                timeStyle.fixedWidth = 20;  
                var bckgTimeStyle = new Texture2D(1,1);
                bckgTimeStyle.SetPixel(0,0, Color.red);
                timeStyle.normal.background = bckgTimeStyle;    
                timeStyle.margin = new RectOffset(0,10, 5,0);
                
                var newTimeStr = GUILayout.TextField(curve.times[pointIndex].ToString().Substring(2), 3,timeStyle);
                var decTime = GUILayout.Button("-");
                GUILayout.EndHorizontal();
                double newTime = 0f;
                if (!Double.TryParse("0." + newTimeStr, out newTime))
                {
                    if (!Double.TryParse("0," + newTimeStr, out newTime))
                        newTime = curve.times[pointIndex];
                }

                if (incTime)
                    newTime += 0.1f;
                if (decTime)
                    newTime -= 0.1f;

                if (newTime != curve.times[pointIndex] &&
                    newTime > curve.times[pointIndex - 1] && newTime < curve.times[pointIndex + 1])
                {
                    timeProperty.floatValue = (float) newTime;
                }
            
                GUILayout.EndArea();                    
            }
            
            Handles.EndGUI();            
        }
        
        void DrawUniformPointsOnCurve(HermiteCurve curve, int resolution)
        {
            float step = 1.0f / resolution;
            
            for (int i = 0; i < resolution; i++)
            {
                var worldPoint = curve.GetNonUniformPoint(i * step);
                Handles.color = new Color(0f, 0f, 1f, 0.5f);
                Handles.DrawSolidDisc(worldPoint, SceneView.currentDrawingSceneView.camera.transform.position - worldPoint, curve.timeEditPointSize);
            }
        }

        void SetHotIndex(int newHotIndex)
        {
            m_hotIndex = newHotIndex;
            m_inTangentSelected = false;
            m_outTangentSelected = false;

            Repaint();
        }

        void DrawTangents(int pointIndex, HermiteCurve curve, bool simple = false)
        {
            if (pointIndex > 0)
            {
                // draw intangent
                var inTangent = curve.inTangents[pointIndex - 1];
                var inTangentWP = curve.transform.TransformPoint(inTangent);
                var pointWP = curve.transform.TransformPoint(curve.points[pointIndex]);

                Handles.color = IntangetColor;
                Handles.DrawLine(pointWP, inTangentWP);

                if (!simple)
                {
                    var buttonSize = HandleUtility.GetHandleSize(inTangentWP) * 0.1f;
                    Handles.color = Color.white;
                    if (Handles.Button(inTangentWP, Quaternion.identity, buttonSize, buttonSize, Handles.CubeHandleCap))
                    {
                        m_hotIndex = pointIndex;
                        m_inTangentSelected = true;
                    }

                    if (m_inTangentSelected)
                    {
                        var newWp = Handles.PositionHandle(inTangentWP,
                            Tools.pivotRotation == PivotRotation.Global ? Quaternion.identity : curve.transform.rotation);
                        if (Vector3.SqrMagnitude(newWp - inTangentWP) > 0)
                        {
                            var inTangents = serializedObject.FindProperty("inTangents");
                            var tangent = inTangents.GetArrayElementAtIndex(pointIndex - 1);
                            SetNewTangentPos(tangent, curve.transform.InverseTransformPoint(newWp), curve, pointIndex);
                            m_lastMoveInTangent = true;
                        }
                    }
                }
            }

            if (pointIndex < curve.points.Count - 1)
            {
                // draw outtangent
                var outTangent = curve.outTangents[pointIndex];
                var outTangentWP = curve.transform.TransformPoint(outTangent);
                var pointWP = curve.transform.TransformPoint(curve.points[pointIndex]);

                Handles.color = OuttangetColor;
                Handles.DrawLine(pointWP, outTangentWP);

                if (!simple)
                {
                    var buttonSize = HandleUtility.GetHandleSize(outTangentWP) * 0.1f;
                    Handles.color = Color.white;
                    if (Handles.Button(outTangentWP, Quaternion.identity, buttonSize, buttonSize,
                        Handles.CubeHandleCap))
                    {
                        m_hotIndex = pointIndex;
                        m_outTangentSelected = true;
                    }

                    if (m_outTangentSelected)
                    {
                        var newWp = Handles.PositionHandle(outTangentWP,
                            Tools.pivotRotation == PivotRotation.Global ? Quaternion.identity : curve.transform.rotation);
                        if (Vector3.SqrMagnitude(newWp - outTangentWP) > 0)
                        {
                            var outTangents = serializedObject.FindProperty("outTangents");
                            var tangent = outTangents.GetArrayElementAtIndex(pointIndex);
                        
                            SetNewTangentPos(tangent, curve.transform.InverseTransformPoint(newWp), curve, pointIndex);        
                            m_lastMoveInTangent = false;
                        }
                    }                    
                }
            }
        }

        void HandleCommands(Vector3 wp)
        {
            if (Event.current.type == EventType.ExecuteCommand)
            {
                if (Event.current.commandName == "FrameSelected")
                {
                    SceneView.currentDrawingSceneView.Frame(new Bounds(wp, Vector3.one * 10), false);
                    Event.current.Use();
                }
            }
            if (Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.Backspace)
                {
                    m_removeIndex = m_hotIndex;
                    Event.current.Use();
                }
            }
        }        
        
        static void DrawCurve(HermiteCurve curve, int stepCount, Color color, bool useHandles)
        {
            if (curve.points.Count > 0)
            {
                var P = 0f;
                var start = curve.GetNonUniformPoint(0);
                var step = 1f / stepCount;
                do
                {
                    P += step;
                    var here = curve.GetNonUniformPoint(P);

                    if (useHandles)
                    {
                        Handles.color = color;
                        Handles.DrawLine(start, here);
                    }
                    else
                    {
                        Gizmos.color = color;
                        Gizmos.DrawLine(start, here);
                    }
                    
                    start = here;
                } while (P + step <= 1);
            }
        }
        
        [DrawGizmo(GizmoType.NonSelected)]
        static void DrawGizmosLoRes(HermiteCurve herm, GizmoType gizmoType)
        {
            DrawCurve(herm, 64, Color.white, false);
            //Debug.Log("[DrawGizmo(GizmoType.NonSelected)] finally works!");
        }
    
        [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.Selected | GizmoType.Active | GizmoType.Pickable | GizmoType.NotInSelectionHierarchy)]
        static void DrawGizmosHiRes(HermiteCurve herm, GizmoType gizmoType)
        {
            Debug.Log("[DrawGizmo(GizmoType.Selected)] finally works!");
            //DrawCurve(herm, 1024, Color.white, false);
        }
        
        void ShowClosestPointOnClosedCurve(SerializedProperty points)
        {
            var curve = target as HermiteCurve;
            var plane = new Plane(curve.transform.up, curve.transform.position);
            var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            float center;
            if (plane.Raycast(ray, out center))
            {
                var hit = ray.origin + ray.direction * center;
                Handles.DrawWireDisc(hit, curve.transform.up, 5);
                var p = SearchForClosestPoint(Event.current.mousePosition);
                var sp = curve.GetNonUniformPoint(p);
                Handles.DrawLine(hit, sp);

                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.shift)
                {
                    int newIdx = 0;
                    for (; newIdx < curve.times.Count; newIdx++)
                    {
                        if (curve.times[newIdx] > p)
                            break;
                    }
                    
                    points.InsertArrayElementAtIndex(newIdx);
                    curve.ValidateData();
                    points.GetArrayElementAtIndex(newIdx).vector3Value = curve.transform.InverseTransformPoint(sp);
                    serializedObject.ApplyModifiedProperties();
                    SetHotIndex(newIdx);
                }
            }
        }

        void ShowClosestPointOnOpenCurve(SerializedProperty points)
        {
            var curve = target as HermiteCurve;
            var plane = new Plane(curve.transform.up, curve.transform.position);
            var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            float center;
            if (plane.Raycast(ray, out center))
            {
                var hit = ray.origin + ray.direction * center;
                var discSize = HandleUtility.GetHandleSize(hit);
                Handles.DrawWireDisc(hit, curve.transform.up, discSize);
                var p = SearchForClosestPoint(Event.current.mousePosition);
                
                if ((hit - curve.GetNonUniformPoint(0)).sqrMagnitude < 25) p = 0;
                if ((hit - curve.GetNonUniformPoint(1)).sqrMagnitude < 25) p = 1;
    
                var sp = curve.GetNonUniformPoint(p);
    
                var extend = Mathf.Approximately(p, 0) || Mathf.Approximately(p, 1);
    
                Handles.color = extend ? Color.red : Color.white;
                Handles.DrawLine(hit, sp);
                Handles.color = Color.white;

                int newIdx = 0;
                for (; newIdx < curve.times.Count; newIdx++)
                {
                    if (curve.times[newIdx] > p)
                        break;
                }

                if (extend && newIdx == 1)
                    newIdx = 0;
    
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.shift)
                {
                    if (extend)
                    {
                        points.InsertArrayElementAtIndex(newIdx);
                        points.GetArrayElementAtIndex(newIdx).vector3Value = curve.transform.InverseTransformPoint(hit);
                        if (newIdx == curve.points.Count - 1)
                            points.MoveArrayElement(newIdx, newIdx + 1);
                        SetHotIndex(newIdx);
                    }
                    else
                    {
                        points.InsertArrayElementAtIndex(newIdx);
                        points.GetArrayElementAtIndex(newIdx).vector3Value = curve.transform.InverseTransformPoint(sp);
                        SetHotIndex(newIdx);
                    }
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }
        
        float SearchForClosestPoint(Vector2 screenPoint, float A = 0f, float B = 1f, float steps = 1000)
        {
            var curve = target as HermiteCurve;
            var smallestDelta = float.MaxValue;
            var step = (B - A) / steps;
            var closestI = A;
            for (var i = 0; i <= steps; i++)
            {
                var p = curve.GetNonUniformPoint(i * step);
                var gp = HandleUtility.WorldToGUIPoint(p);
                var delta = (screenPoint - gp).sqrMagnitude;
                if (delta < smallestDelta)
                {
                    closestI = i;
                    smallestDelta = delta;
                }
            }
            return closestI * step;
        }
    }
}