using UnityEngine;

namespace Bezier
{
  public static class TangentUtility
  {
    public static Vector3 AlignPosition(Tangent tangent, Tangent reference)
    {
      var magnitude = tangent.position.magnitude;
      var direction = -reference.position.normalized;
      return direction * magnitude;
    }

    public static Vector3 VectorPosition(Point point, Point reference, Tangent tangent)
    {
      var magnitude = tangent.position.magnitude;
      var direction = (reference.position - point.position).normalized;
      return direction * magnitude;
    }
  }
}