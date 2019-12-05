using UnityEngine;
using UnityEditor;
using System;
using Bezier;

[CustomEditor(typeof(BezierCurver), true)]
public class BezierCurverEditor : Editor
{
  private static BezierEdit activeBezier;

  private BezierCurver script;
  private Transform transform;

  private int? activePointIndex;
  private HandleType activeHandleType;

  private void OnEnable()
  {
    script = target as BezierCurver;
    transform = script.transform;

    activePointIndex = null;
    activeHandleType = HandleType.Point;

    SceneView.duringSceneGui -= activeBezier.SceneGUI;

    if (activeBezier.curver != script)
    {
      activeBezier.curver = script;
      activeBezier.isEdit = false;
      activeBezier.SceneGUI = SceneGUI;
    }
  }

  private void OnDisable()
  {
    if (activeBezier.curver != null && activeBezier.isEdit)
    {
      SceneView.duringSceneGui += activeBezier.SceneGUI;
    }
  }

  private void SceneGUI(SceneView view)
  {
    Handles.BeginGUI();
    if (GUI.Button(new Rect(10, 10, 150, 40), "Finish Edit Bezier"))
    {
      activeBezier.isEdit = false;
      SceneView.duringSceneGui -= activeBezier.SceneGUI;
      return;
    }
    Handles.EndGUI();

    OnSceneGUI();
  }

  private void OnSceneGUI()
  {
    var worldpoints = BezierUtility.LocalToWorldPoints(script.points, transform);

    DrawBezier(worldpoints);

    for (int index = 0; index < worldpoints.Length; index++)
    {
      Point point = worldpoints[index];
      var isActivePoint = activePointIndex.HasValue && index == activePointIndex;

      if (!isActivePoint)
      {
        SceneButton(point.Position, () => SetActivePointIndex(index), Color.red, Handles.SphereHandleCap);
      }
      else
      {
        EditorGUI.BeginChangeCheck();
        worldpoints[activePointIndex.Value] = DrawHandlerActivePoint(worldpoints[activePointIndex.Value]);
        if (EditorGUI.EndChangeCheck())
        {
          Undo.RecordObject(target, "Set Points");
        }
      }
    }

    var localPoints = BezierUtility.WorldToLocalPoints(worldpoints, transform);
    script.points = localPoints;
  }

  public override void OnInspectorGUI()
  {
    if (activeBezier.curver != null)
    {
      activeBezier.isEdit = EditorGUILayout.Toggle("Edit Bezier", activeBezier.isEdit);
    }
  }

  private void SetActivePointIndex(int index)
  {
    activePointIndex = index;
    activeHandleType = HandleType.Point;
  }

  private void DrawBezier(Point[] points)
  {
    for (int index = 0; index < points.Length - 1; index++)
    {
      Point point = points[index];
      Point nextPoint = points[index + 1];
      var isActivePoint = activePointIndex.HasValue && index == activePointIndex || index == activePointIndex - 1;
      var color = (isActivePoint) ? Color.green : Color.white;

      Handles.DrawBezier(point.Position, nextPoint.Position, point.StartTangentPosition, nextPoint.EndTangentPosition, color, null, 2f);
    }
  }

  private Point DrawHandlerActivePoint(Point point)
  {
    switch (activeHandleType)
    {
      case HandleType.Point:
        point.SetPosition(Handles.PositionHandle(point.Position, Quaternion.identity));
        break;
      case HandleType.StartTangent:
        var newStartPosition = Handles.PositionHandle(point.StartTangentPosition, Quaternion.identity);
        point.SetTangentPosition(newStartPosition, TangentSpace.Start);
        break;
      case HandleType.EndTangent:
        point.SetTangentPosition(Handles.PositionHandle(point.EndTangentPosition, Quaternion.identity), TangentSpace.End);
        break;
    }

    if (activeHandleType != HandleType.Point)
    {
      var color = GetHandlerColor(HandleType.Point);
      SceneButton(point.Position, () => { activeHandleType = HandleType.Point; }, color, Handles.DotHandleCap);
    }
    if (activeHandleType != HandleType.StartTangent)
    {
      var color = GetHandlerColor(HandleType.StartTangent);
      SceneButton(point.StartTangentPosition, () => { activeHandleType = HandleType.StartTangent; }, color, Handles.DotHandleCap);
    }
    if (activeHandleType != HandleType.EndTangent)
    {
      var color = GetHandlerColor(HandleType.EndTangent);
      SceneButton(point.EndTangentPosition, () => { activeHandleType = HandleType.EndTangent; }, color, Handles.DotHandleCap);
    }

    return point;
  }

  private void SceneButton(Vector3 worldPosition, Action triggerAction, Color color, Handles.CapFunction capFunction)
  {
    SetHandlerColor(color);
    var size = HandleUtility.GetHandleSize(worldPosition) / 3;
    var pickSize = size / 2;

    var isPress = Handles.Button(worldPosition, Quaternion.identity, size, pickSize, capFunction);
    if (isPress)
    {
      triggerAction.Invoke();
    }
  }

  private void SetHandlerColor(Color color)
  {
    Handles.color = color;
  }

  private Color GetHandlerColor(HandleType type)
  {
    switch (type)
    {
      case HandleType.Point:
        return Color.yellow;
      case HandleType.StartTangent:
        return Color.blue;
      case HandleType.EndTangent:
        return Color.red;
      default:
        return Color.white;
    }
  }

  enum HandleType
  {
    Point, StartTangent, EndTangent
  }

  struct BezierEdit
  {
    public BezierCurver curver;
    public bool isEdit;
    public Action<SceneView> SceneGUI;
  }
}
