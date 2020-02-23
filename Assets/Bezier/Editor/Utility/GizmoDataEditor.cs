using UnityEditor;

namespace SheepDev.EditorBezier.Utility
{
  public class GizmoDataEditor
  {
    public bool isShow;
    public float scale;
    private float minScale;
    private float maxScale;

    public GizmoDataEditor(float minScale = .3f, float maxScale = 2f)
    {
      this.minScale = minScale;
      this.maxScale = maxScale;
    }

    public void Inspector()
    {
      EditorGUILayout.Space();
      var isRepaint = false;
      var isShow = EditorGUILayout.Toggle("Show Gizmos", this.isShow);

      isRepaint |= this.isShow != isShow;
      this.isShow = isShow;

      if (this.isShow)
      {
        var scale = EditorGUILayout.Slider("Gizmo Size", this.scale, minScale, maxScale);
        isRepaint |= scale != this.scale;
        this.scale = scale;
      }

      if (isRepaint) SceneView.lastActiveSceneView.Repaint();
    }
  }
}