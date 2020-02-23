using SheepDev.Bezier.Utility;
using UnityEngine;

namespace SheepDev.Bezier
{
  [ExecuteInEditMode]
  public class BezierSnap : MonoBehaviour
  {
    [SerializeField] private BezierCurve curve;
    public BezierPosition position;
    public BezierRotation rotation;
    private Transform cacheTransform;

    public BezierCurve Curve => curve;
    public bool HasBezier => curve != null;

    private void OnEnable()
    {
      if (DisableIsInvalid()) return;
    }

    private void Update()
    {
      Snap();
    }

    [ContextMenu("Snap in Bezier")]
    public void Snap()
    {
      if (!HasBezier) return;

      var transform = GetTransform();
      transform.position = GetSnapPosition();

      if (GetRotation(transform, out var targetRotation))
      {
        transform.rotation = targetRotation;
      }
    }

    public void SetCurve(BezierCurve value, bool isEnable = true)
    {
      curve = value;
      enabled = HasBezier && isEnable;
    }

    public Vector3 GetSnapPosition()
    {
      return position.GetTargetPosition(curve);
    }

    public bool GetSnapRotation(out Quaternion targetRotation)
    {
      var transform = GetTransform();
      return GetRotation(transform, out targetRotation);
    }

    private bool GetRotation(Transform transform, out Quaternion targetRotation)
    {
      var section = position.GetSection(curve);

      switch (position.setting)
      {
        case PositionSetting.Default:
          return rotation.GetTargetRotationByDistance(transform, section, position.Distance, out targetRotation);
        case PositionSetting.Porcent:
          return rotation.GetTargetRotationByDistance(transform, section, curve.Size * position.Porcent, out targetRotation);
        case PositionSetting.Section:
          return rotation.GetTargetRotation(transform, position.T, section, out targetRotation);
      }

      throw new System.Exception();
    }

    public Transform GetTransform()
    {
      if (cacheTransform == null)
      {
        cacheTransform = transform;
      }

      return cacheTransform;
    }

    protected bool DisableIsInvalid()
    {
      var isDisable = !HasBezier;
      if (isDisable) enabled = false;
      return isDisable;
    }

    public void OnValidate()
    {
      if (DisableIsInvalid()) return;

      position.SetDistance(curve, position.Distance);
      position.SetSectionIndex(curve, position.SectionIndex);

      rotation.SetUpward(rotation.FixedUpward);
    }
  }
}
