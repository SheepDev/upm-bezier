using SheepDev.Bezier;
using SheepDev.Bezier.Utility;
using UnityEditor;
using UnityEngine;

namespace SheepDev.EditorBezier.Utility
{
  public class BezierPositionEditor
  {
    public void PositionSceneGUI(Vector3 position, float scale)
    {
      var sizeScale = HandleUtility.GetHandleSize(position) * scale;
      Handles.SphereHandleCap(0, position, Quaternion.identity, sizeScale * .2f, EventType.Repaint);
    }

    public void PositionSettingProperty(SerializedProperty property)
    {
      var settingProperty = property.FindPropertyRelative("setting");
      EditorGUILayout.PropertyField(settingProperty);
      var settingValue = (PositionSetting)settingProperty.enumValueIndex;
      switch (settingValue)
      {
        case PositionSetting.Default:
          EditorGUILayout.PropertyField(property.FindPropertyRelative("distance"));
          break;
        case PositionSetting.Porcent:
          EditorGUILayout.PropertyField(property.FindPropertyRelative("porcent"));
          break;
        case PositionSetting.Section:
          EditorGUILayout.PropertyField(property.FindPropertyRelative("sectionIndex"));
          EditorGUILayout.PropertyField(property.FindPropertyRelative("t"));
          break;
      }
    }
  }
}