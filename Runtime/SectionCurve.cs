using UnityEngine;
using static SheepDev.Bezier.MathBezier;

namespace SheepDev.Bezier
{
  public struct SectionCurve
  {
    private readonly Info data;
    private readonly Quaternion rotation;

    public Point Point { get; }
    public Point NextPoint { get; }

    public float Size => data.Size;
    public float TotalSize => data.TotalSize;
    public Vector3 Forward => MathBezier.GetTangent(Point, NextPoint, 0);

    public SectionCurve(Point point, Point nextPoint, Info data) : this()
    {
      this.data = data;
      Point = point;
      NextPoint = nextPoint;
    }

    public bool IsInsideBounds(float distance, DistanceSpace space = DistanceSpace.Relative)
    {
      if (space == DistanceSpace.Total) distance -= data.StartSize;
      return distance <= Size;
    }

    public float GetIntervalByDistance(float distance, DistanceSpace space = DistanceSpace.Relative)
    {
      return data.GetInterval(distance, space);
    }

    public Vector3 GetPosition(float t)
    {
      return MathBezier.GetPosition(Point, NextPoint, t);
    }

    public Quaternion GetRotation(float t)
    {
      return data.GetRotation(t);
    }

    public Quaternion GetRotation(float t, Vector3 up)
    {
      var forward = GetTangent(Point, NextPoint, t);
      return Quaternion.LookRotation(forward, up);
    }

    public void GetPositionAndRotation(float t, out Vector3 position, out Quaternion rotation)
    {
      position = GetPosition(t);
      rotation = GetRotation(t);
    }

    public Vector3 GetPositionByDistance(float distance, DistanceSpace space = DistanceSpace.Relative)
    {
      var t = GetIntervalByDistance(distance, space);
      return GetPosition(t);
    }

    public Quaternion GetRotationByDistance(float distance, DistanceSpace space = DistanceSpace.Relative)
    {
      var t = GetIntervalByDistance(distance, space);
      return GetRotation(t);
    }

    public Quaternion GetRotationByDistance(float distance, Vector3 upward, DistanceSpace space = DistanceSpace.Relative)
    {
      var t = GetIntervalByDistance(distance, space);
      return GetRotation(t, upward);
    }

    public bool GetPositionAndRotationByDistance(float distance, out Vector3 position, out Quaternion rotation, DistanceSpace space = DistanceSpace.Relative)
    {
      position = GetPositionByDistance(distance, space);
      rotation = GetRotationByDistance(distance, space);
      return IsInsideBounds(distance, space);
    }
  }

  public enum DistanceSpace
  {
    Relative, Total
  }

  public class Info
  {
    private readonly float size;
    private readonly IntervalInfo intervalInfo;
    private readonly RotationInfo rotationInfo;

    public Info(float size, IntervalInfo intervalInfo, RotationInfo rotationInfo)
    {
      this.size = size;
      this.intervalInfo = intervalInfo;
      this.rotationInfo = rotationInfo;
    }

    public float Size => intervalInfo.Size;
    public float StartSize => size;
    public float TotalSize => StartSize + intervalInfo.Size;

    public float GetInterval(float distance, DistanceSpace space = DistanceSpace.Relative)
    {
      if (space == DistanceSpace.Total) distance -= size;
      return intervalInfo.GetInverval(distance);
    }

    public Quaternion GetRotation(float t) => rotationInfo.GetRotation(t);
  }
}