using UnityEditor;
using UnityEngine;
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
        var lastWorldpoint = curve.GetWorldPoint(curve.Lenght - 1);
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
        bounds.center = activePoint.Position;
        bounds.Encapsulate(activePoint.TangentStartWorldPosition);
        bounds.Encapsulate(activePoint.TangentEndWorldPosition);
      }
      else
      {
        var worldpoints = curveEditor.Curve.GetWorldPoints();
        bounds.center = worldpoints[0].Position;

        foreach (var point in worldpoints)
        {
          bounds.Encapsulate(point.Position);
          bounds.Encapsulate(point.TangentStartWorldPosition);
          bounds.Encapsulate(point.TangentEndWorldPosition);
        }
      }

      view.Frame(bounds);
    }

    private void AddPoint(BezierPoint lastPoint)
    {
      var depth = HandleUtility.WorldToGUIPointWithDepth(lastPoint.Position).z;
      var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

      var pointPosition = ray.origin + ray.direction * depth;
      var endTangentPosition = (lastPoint.TangentStartWorldPosition + pointPosition) / 2;
      var size = Vector3.Distance(endTangentPosition, pointPosition) / 2;

      var startTangent = new Tangent(Vector3.right * size, TangentType.Aligned);
      var endTangent = new Tangent(-Vector3.right, TangentType.Aligned);

      var point = new BezierPoint(pointPosition, startTangent, endTangent);
      point.SetTangentPosition(endTangentPosition, TangentSpace.End);

      BezierCurveEditor.activeCurve.AddPoint(point);
    }
  }
}