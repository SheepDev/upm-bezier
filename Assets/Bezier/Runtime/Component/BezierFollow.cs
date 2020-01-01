using Bezier;
using UnityEngine;

public class BezierFollow : MonoBehaviour
{
  public BezierCurve curve;
  public float speed;

  private int currentIndex;
  private float currentDistance;
  private Transform cacheTransform;

  private void Update()
  {
    var transform = GetTransform();
    MoveObject(transform);

    currentDistance += speed * Time.deltaTime;
  }

  private void MoveObject(Transform transform)
  {
    var point = curve.GetPoint(currentIndex);

    if (point.GetPositionAndRotationByDistance(currentDistance, out var position, out var rotation))
    {
      transform.position = position;
      transform.rotation = rotation;
    }
    else
    {
      currentIndex = curve.GetNextIndexPoint(currentIndex);
      currentDistance = point.Size - currentDistance;
      MoveObject(transform);
    }
  }

  public Transform GetTransform()
  {
    if (cacheTransform == null)
    {
      cacheTransform = transform;
    }

    return cacheTransform;
  }
}