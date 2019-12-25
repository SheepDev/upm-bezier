using System.Collections.Generic;
using UnityEngine;
using static Bezier.MathBezier;
using static Bezier.BezierPointConverter;

namespace Bezier
{
  [ExecuteInEditMode]
  public class BezierCurve : MonoBehaviour
  {
    [SerializeField]
    private bool isLoop;
    [SerializeField]
    private TransformInfo transformInfo;
    [SerializeField]
    private List<BezierPoint> points;
    [SerializeField]
    private List<BezierPoint> worldpoints;

    // Cache
    private Transform cacheTransform;

    public int Lenght => points.Count;
    public bool IsLoop => isLoop;

    public BezierCurve()
    {
      var tangent1 = new Tangent(Vector3.right * 3, TangentType.Aligned);
      var tangent2 = new Tangent(-Vector3.right * 3, TangentType.Aligned);
      var point1 = new BezierPoint(Vector3.zero, tangent1, tangent2);
      var point2 = new BezierPoint(Vector3.forward * 10, tangent1, tangent2);

      point1.hasNextPoint = true;
      point1.SetNextPoint(point2);
      point2.SetNextPoint(point1);

      points = new List<BezierPoint> { point1, point2 };
      worldpoints = new List<BezierPoint>(points);
      transformInfo = new TransformInfo(Vector3.zero, Quaternion.identity, Vector3.one);
    }

    private void Update()
    {
      UpdateWorldPoints();
    }

    public void SetWorldPoint(int index, BezierPoint worldPoint)
    {
      var transform = GetTransform();
      var currentPoint = WorldToLocalPoint(worldPoint, transform);
      var oldPoint = points[index];

      if (oldPoint.Equals(currentPoint)) return;

      if (GetNextPoint(index, out var nextPoint, out var nextIndex))
      {
        nextPoint.CheckTangentVector(currentPoint, TangentSpace.End);
        currentPoint.CheckTangentVector(nextPoint, TangentSpace.Start);
        currentPoint.SetNextPoint(nextPoint);

        SetPointLocal(nextIndex, nextPoint);
      }

      if (GetPreviousPoint(index, out var previousPoint, out var previousIndex))
      {
        currentPoint.CheckTangentVector(previousPoint, TangentSpace.End);
        previousPoint.CheckTangentVector(currentPoint, TangentSpace.Start);
        previousPoint.SetNextPoint(currentPoint);

        SetPointLocal(previousIndex, previousPoint);
      }

      SetPointLocal(index, currentPoint);
    }

    public void SetPointLocal(int index, BezierPoint localPoint)
    {
      points[index] = localPoint;
      worldpoints[index] = LocalToWorldPoint(localPoint, GetTransform());
    }

    public void SetLoop(bool isLoop)
    {
      this.isLoop = isLoop;

      var lastPoint = GetLastPoint(points);
      var lastWorldPoint = GetLastPoint(worldpoints);
      lastWorldPoint.hasNextPoint = lastPoint.hasNextPoint = isLoop;

      SetLastPoint(points, lastPoint);
      SetLastPoint(worldpoints, lastWorldPoint);
    }

    public void AddWorldPoint(BezierPoint worldpoint)
    {
      var transform = GetTransform();
      var lastWorldPoint = GetLastPoint(worldpoints);
      worldpoint.hasNextPoint = isLoop;
      worldpoint.next = lastWorldPoint.next;

      lastWorldPoint.hasNextPoint = true;
      lastWorldPoint.SetNextPoint(worldpoint);
      SetLastPoint(worldpoints, lastWorldPoint);
      SetLastPoint(points, WorldToLocalPoint(lastWorldPoint, transform));

      var localpoint = WorldToLocalPoint(worldpoint, transform);
      worldpoints.Add(worldpoint);
      points.Add(localpoint);
    }

    public BezierPoint GetWorldPoint(int index)
    {
      if (index < 0 && index >= points.Count) return default;
      UpdateWorldPoints();
      return worldpoints[index];
    }

    public BezierPoint[] GetWorldPoints()
    {
      UpdateWorldPoints();
      return worldpoints.ToArray();
    }

    public Transform GetTransform()
    {
      if (cacheTransform is null)
      {
        cacheTransform = transform;
      }

      return cacheTransform;
    }

    private void UpdateWorldPoints()
    {
      var transform = GetTransform();
      var isNeedToUpdate = !transformInfo.Equals(transform);

      if (isNeedToUpdate)
      {
        transformInfo = new TransformInfo(transform);
        worldpoints = LocalToWorldPoints(points, transform);
      }
    }

    private BezierPoint GetLastPoint(List<BezierPoint> points)
    {
      return points[Lenght - 1];
    }

    private void SetLastPoint(List<BezierPoint> points, BezierPoint point)
    {
      points[Lenght - 1] = point;
    }

    private bool GetNextPoint(int currentIndex, out BezierPoint nextPoint, out int nextIndex, bool isLoop = true)
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

    private bool GetPreviousPoint(int currentIndex, out BezierPoint previousPoint, out int previousIndex, bool isLoop = true)
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

    [System.Serializable]
    public struct TransformInfo
    {
      private Vector3 position;
      private Vector3 scale;
      private Quaternion rotation;

      public TransformInfo(Vector3 position, Quaternion rotation, Vector3 scale)
      {
        this.position = position;
        this.scale = scale;
        this.rotation = rotation;
      }

      public TransformInfo(Transform transform) : this(transform.position, transform.rotation, transform.lossyScale)
      {
      }

      public override bool Equals(object obj)
      {
        return obj is TransformInfo info &&
               position == info.position &&
               scale == info.scale &&
               rotation == info.rotation ||
               obj is Transform transform &&
               position == transform.position &&
               scale == transform.lossyScale &&
               rotation == transform.rotation;
      }

      public override int GetHashCode()
      {
        int hashCode = 701499426;
        hashCode = hashCode * -1521134295 + position.GetHashCode();
        hashCode = hashCode * -1521134295 + scale.GetHashCode();
        hashCode = hashCode * -1521134295 + rotation.GetHashCode();
        return hashCode;
      }
    }
  }
}
