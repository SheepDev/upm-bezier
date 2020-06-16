using UnityEditor;
using UnityEngine;
using static SheepDev.Bezier.Point;
using static SheepDev.Input.InputEditor;

namespace SheepDev.Bezier
{
  public class EventEditor : EditorBehaviour<SelectCurve>
  {
    public SelectCurve selectCurve;

    public override void Reset()
    {
    }

    public override void BeforeSceneGUI(SelectCurve select)
    {
      if (!select.IsEdit) return;
      selectCurve = select;

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
      var isSelectBounds = selectCurve.IsEdit && selectCurve.IsSelectPoint;
      var bounds = isSelectBounds ? SelectPointBounds(selectCurve) : CurveBounds(selectCurve);
      view.Frame(bounds);
    }

    private Bounds SelectPointBounds(SelectCurve selectCurve)
    {
      var bounds = new Bounds();
      var selectPoint = selectCurve.GetSelectPoint();
      bounds.center = selectPoint.position;
      bounds.Encapsulate(selectPoint.GetTangentPosition(TangentSelect.Start));
      bounds.Encapsulate(selectPoint.GetTangentPosition(TangentSelect.End));

      return bounds;
    }

    private Bounds CurveBounds(SelectCurve selectCurve)
    {
      var bounds = new Bounds();
      var curve = selectCurve.Curve;

      for (var index = 0; index < curve.PointLenght; index++)
      {
        var point = curve.GetPoint(index);
        if (index == 0) bounds.center = point.position;

        bounds.Encapsulate(point.position);
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