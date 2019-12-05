using UnityEngine;

namespace Bezier
{
  [System.Serializable]
  public struct Tangent
  {
    public Vector3 position;
    public TangentType type;

    public Tangent(Vector3 position, TangentType type)
    {
      this.position = position;
      this.type = type;
    }
  }

  public enum TangentType
  {
    Aligned, Vector, Free
  }
}