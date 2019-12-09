using UnityEngine;
using UnityEditor;
using System;
using Bezier;
using System.Collections.Generic;

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

  public override void OnInspectorGUI()
  {
    if (activeBezier.curver != null)
    {
      activeBezier.isEdit = EditorGUILayout.Toggle("Edit Bezier", activeBezier.isEdit);
    }

    var isLoopProperty = serializedObject.FindProperty("isLoop");
    EditorGUILayout.PropertyField(isLoopProperty);
    serializedObject.ApplyModifiedProperties();
  }

  private void OnSceneGUI()
  {
    var worldPoints = script.GetWorldPoints();
    EventHandler(worldPoints);
    DrawBezier(worldPoints);
    HandleGUI();

    for (int index = 0; index < script.Lenght; index++)
    {
      var point = script.GetWorldPoint(index);
      var isActivePoint = activePointIndex.HasValue && index == activePointIndex;

      if (!isActivePoint)
      {
        SetHandlerColor(Color.red);
        SceneButton(point.Position, () => SetActivePointIndex(index), Handles.SphereHandleCap, .3f, .15f, .5f);

        SetHandlerColor(Color.white);
        Handles.DrawDottedLine(point.Position, point.StartTangentPosition, 5);
        Handles.DrawDottedLine(point.Position, point.EndTangentPosition, 5);

        var startHandleSize = HandleUtility.GetHandleSize(point.StartTangentPosition) * .2f;
        var endHandleSize = HandleUtility.GetHandleSize(point.EndTangentPosition) * .2f;
        Handles.SphereHandleCap(0, point.StartTangentPosition, Quaternion.identity, startHandleSize, EventType.Repaint);
        Handles.SphereHandleCap(0, point.EndTangentPosition, Quaternion.identity, endHandleSize, EventType.Repaint);
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

  private void EventHandler(List<Point> worldPoints)
  {
    var currentEvent = Event.current;
    var leftMouseDown = currentEvent.type == EventType.MouseDown && currentEvent.button == 0;

    if (leftMouseDown)
    {
      if (currentEvent.control)
      {
        AddPoint(worldPoints);
        currentEvent.Use();
      }
    }

    var pressEscape = currentEvent.type == EventType.KeyDown && currentEvent.keyCode == KeyCode.Escape;
    if (pressEscape)
    {
      currentEvent.Use();
      SetActivePointIndex(null);
    }
  }

  private void HandleGUI()
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

  private void SetActivePointIndex(int? index)
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

  private void DrawBezier(List<Point> points)
  {
    for (int index = 0; index < points.Count - 1; index++)
    {
      Point point = points[index];
      Point nextPoint = points[index + 1];
      var isActivePoint = activePointIndex.HasValue && index == activePointIndex || index == activePointIndex - 1;
      var color = (isActivePoint) ? Color.yellow : Color.white;

      DrawBezier(point, nextPoint, color);
    }

    if (script.isLoop)
    {
      var lastIndex = points.Count - 1;
      var point = points[lastIndex];
      var nextPoint = points[0];
      var isActivePoint = activePointIndex.HasValue && 0 == activePointIndex || lastIndex == activePointIndex;
      var color = (isActivePoint) ? Color.yellow : Color.white;

      DrawBezier(point, nextPoint, color);
    }
  }

  private Point DrawHandlerActivePoint(Point point)
  {
    var handleRotation = (Tools.pivotRotation == PivotRotation.Global) ? Quaternion.identity : script.GetTransform().rotation;

    var newPosition = Vector3.zero;
    switch (activeHandleType)
    {
      case HandleType.Point:
        newPosition = Handles.PositionHandle(point.Position, handleRotation);
        point.SetPosition(newPosition);
        break;
      case HandleType.StartTangent:
        SetHandlerColor(GetHandlerColor(HandleType.StartTangent));
        newPosition = Handles.PositionHandle(point.StartTangentPosition, handleRotation);

        point.SetTangentPosition(newPosition, TangentSpace.Start);
        Handles.DrawLine(point.Position, point.StartTangentPosition);
        break;
      case HandleType.EndTangent:
        SetHandlerColor(GetHandlerColor(HandleType.EndTangent));
        newPosition = Handles.PositionHandle(point.EndTangentPosition, handleRotation);

        point.SetTangentPosition(newPosition, TangentSpace.End);
        Handles.DrawLine(point.Position, point.EndTangentPosition);
        break;
    }

    if (activeHandleType != HandleType.Point)
    {
      var color = GetHandlerColor(HandleType.Point);
      SetHandlerColor(color);

      SceneButton(point.Position, () => { activeHandleType = HandleType.Point; }, Handles.DotHandleCap, .15f, .02f, .5f);
    }
    if (activeHandleType != HandleType.StartTangent)
    {
      var color = GetHandlerColor(HandleType.StartTangent);
      SetHandlerColor(color);

      Handles.DrawDottedLine(point.Position, point.StartTangentPosition, 5);
      SceneButton(point.StartTangentPosition, () => { activeHandleType = HandleType.StartTangent; }, Handles.DotHandleCap, .15f, .02f, .5f);
    }
    if (activeHandleType != HandleType.EndTangent)
    {
      var color = GetHandlerColor(HandleType.EndTangent);
      SetHandlerColor(color);

      Handles.DrawDottedLine(point.Position, point.EndTangentPosition, 5);
      SceneButton(point.EndTangentPosition, () => { activeHandleType = HandleType.EndTangent; }, Handles.DotHandleCap, .15f, .02f, .5f);
    }

    return point;
  }

  private void SceneButton(Vector3 worldPosition, Action triggerAction, Handles.CapFunction capFunction, float sizeMultiplier = 1, float min = 0, float max = 1)
  {
    var size = Mathf.Clamp(HandleUtility.GetHandleSize(worldPosition) * sizeMultiplier, min, max);
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

  private void AddPoint(List<Point> points)
  {
    var lastPoint = points[points.Count - 1];
    var depth = HandleUtility.WorldToGUIPointWithDepth(lastPoint.Position).z;
    var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
    var pointPosition = ray.origin + ray.direction * depth;

    var startTangent = new Tangent(Vector3.right, TangentType.Aligned);
    var endTangent = new Tangent(-Vector3.right, TangentType.Aligned);
    var point = new Point(pointPosition, startTangent, endTangent);
    var endTangentPosition = (lastPoint.StartTangentPosition + point.Position) / 2;
    point.SetTangentPosition(endTangentPosition, TangentSpace.End);

    script.AddWorldPoint(point);
    SetActivePointIndex(points.Count);
  }

  private Color GetHandlerColor(HandleType type)
  {
    switch (type)
    {
      case HandleType.Point:
        return Color.green;
      case HandleType.StartTangent:
        return Color.cyan;
      case HandleType.EndTangent:
        return Color.magenta;
      default:
        return Color.white;
    }
  }

  private void DrawBezier(Point a, Point b, Color color, float width = 2)
  {
    Handles.DrawBezier(a.Position, b.Position, a.StartTangentPosition, b.EndTangentPosition, color, null, width);
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
