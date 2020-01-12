using UnityEngine;

namespace SheepDev.Utility
{
  public static class QuaternionUtility
  {
    public static Quaternion ProjectOnDirection(Quaternion rotation, Vector3 direction)
    {
      var angleY = QuaternionUtility.AngleInAxis(rotation, direction, AxisDirection.Up);
      var rotationY = Quaternion.Euler(0, angleY, 0);
      rotation *= rotationY;

      var angleX = QuaternionUtility.AngleInAxis(rotation, direction, AxisDirection.Right);
      var rotationX = Quaternion.Euler(angleX, 0, 0);
      rotation *= rotationX;

      return rotation;
    }

    public static float AngleInAxis(Quaternion rotation, Vector3 direction, AxisDirection axis)
    {
      var up = rotation * Vector3.up;
      var right = rotation * Vector3.right;
      var forward = rotation * Vector3.forward;

      switch (axis)
      {
        case AxisDirection.Up:
          return CalculateAngleInAxis(direction, up, forward, right);
        case AxisDirection.Right:
          return CalculateAngleInAxis(direction, right, forward, -up);
        default:
          throw new System.Exception();
      }
    }

    private static float CalculateAngleInAxis(Vector3 direction, Vector3 planeNormal, Vector3 angleFrom, Vector3 dotLhs)
    {
      var projectVector = Vector3.ProjectOnPlane(direction, planeNormal);
      var angle = Vector3.Angle(angleFrom, projectVector);
      var dot = Vector3.Dot(dotLhs, projectVector);
      return angle *= (dot < 0) ? -1 : 1;
    }
  }

  public enum AxisDirection
  {
    Up, Right
  }
}