using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Utility.Editor;
using static Bezier.BezierPoint;

namespace Bezier
{
  public class BezierGUI3D : EditorBehaviour<SelectCurve>
  {
    public static bool IsSplitSpline;
    public static bool IsRemovePoint;

    private List<DrawStack> draws = new List<DrawStack>();

    public override void Reset()
    {
      draws.Clear();
    }

    public override void BeforeSceneGUI(SelectCurve select)
    {
      DrawCurve(select.curve);
      DrawHandleBezier(select);

      if (BezierCurveEditor.IsShowRotationHandle)
      {
        var curve = select.curve;
        var distance = 0f;
        for (int index = 0; index < curve.Lenght; index++)
        {
          var point = curve.GetPoint(index);
          if (!point.HasNextPoint) continue;

          for (; distance < point.Size; distance += BezierCurveEditor.Distance)
          {
            point.GetPositionAndRotationByDistance(distance, out var position, out var rotation);
            HandleExtension.RotationAxisView(position, rotation, .2f);
          }

          distance = Mathf.Abs(distance - point.Size);
        }
      }
    }

    public override void SceneGUI(SceneView view)
    {
      draws.Sort();
      for (int i = draws.Count - 1; i >= 0; i--)
      {
        draws[i].Draw();
      }
    }

    private void DrawHandleBezier(SelectCurve select)
    {
      var curve = select.curve;
      var colorPoint = GetHandleColorBySelectPart(SelectBezierPart.Point);
      var colorTangentEnd = GetHandleColorBySelectPart(SelectBezierPart.TangentEnd);
      var colorTangentStart = GetHandleColorBySelectPart(SelectBezierPart.TangentStart);

      for (int index = 0; index < curve.Lenght; index++)
      {
        var point = curve.GetPoint(index);
        var isSelectIndex = select.PointIndex == index;

        var positionPoint = point.WorldPosition;
        var positionTangentStart = point.GetTangentPosition(TangentSelect.Start);
        var positionTangentEnd = point.GetTangentPosition(TangentSelect.End);

        if (IsSplitSpline || !isSelectIndex)
        {
          var depthPoint = HandleUtility.WorldToGUIPointWithDepth(positionPoint).z;
          var depthTangentStart = HandleUtility.WorldToGUIPointWithDepth(positionTangentStart).z;
          var depthTangentEnd = HandleUtility.WorldToGUIPointWithDepth(positionTangentEnd).z;

          if (select.IsEdit)
          {
            if (point.HasNextPoint && IsSplitSpline)
            {
              draws.Add(new DrawAddButtonBezierPoint(select, point, index));
            }
            else if (point.HasNextPoint && IsRemovePoint)
            {
              draws.Add(new DrawRemoveButtonBezierPoint(select, index));
            }
            else
            {
              draws.Add(new DrawButtonSelectIndex(select, positionPoint, depthPoint, index));
            }
          }
          else
            draws.Add(new DrawDot(positionPoint, depthPoint, colorPoint));

          draws.Add(new DrawTangent(positionPoint, positionTangentStart, depthTangentStart, colorTangentStart));
          draws.Add(new DrawTangent(positionPoint, positionTangentEnd, depthTangentEnd, colorTangentEnd));
        }
        else
        {
          Handles.color = GetHandleColorBySelectPart(SelectBezierPart.TangentStart);
          Handles.DrawDottedLine(positionPoint, positionTangentStart, 5);
          Handles.color = GetHandleColorBySelectPart(SelectBezierPart.TangentEnd);
          Handles.DrawDottedLine(positionPoint, positionTangentEnd, 5);

          var position = GetPointPartPosition(point, select.bezierPart);
          var depth = GetDepth(position);
          draws.Add(new DrawHandleSelectBezierPart(select, position, depth));
          SelectEditPart(select, point, draws);
        }
      }
    }

    private void SelectEditPart(SelectCurve select, BezierPoint point, List<DrawStack> draws)
    {
      foreach (SelectBezierPart part in Enum.GetValues(typeof(SelectBezierPart)))
      {
        if (select.bezierPart == part) continue;

        var position = GetPointPartPosition(point, part);
        var depth = GetDepth(position);
        var color = GetHandleColorBySelectPart(part);
        draws.Add(new DrawButtonSelectBezierPart(select, position, depth, part, color));
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

    private static float GetDepth(Vector3 position)
    {
      return HandleUtility.WorldToGUIPointWithDepth(position).z;
    }

    private static Vector3 GetPointPartPosition(BezierPoint point, SelectBezierPart part)
    {
      switch (part)
      {
        case SelectBezierPart.Point:
          return point.WorldPosition;
        case SelectBezierPart.TangentStart:
          return point.GetTangentPosition(TangentSelect.Start);
        case SelectBezierPart.TangentEnd:
          return point.GetTangentPosition(TangentSelect.End);
        default: return default;
      }
    }

    private static Color GetHandleColorBySelectPart(SelectBezierPart part)
    {
      switch (part)
      {
        case SelectBezierPart.Point:
          return Color.green;
        case SelectBezierPart.TangentStart:
          return Color.blue;
        case SelectBezierPart.TangentEnd:
          return Color.red;
        default:
          return Color.white;
      }
    }

    struct DrawDot : DrawStack
    {
      private Vector3 position;
      private Color color;
      private float depth;

      public float Depth => depth;
      public float Layer => 10;

      public DrawDot(Vector3 position, float depth, Color color)
      {
        this.position = position;
        this.depth = depth;
        this.color = color;
      }

      public int CompareTo(DrawStack other)
      {
        var layerCompare = Layer.CompareTo(other.Layer);
        return (layerCompare != 0) ? layerCompare : Depth.CompareTo(other.Depth);
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

      public float Depth => depth;
      public float Layer => 10;

      public DrawTangent(Vector3 pointPosition, Vector3 position, float depth, Color color)
      {
        this.pointPosition = pointPosition;
        this.position = position;
        this.depth = depth;
        this.color = color;
      }

      public int CompareTo(DrawStack other)
      {
        var layerCompare = Layer.CompareTo(other.Layer);
        return (layerCompare != 0) ? layerCompare : Depth.CompareTo(other.Depth);
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
      private readonly SelectCurve select;
      private readonly Vector3 position;
      private readonly float depth;
      private readonly int index;

      public float Depth => depth;
      public float Layer => 7;

      public DrawButtonSelectIndex(SelectCurve select, Vector3 position, float depth, int index)
      {
        this.select = select;
        this.position = position;
        this.depth = depth;
        this.index = index;
      }

      public int CompareTo(DrawStack other)
      {
        var layerCompare = Layer.CompareTo(other.Layer);
        return (layerCompare != 0) ? layerCompare : Depth.CompareTo(other.Depth);
      }

      public void Draw()
      {
        Handles.color = Color.yellow;
        if (HandleExtension.DrawButton(position, Handles.SphereHandleCap, .3f))
        {
          select.SetPointIndex(index);
          select.bezierPart = SelectBezierPart.Point;
        }
      }
    }

    struct DrawButtonSelectBezierPart : DrawStack
    {
      private readonly SelectCurve select;
      private readonly float depth;
      private readonly Color color;
      private readonly Vector3 position;
      private readonly SelectBezierPart selectPart;

      public float Depth => depth;
      public float Layer => 5;

      public DrawButtonSelectBezierPart(SelectCurve select, Vector3 position, float depth, SelectBezierPart selectPart, Color color)
      {
        this.select = select;
        this.position = position;
        this.depth = depth;
        this.selectPart = selectPart;
        this.color = color;
      }

      public int CompareTo(DrawStack other)
      {
        var layerCompare = Layer.CompareTo(other.Layer);
        return (layerCompare != 0) ? layerCompare : Depth.CompareTo(other.Depth);
      }

      public void Draw()
      {
        Handles.color = color;
        if (HandleExtension.DrawButton(position, Handles.SphereHandleCap, .3f))
        {
          select.bezierPart = selectPart;
        }
      }
    }

    struct DrawAddButtonBezierPoint : DrawStack
    {
      private readonly SelectCurve select;
      private readonly BezierPoint point;
      private readonly int index;
      private Vector3 position;
      private readonly float depth;

      public float Depth => depth;
      public float Layer => 5;

      public DrawAddButtonBezierPoint(SelectCurve select, BezierPoint point, int index)
      {
        this.select = select;
        this.point = point;
        this.index = index;
        position = MathBezier.GetIntervalWorldPosition(point, .5f);
        depth = GetDepth(position);
      }

      public int CompareTo(DrawStack other)
      {
        var layerCompare = Layer.CompareTo(other.Layer);
        return (layerCompare != 0) ? layerCompare : Depth.CompareTo(other.Depth);
      }

      public void Draw()
      {
        Handles.color = Color.blue;
        if (HandleExtension.DrawButton(position, Handles.SphereHandleCap, .3f))
        {
          select.AddPoint(index, .5f);
          BezierGUI3D.IsSplitSpline = false;
        }
      }
    }

    struct DrawRemoveButtonBezierPoint : DrawStack
    {
      private readonly SelectCurve select;
      private readonly int index;
      private readonly Vector3 position;
      private readonly float depth;

      public float Depth => depth;
      public float Layer => 5;

      public DrawRemoveButtonBezierPoint(SelectCurve select, int index)
      {
        this.select = select;
        this.index = index;

        var point = select.curve.GetPoint(index);
        this.position = point.WorldPosition;
        depth = GetDepth(position);
      }

      public int CompareTo(DrawStack other)
      {
        var layerCompare = Layer.CompareTo(other.Layer);
        return (layerCompare != 0) ? layerCompare : Depth.CompareTo(other.Depth);
      }

      public void Draw()
      {
        Handles.color = Color.red;
        if (HandleExtension.DrawButton(position, Handles.SphereHandleCap, .3f))
        {
          select.RemovePoint(index);
          BezierGUI3D.IsRemovePoint = false;
        }
      }
    }

    struct DrawHandleSelectBezierPart : DrawStack
    {
      private readonly SelectCurve select;
      private readonly Vector3 position;
      private readonly float depth;

      public float Depth => depth;
      public float Layer => 2;

      public DrawHandleSelectBezierPart(SelectCurve select, Vector3 position, float depth)
      {
        this.select = select;
        this.position = position;
        this.depth = depth;
      }

      public int CompareTo(DrawStack other)
      {
        var layerCompare = Layer.CompareTo(other.Layer);
        return (layerCompare != 0) ? layerCompare : Depth.CompareTo(other.Depth);
      }

      public void Draw()
      {
        var point = select.GetSelectPoint();
        var isGlobal = Tools.pivotRotation == PivotRotation.Global;
        var rotation = isGlobal ? Quaternion.identity : select.curve.GetTransform().rotation;

        EditorGUI.BeginChangeCheck();
        var newPosition = Handles.PositionHandle(position, rotation);

        if (EditorGUI.EndChangeCheck())
        {
          switch (select.bezierPart)
          {
            case SelectBezierPart.Point:
              point.SetPosition(newPosition);
              break;
            case SelectBezierPart.TangentStart:
              point.SetTangentPosition(newPosition, TangentSelect.Start);
              break;
            case SelectBezierPart.TangentEnd:
              point.SetTangentPosition(newPosition, TangentSelect.End);
              break;
          }

          select.SetSelectPoint(point);
        }
      }
    }
  }
}