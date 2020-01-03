using UnityEditor;

public abstract class EditorBehaviour<T>
{
  public abstract void BeforeSceneGUI(T script);
  public abstract void SceneGUI(SceneView view);
  public abstract void Reset();
}