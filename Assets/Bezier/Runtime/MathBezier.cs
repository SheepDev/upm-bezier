using SheepDev.Extension;
using SheepDev.Utility;
using UnityEngine;
using static SheepDev.Bezier.Point;

namespace SheepDev.Bezier
{
  public static class MathBezier
  {
    public static Vector3 GetTangent(Point point, Point nextPoint, float t)
    {
      var positionStart = GetPosition(point, nextPoint, t);
      var positionEnd = GetPosition(point, nextPoint, t + .001f);
      return (positionEnd - positionStart).normalized;
    }

    public static Vector3 GetPosition(Point point, Point nextPoint, float t)
    {
      var positionStart = point.position;
      var positionTangentStart = point.GetTangentPosition(TangentSelect.Start);
      var positionTangentEnd = nextPoint.GetTangentPosition(TangentSelect.End);
      var positionEnd = nextPoint.position;

      return CubicBezier(positionStart, positionEnd, positionTangentStart, positionTangentEnd, t);
    }

    public static IntervalInfo CalculateSize(Point point, Point nextPoint, int resolution = 75, float distanceKey = 1)
    {
      var intervalInfo = IntervalInfo.Create();
      var step = 1f / resolution;
      var previousPosition = point.position;
      var distance = 0f;
      var distanceTotal = 0f;

      intervalInfo.Save(0, 0);
      for (var t = step; t < 1; t += step)
      {
        var currentPosition = MathBezier.GetPosition(point, nextPoint, t);
        distance += Vector3.Distance(previousPosition, currentPosition);
        previousPosition = currentPosition;

        if (distance >= distanceKey)
        {
          distanceTotal += distance;
          intervalInfo.Save(distanceTotal, t);
          distance = 0;
        }
      }

      distance += Vector3.Distance(nextPoint.position, previousPosition);
      distanceTotal += distance;
      intervalInfo.Save(distanceTotal, 1);

      return intervalInfo;
    }

    public static RotationInfo CalculateRotation(Point p1, Point p2, Quaternion rotation, IntervalInfo intervalInfo, float minAngle, float maxAngle, int maxInteration)
    {
      var currentSize = 0f;
      var rotationInfo = RotationInfo.Create();

      rotation = QuaternionUtility.ProjectOnDirection(rotation, GetTangent(p1, p2, 0));
      rotationInfo.Add(0, rotation);

      var size = intervalInfo.Size;
      while (currentSize < size)
      {
        var t = intervalInfo.GetInverval(currentSize);
        var currentdirection = GetTangent(p1, p2, t);
        var index = 0;
        var lenght = 3f;
        var division = lenght;
        var isFinish = false;

        var minAngleSize = float.MinValue;
        var maxAngleSize = float.MaxValue;
        var maxAngleInterval = Vector3.zero;
        var minAngleInterval = Vector3.zero;

        do
        {
          var nextSize = Mathf.Min(size, currentSize + lenght);
          var nextT = intervalInfo.GetInverval(nextSize);
          var nextDirection = GetTangent(p1, p2, nextT);
          var angle = Vector3.Angle(currentdirection, nextDirection);

          var isBounds = index < maxInteration;
          var isMaxAngle = angle > maxAngle;
          var isMinAngle = angle < minAngle;

          if (minAngleSize.IsApproximately(maxAngleSize, .01f))
          {
            var tInterval = intervalInfo.GetInverval(minAngleSize);
            var position = GetPosition(p1, p2, tInterval);
            var nextPosition = GetPosition(p1, p2, tInterval + 0.001f);
            rotation = QuaternionUtility.ProjectOnDirection(rotation, minAngleInterval);
            rotationInfo.Add(tInterval, rotation);


            tInterval = intervalInfo.GetInverval(maxAngleSize);
            position = GetPosition(p1, p2, tInterval);
            nextPosition = GetPosition(p1, p2, tInterval + 0.1f);
            rotation = QuaternionUtility.ProjectOnDirection(rotation, maxAngleInterval);
            rotationInfo.Add(tInterval, rotation);
            break;
          }

          if (isBounds && isMaxAngle)
          {
            division /= 2f;
            lenght -= division;
            maxAngleSize = nextSize;
            maxAngleInterval = GetTangent(p1, p2, nextT);
          }
          else if (isBounds && isMinAngle)
          {
            division /= 2f;
            lenght += division;
            minAngleSize = nextSize;
            minAngleInterval = GetTangent(p1, p2, nextT);
          }
          else
          {
            isFinish = true;
            rotation = QuaternionUtility.ProjectOnDirection(rotation, nextDirection);
            var position = GetPosition(p1, p2, nextT);
            rotationInfo.Add(nextT, rotation);
          }

          index++;
        } while (!isFinish);

        currentSize += lenght;
      }

      return rotationInfo;
    }

    public static Point Split(Point p1, Point p2, float t, out Vector3 resultTangentStart, out Vector3 resultTangentEnd)
    {
      var tangentStart = p1.GetTangentPosition(TangentSelect.Start);
      var tangentEnd = p2.GetTangentPosition(TangentSelect.End);
      var tangentLerp = Vector3.Lerp(tangentStart, tangentEnd, t);

      resultTangentStart = Vector3.Lerp(p1.position, tangentStart, t);
      resultTangentEnd = Vector3.Lerp(p2.position, tangentEnd, t);

      var splitPosition = GetPosition(p1, p2, t);
      var splitTangentStartPosition = Vector3.Lerp(tangentLerp, resultTangentEnd, t) - splitPosition;
      var splitTangentEndPosition = Vector3.Lerp(resultTangentStart, tangentLerp, t) - splitPosition;
      var splitTangentStart = new Tangent(splitTangentStartPosition);
      var splitTangentEnd = new Tangent(splitTangentEndPosition);
      return new Point(splitPosition, splitTangentStart, splitTangentEnd);
    }

    // Reference: https://en.wikipedia.org/wiki/B%C3%A9zier_curve
    public static Vector3 CubicBezier(Vector3 start, Vector3 End, Vector3 tangentStart, Vector3 tangentEnd, float t)
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
