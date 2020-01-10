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
    var section = curve.GetSection(currentIndex);

    if (section.GetPositionAndRotationByDistance(currentDistance, out var position, out var rotation, Space.World, true))
    {
      transform.position = position;
      transform.rotation = rotation;
    }
    else
    {
      currentIndex = curve.GetNextIndexPoint(currentIndex);
      currentDistance -= section.Size;
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