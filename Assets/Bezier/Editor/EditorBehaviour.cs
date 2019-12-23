using UnityEditor;

namespace Bezier
{
  public abstract class EditorBehaviour
  {
    public abstract void OnUpdate(float deltaTime);
    public abstract void OnSceneGUI(SceneView view);
  }
}