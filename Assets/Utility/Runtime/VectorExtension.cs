using UnityEngine;

public static class VectorUtility
{
  public static void MoveVectorWithPivot(ref Vector3 pivot, ref Vector3 position, Vector3 positionTarget)
  {
    var offset = pivot - position;
    position = positionTarget;
    pivot = positionTarget + offset;
  }
}