using UnityEngine;
using static SheepDev.Bezier.BezierPoint;

namespace SheepDev.Bezier
{
  public struct SectionCurve
  {
    internal BezierPoint currentPoint;
    internal BezierPoint nextPoint;
    internal PointData data;

    public BezierPoint CurrentPoint => currentPoint;
    public BezierPoint NextPoint => nextPoint;
    public float Size => data.GetCurveSize();
    internal float? TargetRoll { get; }

    public SectionCurve(BezierPoint currentPoint, BezierPoint nextPoint, PointData data) : this()
    {
      this.currentPoint = currentPoint;
      this.nextPoint = nextPoint;
      this.data = data;
    }

    public SectionCurve(BezierPoint currentPoint, BezierPoint nextPoint, PointData data, float targetRoll) : this(currentPoint, nextPoint, data)
    {
      TargetRoll = targetRoll;
    }

    public Vector3 GetForward()
    {
      return MathBezier.GetTangent(currentPoint, nextPoint, 0);
    }

    public Vector3 GetPosition(float t, Space space = Space.World)
    {
      return MathBezier.GetIntervalPosition(currentPoint, nextPoint, t, space);
    }

    public Vector3 GetPositionByDistance(float distance, Space space = Space.World)
    {
      var t = GetInvertalByDistance(distance);
      return GetPosition(t, space);
    }

    public Quaternion GetRotation(float t, Space space = Space.World, bool isNormalizeRoll = false, bool isInheritRoll = false)
    {
      var distance = Size * t;
      return GetRotationByDistance(distance, space, isNormalizeRoll, isInheritRoll);
    }

    public Quaternion GetRotation(float t, Vector3 upwards, Space space = Space.World)
    {
      var distance = Size * t;
      return GetRotationByDistance(distance, upwards, space);
    }

    public Quaternion GetRotationByDistance(float distance, Space space = Space.World, bool isNormalizeRoll = false, bool isInheritRoll = false)
    {
      var rotation = data.GetRotation(distance);

      if (isNormalizeRoll && TargetRoll.HasValue)
      {
        var startRoll = data.GetRotation(0).eulerAngles.z;
        var euler = rotation.eulerAngles;

        var t = GetInvertalByDistance(distance);
        var currentRoll = Mathf.LerpAngle(startRoll, TargetRoll.Value, t);

        rotation = Quaternion.Euler(euler.x, euler.y, currentRoll);
      }
      else
      {
        var t = GetInvertalByDistance(distance);
        var plusRoll = Mathf.Lerp(currentPoint.GetRoll(isInheritRoll), nextPoint.GetRoll(isInheritRoll), t);

        var euler = rotation.eulerAngles;
        rotation = Quaternion.Euler(euler.x, euler.y, euler.z + plusRoll);
      }

      return space == Space.World ? currentPoint.LocalToWorld(rotation) : rotation;
    }

    public Quaternion GetRotationByDistance(float distance, Vector3 upwards, Space space = Space.World)
    {
      var forward = data.GetRotation(distance) * Vector3.forward;
      return Quaternion.LookRotation(forward, upwards);
    }

    public bool GetPositionAndRotation(float distance, out Vector3 position, out Quaternion rotation, Space space = Space.World, bool isNormalizeRoll = false, bool isInheritRoll = false)
    {
      position = GetPositionByDistance(distance, space);
      rotation = GetRotationByDistance(distance, space, isNormalizeRoll, isInheritRoll);
      return distance <= Size;
    }

    public bool GetPositionAndRotation(float distance, out Vector3 position, out Quaternion rotation, Vector3 upwards, Space space = Space.World)
    {
      position = GetPositionByDistance(distance, space);
      rotation = GetRotationByDistance(distance, upwards, space);
      return distance <= Size;
    }

    public bool GetPositionAndRotationByDistance(float distance, out Vector3 position, out Quaternion rotation, Space space = Space.World, bool isNormalizeRoll = false, bool isInheritRoll = false)
    {
      position = GetPositionByDistance(distance, space);
      rotation = GetRotationByDistance(distance, space, isNormalizeRoll, isInheritRoll);
      return distance <= Size;
    }

    public bool GetPositionAndRotationByDistance(float distance, out Vector3 position, out Quaternion rotation, Vector3 upwards, Space space = Space.World)
    {
      position = GetPositionByDistance(distance, space);
      rotation = GetRotationByDistance(distance, upwards, space);
      return distance <= Size;
    }

    public float GetInvertalByDistance(float distance)
    {
      return data.GetInverval(distance);
    }

    public BezierPoint Split(float t, out Vector3 tangentStartPosition, out Vector3 tangentEndPosition)
    {
      var tangentStart = currentPoint.GetTangentPosition(TangentSelect.Start, Space.Self);
      var tangentEnd = nextPoint.GetTangentPosition(TangentSelect.End, Space.Self);

      tangentStartPosition = Vector3.Lerp(currentPoint.Position, tangentStart, t);
      tangentEndPosition = Vector3.Lerp(nextPoint.Position, tangentEnd, t);

      var tangentLerp = Vector3.Lerp(tangentStart, tangentEnd, t);

      var position = GetPosition(t, Space.Self);
      var splitTangentStartPosition = Vector3.Lerp(tangentLerp, tangentEndPosition, t);
      var splitTangentEndPosition = Vector3.Lerp(tangentStartPosition, tangentLerp, t);
      var splitPoint = new BezierPoint(position, splitTangentStartPosition, splitTangentEndPosition);
      splitPoint.CopyMatrix(currentPoint);
      return splitPoint;
    }
  }
}