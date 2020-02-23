using UnityEngine;

namespace SheepDev.Bezier.Utility
{
  [System.Serializable]
  public struct BezierRotation
  {
    [Header("Rotate Setting")]
    public RotateSetting setting;
    public bool isFixedValueUpward;
    [SerializeField] private Vector3 fixedUpward;

    public bool IsUpward => setting == RotateSetting.Upward;
    public Vector3 FixedUpward => fixedUpward;

    public void SetUpward(Vector3 upward)
    {
      this.fixedUpward = upward == Vector3.zero ? Vector3.up : upward.normalized;
    }

    public bool GetTargetRotationByDistance(Transform transform, SectionCurve section, float distance, out Quaternion targetRotation, DistanceSpace space = DistanceSpace.Total)
    {
      targetRotation = Quaternion.identity;
      if (setting == RotateSetting.None) return false;

      switch (setting)
      {
        case RotateSetting.Default:
          targetRotation = section.GetRotationByDistance(distance, DistanceSpace.Total);
          break;
        case RotateSetting.Upward:
          var upward = GetUpward(transform);
          targetRotation = section.GetRotationByDistance(distance, upward, DistanceSpace.Total);
          break;
      }

      return true;
    }

    public bool GetTargetRotation(Transform transform, float t, SectionCurve section, out Quaternion targetRotation)
    {
      targetRotation = Quaternion.identity;
      if (setting == RotateSetting.None) return false;

      switch (setting)
      {
        case RotateSetting.Default:
          targetRotation = section.GetRotation(t);
          break;
        case RotateSetting.Upward:
          var upward = GetUpward(transform);
          targetRotation = section.GetRotation(t, upward);
          break;
      }

      return true;
    }

    public Vector3 GetUpward(Transform transform)
    {
      return isFixedValueUpward ? fixedUpward : transform.up;
    }
  }

  public enum RotateSetting
  {
    None, Default, Upward
  }
}