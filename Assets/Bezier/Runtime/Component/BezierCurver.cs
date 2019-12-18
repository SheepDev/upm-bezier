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
      var tangent1 = new Tangent(Vector3.right * 3, TangentType.Aligned);
      var tangent2 = new Tangent(-Vector3.right * 3, TangentType.Aligned);

      var point1 = new Point(Vector3.zero, tangent1, tangent2);
      var point2 = new Point(Vector3.forward * 10, tangent1, tangent2);

      ApplyDistance(ref point1, point2);
      points = new List<Point> { point1, point2 };
    }

    [ContextMenu("Build Distance")]
    public void CalculateDistance()
    {
      var pointCount = points.Count;

      for (int index = 0; index < pointCount; index++)
      {
        var point = points[index];

        if (GetNextPoint(index, true, out Point nextPoint, out int nextIndex))
        {
          ApplyDistance(ref point, nextPoint);
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
      var currentPoint = WorldToLocalPoint(worldPoint, GetTransform());
      var oldPoint = points[index];

      if (oldPoint.Equals(currentPoint)) return;

      if (GetNextPoint(index, isLoop, out var nextPoint, out var nextIndex))
      {
        currentPoint.CheckTangentVector(TangentSpace.Start, nextPoint);
        nextPoint.CheckTangentVector(TangentSpace.End, currentPoint);
        ApplyDistance(ref currentPoint, nextPoint);
        points[nextIndex] = nextPoint;
      }

      if (GetPreviousPoint(index, out var previousPoint, out var previousIndex))
      {
        currentPoint.CheckTangentVector(TangentSpace.End, previousPoint);
        previousPoint.CheckTangentVector(TangentSpace.Start, currentPoint);
        ApplyDistance(ref previousPoint, currentPoint);
        points[previousIndex] = previousPoint;
      }

      points[index] = currentPoint;
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

    private bool GetPreviousPoint(int currentIndex, out Point previousPoint, out int previousIndex)
    {
      var hasPrevious = isLoop || HasPreviousPoint(currentIndex);
      previousIndex = (hasPrevious) ? (int)Mathf.Repeat(currentIndex - 1, Lenght) : default;
      previousPoint = (hasPrevious) ? points[previousIndex] : default;
      return hasPrevious;
    }

    private bool HasPreviousPoint(int currentIndex)
    {
      var previousIndex = currentIndex - 1;
      return previousIndex >= 0;
    }
  }
}
