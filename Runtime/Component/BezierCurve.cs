using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static SheepDev.Bezier.Point;

namespace SheepDev.Bezier
{
  public class BezierCurve : MonoBehaviour, IEnumerable<SectionCurve>
  {
    public float minAngle = 8;
    public float maxAngle = 14;
    public int maxInteration;

    [SerializeField] private bool isLoop;
    [SerializeField] private List<PointData> datas;

    public UnityEvent onUpdated;

    private Transform cacheTransform;

    public bool IsLoop => isLoop;
    public int PointLenght => datas.Count;
    public int SectionLenght => (isLoop) ? PointLenght : PointLenght - 1;
    public float Size => datas[SectionLenght - 1].TotalSize;

    public void Add(int index, Point point, Space space = Space.World)
    {
      if (space == Space.World)
      {
        point = Point.ConvertPoint(point, GetTransform().worldToLocalMatrix);
      }

      datas.Insert(index, new PointData(point));
      onUpdated.Invoke();
    }

    public void Remove(int index)
    {
      datas.RemoveAt(index);
      onUpdated.Invoke();
    }

    public void Split(int index, float t)
    {
      var data = datas[index];
      var nextData = GetNextData(index);
      var splitPoint = MathBezier.Split(data.Point, nextData.Point, t, out var tangentStart, out var tangentEnd);

      data.point.SetTangentPosition(tangentStart, TangentSelect.Start);
      nextData.point.SetTangentPosition(tangentEnd, TangentSelect.End);

      var splitIndex = index + 1;
      Add(splitIndex, splitPoint, Space.Self);
    }

    public void SetPoint(int index, Point point, Space space = Space.World)
    {
      if (space == Space.World)
      {
        point = Point.ConvertPoint(point, GetTransform().worldToLocalMatrix);
      }

      var data = datas[index];
      if (data.Point.Equals(point)) return;

      data.SetPoint(point);
      GetPreviousData(index).MarkDirty();
      GetNextData(index).MarkDirty();

      onUpdated.Invoke();
    }

    public void SetPointRoll(int index, float roll)
    {
      onUpdated.Invoke();
    }

    public void SetLoop(bool isLoop)
    {
      this.isLoop = isLoop;
      onUpdated.Invoke();
    }

    public Point GetPoint(int index, Space space = Space.World)
    {
      var data = datas[index];
      var previousData = GetPreviousData(index);
      var nextData = GetNextData(index);
      data.UpdatePoint(previousData.Point, nextData.Point);

      if (space == Space.World)
      {
        return Point.ConvertPoint(data.Point, GetTransform().localToWorldMatrix);
      }

      return data.Point;
    }

    public Point GetPreviousPoint(int index, Space space = Space.World)
    {
      index = GetPreviousIndex(index);
      return GetPoint(index);
    }

    public Point GetNextPoint(int index, Space space = Space.World)
    {
      index = GetNextIndex(index);
      return GetPoint(index);
    }

    public SectionCurve GetSection(int index, Space space = Space.World)
    {
      UpdateData();

      var data = datas[index];
      var point = GetPoint(index, space);
      var nextPoint = GetNextPoint(index, space);
      var rotationInfo = (space == Space.World) ?
        data.rotationInfo.Convert(GetTransform().rotation) : data.rotationInfo;

      var info = new Info(data.startSize, data.intervalInfo, rotationInfo);
      return new SectionCurve(point, nextPoint, info);
    }

    public SectionCurve GetSection(float distance, Space space = Space.World)
    {
      for (int i = 0; i < SectionLenght; i++)
      {
        var data = datas[i];

        if (data.startSize <= distance && distance <= data.TotalSize)
        {
          return GetSection(i, space);
        }
      }

      return GetSection(SectionLenght - 1);
    }

    private PointData GetPreviousData(int index)
    {
      var previousIndex = GetPreviousIndex(index);
      return datas[previousIndex];
    }

    private PointData GetNextData(int index)
    {
      var nextIndex = GetNextIndex(index);
      return datas[nextIndex];
    }

    public int GetNextIndex(int currentIndex)
    {
      return (int)Mathf.Repeat(currentIndex + 1, PointLenght);
    }

    public int GetPreviousIndex(int currentIndex)
    {
      return (int)Mathf.Repeat(currentIndex - 1, PointLenght);
    }

    public Transform GetTransform()
    {
#if UNITY_EDITOR
      if (!Application.isPlaying)
      {
        var isGet = TryGetComponent(out Transform transform);
        return isGet ? transform : default;
      }
      else if (cacheTransform == null)
      {
        cacheTransform = transform;
      }

      return cacheTransform;
#else
      if (cacheTransform == null)
      {
        cacheTransform = transform;
      }

      return transform;
#endif
    }

    private void UpdateData()
    {
      var firstPoint = GetPoint(0, Space.Self);
      var secondPoint = GetPoint(1, Space.Self);
      var forward = MathBezier.GetTangent(firstPoint, secondPoint, 0);
      var rotation = Quaternion.LookRotation(forward, GetTransform().up);

      var isForceCalculateRotation = false;
      var sizeTotal = 0f;

      for (var index = 0; index < PointLenght; index++)
      {
        var data = datas[index];
        var previousPoint = GetPreviousPoint(index);
        var nextPoint = GetNextPoint(index);

        if (data.IsDataDirty) isForceCalculateRotation = true;

        data.UpdateData(rotation, previousPoint, nextPoint, isForceCalculateRotation);
        data.startSize = sizeTotal;
        rotation = data.rotationInfo.GetRotation(1);
        sizeTotal += data.intervalInfo.Size;
      }
    }

    private void OnValidate()
    {
      var isInvalidPoints = datas is null || datas.Count < 2;
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

      var point1 = new Point(Vector3.zero, tangentStart, tangentEnd);
      var point2 = new Point(Vector3.forward * 10, tangentStart, tangentEnd);

      datas = new List<PointData>();
      datas.Add(new PointData(point1));
      datas.Add(new PointData(point2));
    }

    public IEnumerator<SectionCurve> GetEnumerator()
    {
      var count = (isLoop) ? PointLenght : PointLenght - 1;

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
