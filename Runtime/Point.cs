using UnityEngine;
using System;
using System.Collections.Generic;

namespace Bezier
{
  [Serializable]
  public struct Point
  {
    [SerializeField]
    private Vector3 position;
    [SerializeField]
    internal Tangent startTangent;
    [SerializeField]
    internal Tangent endTangent;

    public Vector3 Position => position;
    public Vector3 StartTangentPosition => startTangent.position + Position;
    public Vector3 EndTangentPosition => endTangent.position + Position;
    public Vector3 StartTangentLocalPosition => startTangent.position;
    public Vector3 EndTangentLocalPosition => endTangent.position;
    public TangentType StartTangentType => startTangent.type;
    public TangentType EndTangentType => endTangent.type;

    public Point(Vector3 position, Tangent startTangent, Tangent endTangent)
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
          if (startTangent.type != type)
          {
            startTangent.type = type;
            UpdateAligned(ref startTangent, ref endTangent);
          }
          break;
        case TangentSpace.End:
          if (endTangent.type != type)
          {
            endTangent.type = type;
            UpdateAligned(ref endTangent, ref startTangent);
          }
          break;
      }
    }

    public void SetTangentPosition(Vector3 position, TangentSpace space)
    {
      var newPosition = position - this.position;

      switch (space)
      {
        case TangentSpace.Start:
          if (startTangent.position != newPosition)
          {
            startTangent.position = newPosition;
            UpdateAligned(ref startTangent, ref endTangent);
          }
          break;
        case TangentSpace.End:
          if (endTangent.position != newPosition)
          {
            endTangent.position = newPosition;
            UpdateAligned(ref endTangent, ref startTangent);
          }
          break;
      }
    }

    public void SetTangentLocalPosition(Vector3 position, TangentSpace space)
    {
      switch (space)
      {
        case TangentSpace.Start:
          if (startTangent.position != position)
          {
            startTangent.position = position;
            UpdateAligned(ref startTangent, ref endTangent);
          }
          break;
        case TangentSpace.End:
          if (endTangent.position != position)
          {
            endTangent.position = position;
            UpdateAligned(ref endTangent, ref startTangent);
          }
          break;
      }
    }

    public void UpdateAligned(ref Tangent updated, ref Tangent other)
    {
      if (other.type == TangentType.Aligned)
      {
        var magnitude = other.position.magnitude;
        var direction = -updated.position.normalized;

        other.position = direction * magnitude;
      }
    }

    public void UpdateVector(TangentSpace space, Point referencePoint)
    {
      float magnitude = 0;
      Vector3 direction = Vector3.one;

      switch (space)
      {
        case TangentSpace.Start:
          if (startTangent.type != TangentType.Vector)
            return;
          magnitude = startTangent.position.magnitude;
          direction = (referencePoint.position - position).normalized;
          startTangent.position = direction * magnitude;
          break;
        case TangentSpace.End:
          if (endTangent.type != TangentType.Vector)
            return;
          magnitude = endTangent.position.magnitude;
          direction = (referencePoint.position - position).normalized;
          endTangent.position = direction * magnitude;
          break;
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