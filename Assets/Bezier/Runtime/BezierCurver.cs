using UnityEngine;
using static Bezier.BezierUtility;

namespace Bezier
{
  public class BezierCurver : MonoBehaviour
  {
    [HideInInspector]
    public Point[] points;
    public int Lenght => points.Length;

    // Cache
    private bool isCacheWorldPoints;
    private Point[] worldPoints;
    private Transform cacheTransform;

    public BezierCurver()
    {
      var tangent1 = new Tangent(Vector3.right, TangentType.Aligned);
      var tangent2 = new Tangent(-Vector3.right, TangentType.Aligned);

      var point1 = new Point(Vector3.zero, tangent1, tangent2);
      var point2 = new Point(Vector3.forward * 2, tangent1, tangent2);

      points = new Point[] { point1, point2 };
    }

    public Point GetWorldPoint(int index)
    {
      return (isCacheWorldPoints) ? worldPoints[index] : LocalToWorldPoint(points[index], GetTransform());
    }

    public void SetWorldPoint(int index, Point worldPoint)
    {
      var point = WorldToLocalPoint(worldPoint, GetTransform());
      points[index] = point;

      if(isCacheWorldPoints)
      {
        worldPoints[index] = worldPoint;
      }
    }

    public Point[] GetWorldPoints()
    {
      if (isCacheWorldPoints)
      {
        return worldPoints;
      }

      worldPoints = LocalToWorldPoints(points, GetTransform());
      isCacheWorldPoints = true;
      return worldPoints;
    }

    public void SetWorldPoints(Point[] worldPoints)
    {
      this.worldPoints = worldPoints;
      isCacheWorldPoints = true;

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
  }
}
