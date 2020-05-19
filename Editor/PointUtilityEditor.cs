using SheepDev.Bezier;
using UnityEditor;
using UnityEngine;

public static class PointUtilityEditor
{
  public static void SetPoint(SerializedProperty pointProperty, Point point, Matrix4x4 worldToLocal)
  {
    point = Point.ConvertPoint(point, worldToLocal);
    pointProperty.FindPropertyRelative("position").vector3Value = point.position;

    var tangentStart = pointProperty.FindPropertyRelative("tangentStart");
    tangentStart.FindPropertyRelative("position").vector3Value = point.tangentStart.position;

    var tangentEnd = pointProperty.FindPropertyRelative("tangentEnd");
    tangentEnd.FindPropertyRelative("position").vector3Value = point.tangentEnd.position;
  }
}