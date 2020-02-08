using UnityEditor;
using System.Collections.Generic;
using UnityEngine;
using static SheepDev.Bezier.BezierPoint;

namespace SheepDev.Bezier
{
  [CustomEditor(typeof(BezierCurve), true)]
  public class BezierCurveEditor : Editor
  {
    public static SelectCurve ActiveCurve;
    private static EditorBehaviour<SelectCurve>[] behaviours;

    static BezierCurveEditor()
    {
      behaviours = new EditorBehaviour<SelectCurve>[2];
      behaviours[0] = new EventEditor();
      behaviours[1] = new BezierGUI3D();
    }

    private void OnEnable()
    {
      var activeCurve = new SelectCurve(target as BezierCurve);
      var isEqual = ActiveCurve != null && ActiveCurve.Equals(activeCurve);

      if (!isEqual)
      {
        activeCurve.repaint += Repaint;
        ActiveCurve = activeCurve;
      }
    }

    private void OnDisable()
    {
      if (ActiveCurve.IsEdit && Selection.activeGameObject == null)
      {
        Selection.activeGameObject = ActiveCurve.curve.gameObject;
      }
      else
      {
        ActiveCurve.Edit(false);
      }
    }

    private void OnSceneGUI()
    {
      foreach (var behaviour in behaviours)
      {
        behaviour.Reset();
        behaviour.BeforeSceneGUI(ActiveCurve);
        behaviour.SceneGUI(SceneView.lastActiveSceneView);
      }
    }

    public override void OnInspectorGUI()
    {
      EditButtonGUI();

      var isLoopProperty = serializedObject.FindProperty("isLoop");
      EditorGUILayout.PropertyField(isLoopProperty);

      if (ActiveCurve.IsEdit && ActiveCurve.IsSelectPoint)
      {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Active Point: " + ActiveCurve.PointIndex);
        var point = ActiveCurve.GetSelectPoint();
        var newTangentStartType = EnumTangentTypeGUI(point.GetTangentType(TangentSelect.Start), "Tangent Start Type");
        var newTangentEndType = EnumTangentTypeGUI(point.GetTangentType(TangentSelect.End), "Tangent End Type");
        var roll = EditorGUILayout.FloatField("Roll", point.GetRoll());

        point.SetTangentType(newTangentStartType, TangentSelect.Start);
        point.SetTangentType(newTangentEndType, TangentSelect.End);
        point.SetRoll(roll);

        if (ActiveCurve.SetSelectPoint(point))
        {
          SceneView.RepaintAll();
        }
      }

      serializedObject.ApplyModifiedProperties();

      foreach (var behaviour in behaviours)
      {
        behaviour.InspectorGUI();
      }

      base.OnInspectorGUI();
    }

    private void EditButtonGUI()
    {
      var text = (ActiveCurve.IsEdit) ? "Finish" : "Start Edit";
      if (GUILayout.Button(text)) ActiveCurve.EditToggle();
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
    private int pointIndex;

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
      if (curve.Lenght > 2)
      {
        Undo.RecordObject(curve, "Remove Point Curver");
        pointIndex = -1;
        curve.RemovePoint(index);
      }
    }

    public void SetPointIndex(int value)
    {
      pointIndex = Mathf.Clamp(value, -1, curve.Lenght);
      repaint.Invoke();
    }

    public bool SetSelectPoint(BezierPoint point)
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

    public BezierPoint GetSelectPoint() => (IsSelectPoint) ? curve.GetPoint(PointIndex) : default;

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

  public enum SelectBezierPart
  {
    Point, TangentStart, TangentEnd
  }
}