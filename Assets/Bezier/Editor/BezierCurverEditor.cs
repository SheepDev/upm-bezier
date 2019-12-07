using UnityEngine;
using UnityEditor;
using System;
using Bezier;

[CustomEditor(typeof(BezierCurver), true)]
public class BezierCurverEditor : Editor
{
  private static BezierEdit activeBezier;

  private BezierCurver script;
  private int? activePointIndex;
  private HandleType activeHandleType;

  private void OnEnable()
  {
    script = target as BezierCurver;

    activePointIndex = null;
    activeHandleType = HandleType.Point;

    SceneView.duringSceneGui -= activeBezier.SceneGUI;

    if (activeBezier.curver != script)
    {
      activeBezier.curver = script;
      activeBezier.isEdit = false;
      activeBezier.SceneGUI = SceneGUI;
    }
  }

  private void OnDisable()
  {
    if (activeBezier.curver != null && activeBezier.isEdit)
    {
      SceneView.duringSceneGui += activeBezier.SceneGUI;
    }
  }

  private void SceneGUI(SceneView view)
  {
    OnSceneGUI();
  }

  private void OnSceneGUI()
  {
    HandleGUI();
    DrawBezier(script.GetWorldPoints());

    for (int index = 0; index < script.Lenght; index++)
    {
      var point = script.GetWorldPoint(index);
      var isActivePoint = activePointIndex.HasValue && index == activePointIndex;

      if (!isActivePoint)
      {
        SceneButton(point.Position, () => SetActivePointIndex(index), Color.red, Handles.SphereHandleCap);
      }
      else
      {
        EditorGUI.BeginChangeCheck();
        point = DrawHandlerActivePoint(point);
        if (EditorGUI.EndChangeCheck())
        {
          Undo.RecordObject(target, "Set Point: " + index);
          script.SetWorldPoint(index, point);
        }
      }
    }
  }

  public void HandleGUI()
  {
    Handles.BeginGUI();
    var masterPosition = new Vector2(10, 10);
    var masterWidth = 240;

    // Draw Box
    var boxRect = new Rect(masterPosition, new Vector2(masterWidth, 150));
    var oldColor = GUI.color;
    GUI.color = new Color(1, 1, 1, .3f);
    GUI.Box(boxRect, "");
    GUI.color = oldColor;

    // Draw Name Object
    var style = new GUIStyle();
    style.fontStyle = FontStyle.Bold;
    style.alignment = TextAnchor.MiddleCenter;
    style.fontSize = 15;

    var scriptName = script.name;
    if (scriptName.Length > 20)
    {
      scriptName = scriptName.Substring(0, 20) + "...";
    }

    GUI.Label(new Rect(masterPosition + new Vector2(10, 10), new Vector2(masterWidth - 20, 20)), scriptName, style);
    masterPosition.y += 20;

    // Draw Bezier Type
    if (GetActivePoint(out Point activePoint))
    {
      var startType = EditorGUI.EnumPopup(new Rect(masterPosition + new Vector2(10, 20), new Vector2(masterWidth - 20, 20)), "StartTangent Type", activePoint.StartTangentType);
      activePoint.SetTangentType((TangentType)startType, TangentSpace.Start);
      masterPosition.y += 30;
      var endType = EditorGUI.EnumPopup(new Rect(masterPosition + new Vector2(10, 10), new Vector2(masterWidth - 20, 20)), "EndTangent Type", activePoint.EndTangentType);
      activePoint.SetTangentType((TangentType)endType, TangentSpace.End);
      masterPosition.y += 20;

      SetActivePoint(activePoint);
    }

    // Draw Button
    if (GUI.Button(new Rect(masterPosition + new Vector2((masterWidth / 2) - 75, 20), new Vector2(150, 40)), "Finish Edit Bezier"))
    {
      activeBezier.isEdit = false;
      SceneView.duringSceneGui -= activeBezier.SceneGUI;
      return;
    }
    masterPosition.y += 40;

    Handles.EndGUI();
  }

  public override void OnInspectorGUI()
  {
    if (activeBezier.curver != null)
    {
      activeBezier.isEdit = EditorGUILayout.Toggle("Edit Bezier", activeBezier.isEdit);
    }
  }

  private void SetActivePointIndex(int index)
  {
    activePointIndex = index;
    activeHandleType = HandleType.Point;
  }

  private bool GetActivePoint(out Point point)
  {
    if (activePointIndex.HasValue)
    {
      point = script.GetWorldPoint(activePointIndex.Value);
      return true;
    }

    point = default;
    return false;
  }

  private void SetActivePoint(Point point)
  {
    if (activePointIndex.HasValue)
    {
      script.SetWorldPoint(activePointIndex.Value, point);
    }
  }

  private void DrawBezier(Point[] points)
  {
    for (int index = 0; index < points.Length - 1; index++)
    {
      Point point = points[index];
      Point nextPoint = points[index + 1];
      var isActivePoint = activePointIndex.HasValue && index == activePointIndex || index == activePointIndex - 1;
      var color = (isActivePoint) ? Color.green : Color.white;

      Handles.DrawBezier(point.Position, nextPoint.Position, point.StartTangentPosition, nextPoint.EndTangentPosition, color, null, 2f);
    }
  }

  private Point DrawHandlerActivePoint(Point point)
  {
    switch (activeHandleType)
    {
      case HandleType.Point:
        point.SetPosition(Handles.PositionHandle(point.Position, Quaternion.identity));
        break;
      case HandleType.StartTangent:
        var newStartPosition = Handles.PositionHandle(point.StartTangentPosition, Quaternion.identity);
        point.SetTangentPosition(newStartPosition, TangentSpace.Start);
        break;
      case HandleType.EndTangent:
        point.SetTangentPosition(Handles.PositionHandle(point.EndTangentPosition, Quaternion.identity), TangentSpace.End);
        break;
    }

    if (activeHandleType != HandleType.Point)
    {
      var color = GetHandlerColor(HandleType.Point);
      SceneButton(point.Position, () => { activeHandleType = HandleType.Point; }, color, Handles.DotHandleCap);
    }
    if (activeHandleType != HandleType.StartTangent)
    {
      var color = GetHandlerColor(HandleType.StartTangent);
      SceneButton(point.StartTangentPosition, () => { activeHandleType = HandleType.StartTangent; }, color, Handles.DotHandleCap);
    }
    if (activeHandleType != HandleType.EndTangent)
    {
      var color = GetHandlerColor(HandleType.EndTangent);
      SceneButton(point.EndTangentPosition, () => { activeHandleType = HandleType.EndTangent; }, color, Handles.DotHandleCap);
    }

    return point;
  }

  private void SceneButton(Vector3 worldPosition, Action triggerAction, Color color, Handles.CapFunction capFunction)
  {
    SetHandlerColor(color);
    var size = HandleUtility.GetHandleSize(worldPosition) / 3;
    var pickSize = size / 2;

    var isPress = Handles.Button(worldPosition, Quaternion.identity, size, pickSize, capFunction);
    if (isPress)
    {
      triggerAction.Invoke();
    }
  }

  private void SetHandlerColor(Color color)
  {
    Handles.color = color;
  }

  private Color GetHandlerColor(HandleType type)
  {
    switch (type)
    {
      case HandleType.Point:
        return Color.yellow;
      case HandleType.StartTangent:
        return Color.blue;
      case HandleType.EndTangent:
        return Color.red;
      default:
        return Color.white;
    }
  }

  enum HandleType
  {
    Point, StartTangent, EndTangent
  }

  struct BezierEdit
  {
    public BezierCurver curver;
    public bool isEdit;
    public Action<SceneView> SceneGUI;
  }
}
