using UnityEditor;
using UnityEngine;
using static Bezier.BezierPoint;
using static Input.InputEditor;

namespace Bezier
{
  public class EventEditor : EditorBehaviour
  {
    public override void OnUpdate(float deltaTime)
    {
    }

    public override void OnSceneGUI(SceneView view)
    {
      var curveEditor = BezierCurveEditor.activeCurve;
      var curve = curveEditor.Curve;

      if (GetKeyDown(KeyCode.F, out var fEvent))
      {
        Frame(view);
        fEvent.Use();
      }

      if (GetKeyDown(KeyCode.B, out var bEvent))
      {
        curveEditor.EditToggle();
        bEvent.Use();
      }

      if (GetKeyDown(KeyCode.L, out var lEvent))
      {
        curveEditor.SetLoop(!curveEditor.Curve.IsLoop);
        lEvent.Use();
      }

      if (!curveEditor.IsEdit) return;

      var random = Random.Range(0, 10);

      if (GetKeyUp(KeyCode.C, out var cEvent))
      {
        var lastWorldpoint = curve.GetPoint(curve.Lenght - 1);
        AddPoint(lastWorldpoint);
        cEvent.Use();
      }

      if (GetKeyDown(KeyCode.Escape, out var escapeEvent))
      {
        BezierCurveEditor.activeCurve.DisableActivePointIndex();
        escapeEvent.Use();
      }
    }

    private void Frame(SceneView view)
    {
      var curveEditor = BezierCurveEditor.activeCurve;
      var bounds = new Bounds();

      if (curveEditor.IsActivePointIndex)
      {
        var activePoint = curveEditor.ActivePoint;
        bounds.center = activePoint.WorldPosition;
        bounds.Encapsulate(activePoint.GetTangentPosition(TangentSelect.Start));
        bounds.Encapsulate(activePoint.GetTangentPosition(TangentSelect.End));
      }
      else
      {
        var curve = curveEditor.Curve;
        for (var index = 0; index < curve.Lenght; index++)
        {
          var point = curve.GetPoint(index);
          if (index == 0) bounds.center = point.WorldPosition;
          bounds.Encapsulate(point.WorldPosition);
          bounds.Encapsulate(point.GetTangentPosition(TangentSelect.Start));
          bounds.Encapsulate(point.GetTangentPosition(TangentSelect.End));
        }
      }

      view.Frame(bounds);
    }

    private void AddPoint(BezierPoint lastPoint)
    {
      var depth = HandleUtility.WorldToGUIPointWithDepth(lastPoint.WorldPosition).z;
      var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

      var pointWorldPosition = ray.origin + ray.direction * depth;
      var tangentEndPosition = (lastPoint.GetTangentPosition(TangentSelect.Start) + pointWorldPosition) / 2;
      var size = Vector3.Distance(tangentEndPosition, pointWorldPosition) / 2;

      var tangentStart = new Tangent(Vector3.right * size, TangentType.Aligned);
      var tangentEnd = new Tangent(-Vector3.right, TangentType.Aligned);

      var newPoint = new BezierPoint(Vector3.zero, tangentStart, tangentEnd);
      newPoint.CopyMatrix(lastPoint);
      newPoint.SetPosition(pointWorldPosition);
      newPoint.SetTangentPosition(tangentEndPosition, TangentSelect.End);

      BezierCurveEditor.activeCurve.AddPoint(newPoint);
    }
  }
}