using UnityEngine;

namespace SheepDev.Bezier
{
  [ExecuteInEditMode]
  public class BezierSnap : MonoBehaviour
  {
    [SerializeField] private BezierCurve curve;

    [Header("Position Setting")]
    public PositionSetting positionSetting;
    [SerializeField] private int sectionIndex;
    [SerializeField] [Range(0f, 1f)] private float t;
    [SerializeField] internal float distance;
    [SerializeField] [Range(0f, 1f)] private float porcent;

    [Header("Rotate Setting")]
    public RotateSetting rotateSetting;
    public bool isFixedValueUpward;
    [SerializeField] private Vector3 fixedUpward;
    private Transform cacheTransform;

    public BezierCurve Curve => curve;
    public bool HasBezier => curve != null;
    public int SectionIndex => sectionIndex;
    public float T { get => t; set => t = Mathf.Clamp01(value); }
    public float Porcent { get => porcent; set => porcent = Mathf.Clamp01(value); }

    public BezierSnap()
    {
      rotateSetting = RotateSetting.Default;
      fixedUpward = Vector3.up;
    }

    private void OnEnable()
    {
      if (DisableIsInvalid()) return;
      SetSectionIndex(sectionIndex);
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
      transform.position = GetTargetPosition();
      if (GetTargetRotation(out var targetRotation))
      {
        transform.rotation = targetRotation;
      }
    }

    public void SetCurve(BezierCurve value, bool isEnable = true)
    {
      curve = value;
      enabled = HasBezier && isEnable;
    }

    public void SetSectionIndex(int index)
    {
      var max = HasBezier ? curve.SectionLenght - 1 : 0;
      sectionIndex = Mathf.Clamp(sectionIndex, 0, max);
    }

    public void SetUpward(Vector3 upward)
    {
      this.fixedUpward = upward == Vector3.zero ? Vector3.up : upward.normalized;
    }

    public void SetDistance(float distance)
    {
      if (curve.IsLoop) distance = Mathf.Repeat(distance, curve.Size);
      this.distance = Mathf.Clamp(distance, 0, curve.Size);
    }

    public Vector3 GetTargetPosition()
    {
      var section = GetSection(out float targetDistance);

      switch (positionSetting)
      {
        case PositionSetting.Default:
        case PositionSetting.Porcent:
          return section.GetPositionByDistance(targetDistance, DistanceSpace.Total);
        case PositionSetting.Section:
          return section.GetPosition(t);
      }

      throw new System.Exception();
    }

    public bool GetTargetRotation(out Quaternion targetRotation)
    {
      targetRotation = Quaternion.identity;
      if (rotateSetting == RotateSetting.None) return false;

      var section = GetSection(out float targetDistance);
      var isUseDistance = positionSetting <= PositionSetting.Porcent;

      switch (rotateSetting)
      {
        case RotateSetting.Default:
          targetRotation = isUseDistance ?
            section.GetRotationByDistance(targetDistance, DistanceSpace.Total) : section.GetRotation(t);
          break;
        case RotateSetting.Upward:
          var upward = GetUpward();
          targetRotation = isUseDistance ?
            section.GetRotationByDistance(targetDistance, upward, DistanceSpace.Total) : section.GetRotation(t, upward);
          break;
      }

      return true;
    }

    public Vector3 GetUpward()
    {
      return isFixedValueUpward ? fixedUpward : GetTransform().up;
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

    private SectionCurve GetSection(out float targetDistance)
    {
      targetDistance = 0f;

      switch (positionSetting)
      {
        case PositionSetting.Default:
        case PositionSetting.Porcent:
          var isPorcent = positionSetting == PositionSetting.Porcent;
          targetDistance = isPorcent ? curve.Size * porcent : this.distance;
          return GetSection(targetDistance);
        case PositionSetting.Section:
          return GetSection(sectionIndex);
      }

      throw new System.Exception();
    }

    private SectionCurve GetSection(int index)
    {
      return curve.GetSection(index);
    }

    private SectionCurve GetSection(float distance)
    {
      return curve.GetSection(distance);
    }

    public void OnValidate()
    {
      if (DisableIsInvalid()) return;
      SetSectionIndex(sectionIndex);
      SetUpward(fixedUpward);
      SetDistance(distance);
    }
  }

  public enum PositionSetting
  {
    Default, Porcent, Section
  }

  public enum RotateSetting
  {
    None, Default, Upward
  }
}
