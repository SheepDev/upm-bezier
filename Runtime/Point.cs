using UnityEngine;
using System;

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
        Update();
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
            Update();
          }
          break;
        case TangentSpace.End:
          if (endTangent.type != type)
          {
            endTangent.type = type;
            Update();
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
            Update();
          }
          break;
        case TangentSpace.End:
          if (endTangent.position != newPosition)
          {
            endTangent.position = newPosition;
            Update();
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
            Update();
          }
          break;
        case TangentSpace.End:
          if (endTangent.position != position)
          {
            endTangent.position = position;
            Update();
          }
          break;
      }
    }

    public void Update()
    {
      Debug.Log("Updated");
    }
  }

  public enum TangentSpace
  {
    Start, End
  }
}