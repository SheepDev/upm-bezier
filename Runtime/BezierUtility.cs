using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bezier
{
  public static class BezierUtility
  {
    public static List<Point> LocalToWorldPoints(ICollection<Point> points, Transform transform)
    {
      return ConvertPoints(points, (point) => LocalToWorldPoint(point, transform));
    }

    public static List<Point> WorldToLocalPoints(ICollection<Point> points, Transform transform)
    {
      return ConvertPoints(points, (point) => WorldToLocalPoint(point, transform));
    }

    public static Point LocalToWorldPoint(Point point, Transform transform)
    {
      var newPosition = transform.TransformPoint(point.Position);
      var newStartTangent = transform.TransformPoint(point.StartTangentLocalPosition) - transform.position;
      var newEndTangent = transform.TransformPoint(point.EndTangentLocalPosition) - transform.position;

      return PointBuilder.Copy(point, newPosition, newStartTangent, newEndTangent);
    }

    public static Point WorldToLocalPoint(Point point, Transform transform)
    {
      var newPosition = transform.InverseTransformPoint(point.Position);
      var newStartTangent = transform.InverseTransformPoint(point.StartTangentPosition) - newPosition;
      var newEndTangent = transform.InverseTransformPoint(point.EndTangentPosition) - newPosition;

      return PointBuilder.Copy(point, newPosition, newStartTangent, newEndTangent);
    }

    public static Vector3 GetTangent(Point p1, Point p2, float t)
    {
      var start = GetCurverInterval(p1, p2, t);
      var end = GetCurverInterval(p1, p2, t + .00001f);
      return (end - start).normalized;
    }

    // Reference: https://en.wikipedia.org/wiki/B%C3%A9zier_curve
    public static Vector3 GetCurverInterval(Point p1, Point p2, float t)
    {
      var b1 = Mathf.Pow((1 - t), 3) * p1.Position;
      var b2 = Mathf.Pow((1 - t), 2) * 3 * t * p1.StartTangentPosition;
      var b3 = Mathf.Pow(t, 2) * 3 * (1 - t) * p2.EndTangentPosition;
      var b4 = Mathf.Pow(t, 3) * p2.Position;
      return b1 + b2 + b3 + b4;
    }

    public static float Distance(Point p1, Point p2, float precision = 10)
    {
      var step = .02f;
      float distance = 0f;

      for (float t = 0; t < 1; t += step)
      {
        var point = GetCurverInterval(p1, p2, t);
        var nextPoint = GetCurverInterval(p1, p2, t + step);
        distance += Vector3.Distance(point, nextPoint);
      }

      return distance;
    }

    public static List<Point> ConvertPoints(ICollection<Point> points, Func<Point, Point> action)
    {
      var newPoints = new List<Point>();
      foreach (var point in points)
      {
        newPoints.Add(action.Invoke(point));

      }
      return newPoints;
    }
  }
}
