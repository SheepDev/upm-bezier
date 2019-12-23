using UnityEditor;
using Bezier;

[CustomEditor(typeof(BezierCurver), true)]
public class BezierCurveEditor : Editor
{
  public static CurveEditor activeCurve;

  private void OnEnable()
  {
    if (activeCurve is null)
    {
      activeCurve = new CurveEditor();
    }

    var script = target as BezierCurver;
    activeCurve.Enable(script);
  }

  private void OnDisable()
  {
    activeCurve.Blur();
  }

  public override void OnInspectorGUI()
  {
    activeCurve.Edit(EditorGUILayout.Toggle("Edit Bezier", activeCurve.IsEdit));
    base.OnInspectorGUI();
  }
}
