using UnityEngine;
using static Bezier.BezierPoint;

namespace Bezier
{
  public static class MathBezier
  {
    public static Vector3 GetTangent(BezierPoint point, BezierPoint nextPoint, float t)
    {
      var positionStart = point.Position;
      var positionTangentStart = point.GetTangentPosition(TangentSelect.Start, Space.Self);
      var positionTangentEnd = nextPoint.GetTangentPosition(TangentSelect.End, Space.Self);
      var positionEnd = nextPoint.Position;

      return GetTangent(positionStart, positionEnd, positionTangentStart, positionTangentEnd, t);
    }

    public static Vector3 GetTangent(Vector3 start, Vector3 End, Vector3 tangentStart, Vector3 tangentEnd, float t)
    {
      var positionStart = CalculateBezier(start, End, tangentStart, tangentEnd, t);
      var positionEnd = CalculateBezier(start, End, tangentStart, tangentEnd, t + .00001f);
      return (positionEnd - positionStart).normalized;
    }

    public static Vector3 GetIntervalPosition(BezierPoint point, BezierPoint nextPoint, float t, Space space = Space.World)
    {
      var positionStart = (space == Space.World) ? point.WorldPosition : point.Position;
      var positionTangentStart = point.GetTangentPosition(TangentSelect.Start, space);
      var positionTangentEnd = nextPoint.GetTangentPosition(TangentSelect.End, space);
      var positionEnd = (space == Space.World) ? nextPoint.WorldPosition : nextPoint.Position;

      return CalculateBezier(positionStart, positionEnd, positionTangentStart, positionTangentEnd, t);
    }

    public static IntervalInfo CalculateSize(BezierPoint point, BezierPoint nextPoint, int resolution = 150, float distanceKey = 1)
    {
      var intervalInfo = IntervalInfo.Create();
      var step = 1f / resolution;
      var previousPosition = point.Position;
      var distance = 0f;
      var distanceTotal = 0f;

      intervalInfo.Save(0, 0);
      for (var t = step; t < 1; t += step)
      {
        var currentPosition = MathBezier.GetIntervalPosition(point, nextPoint, t, Space.Self);
        distance += Vector3.Distance(previousPosition, currentPosition);
        previousPosition = currentPosition;

        if (distance >= distanceKey)
        {
          distanceTotal += distance;
          intervalInfo.Save(distanceTotal, t);
          distance = 0;
        }
      }

      distance += Vector3.Distance(nextPoint.Position, previousPosition);
      distanceTotal += distance;
      intervalInfo.Save(distanceTotal, 1);

      return intervalInfo;
    }

    public static RotationInfo CalculateRotationCurve(Quaternion beginRotation, BezierPoint point, BezierPoint nextPoint, IntervalInfo interval, int resolution = 300, float distanceKey = 1)
    {
      var rotation = beginRotation;
      var size = interval.Size;
      var saveSize = size / 10f;
      var steps = size / 30f;

      var distanceSave = steps;
      var distanceTotal = steps;

      var rotationInfo = RotationInfo.Create();
      rotationInfo.Save(rotation, 0);

      do
      {
        var t = interval.GetInverval(distanceTotal);
        var tangent = MathBezier.GetTangent(point, nextPoint, t);
        rotation = QuaternionUtility.ProjectOnDirection(rotation, tangent);

        if (distanceSave >= 1)
        {
          rotationInfo.Save(rotation, distanceTotal);
          distanceSave = 0;
        }

        distanceTotal += steps;
        distanceSave += steps;
      } while (distanceTotal < size);

      rotation = QuaternionUtility.ProjectOnDirection(rotation, nextPoint.Forward);
      rotationInfo.Save(rotation, size);
      return rotationInfo;
    }

    // Reference: https://en.wikipedia.org/wiki/B%C3%A9zier_curve
    public static Vector3 CalculateBezier(Vector3 start, Vector3 End, Vector3 tangentStart, Vector3 tangentEnd, float t)
    {
      var oneMinusT = (1 - t);
      var oneMinusTSquared = oneMinusT * oneMinusT;
      var tSquared = t * t;
      return oneMinusTSquared * oneMinusT * start +
             oneMinusTSquared * 3 * t * tangentStart +
             tSquared * 3 * oneMinusT * tangentEnd +
             tSquared * t * End;
    }
  }
}
