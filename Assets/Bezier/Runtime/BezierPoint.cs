using UnityEngine;
using System;

namespace Bezier
{
  [Serializable]
  public struct BezierPoint
  {
    [SerializeField]
    internal Vector3 position;
    [SerializeField]
    internal Transform transform;
    [SerializeField]
    internal Tangent tangentStart;
    [SerializeField]
    internal Tangent tangentEnd;
    [SerializeField]
    internal bool hasNextPoint;
    [SerializeField]
    internal NeighborPointInfo next;
    [SerializeField]
    private bool isDirty;
    [SerializeField]
    private IntervalInfo intervalInfo;
    [SerializeField]
    internal RotationInfo rotationInfo;
    [SerializeField]
    private float roll;

    public float Size => intervalInfo.size;
    public Vector3 Position => position;
    public Vector3 WorldPosition => LocalToWorld(position);

    public Tangent TangentStart => tangentStart;
    public Tangent TangentEnd => tangentEnd;

    public bool HasNextPoint => hasNextPoint;
    public NeighborPointInfo Next => next;
    public Vector3 NextPointWorldPosition => LocalToWorld(next.position);
    public Vector3 NextTangentWorldPosition => LocalToWorld(next.tangentPosition);
    public Vector3 Forward => MathBezier.GetForward(this);
    public float Roll { get => roll; set => roll = value; }
    internal bool IsDirty => isDirty;

    public BezierPoint(Vector3 localPosition, Tangent tangentStart, Tangent tangentEnd) : this()
    {
      this.position = localPosition;
      this.tangentStart = tangentStart;
      this.tangentEnd = tangentEnd;
    }

    public BezierPoint(Vector3 localPosition, Vector3 tangentStart, Vector3 tangentEnd)
    : this(localPosition,
    new Tangent(tangentStart - localPosition, TangentType.Free),
    new Tangent(tangentEnd - localPosition, TangentType.Free))
    {
    }

    public BezierPoint Split(float t, out Vector3 tangentStartPosition, out Vector3 tangentEndPosition)
    {
      var tangentStart = GetTangentPosition(TangentSelect.Start, Space.Self);
      tangentStartPosition = Vector3.Lerp(Position, tangentStart, t);
      tangentEndPosition = Vector3.Lerp(next.position, next.tangentPosition, t);
      var tangentLerp = Vector3.Lerp(tangentStart, next.tangentPosition, t);

      var position = MathBezier.GetIntervalLocalPosition(this, t);
      var splitTangentStartPosition = Vector3.Lerp(tangentLerp, tangentEndPosition, t);
      var splitTangentEndPosition = Vector3.Lerp(tangentStartPosition, tangentLerp, t);
      var splitPoint = new BezierPoint(position, splitTangentStartPosition, splitTangentEndPosition);
      splitPoint.CopyMatrix(this);
      return splitPoint;
    }

    public void SetPosition(Vector3 position, Space space = Space.World)
    {
      if (space == Space.World)
      {
        position = WorldToLocal(position);
      }

      if (this.position != position)
      {
        this.position = position;
        isDirty = true;
      }
    }

    public void SetTangentType(TangentType type, TangentSelect space)
    {
      switch (space)
      {
        case TangentSelect.Start:
          UpdateTangentType(type, ref tangentStart, ref tangentEnd);
          break;
        case TangentSelect.End:
          UpdateTangentType(type, ref tangentEnd, ref tangentStart);
          break;
      }
    }

    public void SetTangentPosition(Vector3 position, TangentSelect tangentSpace, Space space = Space.World)
    {
      if (space == Space.World)
      {
        position = WorldToLocal(position);
      }

      position -= this.position;

      switch (tangentSpace)
      {
        case TangentSelect.Start:
          UpdateTangentPosition(position, ref tangentStart, ref tangentEnd);
          break;
        case TangentSelect.End:
          UpdateTangentPosition(position, ref tangentEnd, ref tangentStart);
          break;
      }
    }

    public void CopyMatrix(BezierPoint point)
    {
      transform = point.transform;
    }

    public bool GetPositionByDistance(float distance, out Vector3 position, Space space = Space.World)
    {
      var t = intervalInfo.GetInverval(distance);
      position = GetPosition(t, space);
      return distance < Size;
    }

    public bool GetPositionAndRotationByDistance(float distance, out Vector3 position, out Quaternion rotation, Space space = Space.World)
    {
      var euler = rotationInfo.GetRotation(distance).eulerAngles;
      var startRoll = rotationInfo.GetRotation(0).eulerAngles.z + roll;

      var t = GetInvertalByDistance(distance);
      var targetRoll = Mathf.Lerp(startRoll, next.roll, t);

      rotation = Quaternion.Euler(euler.x, euler.y, euler.z + targetRoll);
      if (space == Space.World) rotation = LocalToWorld(rotation);

      return GetPositionByDistance(distance, out position);
    }

    public float GetInvertalByDistance(float distance)
    {
      return intervalInfo.GetInverval(distance);
    }

    public Vector3 GetPosition(float t, Space space = Space.World)
    {
      return (space == Space.World) ? MathBezier.GetIntervalWorldPosition(this, t) : MathBezier.GetIntervalLocalPosition(this, t);
    }

    public Vector3 GetTangentPosition(TangentSelect tangent, Space space = Space.World)
    {
      var position = this.position;
      position += (tangent == TangentSelect.Start) ? tangentStart.localPosition : tangentEnd.localPosition;

      return (space == Space.World) ? LocalToWorld(position) : position;
    }

    public TangentType GetTangentType(TangentSelect tangent)
    {
      var type = TangentType.Aligned;
      type = (tangent == TangentSelect.Start) ? tangentStart.Type : tangentEnd.Type;

      return type;
    }

    internal void CheckTangentVector(BezierPoint referencePoint, TangentSelect space)
    {
      switch (space)
      {
        case TangentSelect.Start:
          UpdateVector(referencePoint.Position, ref tangentStart, ref tangentEnd);
          break;
        case TangentSelect.End:
          UpdateVector(referencePoint.Position, ref tangentEnd, ref tangentStart);
          break;
      }
    }

    internal void SetNextPoint(BezierPoint point)
    {
      var newPosition = point.position;
      var newTangent = point.GetTangentPosition(TangentSelect.End, Space.Self);

      var isEquals = next.position == newPosition && next.tangentPosition == newTangent && next.roll == point.roll;

      if (!isEquals)
      {
        next.position = newPosition;
        next.tangentPosition = newTangent;
        next.forward = point.Forward;
        next.roll = point.roll;

        isDirty = true;
      }
    }

    internal void UpdateSize(bool forceUpdate = false)
    {
      if (!forceUpdate && !IsDirty) return;

      var newInverval = MathBezier.CalculateSize(this);
      intervalInfo.SetInvertal(newInverval);
      isDirty = false;
    }

    private void UpdateVector(Vector3 lookAt, ref Tangent updated, ref Tangent other)
    {
      if (updated.Type != TangentType.Vector) return;

      var direction = (lookAt - position).normalized;
      updated.localPosition = direction * updated.localPosition.magnitude;

      if (other.Type == TangentType.Aligned)
      {
        other.localPosition = AlignPosition(other, updated);
      }
    }

    private void UpdateTangentPosition(Vector3 newPosition, ref Tangent updated, ref Tangent other)
    {
      if (updated.localPosition != newPosition)
      {
        updated.localPosition = newPosition;
        isDirty = true;

        if (other.Type == TangentType.Aligned)
        {
          other.localPosition = AlignPosition(other, updated);
        }
        else if (updated.Type == TangentType.Aligned)
        {
          updated.Type = TangentType.Free;
        }
      }
    }

    private void UpdateTangentType(TangentType type, ref Tangent updated, ref Tangent other)
    {
      if (updated.Type != type)
      {
        updated.Type = type;
        isDirty = true;

        if (updated.Type == TangentType.Aligned)
        {
          updated.localPosition = AlignPosition(updated, other);
        }
        if (other.Type == TangentType.Aligned)
        {
          other.localPosition = AlignPosition(other, updated);
        }
      }
    }

    public Vector3 AlignPosition(Tangent tangent, Tangent reference)
    {
      var direction = -reference.localPosition.normalized;
      if (direction == Vector3.zero) return tangent.localPosition;

      var magnitude = tangent.localPosition.magnitude;
      return direction * magnitude;
    }

    private Vector3 LocalToWorld(Vector3 position)
    {
      return transform.TransformPoint(position);
    }

    private Vector3 WorldToLocal(Vector3 position)
    {
      return transform.InverseTransformPoint(position);
    }

    private Quaternion LocalToWorld(Quaternion rotation)
    {
      return transform.rotation * rotation;
    }

    public override bool Equals(object obj)
    {
      return obj is BezierPoint point &&
             position == point.position &&
             roll == point.roll &&
             tangentStart.Equals(point.tangentStart) &&
             tangentEnd.Equals(point.tangentEnd);
    }

    public override int GetHashCode()
    {
      var hashCode = -225962414;
      hashCode *= -1521134295 + position.GetHashCode();
      hashCode *= -1521134295 + tangentStart.GetHashCode();
      hashCode *= -1521134295 + tangentEnd.GetHashCode();
      return hashCode;
    }

    [Serializable]
    public struct NeighborPointInfo
    {
      public Vector3 forward;
      public Vector3 position;
      public Vector3 tangentPosition;
      public float roll;
    }

    [Serializable]
    public struct IntervalInfo
    {
      public AnimationCurve inverval;
      public float size;

      public void Reset()
      {
        inverval = new AnimationCurve();
      }

      public void SetInvertal(AnimationCurve interval)
      {
        this.inverval = interval;
        size = inverval[interval.length - 1].time;
      }

      public float GetInverval(float distance)
      {
        return this.inverval.Evaluate(distance);
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
    }

    public enum TangentSelect
    {
      Start, End
    }
  }
}