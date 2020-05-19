using UnityEngine;

namespace SheepDev.Bezier
{
  [System.Serializable]
  public struct Tangent
  {
    [HideInInspector] public Vector3 position;
    public TangentType type;

    public Tangent(Vector3 position, TangentType type) : this()
    {
      this.position = position;
      this.type = type;
    }

    public Tangent(Vector3 position) : this(position, TangentType.Free)
    {
    }

    public override bool Equals(object obj)
    {
      return obj is Tangent tangent &&
             position == tangent.position &&
             type == tangent.type;
    }

    public override int GetHashCode()
    {
      var hashCode = -1726561561;
      hashCode = hashCode * -1521134295 + position.GetHashCode();
      hashCode = hashCode * -1521134295 + type.GetHashCode();
      return hashCode;
    }
  }

  public enum TangentType
  {
    Aligned, Vector, Free
  }
}