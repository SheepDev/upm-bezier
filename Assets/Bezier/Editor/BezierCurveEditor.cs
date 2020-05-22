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
      var activeCurve = new SelectCurve(target as BezierCurve, serializedObject);
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

      if (activeCurve.SerializedObject.hasModifiedProperties)
      {
        activeCurve.SerializedObject.ApplyModifiedProperties();
      }
    }

    public override void OnInspectorGUI()
    {
      EditButtonGUI();

      var serializedObject = activeCurve.SerializedObject;
      EditorGUILayout.PropertyField(serializedObject.FindProperty("isLoop"));
      EditorGUILayout.PropertyField(serializedObject.FindProperty("onUpdated"));

      if (activeCurve.IsEdit && activeCurve.IsSelectPoint)
      {
        var dataProperty = serializedObject.FindProperty("datas");
        var pointDataProperty = dataProperty.GetArrayElementAtIndex(activeCurve.GetPointIndex());
        var pointProperty = pointDataProperty.FindPropertyRelative("point");
        EditorGUILayout.PropertyField(pointProperty, new GUIContent($"Active Point {activeCurve.pointIndex}"));
      }

      if (serializedObject.hasModifiedProperties)
      {
        serializedObject.ApplyModifiedProperties();
      }

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
    private SerializedObject serializedCurve;

    private bool isEdit;
    internal int pointIndex;

    public delegate void Callback();
    public Callback repaint;

    public bool IsEdit => isEdit;
    public bool IsSelectPoint => GetPointIndex() >= 0;
    public SerializedObject SerializedObject => serializedCurve;

    public SelectCurve(BezierCurve curve, SerializedObject serializedObject)
    {
      this.curve = curve;
      serializedCurve = serializedObject;
      pointIndex = -1;
    }

    public int GetPointIndex()
    {
      return pointIndex = (int)Mathf.Repeat(pointIndex, curve.PointLenght);
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
      var dataProperty = serializedCurve.FindProperty("datas");
      var worldToLocalMatrix = curve.GetTransform().worldToLocalMatrix;

      var nextIndex = (int)Mathf.Repeat(index + 1, dataProperty.arraySize);
      var point = curve.GetPoint(index, Space.Self);
      var nextPoint = curve.GetPoint(nextIndex, Space.Self);
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
      var dataProperty = serializedCurve.FindProperty("datas");
      dataProperty.DeleteArrayElementAtIndex(index);
    }

    public void SetPointIndex(int value)
    {
      pointIndex = Mathf.Clamp(value, -1, curve.PointLenght);
      repaint.Invoke();
    }

    public Point GetSelectPoint() => (IsSelectPoint) ? curve.GetPoint(GetPointIndex()) : default;

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