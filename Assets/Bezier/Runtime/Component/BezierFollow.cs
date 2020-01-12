using UnityEngine;

namespace SheepDev.Bezier
{
  public class BezierFollow : MonoBehaviour
  {
    [Header("Setup")]
    public BezierCurve curve;
    public float speed;

    [Header("Config")]
    public bool isNormalizeRoll;
    public bool isUseUpwards;
    public bool isInheritRoll;

    [Header("Extra Config")]
    public bool isConstantUpwards;
    public bool isOverwriteUpwards;

    [SerializeField]
    private Vector3 overwriteUpwards;

    [HideInInspector]
    [SerializeField]
    private Vector3 constantUpwards;

    [HideInInspector]
    [SerializeField]
    private int currentIndex;
    [HideInInspector]
    [SerializeField]
    private float currentDistance;
    private Transform cacheTransform;

    public Vector3 OverwriteUpwards { get => overwriteUpwards; set => overwriteUpwards = value.normalized; }

    private void OnEnable()
    {
      if (curve is null)
      {
        enabled = false;
        return;
      }

      if (isConstantUpwards)
      {
        constantUpwards = GetTransform().up;
      }
    }

    private void Update()
    {
      var transform = GetTransform();
      MoveObject(transform);
      currentDistance += speed * Time.deltaTime;
    }

    private void MoveObject(Transform transform)
    {
      var section = curve.GetSection(currentIndex);

      if (GetPositionAndRotation(section, out var position, out var rotation))
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

    private bool GetPositionAndRotation(SectionCurve section, out Vector3 position, out Quaternion rotation)
    {
      if (isUseUpwards)
      {
        var upwards = GetUpward();
        return section.GetPositionAndRotationByDistance(currentDistance, out position, out rotation, upwards);
      }

      return section.GetPositionAndRotationByDistance(currentDistance, out position, out rotation, Space.World, isNormalizeRoll, isInheritRoll);
    }

    private Vector3 GetUpward()
    {
      if (isOverwriteUpwards) return OverwriteUpwards;
      return (isConstantUpwards) ? constantUpwards : GetTransform().up;
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
      overwriteUpwards = (overwriteUpwards == Vector3.zero) ? Vector3.up : overwriteUpwards.normalized;
    }
  }
}