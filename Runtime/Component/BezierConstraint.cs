using UnityEngine;

namespace SheepDev.Bezier
{
  public class BezierConstraint : MonoBehaviour
  {
    [Header("Target")]
    [SerializeField] private BezierCurve curve;
    [SerializeField] private int targetIndex;

    [Header("Snap Setting")]
    public bool isSnapOnEnable;
    public SnapSetting snapEnable;
    public SnapSetting snapUpdate;

    private Vector3 lastPosition;
    private Transform cacheTransform;

    public bool HasCurve => curve != null;
    public int TargetIndex => targetIndex;
    public Point TargetPoint => HasCurve ? curve.GetPoint(TargetIndex) : default;

    public BezierCurve Curve => curve;

    private void OnEnable()
    {
      if (!HasCurve)
      {
        enabled = false;
      }
      else if (isSnapOnEnable)
      {
        Snap(snapEnable);
      }
    }

    private void Update()
    {
      var transform = GetTransform();
      if (lastPosition != transform.position)
      {
        Snap();
      }
    }

    public void Snap()
    {
      Snap(snapUpdate);
    }

    public void Snap(SnapSetting type)
    {
      if (type == SnapSetting.ConstraintToPoint)
      {
        ConstraintToPoint();
      }
      else
      {
        PointToConstraint();
      }
    }

    public void ConstraintToPoint()
    {
      var transform = GetTransform();
      var point = curve.GetPoint(targetIndex);
      var targetPosition = point.position;

      lastPosition = transform.position = targetPosition;
    }

    public void PointToConstraint()
    {
      var transform = GetTransform();
      var point = curve.GetPoint(targetIndex);
      var targetPosition = transform.position;

      point.position = targetPosition;
      // curve.SetPoint(targetIndex, point);
      lastPosition = targetPosition;
    }

    public void SetTargetIndex(int value)
    {
      var max = HasCurve ? curve.PointLenght - 1 : 0;
      targetIndex = Mathf.Clamp(targetIndex, 0, max);
    }

    public void SetCurve(BezierCurve curve)
    {
      this.curve = curve;
      enabled &= HasCurve;
    }

    public Transform GetTransform()
    {
      if (cacheTransform == null)
      {
        cacheTransform = transform;
      }

      return cacheTransform;
    }

    public void OnValidate()
    {
      if (HasCurve)
      {
        SetTargetIndex(targetIndex);
      }
      else
      {
        enabled = false;
      }
    }
  }

  public enum SnapSetting
  {
    ConstraintToPoint, PointToConstraint
  }
}