using UnityEngine;

namespace SheepDev.Bezier
{
  public class BezierPointConstraint : MonoBehaviour
  {
    public BezierCurve curve;
    public SnapType snapEnable;
    public SnapType snap;
    public int targetPointIndex;

    private Vector3 lastPosition;
    private Transform cacheTransform;

    private void OnEnable()
    {
      if (curve is null)
      {
        enabled = false;
        return;
      }

      var transform = GetTransform();
      Snap(snapEnable);
    }

    private void Update()
    {
      var transform = GetTransform();
      var targetPosition = transform.position;

      if (lastPosition != targetPosition)
      {
        Snap();
      }
    }

    public void Snap()
    {
      Snap(snap);
    }

    public void Snap(SnapType type)
    {
      if (type == SnapType.ConstraintToPoint)
      {
        ConstraintToPoint();
      }
      else
      {
        PointToConstraint();
      }
    }

    [ContextMenu("Constraint To Point")]
    public void ConstraintToPoint()
    {
      var transform = GetTransform();
      var point = curve.GetPoint(targetPointIndex);
      var targetPosition = point.position;

      lastPosition = transform.position = targetPosition;
    }

    [ContextMenu("Point To Constraint")]
    public void PointToConstraint()
    {
      var transform = GetTransform();
      var point = curve.GetPoint(targetPointIndex);
      var targetPosition = transform.position;

      point.position = targetPosition;
      curve.SetPoint(targetPointIndex, point);
      lastPosition = targetPosition;
    }

    public Transform GetTransform()
    {
      if (cacheTransform == null)
      {
        cacheTransform = transform;
      }

      return cacheTransform;
    }

    private void OnValidate()
    {
      if (curve is null)
      {
        enabled = false;
        return;
      }

      targetPointIndex = Mathf.Clamp(targetPointIndex, 0, curve.PointLenght - 1);
    }

    private void OnDrawGizmosSelected()
    {
      if (curve != null)
      {
        var transform = GetTransform();
        var point = curve.GetPoint(targetPointIndex);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, point.position);
      }
    }
  }

  public enum SnapType
  {
    ConstraintToPoint, PointToConstraint
  }
}