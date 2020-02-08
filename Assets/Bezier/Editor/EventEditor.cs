using UnityEditor;
using UnityEngine;
using static SheepDev.Bezier.BezierPoint;
using static SheepDev.Input.InputEditor;

namespace SheepDev.Bezier
{
  public class EventEditor : EditorBehaviour<SelectCurve>
  {
    public override void Reset()
    {
    }

    public override void BeforeSceneGUI(SelectCurve select)
    {
      if (!select.IsEdit) return;

      if (GetKeyUp(KeyCode.C, out var cEvent))
      {
        BezierGUI3D.IsSplitSpline = !BezierGUI3D.IsSplitSpline;
        if (BezierGUI3D.IsSplitSpline)
        {
          BezierGUI3D.IsRemovePoint = false;
        }
        cEvent.Use();
      }

      if (GetKeyUp(KeyCode.V, out var vEvent))
      {
        BezierGUI3D.IsRemovePoint = !BezierGUI3D.IsRemovePoint;
        if (BezierGUI3D.IsRemovePoint)
        {
          BezierGUI3D.IsSplitSpline = false;
        }
        vEvent.Use();
      }
    }

    public override void SceneGUI(SceneView view)
    {
      if (GetKeyDown(KeyCode.F, out var fEvent))
      {
        Frame(view);
        fEvent.Use();
      }
    }

    private void Frame(SceneView view)
    {
      var selectCurve = BezierCurveEditor.ActiveCurve;
      var isSelectBounds = selectCurve.IsEdit && selectCurve.IsSelectPoint;
      var bounds = isSelectBounds ? SelectPointBounds(selectCurve) : CurveBounds(selectCurve);
      view.Frame(bounds);
    }

    private Bounds SelectPointBounds(SelectCurve selectCurve)
    {
      var bounds = new Bounds();
      var selectPoint = selectCurve.GetSelectPoint();
      bounds.center = selectPoint.WorldPosition;
      bounds.Encapsulate(selectPoint.GetTangentPosition(TangentSelect.Start));
      bounds.Encapsulate(selectPoint.GetTangentPosition(TangentSelect.End));

      return bounds;
    }

    private Bounds CurveBounds(SelectCurve selectCurve)
    {
      var bounds = new Bounds();
      var curve = selectCurve.curve;

      for (var index = 0; index < curve.Lenght; index++)
      {
        var point = curve.GetPoint(index);
        if (index == 0) bounds.center = point.WorldPosition;

        bounds.Encapsulate(point.WorldPosition);
        bounds.Encapsulate(point.GetTangentPosition(TangentSelect.Start));
        bounds.Encapsulate(point.GetTangentPosition(TangentSelect.End));
      }

      return bounds;
    }

    public override void InspectorGUI()
    {
    }
  }
}