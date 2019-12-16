using UnityEngine;

namespace Bezier
{
  public struct WayPoint
  {
    public Vector3 position;
    public Quaternion rotation;

    public WayPoint(Vector3 position, Quaternion rotation) : this()
    {
      this.position = position;
      this.rotation = rotation;
    }
  }
}