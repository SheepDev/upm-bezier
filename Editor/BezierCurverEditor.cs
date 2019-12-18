﻿using UnityEngine;
using UnityEditor;
using System;
using Bezier;
using System.Collections.Generic;
using static Input.InputEditor;
using Utility.Editor;

[CustomEditor(typeof(BezierCurver), true)]
public class BezierCurverEditor : Editor
{
  private static BezierEdit activeBezier;
  private int? activePointIndex;
  private HandleType activeHandleType;
  private BezierCurver Curver => activeBezier.curver;

  private List<Point> cacheWorldPoints;
  private Vector2 GUI2DPosition;

  private void OnEnable()
  {
    GUI2DPosition = new Vector2(-250, 10);
    SetActiveScript(target as BezierCurver);
  }

  private void OnDisable()
  {
    if (activeBezier.curver != null && activeBezier.isEdit)
    {
      SceneView.duringSceneGui += activeBezier.SceneGUI = (SceneView view) => OnSceneGUI();
    }
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

    base.OnInspectorGUI();
  }

  private void OnSceneGUI()
  {
    if (Curver is null)
    {
      Destroy();
    }

    cacheWorldPoints = Curver.GetWorldPoints();
    DrawBezier();
    EventHandler();
    SceneGUI2D();

    if (activeBezier.isEdit)
    {
      SceneGUI3D();
    }
  }

  private void EventHandler()
  {
    if (GetKeyDown(KeyCode.F, out var fEvent))
    {
      var bounds = new Bounds();
      if (GetActivePoint(out Point activePoint))
      {
        bounds.center = activePoint.Position;
        bounds.Encapsulate(activePoint.StartTangentPosition);
        bounds.Encapsulate(activePoint.EndTangentPosition);
      }
      else
      {
        var isFirst = true;
        foreach (var point in cacheWorldPoints)
        {
          if (isFirst)
          {
            bounds.center = point.Position;
            isFirst = false;
          }
          else
          {
            bounds.Encapsulate(point.Position);
          }

          bounds.Encapsulate(point.StartTangentPosition);
          bounds.Encapsulate(point.EndTangentPosition);
        }
      }

      SceneView.lastActiveSceneView.Frame(bounds, false);
      fEvent.Use();
    }

    if (GetKeyDown(KeyCode.B, out var bEvent))
    {
      SetEdit(!activeBezier.isEdit);
      bEvent.Use();
    }

    if (GetKeyDown(KeyCode.L, out var lEvent))
    {
      Curver.isLoop = !Curver.isLoop;
      lEvent.Use();
    }

    if (!activeBezier.isEdit) return;

    if (GetMouseDown(0, out var mouseLeftEvent))
    {
      if (mouseLeftEvent.control)
      {
        AddPoint(cacheWorldPoints);
        mouseLeftEvent.Use();
      }
    }

    if (GetKeyDown(KeyCode.Escape, out var escapeEvent))
    {
      SetActivePointIndex(null);
      escapeEvent.Use();
    }
  }

  private void SceneGUI3D()
  {
    for (int index = 0; index < Curver.Lenght; index++)
    {
      var point = Curver.GetWorldPoint(index);
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
          Curver.SetWorldPoint(index, point);
        }
      }
    }
  }

  private void SceneGUI2D()
  {
    Handles.BeginGUI();
    var isEdit = activeBezier.isEdit;

    GUI2DPosition = Vector2.MoveTowards(GUI2DPosition, new Vector2(10, 10), Time.deltaTime * 60);
    GUI2DPosition.y = 10;
    var maxWidth = 240;

    // Draw Box
    var boxRect = new Rect(GUI2DPosition, new Vector2(maxWidth, (isEdit) ? 140 : 90));
    var oldColor = GUI.color;
    GUI.color = new Color(1, 1, 1, .3f);
    GUI.Box(boxRect, "");
    GUI.color = oldColor;

    // Draw Name Object
    var style = new GUIStyle();
    style.fontStyle = FontStyle.Bold;
    style.alignment = TextAnchor.MiddleCenter;
    style.fontSize = 15;

    var scriptName = Curver.name;
    if (scriptName.Length > 20)
    {
      scriptName = scriptName.Substring(0, 20) + "...";
    }

    GUI.Label(new Rect(GUI2DPosition + new Vector2(10, 10), new Vector2(maxWidth - 20, 20)), scriptName, style);
    GUI2DPosition.y += 30;

    // Draw Bezier Type
    if (isEdit)
    {
      var isFound = GetActivePoint(out Point activePoint);
      EditorGUI.BeginDisabledGroup(!isFound);

      var startType = (TangentType)EditorGUI.EnumPopup(new Rect(GUI2DPosition + new Vector2(10, 10), new Vector2(maxWidth - 20, 10)), "StartTangent Type", activePoint.StartTangentType);
      activePoint.SetTangentType(startType, TangentSpace.Start);
      GUI2DPosition.y += 20;
      var endType = (TangentType)EditorGUI.EnumPopup(new Rect(GUI2DPosition + new Vector2(10, 10), new Vector2(maxWidth - 20, 20)), "EndTangent Type", activePoint.EndTangentType);
      activePoint.SetTangentType(endType, TangentSpace.End);
      GUI2DPosition.y += 20;

      SetActivePoint(activePoint);
      EditorGUI.EndDisabledGroup();
      GUI2DPosition.y += 10;
    }

    // Draw Button
    var text = (isEdit) ? "Finish Edit" : "Start Edit";
    if (GUI.Button(new Rect(GUI2DPosition + new Vector2((maxWidth / 2) - 75, 10), new Vector2(150, 30)), text))
    {
      SetEdit(!isEdit);
    }

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
      point = Curver.GetWorldPoint(activePointIndex.Value);
      return true;
    }

    point = default;
    return false;
  }

  private void SetActivePoint(Point point)
  {
    if (activePointIndex.HasValue)
    {
      Curver.SetWorldPoint(activePointIndex.Value, point);
    }
  }

  private Point DrawHandlerActivePoint(Point point)
  {
    var handleRotation = (Tools.pivotRotation == PivotRotation.Global) ? Quaternion.identity : Curver.GetTransform().rotation;

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

    Curver.AddWorldPoint(point);
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

  private Color GetHandlerColor(int index)
  {
    var nextIndex = index + 1;
    if (nextIndex == cacheWorldPoints.Count)
    {
      nextIndex = 0;
    }

    var isActivePoint = activePointIndex.HasValue && index == activePointIndex || nextIndex == activePointIndex;
    return (isActivePoint) ? Color.yellow : Color.white;
  }

  private Color GetHandlerIndexColor(int index)
  {
    var selector = (int)Mathf.Repeat(index, 4);

    switch (selector)
    {
      case 0:
        return Color.blue;
      case 1:
        return Color.green;
      case 2:
        return Color.red;
      case 3:
        return Color.yellow;
      default:
        return Color.white;
    }
  }

  private void DrawBezier()
  {
    var lastPoint = cacheWorldPoints.Count - 1;
    for (int index = 0; index < cacheWorldPoints.Count; index++)
    {
      var isLastPoint = index == lastPoint;
      var point = cacheWorldPoints[index];
      var nextPoint = (isLastPoint) ? cacheWorldPoints[0] : cacheWorldPoints[index + 1];

      if (!isLastPoint || isLastPoint && Curver.isLoop)
      {
        DrawBezier(point, nextPoint, GetHandlerColor(index));

        Handles.color = GetHandlerIndexColor(index);
        var isFirstKey = true;
        foreach (var key in point.TCurveDistance.keys)
        {
          var position = BezierUtility.GetCurverInterval(point, nextPoint, key.value);
          Handles.SphereHandleCap(0, position, Quaternion.identity, (isFirstKey) ? .2f : .4f, EventType.Repaint);
          isFirstKey = false;
        }
      }
    }
  }

  private void DrawBezier(Point a, Point b, Color color, float width = 2)
  {
    Handles.DrawBezier(a.Position, b.Position, a.StartTangentPosition, b.EndTangentPosition, color, null, width);
  }

  private void SetActiveScript(BezierCurver curver)
  {
    if (!(curver is null))
    {
      activeBezier.curver = curver;
    }

    Destroy();
    SetEdit(false);
  }

  private void Destroy()
  {
    SceneView.duringSceneGui -= activeBezier.SceneGUI;
  }

  private void SetEdit(bool isEdit)
  {
    var isActive = activeBezier.isEdit = isEdit;
    Tools.hidden = isActive;

    if (!isActive)
    {
      Selection.activeObject = Curver.gameObject;
      SetActivePointIndex(null);
    }
  }

  enum HandleType
  {
    Point, StartTangent, EndTangent
  }

  struct BezierEdit
  {
    delegate void SetEdit(bool isEdit);

    public BezierCurver curver;
    public bool isEdit;
    public Action<SceneView> SceneGUI;
  }
}