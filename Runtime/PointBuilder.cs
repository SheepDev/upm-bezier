using UnityEngine;

namespace Bezier
{
  public static class PointBuilder
  {
    public static Point Copy(Point point, Vector3 position, Vector3 startTangentPosition, Vector3 endTangentPosition)
    {
      var startTangent = new Tangent(startTangentPosition, point.StartTangentType);
      var endTangent = new Tangent(endTangentPosition, point.EndTangentType);
      return Copy(point, position, startTangent, endTangent);
    }

    public static Point Copy(Point point, Vector3 position, Tangent startTangent, Tangent endTangent)
    {
      Point copyPoint = new Point();
      copyPoint.position = position;
      copyPoint.startTangent = startTangent;
      copyPoint.endTangent = endTangent;
      copyPoint.arcDistance = point.arcDistance;
      copyPoint.tDistance = point.tDistance;

      return copyPoint;
    }
  }
}