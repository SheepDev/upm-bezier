using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using SheepDev.Utility.Editor;
using static SheepDev.Bezier.BezierPoint;

namespace SheepDev.Bezier
{
  public class BezierGUI3D : EditorBehaviour<SelectCurve>
  {
    public static bool IsSplitSpline;
    public static bool IsRemovePoint;

    public bool isShowRotation;
    public int curveDivision;
    public Vector3 upward;
    public RotationSetting rotationSetting;

    private List<DrawStack> draws = new List<DrawStack>();

    public BezierGUI3D()
    {
      draws = new List<DrawStack>();
      upward = Vector3.up;
    }

    public override void Reset()
    {
      draws.Clear();
    }

    public override void BeforeSceneGUI(SelectCurve select)
    {
      DrawCurve(select.curve);

      if (select.IsEdit)
      {
        DrawHandleBezier(select);
      }

      if (isShowRotation)
      {
        var curve = select.curve;
        var distance = 0f;
        var stepDistance = curve.GetSize() / curveDivision;

        foreach (var section in curve)
        {
          for (; distance < section.Size; distance += stepDistance)
          {
            var position = Vector3.zero;
            var rotation = Quaternion.identity;

            if (rotationSetting == RotationSetting.Upwards)
            {
              section.GetPositionAndRotationByDistance(distance, out position, out rotation, upward);
            }
            else
            {
              var isNormalize = rotationSetting == RotationSetting.Normalize;
              var isInherit = rotationSetting == RotationSetting.Inheritroll;
              section.GetPositionAndRotationByDistance(distance, out position, out rotation, Space.World, isNormalize, isInherit);
            }

            HandleExtension.RotationAxisView(position, rotation);
          }

          distance -= section.Size;
        }
      }
    }

    public override void InspectorGUI()
    {
      EditorGUILayout.Space();
      EditorGUILayout.LabelField("Rotation Handler Settings");

      var isShowRotation = EditorGUILayout.Toggle("Show Rotation", this.isShowRotation);
      BezierCurveEditor.RepaintIfChange(isShowRotation, this.isShowRotation);
      this.isShowRotation = isShowRotation;

      if (!isShowRotation) return;

      var curveDivision = EditorGUILayout.IntSlider("Step Distance", this.curveDivision, 10, 50);
      BezierCurveEditor.RepaintIfChange(curveDivision, this.curveDivision);
      this.curveDivision = curveDivision;

      var rotationSetting = (RotationSetting)EditorGUILayout.EnumFlagsField("Rotation config", this.rotationSetting);
      BezierCurveEditor.RepaintIfChange(rotationSetting, this.rotationSetting);
      this.rotationSetting = rotationSetting;

      if (rotationSetting == RotationSetting.Upwards)
      {
        var upward = EditorGUILayout.Vector3Field("Upward", this.upward).normalized;
        BezierCurveEditor.RepaintIfChange(upward, this.upward);
        this.upward = upward;
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

      for (var index = 0; index < curve.Lenght; index++)
      {
        var point = curve.GetPoint(index);
        var positionPoint = point.WorldPosition;
        var positionTangentStart = point.GetTangentPosition(TangentSelect.Start);
        var positionTangentEnd = point.GetTangentPosition(TangentSelect.End);

        var depthPoint = GetDepth(positionPoint);
        var depthTangentStart = GetDepth(positionTangentStart);
        var depthTangentEnd = GetDepth(positionTangentEnd);

        if (select.IsEdit)
        {
          if (IsSplitSpline)
          {
            draws.Add(new DrawAddButtonBezierPoint(select, index));
            draws.Add(new DrawDot(positionPoint, depthPoint, colorPoint));
          }
          else if (IsRemovePoint)
          {
            draws.Add(new DrawRemoveButtonBezierPoint(select, index));
          }
          else if (select.PointIndex != index)
          {
            draws.Add(new DrawButtonSelectIndex(select, positionPoint, depthPoint, index));
          }
        }
        else
        {
          draws.Add(new DrawDot(positionPoint, depthPoint, colorPoint));
        }

        draws.Add(new DrawTangent(positionPoint, positionTangentStart, depthTangentStart, colorTangentStart));
        draws.Add(new DrawTangent(positionPoint, positionTangentEnd, depthTangentEnd, colorTangentEnd));
      }

      if (select.IsSelectPoint && !(IsSplitSpline ^ IsRemovePoint))
      {
        var index = select.PointIndex;
        var point = select.curve.GetPoint(index);

        Handles.color = GetHandleColorBySelectPart(SelectBezierPart.TangentStart);
        Handles.DrawDottedLine(point.WorldPosition, point.GetTangentPosition(TangentSelect.Start), 5);
        Handles.color = GetHandleColorBySelectPart(SelectBezierPart.TangentEnd);
        Handles.DrawDottedLine(point.WorldPosition, point.GetTangentPosition(TangentSelect.End), 5);

        var handlePosition = GetPointPartPosition(point, select.bezierPart);
        var handleDepth = GetDepth(handlePosition);
        draws.Add(new DrawHandleSelectBezierPart(select, handlePosition, handleDepth));

        foreach (SelectBezierPart part in Enum.GetValues(typeof(SelectBezierPart)))
        {
          if (select.bezierPart == part) continue;

          var position = GetPointPartPosition(point, part);
          var depth = GetDepth(position);
          var color = GetHandleColorBySelectPart(part);
          draws.Add(new DrawButtonSelectBezierPart(select, position, depth, part, color));
        }
      }
    }

    public static void DrawCurve(BezierCurve curve, float width = 2)
    {
      foreach (var item in curve)
      {
        DrawBezier(item, Color.white, width);
      }
    }

    public static void DrawCurve(BezierCurve curve, Color color, float width = 2)
    {
      foreach (var item in curve)
      {
        DrawBezier(item, color, width);
      }
    }

    private static void DrawBezier(SectionCurve section, Color color, float width = 2)
    {
      var positionStart = section.GetPosition(0);
      var positionTangentStart = section.CurrentPoint.GetTangentPosition(TangentSelect.Start);
      var positionTangentEnd = section.NextPoint.GetTangentPosition(TangentSelect.End);
      var positionEnd = section.GetPosition(1);

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

    [Flags]
    public enum RotationSetting
    {
      None = 0, Upwards = 1 << 0, Normalize = 1 << 1, Inheritroll = 1 << 2
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

    struct DrawAddButtonBezierPoint : DrawStack
    {
      private readonly SelectCurve select;
      private readonly int index;
      private readonly Vector3 position;
      private readonly float depth;

      public float Depth => depth;
      public float Layer => 5;

      public DrawAddButtonBezierPoint(SelectCurve select, int index)
      {
        this.select = select;
        this.index = index;
        var section = select.curve.GetSection(index);
        position = section.GetPosition(.5f);
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
  }
}