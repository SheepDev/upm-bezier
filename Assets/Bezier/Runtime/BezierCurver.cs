using UnityEngine;

namespace Bezier
{
  public class BezierCurver : MonoBehaviour
  {
    [HideInInspector]
    public Point[] points;

    public BezierCurver()
    {
      var tangent1 = new Tangent(Vector3.right, TangentType.Aligned);
      var tangent2 = new Tangent(-Vector3.right, TangentType.Aligned);

      var point1 = new Point(Vector3.zero, tangent1, tangent2);
      var point2 = new Point(Vector3.forward * 2, tangent1, tangent2);

      points = new Point[] { point1, point2 };
    }
  }
}
