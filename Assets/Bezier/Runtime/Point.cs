using UnityEngine;
using System;

namespace SheepDev.Bezier
{
  [Serializable]
  public struct Point
  {
    public Vector3 position;
    [SerializeField] private Tangent tangentStart;
    [SerializeField] private Tangent tangentEnd;
    [SerializeField] public float roll;

    public Point(Vector3 position, Tangent tangentStart, Tangent tangentEnd, float roll = 0) : this()
    {
      this.position = position;
      this.tangentStart = tangentStart;
      this.tangentEnd = tangentEnd;
      this.roll = roll;
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

    public void SetTangentPosition(Vector3 position, TangentSelect tangentSpace)
    {
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

    public Vector3 GetTangentPosition(TangentSelect tangent)
    {
      return position + ((tangent == TangentSelect.Start) ? tangentStart.position : tangentEnd.position);
    }

    public TangentType GetTangentType(TangentSelect tangent)
    {
      return (tangent == TangentSelect.Start) ? tangentStart.type : tangentEnd.type;
    }

    internal void CheckTangentVector(Point referencePoint, TangentSelect space)
    {
      switch (space)
      {
        case TangentSelect.Start:
          UpdateVector(referencePoint.position, ref tangentStart, ref tangentEnd);
          break;
        case TangentSelect.End:
          UpdateVector(referencePoint.position, ref tangentEnd, ref tangentStart);
          break;
      }
    }

    private void UpdateVector(Vector3 lookAt, ref Tangent updated, ref Tangent other)
    {
      if (updated.type != TangentType.Vector) return;

      var direction = (lookAt - position).normalized;
      updated.position = direction * updated.position.magnitude;

      if (other.type == TangentType.Aligned)
      {
        other.position = AlignPosition(other, updated);
      }
    }

    private void UpdateTangentPosition(Vector3 newPosition, ref Tangent updated, ref Tangent other)
    {
      if (updated.position != newPosition)
      {
        updated.position = newPosition;

        if (other.type == TangentType.Aligned)
        {
          other.position = AlignPosition(other, updated);
        }
        else if (updated.type == TangentType.Aligned)
        {
          updated.type = TangentType.Free;
        }
      }
    }

    private void UpdateTangentType(TangentType type, ref Tangent updated, ref Tangent other)
    {
      if (updated.type != type)
      {
        updated.type = type;

        if (updated.type == TangentType.Aligned)
        {
          updated.position = AlignPosition(updated, other);
        }
        if (other.type == TangentType.Aligned)
        {
          other.position = AlignPosition(other, updated);
        }
      }
    }

    private Vector3 AlignPosition(Tangent tangent, Tangent reference)
    {
      var direction = -reference.position.normalized;
      if (direction == Vector3.zero) return tangent.position;

      var magnitude = tangent.position.magnitude;
      return direction * magnitude;
    }

    public static Point ConvertPoint(Point point, Matrix4x4 matrix)
    {
      point.position = matrix.MultiplyPoint3x4(point.position);
      point.tangentStart = ConvertTangent(point.tangentStart, matrix);
      point.tangentEnd = ConvertTangent(point.tangentEnd, matrix);
      return point;
    }

    private static Tangent ConvertTangent(Tangent tangent, Matrix4x4 matrix)
    {
      tangent.position = matrix.MultiplyVector(tangent.position);
      return tangent;
    }

    public override bool Equals(object obj)
    {
      return obj is Point point &&
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