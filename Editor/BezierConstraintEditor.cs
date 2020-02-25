
using SheepDev.Bezier;
using UnityEditor;
using UnityEngine;

namespace SheepDev.EditorBezier.Utility
{
  [CustomEditor(typeof(BezierConstraint), true)]
  public class BezierConstraintEditor : Editor
  {
    private BezierConstraint script;

    private void OnEnable()
    {
      script = target as BezierConstraint;
    }

    public override void OnInspectorGUI()
    {
      TargetGUI();
      SnapGUI();

      if (serializedObject.hasModifiedProperties)
      {
        script.OnValidate();
        serializedObject.ApplyModifiedProperties();
      }

      ActionGUI();
    }

    private void TargetGUI()
    {
      var property = serializedObject.FindProperty("curve");
      EditorGUILayout.PropertyField(property);

      EditorGUI.BeginDisabledGroup(!script.HasCurve);
      var max = script.HasCurve ? script.Curve.PointLenght - 1 : 0;
      property = serializedObject.FindProperty("targetIndex");
      EditorGUILayout.IntSlider(property, 0, max);
      EditorGUI.EndDisabledGroup();
    }

    private void SnapGUI()
    {
      var property = serializedObject.FindProperty("isSnapOnEnable");
      EditorGUILayout.PropertyField(property);
      property = serializedObject.FindProperty("snapEnable");
      EditorGUILayout.PropertyField(property);
      property = serializedObject.FindProperty("snapUpdate");
      EditorGUILayout.PropertyField(property);
    }

    private void ActionGUI()
    {
      EditorGUILayout.Space();

      EditorGUI.BeginDisabledGroup(!script.HasCurve);
      EditorGUILayout.BeginHorizontal();

      if (GUILayout.Button("Point to Constraint"))
      {
        Undo.RecordObject(script.Curve, "Snap point in constraint");
        script.PointToConstraint();
      }

      if (GUILayout.Button("Constraint to Point"))
      {
        Undo.RecordObject(script.transform, "Snap constraint in point");
        script.ConstraintToPoint();
      }

      EditorGUILayout.EndHorizontal();
      EditorGUI.EndDisabledGroup();
    }

    [DrawGizmo(GizmoType.InSelectionHierarchy)]
    static void DrawGizmoSelect(BezierConstraint script, GizmoType gizmoType)
    {
      Gizmo(script, true);
    }

    [DrawGizmo(GizmoType.NotInSelectionHierarchy)]
    static void DrawGizmoNotSelect(BezierConstraint script, GizmoType gizmoType)
    {
      Gizmo(script, false);
    }

    static void Gizmo(BezierConstraint script, bool isSelect)
    {
      var transform = script.GetTransform();
      var point = script.TargetPoint;

      Handles.color = isSelect ? Color.green : Color.yellow;
      Handles.DrawDottedLine(transform.position, point.position, 10);

      if (!isSelect) return;
      Handles.BeginGUI();
      var halfDirection = (point.position - transform.position) / 2f;
      var upCameraDirection = (SceneView.lastActiveSceneView.rotation * Vector3.up) * 1;
      Handles.Label(transform.position + halfDirection + upCameraDirection, script.TargetIndex.ToString());
      Handles.EndGUI();
    }
  }
}