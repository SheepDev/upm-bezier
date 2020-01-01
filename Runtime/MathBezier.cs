using UnityEngine;
using static Bezier.BezierPoint;

namespace Bezier
{
  public static class MathBezier
  {
    public static Vector3 GetForward(BezierPoint point)
    {
      return GetTangent(point, 0);
    }

    public static Vector3 GetForward(BezierPoint point, BezierPoint nextPoint)
    {
      var positionStart = point.Position;
      var positionTangentStart = point.GetTangentPosition(TangentSelect.Start, Space.Self);
      var positionTangentEnd = nextPoint.GetTangentPosition(TangentSelect.End, Space.Self);
      var positionEnd = nextPoint.Position;

      return GetTangent(positionStart, positionEnd, positionTangentStart, positionTangentEnd, 0);
    }

    public static Vector3 GetTangent(BezierPoint point, float t)
    {
      var positionStart = point.Position;
      var positionTangentStart = point.GetTangentPosition(TangentSelect.Start, Space.Self);
      var positionTangentEnd = point.Next.tangentPosition;
      var positionEnd = point.Next.position;

      return GetTangent(positionStart, positionEnd, positionTangentStart, positionTangentEnd, t);
    }

    public static Vector3 GetTangent(Vector3 start, Vector3 End, Vector3 tangentStart, Vector3 tangentEnd, float t)
    {
      var positionStart = CalculateBezier(start, End, tangentStart, tangentEnd, t);
      var positionEnd = CalculateBezier(start, End, tangentStart, tangentEnd, t + .00001f);
      return (positionEnd - positionStart).normalized;
    }

    public static Vector3 GetIntervalWorldPosition(BezierPoint point, float t)
    {
      var positionStart = point.WorldPosition;
      var positionTangentStart = point.GetTangentPosition(TangentSelect.Start);
      var positionTangentEnd = point.NextTangentWorldPosition;
      var positionEnd = point.NextPointWorldPosition;

      return CalculateBezier(positionStart, positionEnd, positionTangentStart, positionTangentEnd, t);
    }

    public static Vector3 GetIntervalLocalPosition(BezierPoint point, float t)
    {
      var positionStart = point.Position;
      var positionTangentStart = point.GetTangentPosition(TangentSelect.Start, Space.Self);
      var positionTangentEnd = point.Next.tangentPosition;
      var positionEnd = point.next.position;

      return CalculateBezier(positionStart, positionEnd, positionTangentStart, positionTangentEnd, t);
    }

    public static AnimationCurve CalculateSize(BezierPoint point, int resolution = 150, float distanceKey = 1)
    {
      var tDistance = new AnimationCurve();
      var step = 1f / resolution;
      var previousPosition = point.Position;
      var distance = 0f;
      var distanceTotal = 0f;

      tDistance.AddKey(new Keyframe(0, 0, 0, 0, 0, 0));

      for (var t = step; t < 1; t += step)
      {
        var position = MathBezier.GetIntervalLocalPosition(point, t);
        distance += Vector3.Distance(position, previousPosition);
        previousPosition = position;

        if (distance >= distanceKey)
        {
          distanceTotal += distance;
          tDistance.AddKey(new Keyframe(distanceTotal, t, 0, 0, 0, 0));
          distance = 0;
        }
      }

      distance += Vector3.Distance(point.Next.position, previousPosition);
      distanceTotal += distance;
      tDistance.AddKey(new Keyframe(distanceTotal, 1, 0, 0, 0, 0));

      return tDistance;
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
