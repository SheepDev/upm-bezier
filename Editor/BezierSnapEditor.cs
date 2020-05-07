using SheepDev.Bezier.Utility;
using SheepDev.EditorBezier.Utility;
using UnityEditor;

namespace SheepDev.Bezier
{
  [CanEditMultipleObjects]
  [CustomEditor(typeof(BezierSnap), true)]
  public class BezierSnapEditor : Editor
  {
    private static GizmoDataEditor gizmoData;

    private BezierSnap script;
    private BezierRotationEditor rotationEditor;
    private BezierPositionEditor positionEditor;

    static BezierSnapEditor()
    {
      gizmoData = new GizmoDataEditor();
    }

    public BezierSnapEditor()
    {
      rotationEditor = new BezierRotationEditor();
      positionEditor = new BezierPositionEditor();
    }

    private void OnEnable()
    {
      script = target as BezierSnap;
    }

    private void OnDisable()
    {
      Tools.hidden = false;
    }

    private void OnSceneGUI()
    {
      var isEnable = script.isActiveAndEnabled;
      Tools.hidden = isEnable && Tools.current != Tool.Scale;

      if (!gizmoData.isShow) return;

      var gizmoScale = gizmoData.scale;
      var targetPosition = script.GetSnapPosition();
      positionEditor.PositionSceneGUI(targetPosition, gizmoScale);

      bool hasRotation = script.GetSnapRotation(out var targetRotation);
      if (!hasRotation) return;

      if (script.rotation.setting == RotateSetting.Upward)
      {
        var upward = script.rotation.GetUpward(script.GetTransform());
        rotationEditor.RotationUpwardSceneGUI(targetPosition, targetRotation, upward, gizmoScale);
      }
      else
      {
        rotationEditor.RotationSceneGUI(targetPosition, targetRotation, gizmoScale);
      }
    }

    public override void OnInspectorGUI()
    {
      var isValid = CurveProperty();

      EditorGUI.BeginDisabledGroup(!isValid);
      EditorGUILayout.Space();
      EditorGUILayout.PropertyField(serializedObject.FindProperty("isBackward"));
      EditorGUILayout.Space();
      positionEditor.PositionSettingProperty(serializedObject.FindProperty("position"));
      EditorGUILayout.Space();
      rotationEditor.RotationSettingProperty(serializedObject.FindProperty("rotation"));
      EditorGUI.EndDisabledGroup();

      if (serializedObject.hasModifiedProperties)
      {
        script.OnValidate();
        serializedObject.ApplyModifiedProperties();
      }

      gizmoData.Inspector();
    }

    private bool CurveProperty()
    {
      var curveProperty = serializedObject.FindProperty("curve");
      EditorGUILayout.PropertyField(curveProperty);
      var isValid = curveProperty.objectReferenceValue != null;

      if (!isValid)
      {
        EditorGUILayout.HelpBox("Need bezier for working correct!", MessageType.Warning);
      }

      return isValid;
    }
  }
}