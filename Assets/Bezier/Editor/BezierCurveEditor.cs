using UnityEditor;
using Bezier;

[CustomEditor(typeof(BezierCurve), true)]
public class BezierCurveEditor : Editor
{
  public static CurveEditor activeCurve;

  private void OnEnable()
  {
    if (activeCurve is null)
    {
      activeCurve = new CurveEditor();
    }

    var script = target as BezierCurve;
    activeCurve.Enable(script);
  }

  private void OnDisable()
  {
    activeCurve.Blur();
  }

  public override void OnInspectorGUI()
  {
    var isEdit = EditorGUILayout.Toggle("Edit Bezier", activeCurve.IsEdit);
    activeCurve.Edit(isEdit);

    var isLoop = EditorGUILayout.Toggle("Is Loop", activeCurve.Curve.IsLoop);
    activeCurve.SetLoop(isLoop);

    var isDebug = EditorGUILayout.Toggle("Is Debug", activeCurve.isDebug);
    activeCurve.isDebug = isDebug;

    serializedObject.ApplyModifiedProperties();
    base.OnInspectorGUI();
  }

  [DrawGizmo(GizmoType.NonSelected)]
  static void DrawGizmoBezierCurveNonSelected(BezierCurve scr, GizmoType gizmoType)
  {
    BezierGUI3D.DrawCurve(scr);
  }
}
