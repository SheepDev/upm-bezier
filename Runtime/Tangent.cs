using UnityEngine;

namespace Bezier
{
  [System.Serializable]
  public struct Tangent
  {
    [SerializeField]
    private Vector3 position;
    [SerializeField]
    private TangentType type;

    public Vector3 localPosition { get => position; internal set => position = value; }
    public TangentType Type { get => type; internal set => type = value; }

    public Tangent(Vector3 localPosition, TangentType type) : this()
    {
      position = localPosition;
      Type = type;
    }

    public override bool Equals(object obj)
    {
      return obj is Tangent tangent &&
             localPosition == tangent.localPosition &&
             Type == tangent.Type;
    }

    public override int GetHashCode()
    {
      var hashCode = -1726561561;
      hashCode = hashCode * -1521134295 + localPosition.GetHashCode();
      hashCode = hashCode * -1521134295 + Type.GetHashCode();
      return hashCode;
    }
  }

  public enum TangentType
  {
    Aligned, Vector, Free
  }
}