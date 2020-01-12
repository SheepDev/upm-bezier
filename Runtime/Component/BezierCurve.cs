using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SheepDev.Bezier.BezierPoint;

namespace SheepDev.Bezier
{
  public class BezierCurve : MonoBehaviour, IEnumerable<SectionCurve>
  {
    [SerializeField]
    private bool isLoop;
    [SerializeField]
    private List<BezierPoint> points;
    [SerializeField]
    private List<PointData> datas;

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

      var previousIndex = GetPreviousIndexPoint(index);
      var nextIndex = GetNextIndexPoint(index);

      CheckPoint(previousIndex);
      CheckPoint(index);
      CheckPoint(nextIndex);

      UpdateDataSize(previousIndex);
      UpdateDataSize(index);
      UpdateDataSize(nextIndex);

      UpdateRotation();
    }

    public void SetPointRoll(int index, float roll)
    {
      var point = points[index];
      point.SetRoll(roll);
      points[index] = point;

      UpdateRoll(index);
    }

    public void SetLoop(bool isLoop)
    {
      this.isLoop = isLoop;
    }

    public void Split(int index, float t)
    {
      var section = GetSection(index);
      var splitPoint = section.Split(t, out var tangentStartPosition, out var tangentEndPosition);
      splitPoint.isDirty = true;

      section.currentPoint.SetTangentPosition(tangentStartPosition, TangentSelect.Start, Space.Self);
      section.nextPoint.SetTangentPosition(tangentEndPosition, TangentSelect.End, Space.Self);

      var nextIndex = GetNextIndexPoint(index);
      points[index] = section.CurrentPoint;
      points[nextIndex] = section.NextPoint;

      var splitIndex = index + 1;
      points.Insert(splitIndex, splitPoint);
      datas.Insert(splitIndex, new PointData());

      CheckPoint(splitIndex);
      UpdateDataSize(index);
      UpdateDataSize(splitIndex);

      UpdateRotation();
    }

    public void RemovePoint(int index)
    {
      points.RemoveAt(index);
      datas.RemoveAt(index);

      var previousIndex = GetPreviousIndexPoint(index);
      var nextIndex = GetNextIndexPoint(previousIndex);

      CheckPoint(previousIndex);
      CheckPoint(nextIndex);
      UpdateDataSize(previousIndex, true);
      UpdateDataSize(nextIndex);

      UpdateRotation();
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

    public SectionCurve GetSection(int index)
    {
      var nextIndex = GetNextIndexPoint(index);
      var currentPoint = points[index];
      var nextPoint = points[nextIndex];

      if (index == Lenght - 1)
      {
        var targetRotation = datas[nextIndex].GetRotation(0);
        var targetRoll = targetRotation.eulerAngles.z + nextPoint.GetRoll();
        return new SectionCurve(currentPoint, nextPoint, datas[index], targetRoll);
      }

      return new SectionCurve(currentPoint, nextPoint, datas[index]);
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

    public int GetPreviousIndexPoint(int currentIndex)
    {
      return (int)Mathf.Repeat(currentIndex - 1, Lenght);
    }

    private void CheckPoint(int index)
    {
      var previousIndex = GetPreviousIndexPoint(index);
      var nextIndex = GetNextIndexPoint(index);

      var previousPoint = points[previousIndex];
      var currentPoint = points[index];
      var nextPoint = points[nextIndex];

      previousPoint.CheckTangentVector(currentPoint, TangentSelect.Start);
      currentPoint.CheckTangentVector(nextPoint, TangentSelect.Start);
      currentPoint.CheckTangentVector(previousPoint, TangentSelect.End);
      nextPoint.CheckTangentVector(currentPoint, TangentSelect.End);

      points[previousIndex] = previousPoint;
      points[index] = currentPoint;
      points[nextIndex] = nextPoint;
    }

    private void UpdateDataSize(int index, bool isForce = false)
    {
      var nextIndex = GetNextIndexPoint(index);
      var point = points[index];
      var nextPoint = points[nextIndex];

      if (isForce || point.isDirty || nextPoint.isDirty)
      {
        var data = datas[index];
        data.UpdateInterval(point, nextPoint);
        datas[index] = data;

        point.isDirty = false;
      }

      points[index] = point;
    }

    private void UpdateRotation()
    {
      var beginRotation = Quaternion.LookRotation(points[0].Forward, transform.up);
      var rotation = beginRotation;

      for (int index = 0; index < Lenght; index++)
      {
        var nextIndex = GetNextIndexPoint(index);
        var point = points[index];
        var nextPoint = points[nextIndex];
        var data = datas[index];

        data.UpdateRotation(point, nextPoint, rotation);
        datas[index] = data;

        if (index < Lenght - 1)
        {
          nextPoint.inheritRoll = point.inheritRoll + point.GetRoll();
          points[nextIndex] = nextPoint;
        }

        rotation = data.GetRotation(data.GetCurveSize());
      }
    }

    private void UpdateRoll(int beginIndex)
    {
      for (int index = beginIndex; index < Lenght - 1; index++)
      {
        var nextIndex = GetNextIndexPoint(index);
        var point = points[index];
        var nextPoint = points[nextIndex];

        nextPoint.inheritRoll = point.inheritRoll + point.GetRoll();
        points[nextIndex] = nextPoint;
      }
    }

    private void OnValidate()
    {
      var isInvalidPoints = points is null || points.Count < 2;
      if (isInvalidPoints)
      {
        Reset();
      }
    }

    private void Reset()
    {
      var transform = GetTransform();
      var tangentStart = new Tangent(Vector3.right * 3, TangentType.Aligned);
      var tangentEnd = new Tangent(-Vector3.right * 3, TangentType.Aligned);

      var point1 = new BezierPoint(Vector3.zero, tangentStart, tangentEnd);
      var point2 = new BezierPoint(transform.forward * 10, tangentStart, tangentEnd);

      point1.transform = point2.transform = transform;
      points = new List<BezierPoint> { point1, point2 };

      var beginRotation = Quaternion.LookRotation(point1.Forward, transform.up);
      var data1 = PointData.Build(point1, point2, beginRotation);

      var rotation = data1.GetRotation(data1.GetCurveSize());
      var data2 = PointData.Build(point2, point1, rotation);

      datas = new List<PointData> { data1, data2 };
    }

    public IEnumerator<SectionCurve> GetEnumerator()
    {
      var count = (isLoop) ? points.Count : points.Count - 1;
      for (int index = 0; index < count; index++)
      {
        yield return GetSection(index);
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}
