using UnityEngine;
using System;

namespace SheepDev.Bezier
{
  [Serializable]
  public struct BezierPoint
  {
    [SerializeField]
    internal Vector3 position;
    [SerializeField]
    internal Matrix4x4 matrix;
    [SerializeField]
    internal Tangent tangentStart;
    [SerializeField]
    internal Tangent tangentEnd;
    [SerializeField]
    internal bool isDirty;
    [SerializeField]
    internal float inheritRoll;
    [SerializeField]
    private float roll;

    public Vector3 Position => position;
    public Vector3 WorldPosition => LocalToWorld(position);

    public Tangent TangentStart => tangentStart;
    public Tangent TangentEnd => tangentEnd;

    public Vector3 Forward => tangentStart.localPosition.normalized;

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

    public void SetRoll(float value)
    {
      roll = value;
    }

    public void CopyMatrix(BezierPoint point)
    {
      matrix = point.matrix;
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

    public float GetRoll(bool isInheritRoll = false)
    {
      return (isInheritRoll) ? inheritRoll + roll : roll;
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

    private Vector3 AlignPosition(Tangent tangent, Tangent reference)
    {
      var direction = -reference.localPosition.normalized;
      if (direction == Vector3.zero) return tangent.localPosition;

      var magnitude = tangent.localPosition.magnitude;
      return direction * magnitude;
    }

    private Vector3 LocalToWorld(Vector3 position)
    {
      return matrix.MultiplyPoint3x4(position);
    }

    private Vector3 WorldToLocal(Vector3 position)
    {
      return matrix.inverse.MultiplyPoint3x4(position);
    }

    private Quaternion LocalToWorld(Quaternion rotation)
    {
      return matrix.rotation * rotation;
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

    public enum TangentSelect
    {
      Start, End
    }
  }
}