using System;
using System.Collections.Generic;
using UnityEngine;
using static SheepDev.Bezier.Point;

namespace SheepDev.Bezier
{
  [Serializable]
  public class PointData
  {
    [SerializeField] internal Point point;
    [SerializeField] private bool isPointDirty;
    [SerializeField] private bool isDataDirty;
    public IntervalInfo intervalInfo;
    public RotationInfo rotationInfo;
    [SerializeField] internal float startSize;
    public float TotalSize => startSize + intervalInfo.Size;

    public Point Point => point;
    public bool IsPointDirty => isPointDirty;
    public bool IsDataDirty => isDataDirty;

    public PointData(Point point)
    {
      SetPoint(point);
    }

    public void UpdatePoint(Point previousPoint, Point nextPoint)
    {
      if (isPointDirty)
      {
        point.CheckTangentVector(previousPoint, TangentSelect.End);
        point.CheckTangentVector(nextPoint, TangentSelect.Start);
        isPointDirty = false;
      }
    }

    public void UpdateData(Quaternion baseRotation, Point previousPoint, Point nextPoint, bool isForceRotation = false, float minAngle = 5, float maxAngle = 10, int maxInteration = 20)
    {
      UpdatePoint(previousPoint, nextPoint);

      if (isDataDirty) intervalInfo = MathBezier.CalculateSize(Point, nextPoint);

      if (isDataDirty || isForceRotation)
      {
        rotationInfo = MathBezier.CalculateRotation(Point, nextPoint, baseRotation, intervalInfo, minAngle, maxAngle, maxInteration);
      }

      isDataDirty = false;
    }

    public void MarkDirty()
    {
      isPointDirty = isDataDirty = true;
    }

    public void SetPoint(Point value)
    {
      point = value;
      MarkDirty();
    }

    public override bool Equals(object obj)
    {
      return obj is PointData data &&
             EqualityComparer<Point>.Default.Equals(Point, data.Point);
    }

    public override int GetHashCode()
    {
      int hashCode = -254689040;
      return hashCode * -1521134295 + Point.GetHashCode();
    }
  }

  [Serializable]
  public struct RotationInfo
  {
    [SerializeField] internal List<Rotation> rotations;
    [SerializeField] internal float plusRoll;

    public RotationInfo(List<Rotation> list) : this()
    {
      this.rotations = list;
    }

    private int Size => rotations.Count;

    public void Add(float t, Quaternion rotation)
    {
      rotations.Add(new Rotation() { t = t, value = rotation });
    }

    public Quaternion GetRotation(float t)
    {
      var rotation = GetIntervalRotation(t);

      if (plusRoll != 0)
      {
        var currentRoll = Mathf.Lerp(0, plusRoll, t);
        rotation *= Quaternion.AngleAxis(currentRoll, Vector3.forward);
      }

      return rotation;
    }

    private Quaternion GetIntervalRotation(float t)
    {
      if (rotations.Count <= 1) return rotations[0].value;
      if (t >= 1) return rotations[Size - 1].value;

      for (int index = 0; index < Size - 1; index++)
      {
        var roll = rotations[index];
        var nextRoll = rotations[index + 1];

        if (t >= roll.t && t < nextRoll.t)
        {
          var max = nextRoll.t - roll.t;
          var current = t - roll.t;
          var interval = (current / max);
          return Quaternion.Lerp(roll.value, nextRoll.value, interval);
        }
      }

      throw new Exception("Not find rotation");
    }

    public RotationInfo Convert(Quaternion worldRotation)
    {
      var rotations = new List<Rotation>(this.rotations);

      for (int index = 0; index < Size; index++)
      {
        var rotation = rotations[index];
        rotation.value = worldRotation * rotation.value;
        rotations[index] = rotation;
      }

      return new RotationInfo(rotations) { plusRoll = this.plusRoll };
    }

    public static RotationInfo Create()
    {
      return new RotationInfo(new List<Rotation>());
    }

    [Serializable]
    public struct Rotation
    {
      public float t;
      public Quaternion value;
    }
  }

  [Serializable]
  public struct IntervalInfo
  {
    [SerializeField] private AnimationCurve intervalCurve;

    public float Size => (intervalCurve.keys.Length != 0) ? intervalCurve.keys[intervalCurve.keys.Length - 1].time : 0;

    private IntervalInfo(AnimationCurve curve) : this()
    {
      intervalCurve = curve;
    }

    public void Reset()
    {
      intervalCurve = new AnimationCurve();
    }

    public void Save(float time, float value)
    {
      intervalCurve.AddKey(new Keyframe(time, value, 0, 0, 0, 0));
    }

    public float GetInverval(float distance)
    {
      return this.intervalCurve.Evaluate(distance);
    }

    public static IntervalInfo Create()
    {
      return new IntervalInfo(new AnimationCurve());
    }
  }
}