using UnityEditor;
using System.Collections.Generic;

namespace Bezier
{
  [CustomEditor(typeof(BezierCurve), true)]
  public class BezierCurveEditor : Editor
  {
    private static EditorBehaviour<SelectCurve>[] behaviours;
    public static Dictionary<int, SelectCurve> SelectCurvers { get; private set; }
    public static bool IsShowRotationHandle { get; internal set; }
    public static float Distance { get; internal set; }

    static BezierCurveEditor()
    {
      SceneView.duringSceneGui += UpdateScene;
      Distance = 2;
      SelectCurvers = new Dictionary<int, SelectCurve>();
      behaviours = new EditorBehaviour<SelectCurve>[3];
      behaviours[0] = new EventEditor();
      behaviours[1] = new BezierGUI3D();
      behaviours[2] = new BezierGUI2D();
    }

    private static void UpdateScene(SceneView view)
    {
      var curveCount = SelectCurvers.Count;
      if (curveCount == 0) return;

      ValidSelectCurve();

      foreach (var behaviour in behaviours)
      {
        behaviour.Reset();

        foreach (var item in SelectCurvers)
        {
          behaviour.BeforeSceneGUI(item.Value);
        }

        behaviour.SceneGUI(view);
      }
    }

    public static void HiddenTool()
    {
      foreach (var item in SelectCurvers)
      {
        if (item.Value.IsEdit)
        {
          Tools.hidden = true;
          return;
        }
      }

      Tools.hidden = false;
    }

    public override void OnInspectorGUI()
    {
      base.OnInspectorGUI();
    }

    private static void ValidSelectCurve()
    {
      var keys = new List<int>(SelectCurvers.Keys);
      foreach (var key in keys)
      {
        var curve = SelectCurvers[key].curve;
        if (curve == null || curve.gameObject is null)
        {
          SelectCurvers.Remove(key);
        }
      }
    }

    [DrawGizmo(GizmoType.NotInSelectionHierarchy)]
    static void DrawGizmoBezierCurveNonSelected(BezierCurve script, GizmoType gizmoType)
    {
      BezierGUI3D.DrawCurve(script);

      var hashcode = script.GetHashCode();
      if (SelectCurvers.TryGetValue(hashcode, out var select))
      {
        if (!select.IsEdit)
        {
          SelectCurvers.Remove(hashcode);
        }
        else
        {
          select.isSelectInHierarchy = false;
          SelectCurvers[hashcode] = select;
        }
      }
    }

    [DrawGizmo(GizmoType.InSelectionHierarchy)]
    static void DrawGizmoBezierCurveSelected(BezierCurve script, GizmoType gizmoType)
    {
      var hashcode = script.GetHashCode();
      if (!SelectCurvers.ContainsKey(hashcode))
      {
        var selectCurve = new SelectCurve(script);
        SelectCurvers.Add(hashcode, selectCurve);
      }
      else
      {
        var select = SelectCurvers[hashcode];
        select.isSelectInHierarchy = true;
        SelectCurvers[hashcode] = select;
      }
    }
  }

  public enum SelectBezierPart
  {
    Point, TangentStart, TangentEnd
  }

  public class SelectCurve
  {
    public BezierCurve curve;
    public SelectBezierPart bezierPart;
    public bool isSelectInHierarchy;
    private int pointIndex;
    private bool isEdit;

    public bool IsEdit => isEdit;
    public bool IsSelectPoint => PointIndex >= 0;

    public int PointIndex => pointIndex;

    public SelectCurve(BezierCurve curve)
    {
      this.curve = curve;
      this.isSelectInHierarchy = true;
      this.SetIsEdit(false);
    }

    public void AddPoint(int index, float t)
    {
      Undo.RecordObject(curve, "Split Curver");
      curve.Split(index, t);
    }

    public void RemovePoint(int index)
    {
      if (curve.Lenght > 2)
      {
        Undo.RecordObject(curve, "Remove Point Curver");
        pointIndex = -1;
        curve.RemovePoint(index);
      }
    }

    public void SetPointIndex(int value)
    {
      pointIndex = value;
    }

    public void SetIsEdit(bool value)
    {
      isEdit = value;
      BezierCurveEditor.HiddenTool();

      if (!value) SetPointIndex(-1);
    }

    public void SetLoop(bool isLoop)
    {
      if (curve.IsLoop != isLoop)
      {
        Undo.RecordObject(curve, "Set Curver Loop in " + curve.GetHashCode());
        curve.SetLoop(isLoop);
      }
    }

    public void SetSelectPoint(BezierPoint point)
    {
      if (PointIndex < 0) return;
      var oldPoint = curve.GetPoint(PointIndex);

      if (!oldPoint.Equals(point))
      {
        Undo.RecordObject(curve, "Set Point " + PointIndex + " in " + curve.GetHashCode());
        curve.SetPoint(PointIndex, point);
      }
    }

    public BezierPoint GetSelectPoint()
    {
      return (IsSelectPoint) ? curve.GetPoint(PointIndex) : default;
    }

    public void LoopToggle()
    {
      SetLoop(!curve.IsLoop);
    }

    public void EditToggle()
    {
      SetIsEdit(!IsEdit);
    }

    public override bool Equals(object obj)
    {
      return obj is SelectCurve curve &&
             EqualityComparer<BezierCurve>.Default.Equals(this.curve, curve.curve);
    }

    public override int GetHashCode()
    {
      return 211918132 + EqualityComparer<BezierCurve>.Default.GetHashCode(curve);
    }
  }
}