using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Utility.Editor;

namespace Bezier
{
  public class BezierGUI3D : EditorBehaviour
  {
    private EditPart currentSelectPart;
    private Quaternion localRotation;

    public override void OnUpdate(float deltaTime)
    {
    }

    public override void OnSceneGUI(SceneView view)
    {
      var curveEditor = BezierCurveEditor.activeCurve;
      var curve = curveEditor.Curve;
      var worldpoints = curve.GetWorldPoints();
      localRotation = curve.GetTransform().rotation;

      DrawCurve(worldpoints, curve.isLoop);
      DrawBezierPoints(worldpoints, curveEditor);
    }

    private void ForLoop(List<Point> points, bool isLoop, Action<Point, Point, int> callback)
    {
      var size = points.Count;
      var lastIndex = size - 1;

      for (int index = 0; index < size; index++)
      {
        var isLastPoint = index == lastIndex;
        var currentPoint = points[index];
        var nextPoint = (isLastPoint) ? points[0] : points[index + 1];

        if (!isLastPoint || isLoop)
        {
          callback.Invoke(currentPoint, nextPoint, index);
        }
      }
    }

    private void DrawCurve(List<Point> wordpoints, bool isLoop)
    {
      ForLoop(wordpoints, isLoop, (p1, p2, index) => DrawBezier(p1, p2, Color.white));
    }

    private void DrawBezier(Point a, Point b, Color color, float width = 2)
    {
      Handles.DrawBezier(a.Position, b.Position, a.StartTangentPosition, b.EndTangentPosition, color, null, width);
    }

    private void DrawBezierPoints(List<Point> worldpoints, CurveEditor curveEditor)
    {
      var draws = new List<DrawStack>();
      var colorPoint = GetHandleColorBySelectPart(EditPart.Point);
      var colorTangentEnd = GetHandleColorBySelectPart(EditPart.TangentEnd);
      var colorTangentStart = GetHandleColorBySelectPart(EditPart.TangentStart);

      for (int index = 0; index < worldpoints.Count; index++)
      {
        var point = worldpoints[index];
        var isActiveIndex = curveEditor.ActivePointIndex == index;

        var depthPoint = HandleUtility.WorldToGUIPointWithDepth(point.Position).z;
        var depthTangentStart = HandleUtility.WorldToGUIPointWithDepth(point.StartTangentPosition).z;
        var depthTangentEnt = HandleUtility.WorldToGUIPointWithDepth(point.EndTangentPosition).z;

        if (isActiveIndex)
        {
          SelectEditPart(point, draws);
        }
        else
        {
          if (curveEditor.IsEdit)
            draws.Add(new DrawButtonSelectIndex(point.Position, depthPoint, index, SetEditPart));
          else
            draws.Add(new DrawDot(point.Position, depthPoint, colorPoint));

          draws.Add(new DrawTangent(point.Position, point.EndTangentPosition, depthTangentEnt, colorTangentEnd));
          draws.Add(new DrawTangent(point.Position, point.StartTangentPosition, depthTangentStart, colorTangentStart));
        }
      }

      draws.Sort();
      for (int i = draws.Count - 1; i >= 0; i--)
      {
        draws[i].Draw();
      }

      if (curveEditor.IsActivePointIndex)
      {
        DrawHandleActivePoint(curveEditor.ActivePoint);
      }
    }

    private void DrawHandleActivePoint(Point point)
    {
      Handles.color = GetHandleColorBySelectPart(EditPart.TangentStart);
      Handles.DrawDottedLine(point.Position, point.StartTangentPosition, 5);
      Handles.color = GetHandleColorBySelectPart(EditPart.TangentEnd);
      Handles.DrawDottedLine(point.Position, point.EndTangentPosition, 5);

      var position = Vector3.zero;
      switch (currentSelectPart)
      {
        case EditPart.Point:
          position = point.Position;
          break;
        case EditPart.TangentStart:
          position = point.StartTangentPosition;
          break;
        case EditPart.TangentEnd:
          position = point.EndTangentPosition;
          break;
      }

      var isGlobal = Tools.pivotRotation == PivotRotation.Global;
      var rotation = isGlobal ? Quaternion.identity : localRotation;

      EditorGUI.BeginChangeCheck();
      var newPosition = Handles.PositionHandle(position, rotation);

      if (EditorGUI.EndChangeCheck())
      {
        switch (currentSelectPart)
        {
          case EditPart.Point:
            point.SetPosition(newPosition);
            break;
          case EditPart.TangentStart:
            point.SetTangentPosition(newPosition, TangentSpace.Start);
            break;
          case EditPart.TangentEnd:
            point.SetTangentPosition(newPosition, TangentSpace.End);
            break;
        }

        BezierCurveEditor.activeCurve.SetActivePoint(point);
      }
    }

    private void SelectEditPart(Point point, List<DrawStack> draws)
    {
      foreach (EditPart part in Enum.GetValues(typeof(EditPart)))
      {
        if (currentSelectPart == part) continue;
        var position = Vector3.zero;

        switch (part)
        {
          case EditPart.Point:
            position = point.Position;
            break;
          case EditPart.TangentStart:
            position = point.StartTangentPosition;
            break;
          case EditPart.TangentEnd:
            position = point.EndTangentPosition;
            break;
        }

        var depth = GetDepth(position);
        var color = GetHandleColorBySelectPart(part);
        draws.Add(new DrawButtonEditPart(position, depth, part, color, SetEditPart));
      }
    }

    private void SetActiveIndex(int index)
    {
      BezierCurveEditor.activeCurve.SetActivePointIndex(index);
      currentSelectPart = EditPart.Point;
    }

    private void SetEditPart(EditPart editPart)
    {
      currentSelectPart = editPart;
    }

    private float GetDepth(Vector3 position)
    {
      return HandleUtility.WorldToGUIPointWithDepth(position).z;
    }

    private static Color GetHandleColorBySelectPart(EditPart part)
    {
      switch (part)
      {
        case EditPart.Point:
          return Color.green;
        case EditPart.TangentStart:
          return Color.blue;
        case EditPart.TangentEnd:
          return Color.red;
        default:
          return Color.white;
      }
    }

    enum EditPart
    {
      Point, TangentStart, TangentEnd
    }

    struct DrawDot : DrawStack
    {
      private Vector3 position;
      private Color color;
      private float depth;

      public float layer => depth;

      public DrawDot(Vector3 position, float depth, Color color)
      {
        this.position = position;
        this.depth = depth;
        this.color = color;
      }

      public int CompareTo(DrawStack other)
      {
        return layer.CompareTo(other.layer);
      }

      public void Draw()
      {
        Handles.color = color;
        HandleExtension.DrawDot(position, .1f);
      }
    }

    struct DrawTangent : DrawStack
    {
      private Vector3 pointPosition;
      private Vector3 position;
      private Color color;
      private float depth;

      public float layer => depth;

      public DrawTangent(Vector3 pointPosition, Vector3 position, float depth, Color color)
      {
        this.pointPosition = pointPosition;
        this.position = position;
        this.depth = depth;
        this.color = color;
      }

      public int CompareTo(DrawStack other)
      {
        return layer.CompareTo(other.layer);
      }

      public void Draw()
      {
        Handles.color = color;
        Handles.DrawLine(pointPosition, position);
        HandleExtension.DrawDot(position, .1f);
      }
    }

    struct DrawButtonSelectIndex : DrawStack
    {
      private Vector3 position;
      private float depth;
      private int index;
      private readonly Action<EditPart> setPart;

      public float layer => depth;

      public DrawButtonSelectIndex(Vector3 position, float depth, int index, Action<EditPart> setPart)
      {
        this.position = position;
        this.depth = depth;
        this.index = index;
        this.setPart = setPart;
      }

      public int CompareTo(DrawStack other)
      {
        return layer.CompareTo(other.layer);
      }

      public void Draw()
      {
        Handles.color = Color.yellow;
        if (HandleExtension.DrawButton(position, Handles.SphereHandleCap, .3f))
        {
          BezierCurveEditor.activeCurve.SetActivePointIndex(index);
          setPart.Invoke(EditPart.Point);
        }
      }
    }

    struct DrawButtonEditPart : DrawStack
    {
      private Vector3 position;
      private float depth;
      private EditPart part;
      private Color color;
      private Action<EditPart> setPart;

      public float layer => depth;

      public DrawButtonEditPart(Vector3 position, float depth, EditPart part, Color color, Action<EditPart> setPart)
      {
        this.position = position;
        this.depth = depth;
        this.part = part;
        this.color = color;
        this.setPart = setPart;
      }

      public int CompareTo(DrawStack other)
      {
        return layer.CompareTo(other.layer);
      }

      public void Draw()
      {
        Handles.color = color;
        if (HandleExtension.DrawButton(position, Handles.SphereHandleCap, .3f))
        {
          setPart.Invoke(part);
        }
      }
    }
  }
}