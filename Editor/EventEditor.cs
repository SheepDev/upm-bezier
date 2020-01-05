using UnityEditor;
using UnityEngine;
using static Bezier.BezierPoint;
using static Input.InputEditor;

namespace Bezier
{
  public class EventEditor : EditorBehaviour<SelectCurve>
  {
    public override void Reset()
    {
    }

    public override void BeforeSceneGUI(SelectCurve select)
    {
      if (GetKeyUp(KeyCode.C, out var cEvent))
      {
        BezierGUI3D.IsSplitSpline = !BezierGUI3D.IsSplitSpline;
        cEvent.Use();
      }
    }

    public override void SceneGUI(SceneView view)
    {
      if (GetKeyDown(KeyCode.L, out var lEvent))
      {
        foreach (var item in BezierCurveEditor.SelectCurvers)
        {
          item.Value.LoopToggle();
        }

        lEvent.Use();
      }

      if (GetKeyDown(KeyCode.F, out var fEvent))
      {
        Frame(view);
        fEvent.Use();
      }
    }

    private void Frame(SceneView view)
    {
      var isSelectPointBounds = false;

      foreach (var item in BezierCurveEditor.SelectCurvers)
      {
        if (item.Value.IsSelectPoint)
        {
          isSelectPointBounds = true;
          break;
        }
      }

      view.Frame((isSelectPointBounds) ? SelectPointBounds() : CurveBounds());
    }

    private Bounds SelectPointBounds()
    {
      var bounds = new Bounds();

      foreach (var item in BezierCurveEditor.SelectCurvers)
      {
        var select = item.Value;
        if (!select.IsSelectPoint) continue;

        var selectPoint = select.GetSelectPoint();

        bounds.center = selectPoint.WorldPosition;
        bounds.Encapsulate(selectPoint.GetTangentPosition(TangentSelect.Start));
        bounds.Encapsulate(selectPoint.GetTangentPosition(TangentSelect.End));
      }

      return bounds;
    }

    private Bounds CurveBounds()
    {
      var bounds = new Bounds();

      foreach (var item in BezierCurveEditor.SelectCurvers)
      {
        var curve = item.Value.curve;
        for (var index = 0; index < curve.Lenght; index++)
        {
          var point = curve.GetPoint(index);
          if (index == 0) bounds.center = point.WorldPosition;

          bounds.Encapsulate(point.WorldPosition);
          bounds.Encapsulate(point.GetTangentPosition(TangentSelect.Start));
          bounds.Encapsulate(point.GetTangentPosition(TangentSelect.End));
        }
      }

      return bounds;
    }
  }
}