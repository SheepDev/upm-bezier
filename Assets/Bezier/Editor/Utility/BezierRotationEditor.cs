using SheepDev.Bezier;
using SheepDev.Bezier.Utility;
using SheepDev.Utility.Editor;
using UnityEditor;
using UnityEngine;

namespace SheepDev.EditorBezier.Utility
{
  public class BezierRotationEditor
  {
    public void RotationSceneGUI(Vector3 position, Quaternion rotation, float scale)
    {
      HandleExtension.RotationAxisView(position, rotation, scale);
    }

    public void RotationUpwardSceneGUI(Vector3 position, Quaternion rotation, Vector3 upward, float scale)
    {
      var size = HandleUtility.GetHandleSize(position);
      Handles.color = Color.cyan;
      Handles.ArrowHandleCap(0, position, Quaternion.LookRotation(upward), size * 1.5f * scale, EventType.Repaint);

      RotationSceneGUI(position, rotation, scale);
    }

    public void RotationSettingProperty(SerializedProperty property)
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