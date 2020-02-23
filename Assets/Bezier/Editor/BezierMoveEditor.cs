using SheepDev.Bezier.Utility;
using SheepDev.EditorBezier.Utility;
using UnityEditor;

namespace SheepDev.Bezier
{
  [CanEditMultipleObjects]
  [CustomEditor(typeof(BezierMove), true)]
  public class BezierMoveEditor : Editor
  {
    public static GizmoDataEditor gizmoData;

    private BezierMove script;
    public BezierPositionEditor positionEditor;
    public BezierRotationEditor rotationEditor;

    static BezierMoveEditor()
    {
      gizmoData = new GizmoDataEditor();
    }

    public BezierMoveEditor()
    {
      positionEditor = new BezierPositionEditor();
      rotationEditor = new BezierRotationEditor();
    }

    private void OnEnable()
    {
      script = target as BezierMove;
    }

    private void OnSceneGUI()
    {
      if (!gizmoData.isShow) return;

      var gizmoScale = gizmoData.scale;
      var targetPosition = script.GetTargetPosition();
      positionEditor.PositionSceneGUI(targetPosition, gizmoScale);

      var hasRotation = script.GetTargetRotation(out var targetRotation);
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
      var curveProperty = serializedObject.FindProperty("curve");
      EditorGUILayout.PropertyField(curveProperty);

      var property = serializedObject.FindProperty("position");
      PositionSettingProperty(property);

      property = serializedObject.FindProperty("rotation");
      RotateSettingProperty(property);

      EditorGUILayout.Space();
      property = serializedObject.FindProperty("speed");

      do
      {
        EditorGUILayout.PropertyField(property);
      } while (property.Next(true));

      if (serializedObject.hasModifiedProperties)
      {
        script.OnValidate();
        serializedObject.ApplyModifiedProperties();
      }

      gizmoData.Inspector();
    }

    private void PositionSettingProperty(SerializedProperty property)
    {
      var distanceProperty = property.FindPropertyRelative("distance");
      EditorGUILayout.PropertyField(distanceProperty);
    }

    private void RotateSettingProperty(SerializedProperty property)
    {
      var depth = property.depth;
      property.Next(true);
      EditorGUILayout.PropertyField((SerializedProperty)property);
      var settingValue = (RotateSetting)property.enumValueIndex;

      EditorGUI.BeginDisabledGroup(settingValue < RotateSetting.Upward);
      while (property.Next(false))
      {
        if (property.depth == depth) break;
        EditorGUILayout.PropertyField(property);
      }
      EditorGUI.EndDisabledGroup();
    }
  }
}