using System.Collections.Generic;
using UnityEngine;
using static Bezier.BezierPoint;

namespace Bezier
{
  [ExecuteInEditMode]
  public class BezierCurve : MonoBehaviour
  {
    [SerializeField]
    private bool isLoop;
    [SerializeField]
    private List<BezierPoint> points;

    // Cache
    private Transform cacheTransform;

    public int Lenght => points.Count;
    public bool IsLoop => isLoop;

    public void SetPoint(int index, BezierPoint point)
    {
      var oldPoint = points[index];
      if (oldPoint.Equals(point)) return;

      var transform = GetTransform();
      point.transform = transform;
      points[index] = point;

      var previousIndex = GetPreviousPoint(index);
      var nextIndex = GetNextIndexPoint(index);

      UpdatePoint(previousIndex);
      UpdatePoint(index);
      UpdatePoint(nextIndex);

      UpdateRotationPoints(transform.up);
    }

    public void SetLoop(bool isLoop)
    {
      var lastIndex = Lenght - 1;
      var lastPoint = points[lastIndex];

      this.isLoop = lastPoint.hasNextPoint = isLoop;
      points[lastIndex] = lastPoint;
    }

    public void AddWorldPoint(BezierPoint newPoint)
    {
      newPoint.transform = GetTransform();

      var lastIndex = Lenght - 1;
      var lastPoint = points[lastIndex];
      lastPoint.SetNextPoint(newPoint);
      lastPoint.hasNextPoint = true;

      var firstPoint = points[0];
      newPoint.SetNextPoint(firstPoint);
      newPoint.hasNextPoint = isLoop;

      points[lastIndex] = lastPoint;
      points.Add(newPoint);
    }

    public BezierPoint GetPoint(int index)
    {
      var point = points[index];

      if (point.transform == null)
      {
        point.transform = GetTransform();
        points[index] = point;
      }

      return point;
    }

    public Transform GetTransform()
    {
      if (cacheTransform == null)
      {
        cacheTransform = transform;
      }

      return transform;
    }

    public int GetNextIndexPoint(int currentIndex)
    {
      return (int)Mathf.Repeat(currentIndex + 1, Lenght);
    }

    public int GetPreviousPoint(int currentIndex)
    {
      return (int)Mathf.Repeat(currentIndex - 1, Lenght);
    }

    private void UpdateRotationPoints(Vector3 upwards)
    {
      var forward = points[0].Forward;
      var currentRotation = Quaternion.LookRotation(forward, upwards);

      for (int index = 0; index < Lenght; index++)
      {
        var point = points[index];
        var pointSize = point.Size;
        var saveSize = pointSize / 10f;
        var steps = pointSize / 30f;
        var distanceSave = steps;
        var distanceTotal = steps;

        point.rotationInfo.Reset();
        point.rotationInfo.Save(currentRotation, 0);

        do
        {
          var t = point.GetInvertalByDistance(distanceTotal);
          var tangent = MathBezier.GetTangent(point, t);
          currentRotation = QuaternionUtility.ProjectOnDirection(currentRotation, tangent);

          if (distanceSave >= 1)
          {
            point.rotationInfo.Save(currentRotation, distanceTotal);
            distanceSave = 0;
          }

          distanceTotal += steps;
          distanceSave += steps;
        } while (distanceTotal < pointSize);

        var nextIndex = GetNextIndexPoint(index);
        var nextPoint = points[nextIndex];
        currentRotation = QuaternionUtility.ProjectOnDirection(currentRotation, nextPoint.Forward);
        point.rotationInfo.Save(currentRotation, pointSize);
        points[index] = point;
      }
    }

    private void UpdatePoint(int index)
    {
      var previousIndex = GetPreviousPoint(index);
      var nextIndex = GetNextIndexPoint(index);

      var previousPoint = points[previousIndex];
      var currentPoint = points[index];
      var nextPoint = points[nextIndex];

      previousPoint.CheckTangentVector(currentPoint, TangentSelect.Start);
      currentPoint.CheckTangentVector(nextPoint, TangentSelect.Start);
      currentPoint.CheckTangentVector(previousPoint, TangentSelect.End);
      nextPoint.CheckTangentVector(currentPoint, TangentSelect.End);

      previousPoint.SetNextPoint(currentPoint);
      currentPoint.SetNextPoint(nextPoint);

      previousPoint.UpdateSize();
      currentPoint.UpdateSize();

      points[previousIndex] = previousPoint;
      points[index] = currentPoint;
      points[nextIndex] = nextPoint;
    }

    private void OnValidate()
    {
      var isInvalidPoints = points is null || points.Count > 1;
      if (isInvalidPoints)
      {
        Reset();
      }
    }

    private void Reset()
    {
      var tangent1 = new Tangent(Vector3.right * 3, TangentType.Aligned);
      var tangent2 = new Tangent(-Vector3.right * 3, TangentType.Aligned);

      var point1 = new BezierPoint(Vector3.zero, tangent1, tangent2);
      point1.hasNextPoint = true;

      var point2 = new BezierPoint(Vector3.forward * 10, tangent1, tangent2);
      point2.next.position = point1.Position;
      point2.next.tangentPosition = point1.GetTangentPosition(TangentSelect.End, Space.Self);

      point1.SetNextPoint(point2);
      point2.SetNextPoint(point1);

      point1.UpdateSize(true);
      point2.UpdateSize(true);

      points = new List<BezierPoint> { point1, point2 };

      var transform = GetTransform();
      UpdateRotationPoints(transform.up);
    }
  }
}
