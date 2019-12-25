using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bezier
{
  public static class BezierPointConverter
  {
    public static List<BezierPoint> LocalToWorldPoints(ICollection<BezierPoint> points, Transform transform)
    {
      return PointsConverter(points, (point) => LocalToWorldPoint(point, transform));
    }

    public static List<BezierPoint> WorldToLocalPoints(ICollection<BezierPoint> points, Transform transform)
    {
      return PointsConverter(points, (point) => WorldToLocalPoint(point, transform));
    }

    public static BezierPoint LocalToWorldPoint(BezierPoint point, Transform transform)
    {
      point.position = transform.TransformPoint(point.Position);
      point.startTangent.position = transform.TransformPoint(point.TangentStart.position) - transform.position;
      point.endTangent.position = transform.TransformPoint(point.TangentEnd.position) - transform.position;

      point.next.position = transform.TransformPoint(point.next.position);
      point.next.tangentPosition = transform.TransformPoint(point.next.tangentPosition);

      return point;
    }

    public static BezierPoint WorldToLocalPoint(BezierPoint point, Transform transform)
    {
      var localPosition = point.position = transform.InverseTransformPoint(point.Position);
      point.startTangent.position = transform.InverseTransformPoint(point.TangentStartWorldPosition) - localPosition;
      point.endTangent.position = transform.InverseTransformPoint(point.TangentEndWorldPosition) - localPosition;

      point.next.position = transform.InverseTransformPoint(point.next.position);
      point.next.tangentPosition = transform.InverseTransformPoint(point.next.tangentPosition);

      return point;
    }

    public static List<BezierPoint> PointsConverter(ICollection<BezierPoint> points, Func<BezierPoint, BezierPoint> converter)
    {
      var convertedPoints = new List<BezierPoint>(points.Count);
      foreach (var point in points)
      {
        convertedPoints.Add(converter.Invoke(point));
      }
      return convertedPoints;
    }
  }
}