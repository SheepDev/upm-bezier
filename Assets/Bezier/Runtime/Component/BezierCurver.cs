using System.Collections.Generic;
using UnityEngine;
using static Bezier.BezierUtility;

namespace Bezier
{
  public class BezierCurver : MonoBehaviour
  {
    public bool isLoop;
    [SerializeField]
    private List<Point> points;
    public int Lenght => points.Count;

    // Cache
    private Transform cacheTransform;

    public BezierCurver()
    {
      var tangent1 = new Tangent(Vector3.right, TangentType.Aligned);
      var tangent2 = new Tangent(-Vector3.right, TangentType.Aligned);

      var point1 = new Point(Vector3.zero, tangent1, tangent2);
      var point2 = new Point(Vector3.forward * 2, tangent1, tangent2);
      point1.arcDistance = BezierUtility.Distance(point1, point2);

      points = new List<Point> { point1, point2 };
    }

    [ContextMenu("Build Distance")]
    public void CalculateDistance()
    {
      var pointCount = points.Count;
      var step = 1f / 500f;

      for (int index = 0; index < pointCount; index++)
      {
        var point = points[index];

        if (GetNextPoint(index, true, out Point nextPoint, out int nextIndex))
        {
          var previousPosition = point.Position;
          var distance = 0f;
          var distanceTotal = 0f;
          var tDistance = new AnimationCurve();
          tDistance.AddKey(new Keyframe(0, 0, 0, 0, 0, 0));

          for (var t = step; t < 1; t += step)
          {
            var position = BezierUtility.GetCurverInterval(point, nextPoint, t);
            distance += Vector3.Distance(position, previousPosition);
            previousPosition = position;

            if (distance >= 1)
            {
              distanceTotal += distance;
              tDistance.AddKey(new Keyframe(distanceTotal, t, 0, 0, 0, 0));
              distance = 0;
            }
          }

          distance += Vector3.Distance(nextPoint.position, previousPosition);
          distanceTotal += distance;
          tDistance.AddKey(new Keyframe(distanceTotal, 1, 0, 0, 0, 0));

          point.arcDistance = distanceTotal;
          point.tDistance = tDistance;
          points[index] = point;
        }
      }
    }

    public Point GetWorldPoint(int index)
    {
      return LocalToWorldPoint(points[index], GetTransform());
    }

    public void SetWorldPoint(int index, Point worldPoint)
    {
      var point = WorldToLocalPoint(worldPoint, GetTransform());
      var oldPoint = points[index];

      if (oldPoint.Equals(point)) return;

      if (GetNextPoint(index, isLoop, out var nextPoint, out var nextIndex))
      {
        point.UpdateVector(TangentSpace.Start, nextPoint);
        point.arcDistance = BezierUtility.Distance(point, nextPoint);

        nextPoint.UpdateVector(TangentSpace.End, point);
        points[nextIndex] = nextPoint;
      }

      if (GetPreviousPoint(index, out var previousPoint, out var previousIndex))
      {
        point.UpdateVector(TangentSpace.End, previousPoint);
        previousPoint.UpdateVector(TangentSpace.Start, point);
        previousPoint.arcDistance = BezierUtility.Distance(previousPoint, point);

        points[previousIndex] = previousPoint;
      }

      points[index] = point;
    }

    public void AddWorldPoint(Point point)
    {
      points.Add(WorldToLocalPoint(point, GetTransform()));
    }

    public List<Point> GetWorldPoints()
    {
      return LocalToWorldPoints(points, GetTransform());
    }

    public void SetWorldPoints(Point[] worldPoints)
    {
      points = WorldToLocalPoints(worldPoints, GetTransform());
    }

    public Transform GetTransform()
    {
      if (cacheTransform == null)
      {
        cacheTransform = transform;
      }

      return cacheTransform;
    }

    private bool GetNextPoint(int currentIndex, bool isLoop, out Point nextPoint, out int nextIndex)
    {
      var hasNext = isLoop || HasNextPoint(currentIndex);
      nextIndex = (hasNext) ? (int)Mathf.Repeat(currentIndex + 1, Lenght) : default;
      nextPoint = (hasNext) ? points[nextIndex] : default;
      return hasNext;
    }

    private bool HasNextPoint(int currentIndex)
    {
      var nextIndex = currentIndex + 1;
      return nextIndex < Lenght;
    }

    private bool GetPreviousPoint(int index, out Point point, out int outIndex)
    {
      var previousIndex = index - 1;
      if (previousIndex >= 0)
      {
        point = points[previousIndex];
        outIndex = previousIndex;
        return true;
      }
      else if (isLoop)
      {
        outIndex = Lenght - 1;
        point = points[outIndex];
        return true;
      }

      point = default;
      outIndex = default;
      return false;
    }
  }
}
