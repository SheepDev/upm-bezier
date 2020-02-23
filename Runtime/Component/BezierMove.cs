using SheepDev.Bezier.Utility;
using UnityEngine;

namespace SheepDev.Bezier
{
  [ExecuteInEditMode]
  public class BezierMove : MonoBehaviour
  {
    public BezierCurve curve;
    [SerializeField] private BezierPosition position;
    public BezierRotation rotation;

    [Header("Motion Settings")]
    public float speed;
    public float slerpSpeed;

    private Transform cacheTransform;

    public BezierPosition Position => position;

    public BezierMove()
    {
      position.setting = PositionSetting.Default;
    }

    private void Update()
    {
      var transform = GetTransform();
      transform.position = GetTargetPosition();

      if (GetRotation(transform, out var targetRotation))
      {
        if (rotation.IsUpward)
        {
          var currentUp = transform.rotation * Vector3.up;
          var desiredUp = targetRotation * Vector3.up;
          var targetUp = Vector3.RotateTowards(currentUp, desiredUp, Time.deltaTime * slerpSpeed, 1);
          var rotation = Quaternion.LookRotation(targetRotation * Vector3.forward, targetUp);

#if UNITY_EDITOR
          if (Application.isEditor && !Application.isPlaying)
          {
            rotation = targetRotation;
          }
#endif
          transform.rotation = rotation;
        }
        else
        {
          transform.rotation = targetRotation;
        }
      }

#if UNITY_EDITOR
      if (Application.isEditor && !Application.isPlaying) return;
#endif

      var nextDistance = position.Distance + Time.deltaTime * speed;
      position.SetDistance(curve, nextDistance);
    }

    public Vector3 GetTargetPosition()
    {
      return position.GetTargetPosition(curve);
    }

    public bool GetTargetRotation(out Quaternion targetRotation)
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

    public void OnValidate()
    {
      position.SetDistance(curve, position.Distance);
      rotation.SetUpward(rotation.FixedUpward);
    }
  }
}