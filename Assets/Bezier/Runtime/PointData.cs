using System;
using UnityEngine;

namespace SheepDev.Bezier
{
  [Serializable]
  public struct PointData
  {
    [SerializeField]
    private IntervalInfo intervalInfo;
    [SerializeField]
    private RotationInfo rotationInfo;

    public void UpdateInterval(BezierPoint point, BezierPoint nextPoint)
    {
      intervalInfo = MathBezier.CalculateSize(point, nextPoint);
    }

    public void UpdateRotation(BezierPoint point, BezierPoint nextPoint, Quaternion rotation)
    {
      rotationInfo = MathBezier.CalculateRotationCurve(rotation, point, nextPoint, intervalInfo);
    }

    public float GetCurveSize()
    {
      return intervalInfo.Size;
    }

    public float GetInverval(float t)
    {
      return intervalInfo.GetInverval(t);
    }

    public Quaternion GetRotation(float time)
    {
      return rotationInfo.GetRotation(time);
    }

    public static PointData Build(BezierPoint point, BezierPoint nextPoint, Quaternion rotation)
    {
      var data = new PointData();
      data.UpdateInterval(point, nextPoint);
      data.UpdateRotation(point, nextPoint, rotation);

      return data;
    }
  }

  [Serializable]
  public struct IntervalInfo
  {
    [SerializeField]
    private AnimationCurve intervalCurve;

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

  [Serializable]
  public struct RotationInfo
  {
    public AnimationCurve x;
    public AnimationCurve y;
    public AnimationCurve z;
    public AnimationCurve w;

    public void Reset()
    {
      x = new AnimationCurve();
      y = new AnimationCurve();
      z = new AnimationCurve();
      w = new AnimationCurve();
    }

    public void Save(Quaternion rotation, float time)
    {
      x.AddKey(new Keyframe(time, rotation.x, 0, 0, 0, 0));
      y.AddKey(new Keyframe(time, rotation.y, 0, 0, 0, 0));
      z.AddKey(new Keyframe(time, rotation.z, 0, 0, 0, 0));
      w.AddKey(new Keyframe(time, rotation.w, 0, 0, 0, 0));
    }

    public Quaternion GetRotation(float time)
    {
      var x = this.x.Evaluate(time);
      var y = this.y.Evaluate(time);
      var z = this.z.Evaluate(time);
      var w = this.w.Evaluate(time);

      return new Quaternion(x, y, z, w);
    }

    public static RotationInfo Create()
    {
      var info = new RotationInfo();
      info.Reset();
      return info;
    }
  }
}