using UnityEngine;
using System;

namespace Bezier
{
  [Serializable]
  public struct Point
  {
    [SerializeField]
    internal Vector3 position;
    [SerializeField]
    internal Tangent startTangent;
    [SerializeField]
    internal Tangent endTangent;
    [SerializeField]
    internal float arcDistance;
    [SerializeField]
    internal AnimationCurve tDistance;

    public float Size => tDistance.keys[tDistance.length - 1].time;
    public Vector3 Position => position;
    public Vector3 StartTangentPosition => startTangent.position + Position;
    public Vector3 EndTangentPosition => endTangent.position + Position;
    public Vector3 StartTangentLocalPosition => startTangent.position;
    public Vector3 EndTangentLocalPosition => endTangent.position;
    public TangentType StartTangentType => startTangent.type;
    public TangentType EndTangentType => endTangent.type;
    public AnimationCurve TCurveDistance => tDistance;

    public Point(Vector3 position, Tangent startTangent, Tangent endTangent) : this()
    {
      this.position = position;
      this.startTangent = startTangent;
      this.endTangent = endTangent;
    }

    public void SetPosition(Vector3 position)
    {
      if (this.position != position)
      {
        this.position = position;
      }
    }

    public void SetTangentType(TangentType type, TangentSpace space)
    {
      switch (space)
      {
        case TangentSpace.Start:
          UpdateTangentType(type, ref startTangent, ref endTangent);
          break;
        case TangentSpace.End:
          UpdateTangentType(type, ref endTangent, ref startTangent);
          break;
      }
    }

    public void SetTangentPosition(Vector3 position, TangentSpace space)
    {
      var newPosition = position - this.position;

      switch (space)
      {
        case TangentSpace.Start:
          UpdateTangentPosition(newPosition, ref startTangent, ref endTangent);
          break;
        case TangentSpace.End:
          UpdateTangentPosition(newPosition, ref endTangent, ref startTangent);
          break;
      }
    }

    public void SetTangentLocalPosition(Vector3 position, TangentSpace space)
    {
      switch (space)
      {
        case TangentSpace.Start:
          UpdateTangentPosition(position, ref startTangent, ref endTangent);
          break;
        case TangentSpace.End:
          UpdateTangentPosition(position, ref endTangent, ref startTangent);
          break;
      }
    }

    public void CheckTangentVector(TangentSpace space, Point referencePoint)
    {
      switch (space)
      {
        case TangentSpace.Start:
          UpdateVector(ref startTangent, ref endTangent, referencePoint);
          break;
        case TangentSpace.End:
          UpdateVector(ref endTangent, ref startTangent, referencePoint);
          break;
      }
    }

    private void UpdateVector(ref Tangent updated, ref Tangent other, Point reference)
    {
      if (updated.type != TangentType.Vector) return;

      updated.position = TangentUtility.VectorPosition(this, reference, updated);

      if (other.type == TangentType.Aligned)
      {
        other.position = TangentUtility.AlignPosition(other, updated);
      }
    }

    private void UpdateTangentPosition(Vector3 newPosition, ref Tangent updated, ref Tangent other)
    {
      if (updated.position != newPosition)
      {
        updated.position = newPosition;

        if (other.type == TangentType.Aligned)
        {
          other.position = TangentUtility.AlignPosition(other, updated);
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
          updated.position = TangentUtility.AlignPosition(updated, other);
        }
        if (other.type == TangentType.Aligned)
        {
          other.position = TangentUtility.AlignPosition(other, updated);
        }
      }
    }

    public override bool Equals(object obj)
    {
      return obj is Point point &&
             position == point.position &&
             startTangent.Equals(point.startTangent) &&
             endTangent.Equals(point.endTangent);
    }

    public override int GetHashCode()
    {
      var hashCode = -225962414;
      hashCode *= -1521134295 + position.GetHashCode();
      hashCode *= -1521134295 + startTangent.GetHashCode();
      hashCode *= -1521134295 + endTangent.GetHashCode();
      return hashCode;
    }
  }

  public enum TangentSpace
  {
    Start, End
  }
}