using UnityEngine;

namespace SheepDev.Bezier
{
  public struct Waypoint
  {
    public Vector3 position;
    public Quaternion rotation;

    public Waypoint(Vector3 position, Quaternion rotation) : this()
    {
      this.position = position;
      this.rotation = rotation;
    }
  }
}
