using System;
using UnityEngine;

namespace Bezier
{
  public static class BezierUtility
  {
    public static Point[] LocalToWorldPoints(Point[] points, Transform transform)
    {
      return ConvertPoints(points, (point) => LocalToWorldPoint(point, transform));
    }

    public static Point[] WorldToLocalPoints(Point[] points, Transform transform)
    {
      return ConvertPoints(points, (point) => WorldToLocalPoint(point, transform));
    }

    public static Point LocalToWorldPoint(Point point, Transform transform)
    {
      var newPosition = transform.TransformPoint(point.Position);
      var newStartTangent = transform.TransformPoint(point.StartTangentLocalPosition);
      var newEndTangent = transform.TransformPoint(point.EndTangentLocalPosition);

      return new Point(newPosition, new Tangent(newStartTangent, point.StartTangentType), new Tangent(newEndTangent, point.EndTangentType));
    }

    public static Point WorldToLocalPoint(Point point, Transform transform)
    {
      var newPosition = transform.InverseTransformPoint(point.Position);
      var newStartTangent = transform.InverseTransformPoint(point.StartTangentLocalPosition);
      var newEndTangent = transform.InverseTransformPoint(point.EndTangentLocalPosition);

      return new Point(newPosition, new Tangent(newStartTangent, point.StartTangentType), new Tangent(newEndTangent, point.EndTangentType));
    }

    private static Point[] ConvertPoints(Point[] points, Func<Point, Point> action)
    {
      var pointCount = points.Length;
      var newPoints = new Point[pointCount];

      for (int index = 0; index < pointCount; index++)
      {
        var point = points[index];
        newPoints[index] = action.Invoke(point);
      }

      return newPoints;
    }
  }
}
