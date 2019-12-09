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

      return new Point(newPosition, new Tangent(newStartTangent, point.StartTangentType), new Tangent(newEndTangent, point.EndTangentType));
    }

    public static Point WorldToLocalPoint(Point point, Transform transform)
    {
      var newPosition = transform.InverseTransformPoint(point.Position);
      var newStartTangent = transform.InverseTransformPoint(point.StartTangentPosition) - newPosition;
      var newEndTangent = transform.InverseTransformPoint(point.EndTangentPosition) - newPosition;

      return new Point(newPosition, new Tangent(newStartTangent, point.StartTangentType), new Tangent(newEndTangent, point.EndTangentType));
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
