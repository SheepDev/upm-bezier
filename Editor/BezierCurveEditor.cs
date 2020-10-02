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
    private static SelectCurve activeCurve;

    static BezierCurveEditor()
    {
      behaviours = new EditorBehaviour<SelectCurve>[2];
      behaviours[0] = new EventEditor();
      behaviours[1] = new BezierGUI3D();
    }

    private void OnEnable()
    {
      activeCurve = new SelectCurve(serializedObject);
      activeCurve.repaint += Repaint;
    }

    private void OnDisable()
    {
      Tools.hidden = false;
    }

    private void OnSceneGUI()
    {
      if (activeCurve == null || !activeCurve.IsValid)
        return;

      foreach (var behaviour in behaviours)
      {
        behaviour.Reset();
        behaviour.BeforeSceneGUI(activeCurve);
        behaviour.SceneGUI(SceneView.lastActiveSceneView);
      }

      activeCurve.Save();
    }

    public override void OnInspectorGUI()
    {
      EditButtonGUI();

      var serializedObject = activeCurve.SerializedObject;
      EditorGUILayout.PropertyField(serializedObject.FindProperty("isLoop"));
      EditorGUILayout.PropertyField(serializedObject.FindProperty("onUpdated"));

      if (GUILayout.Button("Call Update event"))
        activeCurve.Curve.onUpdated.Invoke();

      if (GUILayout.Button("Force Data Update"))
      {
        activeCurve.Curve.ForceUpdateData();
        activeCurve.Curve.onUpdated.Invoke();
      }

      if (activeCurve.IsEdit && activeCurve.IsSelectPoint)
      {
        var dataProperty = serializedObject.FindProperty("datas");
        var pointDataProperty = dataProperty.GetArrayElementAtIndex(activeCurve.GetPointIndex());
        var pointProperty = pointDataProperty.FindPropertyRelative("point");
        EditorGUILayout.PropertyField(pointProperty, new GUIContent($"Active Point {activeCurve.pointIndex}"));
      }

      activeCurve.Save();

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
    public SelectBezierPart bezierPart;
    private SerializedObject serializedObject;

    internal int pointIndex;

    public delegate void Callback();
    public Callback repaint;

    public bool IsValid => serializedObject != null;
    public bool IsSelectPoint => GetPointIndex() >= 0;
    public SerializedObject SerializedObject => serializedObject;
    public BezierCurve Curve => serializedObject.targetObject as BezierCurve;
    public bool IsEdit { get; private set; }

    public SelectCurve(SerializedObject serializedObject)
    {
      this.serializedObject = serializedObject;
      pointIndex = -1;
    }

    public int GetPointIndex()
    {
      return pointIndex = (int)Mathf.Repeat(pointIndex, Curve.PointLenght);
    }

    public void EditToggle()
    {
      Edit(!IsEdit);
    }

    public void Edit(bool isEdit)
    {
      this.IsEdit = isEdit;
      Tools.hidden = isEdit;

      if (!isEdit) SetPointIndex(-1);
      SceneView.RepaintAll();
    }

    public void AddPoint(int index, float t)
    {
      var curve = Curve;
      var dataProperty = serializedObject.FindProperty("datas");
      var worldToLocalMatrix = curve.GetTransform().worldToLocalMatrix;

      var point = curve.GetPoint(index, Space.World);
      var nextIndex = (int)Mathf.Repeat(index + 1, dataProperty.arraySize);
      var nextPoint = curve.GetPoint(nextIndex, Space.World);
      var splitPoint = MathBezier.Split(point, nextPoint, t, out var tangentStart, out var tangentEnd);

      point.SetTangentPosition(tangentStart, TangentSelect.Start);
      nextPoint.SetTangentPosition(tangentEnd, TangentSelect.End);

      var pointProperty = dataProperty.GetArrayElementAtIndex(index)
        .FindPropertyRelative("point");
      PointUtilityEditor.SetPoint(pointProperty, point, worldToLocalMatrix);

      var nextPointProperty = dataProperty.GetArrayElementAtIndex(nextIndex)
        .FindPropertyRelative("point");
      PointUtilityEditor.SetPoint(nextPointProperty, nextPoint, worldToLocalMatrix);

      var insertIndex = index + 1;
      dataProperty.InsertArrayElementAtIndex(insertIndex);
      var splitPointDataProperty = dataProperty.GetArrayElementAtIndex(insertIndex)
        .FindPropertyRelative("point");
      PointUtilityEditor.SetPoint(splitPointDataProperty, splitPoint, worldToLocalMatrix);

      pointIndex = insertIndex;
    }

    public void RemovePoint(int index)
    {
      var dataProperty = serializedObject.FindProperty("datas");
      dataProperty.DeleteArrayElementAtIndex(index);
    }

    public void SetPointIndex(int value)
    {
      pointIndex = Mathf.Clamp(value, -1, Curve.PointLenght);
      repaint.Invoke();
    }

    public Point GetSelectPoint()
    {
      return IsSelectPoint ? Curve.GetPoint(GetPointIndex()) : default;
    }

    public void Save()
    {
      if (serializedObject.hasModifiedProperties)
      {
        serializedObject.ApplyModifiedProperties();
      }
    }

    public override bool Equals(object obj)
    {
      return obj is SelectCurve select && select.Curve == this.Curve;
    }

    public override int GetHashCode()
    {
      return 211918132 + EqualityComparer<BezierCurve>.Default.GetHashCode(Curve);
    }
  }

  public enum SelectBezierPart
  {
    Point, TangentStart, TangentEnd
  }
}