using UnityEditor;
using UnityEngine;

namespace SheepDev.Utility.Editor
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

    public static bool DrawButton(Vector3 position, Handles.CapFunction capFunction, float sizeMultipler = 1)
    {
      var size = HandleUtility.GetHandleSize(position) * sizeMultipler;
      Handles.SphereHandleCap(0, position, Quaternion.identity, size, EventType.Repaint);

      var buttonSize = size * .8f;
      var pickSize = buttonSize / 2;

      var oldColor = Handles.color;
      Handles.color = Color.white;
      var isPress = Handles.Button(position, Quaternion.identity, buttonSize, pickSize, capFunction);
      Handles.color = oldColor;

      return isPress;
    }

    public static void DrawDot(Vector3 position, float sizeMultipler = 1)
    {
      var size = HandleUtility.GetHandleSize(position) * sizeMultipler;
      Handles.DotHandleCap(0, position, Quaternion.identity, size, EventType.Repaint);

      Handles.color = Color.white;
      Handles.DotHandleCap(0, position, Quaternion.identity, size * .6f, EventType.Repaint);
    }
  }
}