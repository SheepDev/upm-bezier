using UnityEditor;
using UnityEngine;

namespace Utility.Editor
{
  public static class HandleExtension
  {
    public static void RotationAxisView(Vector3 position, Quaternion rotation, float size = .5f)
    {
      var oldColor = Handles.color;
      var sizeHandle = HandleUtility.GetHandleSize(position) * size;

      Handles.color = Color.blue;
      Handles.ArrowHandleCap(0, position, rotation, sizeHandle, EventType.Repaint);

      Handles.color = Color.green;
      var upRotation = Quaternion.LookRotation(rotation * Vector3.up);
      Handles.ArrowHandleCap(0, position, upRotation, sizeHandle, EventType.Repaint);

      Handles.color = Color.red;
      var rightRotation = Quaternion.LookRotation(rotation * Vector3.right);
      Handles.ArrowHandleCap(0, position, rightRotation, sizeHandle, EventType.Repaint);

      Handles.color = oldColor;
    }
  }
}