using UnityEngine;

namespace SheepDev.Bezier
{
  [RequireComponent(typeof(BezierSnap))]
  public class BezierMove : MonoBehaviour
  {
    public float speed;
    public float slerpSpeed;

    public BezierSnap snap { get; private set; }

    private void Awake()
    {
      snap = GetComponent<BezierSnap>();
      snap.positionSetting = PositionSetting.Default;
    }

    private void Update()
    {
      var transform = snap.GetTransform();
      snap.SetDistance(snap.distance + Time.deltaTime * speed);

      transform.position = snap.GetTargetPosition();
      if (snap.GetTargetRotation(out var targetRotation))
      {
        if (snap.rotateSetting == RotateSetting.Upward)
        {
          var currentUp = transform.rotation * Vector3.up;
          var desiredUp = targetRotation * Vector3.up;
          var targetUp = Vector3.RotateTowards(currentUp, desiredUp, Time.deltaTime * slerpSpeed, 1);
          var rotation = Quaternion.LookRotation(targetRotation * Vector3.forward, targetUp);
          transform.rotation = rotation;
        }
        else
        {
          transform.rotation = targetRotation;
        }
      }
    }

    private void OnValidate()
    {
      var snap = GetComponent<BezierSnap>();
      snap.positionSetting = PositionSetting.Default;
      snap.enabled = false;
    }
  }
}