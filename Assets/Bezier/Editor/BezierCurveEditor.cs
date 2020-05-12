using UnityEditor;
using System.Collections.Generic;
using UnityEngine;
using static SheepDev.Bezier.Point;

namespace SheepDev.Bezier
{
  [CustomEditor(typeof(BezierCurve), true)]
  public class BezierCurveEditor : Editor
  {
    private static EditorBehaviour<SelectCurve>[] behaviours;
    public SelectCurve activeCurve;

    private static bool IsEdit;
    private static int SelectIndexPoint;

    static BezierCurveEditor()
    {
      behaviours = new EditorBehaviour<SelectCurve>[2];
      behaviours[0] = new EventEditor();
      behaviours[1] = new BezierGUI3D();
    }

    private void OnEnable()
    {
      var activeCurve = new SelectCurve(target as BezierCurve);
      var isEqual = this.activeCurve != null && this.activeCurve.Equals(activeCurve);

      if (!isEqual)
      {
        activeCurve.repaint += Repaint;
        activeCurve.Edit(IsEdit);
        activeCurve.pointIndex = SelectIndexPoint;
        this.activeCurve = activeCurve;
      }
    }

    private void OnDisable()
    {
      Tools.hidden = false;

      if (activeCurve.IsEdit && Selection.activeGameObject == null)
      {
        IsEdit = activeCurve.IsEdit;
        SelectIndexPoint = activeCurve.pointIndex;
        Selection.activeGameObject = activeCurve.curve.gameObject;
      }
      else
      {
        IsEdit = false;
        SelectIndexPoint = -1;
      }
    }

    private void OnSceneGUI()
    {
      foreach (var behaviour in behaviours)
      {
        behaviour.Reset();
        behaviour.BeforeSceneGUI(activeCurve);
        behaviour.SceneGUI(SceneView.lastActiveSceneView);
      }
    }

    public override void OnInspectorGUI()
    {
      EditButtonGUI();

      var isLoopProperty = serializedObject.FindProperty("isLoop");
      EditorGUILayout.PropertyField(isLoopProperty);

      if (activeCurve.IsEdit && activeCurve.IsSelectPoint)
      {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Active Point: " + activeCurve.PointIndex);
        var point = activeCurve.GetSelectPoint();
        var newTangentStartType = EnumTangentTypeGUI(point.GetTangentType(TangentSelect.Start), "Tangent Start Type");
        var newTangentEndType = EnumTangentTypeGUI(point.GetTangentType(TangentSelect.End), "Tangent End Type");
        var roll = EditorGUILayout.FloatField("Roll", point.roll);

        point.SetTangentType(newTangentStartType, TangentSelect.Start);
        point.SetTangentType(newTangentEndType, TangentSelect.End);
        point.roll = roll;

        if (activeCurve.SetSelectPoint(point))
        {
          SceneView.RepaintAll();
        }
      }

      var property = serializedObject.FindProperty("onUpdated");

      do
      {
        EditorGUILayout.PropertyField(property);
      } while (property.Next(false));

      serializedObject.ApplyModifiedProperties();

      foreach (var behaviour in behaviours)
      {
        behaviour.InspectorGUI();
      }
    }

    private void EditButtonGUI()
    {
      var text = (activeCurve.IsEdit) ? "Finish" : "Start Edit";
      if (GUILayout.Button(text)) activeCurve.EditToggle();
    }

    private TangentType EnumTangentTypeGUI(TangentType selected, string name)
    {
      return (TangentType)EditorGUILayout.EnumPopup(name, selected);
    }

    [DrawGizmo(GizmoType.NotInSelectionHierarchy)]
    static void DrawGizmoBezierCurveNonSelected(BezierCurve script, GizmoType gizmoType)
    {
      BezierGUI3D.DrawCurve(script);
    }

    public static void RepaintIfChange(System.Object obj, System.Object other)
    {
      if (!obj.Equals(other))
      {
        SceneView.RepaintAll();
      }
    }
  }

  public class SelectCurve
  {
    public BezierCurve curve;
    public SelectBezierPart bezierPart;

    private bool isEdit;
    internal int pointIndex;

    public delegate void Callback();
    public Callback repaint;

    public bool IsEdit => isEdit;
    public int PointIndex => pointIndex;
    public bool IsSelectPoint => PointIndex >= 0;

    public SelectCurve(BezierCurve curve)
    {
      this.curve = curve;
      pointIndex = -1;
    }

    public void EditToggle()
    {
      Edit(!IsEdit);
    }

    public void Edit(bool isEdit)
    {
      this.isEdit = isEdit;
      Tools.hidden = isEdit;

      if (!isEdit) SetPointIndex(-1);
      SceneView.RepaintAll();
    }

    public void AddPoint(int index, float t)
    {
      Undo.RecordObject(curve, "Split Curver");
      curve.Split(index, t);
    }

    public void RemovePoint(int index)
    {
      if (curve.PointLenght > 2)
      {
        Undo.RecordObject(curve, "Remove Point Curver");
        pointIndex = -1;
        curve.Remove(index);
      }
    }

    public void SetPointIndex(int value)
    {
      pointIndex = Mathf.Clamp(value, -1, curve.PointLenght);
      repaint.Invoke();
    }

    public bool SetSelectPoint(Point point)
    {
      var oldPoint = curve.GetPoint(PointIndex);

      if (!oldPoint.Equals(point))
      {
        Undo.RecordObject(curve, "Set Point " + PointIndex + " in " + curve.GetHashCode());
        curve.SetPoint(PointIndex, point);
        return true;
      }

      return false;
    }

    public Point GetSelectPoint() => (IsSelectPoint) ? curve.GetPoint(PointIndex) : default;

    public override bool Equals(object obj)
    {
      return obj is SelectCurve select && select.curve == this.curve;
    }

    public override int GetHashCode()
    {
      return 211918132 + EqualityComparer<BezierCurve>.Default.GetHashCode(curve);
    }
  }

  public enum SelectBezierPart
  {
    Point, TangentStart, TangentEnd
  }
}