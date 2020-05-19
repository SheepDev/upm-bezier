using UnityEngine;
using static SheepDev.Bezier.Point;

namespace SheepDev.Bezier
{
  public static class PointUtility
  {
    public static void VectorTangent(ref Point previous, ref Point point, ref Point next)
    {
      if (previous.tangentStart.type == TangentType.Vector)
        VectorTangent(ref previous.tangentStart, ref previous.tangentEnd, previous.position, point.position);
      if (next.tangentEnd.type == TangentType.Vector)
        VectorTangent(ref next.tangentEnd, ref next.tangentStart, next.position, point.position);

      if (point.tangentStart.type == TangentType.Vector)
        VectorTangent(ref point.tangentStart, ref point.tangentEnd, point.position, next.position);
      if (point.tangentEnd.type == TangentType.Vector)
        VectorTangent(ref point.tangentEnd, ref point.tangentStart, point.position, previous.position);
    }

    public static void VectorTangent(ref Tangent tangent, ref Tangent otherTangent, Vector3 pointPosition, Vector3 lookAt)
    {
      var direction = (lookAt - pointPosition).normalized;
      var lenght = tangent.position.magnitude;
      tangent.position = direction * lenght;

      if (otherTangent.type == TangentType.Aligned)
        AlignTangent(ref otherTangent, tangent);
    }

    private static void AlignTangent(ref Tangent tangent, Tangent otherTangent)
    {
      var direction = -otherTangent.position.normalized;
      if (direction == Vector3.zero) return;

      var lenght = tangent.position.magnitude;
      tangent.position = direction * lenght;
    }
  }
}