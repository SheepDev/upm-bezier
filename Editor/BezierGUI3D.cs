using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Utility.Editor;
using static Bezier.BezierPoint;

namespace Bezier
{
  public class BezierGUI3D : EditorBehaviour
  {
    private EditPart currentSelectPart;

    public override void OnUpdate(float deltaTime)
    {
    }

    public override void OnSceneGUI(SceneView view)
    {
      var curveEditor = BezierCurveEditor.activeCurve;

      DrawCurve(curveEditor.Curve);
      DrawBezierPoints(curveEditor);

      if (curveEditor.isDebug)
      {
        var curve = curveEditor.Curve;
        var distance = 0f;
        for (int i = 0; i < curve.Lenght; i++)
        {
          var point = curve.GetPoint(i);
          if (!point.HasNextPoint) continue;

          for (; distance < point.Size; distance += 0.5f)
          {
            point.GetPositionAndRotationByDistance(distance, out var position, out var rotation);
            HandleExtension.RotationAxisView(position, rotation, .2f);
          }

          distance = Mathf.Abs(distance - point.Size);
        }
      }
    }

    public static void DrawCurve(BezierCurve curve)
    {
      for (int index = 0; index < curve.Lenght; index++)
      {
        var point = curve.GetPoint(index);
        if (point.HasNextPoint)
        {
          DrawBezier(point, Color.white);
        }
      }
    }

    private static void DrawBezier(BezierPoint point, Color color, float width = 2)
    {
      var positionStart = point.WorldPosition;
      var positionTangentStart = point.GetTangentPosition(TangentSelect.Start);
      var positionTangentEnd = point.NextTangentWorldPosition;
      var positionEnd = point.NextPointWorldPosition;

      Handles.DrawBezier(positionStart, positionEnd, positionTangentStart, positionTangentEnd, color, null, width);
    }

    private void DrawBezierPoints(CurveEditor curveEditor)
    {
      var draws = new List<DrawStack>();
      var curve = curveEditor.Curve;
      var colorPoint = GetHandleColorBySelectPart(EditPart.Point);
      var colorTangentEnd = GetHandleColorBySelectPart(EditPart.TangentEnd);
      var colorTangentStart = GetHandleColorBySelectPart(EditPart.TangentStart);

      for (int index = 0; index < curve.Lenght; index++)
      {
        var point = curve.GetPoint(index);
        var isActiveIndex = curveEditor.ActivePointIndex == index;

        var positionPoint = point.WorldPosition;
        var positionTangentStart = point.GetTangentPosition(TangentSelect.Start);
        var positionTangentEnd = point.GetTangentPosition(TangentSelect.End);

        var depthPoint = HandleUtility.WorldToGUIPointWithDepth(positionPoint).z;
        var depthTangentStart = HandleUtility.WorldToGUIPointWithDepth(positionTangentStart).z;
        var depthTangentEnd = HandleUtility.WorldToGUIPointWithDepth(positionTangentEnd).z;

        if (isActiveIndex)
        {
          SelectEditPart(point, draws);
        }
        else
        {
          if (curveEditor.IsEdit)
            draws.Add(new DrawButtonSelectIndex(positionPoint, depthPoint, index, SetEditPart));
          else
            draws.Add(new DrawDot(positionPoint, depthPoint, colorPoint));

          draws.Add(new DrawTangent(positionPoint, positionTangentStart, depthTangentStart, colorTangentStart));
          draws.Add(new DrawTangent(positionPoint, positionTangentEnd, depthTangentEnd, colorTangentEnd));
        }
      }

      draws.Sort();
      for (int i = draws.Count - 1; i >= 0; i--)
      {
        draws[i].Draw();
      }

      if (curveEditor.IsActivePointIndex)
      {
        DrawHandleActivePoint(curveEditor.ActivePoint, curve.GetTransform().rotation);
      }
    }

    private void DrawHandleActivePoint(BezierPoint point, Quaternion curveRotation)
    {
      var positionPoint = point.WorldPosition;
      var positionTangentStart = point.GetTangentPosition(TangentSelect.Start);
      var positionTangentEnd = point.GetTangentPosition(TangentSelect.End);

      Handles.color = GetHandleColorBySelectPart(EditPart.TangentStart);
      Handles.DrawDottedLine(positionPoint, positionTangentStart, 5);
      Handles.color = GetHandleColorBySelectPart(EditPart.TangentEnd);
      Handles.DrawDottedLine(positionPoint, positionTangentEnd, 5);

      var position = GetPointPartPosition(point, currentSelectPart);
      var isGlobal = Tools.pivotRotation == PivotRotation.Global;
      var rotation = isGlobal ? Quaternion.identity : curveRotation;

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
            point.SetTangentPosition(newPosition, TangentSelect.Start);
            break;
          case EditPart.TangentEnd:
            point.SetTangentPosition(newPosition, TangentSelect.End);
            break;
        }

        BezierCurveEditor.activeCurve.SetActivePoint(point);
      }
    }

    private void SelectEditPart(BezierPoint point, List<DrawStack> draws)
    {
      foreach (EditPart part in Enum.GetValues(typeof(EditPart)))
      {
        if (currentSelectPart == part) continue;
        var position = GetPointPartPosition(point, part);
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

    private static Vector3 GetPointPartPosition(BezierPoint point, EditPart part)
    {
      switch (part)
      {
        case EditPart.Point:
          return point.WorldPosition;
        case EditPart.TangentStart:
          return point.GetTangentPosition(TangentSelect.Start);
        case EditPart.TangentEnd:
          return point.GetTangentPosition(TangentSelect.End);
        default: return default;
      }
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