using SheepDev.Utility.Editor;
using UnityEditor;
using UnityEngine;

namespace SheepDev.Bezier
{
  [CanEditMultipleObjects]
  [CustomEditor(typeof(BezierSnap), true)]
  public class BezierSnapEditor : Editor
  {
    private static bool IsShowGizmo;
    private static float GizmoGlobalScala;
    private BezierSnap script;
    private bool HasBezierMove => script.GetComponent<BezierMove>() != null;

    static BezierSnapEditor()
    {
      GizmoGlobalScala = 1f;
    }

    private void OnEnable()
    {
      script = target as BezierSnap;

      if (HasBezierMove)
      {
        script.positionSetting = PositionSetting.Default;
      }
    }

    private void OnDisable()
    {
      Tools.hidden = false;
    }

    private void OnSceneGUI()
    {
      var isEnable = script.isActiveAndEnabled;
      Tools.hidden = isEnable && Tools.current != Tool.Scale;

      if (!IsShowGizmo) return;

      var position = script.GetTargetPosition();
      var sizeScale = HandleUtility.GetHandleSize(position) * GizmoGlobalScala;
      Handles.SphereHandleCap(0, position, Quaternion.identity, sizeScale * .2f, EventType.Repaint);

      var hasRotation = script.GetTargetRotation(out var targetRotation);

      if (hasRotation && script.rotateSetting == RotateSetting.Upward)
      {
        var upward = script.GetUpward();
        Handles.color = Color.cyan;
        Handles.ArrowHandleCap(0, position, Quaternion.LookRotation(upward), sizeScale * 1.5f, EventType.Repaint);
      }

      if (hasRotation) HandleExtension.RotationAxisView(position, targetRotation, 1f * GizmoGlobalScala);
    }

    public override void OnInspectorGUI()
    {
      var isValid = CurveProperty();

      EditorGUI.BeginDisabledGroup(!isValid);
      EditorGUILayout.Space();
      PositionSettingProperty();
      EditorGUILayout.Space();
      RotateSettingProperty();
      EditorGUI.EndDisabledGroup();

      if (serializedObject.hasModifiedProperties)
      {
        script.OnValidate();
        serializedObject.ApplyModifiedProperties();
      }

      EditorProperty();
    }

    private bool CurveProperty()
    {
      var curveProperty = GetAndDrawProperty("curve");
      var isValid = curveProperty.objectReferenceValue != null;

      if (!isValid)
      {
        EditorGUILayout.HelpBox("Need bezier for working correct!", MessageType.Warning);
      }

      return isValid;
    }

    private void PositionSettingProperty()
    {
      var disableChangeSetting = HasBezierMove;

      EditorGUI.BeginDisabledGroup(disableChangeSetting);
      var settingProperty = GetAndDrawProperty("positionSetting");
      EditorGUI.EndDisabledGroup();

      if (disableChangeSetting)
      {
        settingProperty.enumValueIndex = (int)PositionSetting.Default;
        EditorGUILayout.HelpBox("not possible to change, because is has bezier move!", MessageType.Info);
      }

      var settingValue = (PositionSetting)settingProperty.enumValueIndex;
      switch (settingValue)
      {
        case PositionSetting.Default:
          GetAndDrawProperty("distance");
          break;
        case PositionSetting.Porcent:
          GetAndDrawProperty("porcent");
          break;
        case PositionSetting.Section:
          GetAndDrawProperty("sectionIndex");
          GetAndDrawProperty("t");
          break;
      }
    }

    private void RotateSettingProperty()
    {
      var setting = (RotateSetting)GetAndDrawProperty("rotateSetting").enumValueIndex;
      if (setting == RotateSetting.Upward)
      {
        var isFixed = GetAndDrawProperty("isFixedValueUpward").boolValue;

        EditorGUI.BeginDisabledGroup(!isFixed);
        GetAndDrawProperty("fixedUpward");
        EditorGUI.EndDisabledGroup();
      }
    }

    private void EditorProperty()
    {
      EditorGUILayout.Space();
      var isShow = EditorGUILayout.Toggle("Show Gizmos", IsShowGizmo);
      var isRepaint = false;

      isRepaint |= isShow != IsShowGizmo;
      IsShowGizmo = isShow;

      if (IsShowGizmo)
      {
        var gizmoScala = EditorGUILayout.Slider("Gizmo Size", GizmoGlobalScala, .6f, 2);
        isRepaint |= gizmoScala != GizmoGlobalScala;
        GizmoGlobalScala = gizmoScala;
      }

      if (isRepaint) SceneView.lastActiveSceneView.Repaint();
    }

    private SerializedProperty GetAndDrawProperty(string name)
    {
      var property = serializedObject.FindProperty(name);
      EditorGUILayout.PropertyField(property);
      return property;
    }
  }
}