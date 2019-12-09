using System.Collections.Generic;
using UnityEngine;
using static Bezier.BezierUtility;

namespace Bezier
{
  public class BezierCurver : MonoBehaviour
  {
    public bool isLoop;
    [HideInInspector]
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
      var point3 = new Point(Vector3.forward * 3, tangent1, tangent2);

      points = new List<Point> { point1, point2, point3 };
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

      if (GetNextPoint(index, out var nextPoint, out var nextIndex))
      {
        point.UpdateVector(TangentSpace.Start, nextPoint);
        nextPoint.UpdateVector(TangentSpace.End, point);
        points[nextIndex] = nextPoint;
      }

      if (GetPreviousPoint(index, out var previousPoint, out var previousIndex))
      {
        point.UpdateVector(TangentSpace.End, previousPoint);
        previousPoint.UpdateVector(TangentSpace.Start, point);
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

    private bool GetNextPoint(int index, out Point point, out int outIndex)
    {
      var nextIndex = index + 1;
      if (nextIndex < Lenght)
      {
        point = points[nextIndex];
        outIndex = nextIndex;
        return true;
      }
      else if (isLoop)
      {
        point = points[0];
        outIndex = 0;
        return true;
      }

      point = default;
      outIndex = default;
      return false;
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
