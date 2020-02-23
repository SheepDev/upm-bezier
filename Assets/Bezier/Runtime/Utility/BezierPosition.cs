using UnityEngine;

namespace SheepDev.Bezier.Utility
{
  [System.Serializable]
  public struct BezierPosition
  {
    [Header("Position Setting")]
    public PositionSetting setting;
    [SerializeField] private int sectionIndex;
    [SerializeField] private float distance;
    [SerializeField] [Range(0f, 1f)] private float t;
    [SerializeField] [Range(0f, 1f)] private float porcent;

    public float Distance => distance;
    public int SectionIndex => sectionIndex;
    public float Porcent { get => porcent; set => porcent = Mathf.Clamp01(value); }
    public float T { get => t; set => t = Mathf.Clamp01(value); }

    public Vector3 GetTargetPosition(BezierCurve curve)
    {
      if (curve == null) return Vector3.zero;

      var section = GetSection(curve);

      switch (setting)
      {
        case PositionSetting.Default:
          return section.GetPositionByDistance(distance, DistanceSpace.Total);
        case PositionSetting.Porcent:
          return section.GetPositionByDistance(curve.Size * porcent, DistanceSpace.Total);
        case PositionSetting.Section:
          return section.GetPosition(t);
      }

      throw new System.Exception();
    }

    public void SetSectionIndex(BezierCurve curve, int index)
    {
      var max = curve.SectionLenght - 1;
      sectionIndex = Mathf.Clamp(sectionIndex, 0, max);
    }

    public void SetDistance(BezierCurve curve, float distance)
    {
      if (curve == null)
      {
        this.distance = 0;
      }
      else if (curve.IsLoop)
      {
        this.distance = Mathf.Repeat(distance, curve.Size);
      }
      else
      {
        this.distance = Mathf.Clamp(distance, 0, curve.Size);
      }
    }

    public SectionCurve GetSection(BezierCurve curve)
    {
      switch (setting)
      {
        case PositionSetting.Default:
          return GetSection(curve, distance);
        case PositionSetting.Porcent:
          return GetSection(curve, curve.Size * porcent);
        case PositionSetting.Section:
          return GetSection(curve, sectionIndex);
      }

      throw new System.Exception();
    }

    private SectionCurve GetSection(BezierCurve curve, int index)
    {
      return curve.GetSection(index);
    }

    private SectionCurve GetSection(BezierCurve curve, float distance)
    {
      return curve.GetSection(distance);
    }
  }

  public enum PositionSetting
  {
    Default, Porcent, Section
  }
}