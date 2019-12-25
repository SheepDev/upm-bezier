using System;
using UnityEditor;
using UnityEngine;

namespace Bezier
{
  public class CurveEditor
  {
    private bool isEdit;
    private int activePointIndex;
    private BezierCurve script;
    private EditorBehaviour[] behaviours;

    private EditorApplication.CallbackFunction Update;
    private Action<SceneView> SceneGUI;

    public bool IsEdit => isEdit;
    public BezierCurve Curve => script;
    public string Name => (Curve != null) ? Curve.name : "None";

    public int ActivePointIndex => activePointIndex;
    public bool IsActivePointIndex => activePointIndex >= 0 && activePointIndex < Curve.Lenght;
    public BezierPoint ActivePoint => (IsActivePointIndex) ? script.GetWorldPoint(activePointIndex) : default;

    public CurveEditor()
    {
      activePointIndex = -1;
      var gui2D = new BezierGUI2D(new Vector2(10, 10));
      var gui3D = new BezierGUI3D();
      var eventEditor = new EventEditor();

      behaviours = new EditorBehaviour[3];
      behaviours[0] = eventEditor;
      behaviours[1] = gui3D;
      behaviours[2] = gui2D;
    }

    private void OnSceneGUI(SceneView view)
    {
      Valid();

      foreach (var behaviour in behaviours)
      {
        behaviour.OnSceneGUI(view);
      }
    }

    public void Enable(BezierCurve script)
    {
      if (this.Curve == script) return; else Disable();

      this.script = script;
      Edit(false);

      // EditorApplication.update += Update = OnUpdate;
      SceneView.duringSceneGui += SceneGUI = OnSceneGUI;
    }

    public void Valid()
    {
      if (script is null)
      {
        Disable();
      }
    }

    public void Disable()
    {
      SceneView.duringSceneGui -= SceneGUI;
      // EditorApplication.update -= Update;

      Edit(false);
      script = null;
    }

    public void Blur()
    {
      if (Curve is null || !IsEdit)
      {
        Disable();
      }
    }

    public void SetActivePoint(BezierPoint point)
    {
      if (activePointIndex < 0) return;

      var oldPoint = script.GetWorldPoint(activePointIndex);

      if (!oldPoint.Equals(point))
      {
        Undo.RecordObject(script, "Set Active Point " + activePointIndex);
        script.SetWorldPoint(activePointIndex, point);
      }
    }

    public void AddPoint(BezierPoint point)
    {
      var lenght = script.Lenght;
      Undo.RecordObject(script, "Add Point " + lenght);
      script.AddWorldPoint(point);
      SetActivePointIndex(lenght);
    }

    public void SetLoop(bool isLoop)
    {
      if (script.IsLoop != isLoop)
      {
        Undo.RecordObject(script, "Set Curver Loop");
        script.SetLoop(isLoop);
      }
    }

    public void SetActivePointIndex(int index)
    {
      if (index >= 0 && index < Curve.Lenght)
      {
        activePointIndex = index;
      }
    }

    public void DisableActivePointIndex()
    {
      activePointIndex = -1;
    }

    public void Edit(bool value)
    {
      if (isEdit == value) return;

      isEdit = value;
      Tools.hidden = value;

      if (!value)
      {
        activePointIndex = -1;

        if (script != null)
        {
          Selection.activeObject = script.gameObject;
        }
      }
    }

    public void EditToggle()
    {
      Edit(!isEdit);
    }
  }
}