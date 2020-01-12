using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static SheepDev.Bezier.BezierPoint;

namespace SheepDev.Bezier
{
  public class BezierGUI2D : EditorBehaviour<SelectCurve>
  {
    private Rect cameraRect;
    private List<DrawStack> draws;
    public Vector2 position;
    public float maxWidth;

    public BezierGUI2D(float maxWidth = 250)
    {
      draws = new List<DrawStack>();
      this.maxWidth = maxWidth;
    }

    public override void Reset()
    {
      position = new Vector2(10, 10);
      draws.Clear();
    }

    public override void BeforeSceneGUI(SelectCurve script)
    {
      SelectCurveGUI2D(script, position);
    }

    public override void SceneGUI(SceneView view)
    {
      Handles.BeginGUI();
      EditorGUI2D(view);

      draws.Sort();
      foreach (var draw in draws)
      {
        draw.Draw();
      }

      Handles.EndGUI();
    }

    private void SelectCurveGUI2D(SelectCurve select, Vector2 position)
    {
      var titlePosition = position + new Vector2(10, 10); // left & top padding
      draws.Add(new GuiTitle(new Rect(titlePosition, new Vector2(maxWidth, 20)), select.curve.name, TextAnchor.MiddleCenter));
      position.y += 30;

      var buttonPosition = position + new Vector2(0, 10); // left & top padding
      draws.Add(new GuiEditButton(select, maxWidth, buttonPosition, new Vector2(200, 50)));
      position.y += 60;

      if (select.IsEdit && select.IsSelectPoint)
      {
        draws.Add(new GuiTangentType(select, maxWidth, position, new Vector2(10, 20)));
        position.y += 60;
        draws.Add(new GuiRoll(select, maxWidth, position, new Vector2(10, 10)));
        position.y += 30;
      }

      var height = position.y - this.position.y + 10;
      draws.Add(new GuiBackground(new Rect(this.position, new Vector2(maxWidth, height)), new Color(1, 1, 1, .3f)));
      this.position.y += height + 10;
    }

    private void EditorGUI2D(SceneView view)
    {
      var rectCamera = view.camera.pixelRect;
      var size = new Vector2(150, 170);
      var position = rectCamera.size - size - new Vector2(10, 10);
      draws.Add(new GuiBackground(new Rect(position, size), new Color(1, 1, 1, .3f)));

      var elementSize = new Vector2(size.x - 20, 20);
      draws.Add(new GuiRotationConfig(new Rect(position, elementSize)));
    }

    struct GuiBackground : DrawStack
    {
      private readonly Rect rect;
      private readonly Color color;

      public float Depth => 0;
      public float Layer => 0;

      public GuiBackground(Rect rect, Color color)
      {
        this.rect = rect;
        this.color = color;
      }

      public int CompareTo(DrawStack other)
      {
        var layerCompare = Layer.CompareTo(other.Layer);
        return (layerCompare != 0) ? layerCompare : Depth.CompareTo(other.Depth);
      }

      public void Draw()
      {
        var oldColor = GUI.color;
        GUI.color = color;
        GUI.Box(rect, "");
        GUI.color = oldColor;
      }
    }

    struct GuiTitle : DrawStack
    {
      private string text;
      private readonly Rect rect;
      private readonly TextAnchor anchor;

      public float Depth => 0;
      public float Layer => 1;

      public GuiTitle(Rect rect, string text, TextAnchor anchor)
      {
        this.rect = rect;
        this.text = text;
        this.anchor = anchor;
      }

      public int CompareTo(DrawStack other)
      {
        var layerCompare = Layer.CompareTo(other.Layer);
        return (layerCompare != 0) ? layerCompare : Depth.CompareTo(other.Depth);
      }

      public void Draw()
      {
        var style = new GUIStyle();
        style.fontStyle = FontStyle.Bold;
        style.alignment = anchor;
        style.fontSize = 15;

        if (text.Length > 20)
        {
          text = text.Substring(0, 20) + "...";
        }

        GUI.Label(rect, text, style);
      }
    }

    struct GuiEditButton : DrawStack
    {
      private readonly SelectCurve select;
      private readonly float maxWidth;
      private readonly Vector2 size;
      private Vector2 position;

      public float Depth => 0;
      public float Layer => 1;

      public GuiEditButton(SelectCurve select, float maxWidth, Vector2 position, Vector2 size)
      {
        this.select = select;
        this.maxWidth = maxWidth;
        this.position = position;
        this.size = size;

        this.size.x = Mathf.Min(maxWidth, size.x);
      }

      public int CompareTo(DrawStack other)
      {
        var layerCompare = Layer.CompareTo(other.Layer);
        return (layerCompare != 0) ? layerCompare : Depth.CompareTo(other.Depth);
      }

      public void Draw()
      {
        var text = (select.IsEdit) ? "Finish Edit" : "Start Edit";
        position.x += (maxWidth / 2) - (size.x / 2);

        var isPress = GUI.Button(new Rect(position, size), text);
        if (isPress)
        {
          select.EditToggle();
        }
      }
    }

    struct GuiTangentType : DrawStack
    {
      private readonly SelectCurve select;
      private readonly float maxWidth;
      private readonly Vector2 padding;
      private Vector2 position;

      public float Depth => 0;
      public float Layer => 1;

      public GuiTangentType(SelectCurve select, float maxWidth, Vector2 position, Vector2 padding)
      {
        this.select = select;
        this.maxWidth = maxWidth;
        this.position = position;
        this.padding = padding;
      }

      public int CompareTo(DrawStack other)
      {
        var layerCompare = Layer.CompareTo(other.Layer);
        return (layerCompare != 0) ? layerCompare : Depth.CompareTo(other.Depth);
      }

      public void Draw()
      {
        var point = select.GetSelectPoint();
        var width = maxWidth - padding.x * 2;
        var size = new Vector2(width, 10);
        position.x += padding.x;
        position.y += padding.y;

        var newTypeTangentStart =
          (TangentType)EditorGUI.EnumPopup(new Rect(position, size), "Type Tangent Start", point.TangentStart.Type);
        position.y += padding.y;
        var newTypeTangentEnd =
          (TangentType)EditorGUI.EnumPopup(new Rect(position, size), "Type Tangent End", point.TangentEnd.Type);

        var isModify = point.TangentStart.Type != newTypeTangentStart || point.TangentEnd.Type != newTypeTangentEnd;
        if (isModify)
        {
          point.SetTangentType(newTypeTangentStart, TangentSelect.Start);
          point.SetTangentType(newTypeTangentEnd, TangentSelect.End);
          select.SetSelectPoint(point);
        }
      }
    }

    struct GuiRotationConfig : DrawStack
    {
      private Rect rect;

      public float Depth => 0;
      public float Layer => 1;

      public GuiRotationConfig(Rect rect)
      {
        this.rect = rect;
      }

      public int CompareTo(DrawStack other)
      {
        var layerCompare = Layer.CompareTo(other.Layer);
        return (layerCompare != 0) ? layerCompare : Depth.CompareTo(other.Depth);
      }

      public void Draw()
      {
        rect.position += new Vector2(10, 10);
        BezierCurveEditor.IsShowRotationHandle = GUI.Toggle(rect, BezierCurveEditor.IsShowRotationHandle, "Show Rotation");
        rect.position += new Vector2(0, 20);
        BezierCurveEditor.IsNormalizeRotationHandle = GUI.Toggle(rect, BezierCurveEditor.IsNormalizeRotationHandle, "Is Normalize");
        rect.position += new Vector2(0, 20);
        BezierCurveEditor.IsInheritRoll = GUI.Toggle(rect, BezierCurveEditor.IsInheritRoll, "Use Inherit Roll");
        rect.position += new Vector2(0, 20);
        BezierCurveEditor.IsUseUpwards = GUI.Toggle(rect, BezierCurveEditor.IsUseUpwards, "Use upwards");

        EditorGUI.BeginDisabledGroup(!BezierCurveEditor.IsUseUpwards);
        rect.position += new Vector2(0, 25);
        BezierCurveEditor.Upwards = EditorGUI.Vector3Field(rect, "Upwards", BezierCurveEditor.Upwards);
        EditorGUI.EndDisabledGroup();

        rect.position += new Vector2(0, 50);
        BezierCurveEditor.Distance =
        GUI.HorizontalSlider(rect, BezierCurveEditor.Distance, 1, 5);
      }
    }

    struct GuiToggle : DrawStack
    {
      private readonly Rect rect;

      public float Depth => 0;
      public float Layer => 1;

      public GuiToggle(Rect rect)
      {
        this.rect = rect;
      }

      public int CompareTo(DrawStack other)
      {
        var layerCompare = Layer.CompareTo(other.Layer);
        return (layerCompare != 0) ? layerCompare : Depth.CompareTo(other.Depth);
      }

      public void Draw()
      {
        BezierCurveEditor.IsShowRotationHandle = GUI.Toggle(rect, BezierCurveEditor.IsShowRotationHandle, "Show Rotation");
      }
    }

    struct GuiRoll : DrawStack
    {
      private readonly SelectCurve select;
      private readonly float maxWidth;
      private readonly Vector2 padding;
      private Vector2 position;

      public float Depth => 0;
      public float Layer => 1;

      public GuiRoll(SelectCurve select, float maxWidth, Vector2 position, Vector2 padding)
      {
        this.select = select;
        this.maxWidth = maxWidth;
        this.position = position;
        this.padding = padding;
      }

      public int CompareTo(DrawStack other)
      {
        var layerCompare = Layer.CompareTo(other.Layer);
        return (layerCompare != 0) ? layerCompare : Depth.CompareTo(other.Depth);
      }

      public void Draw()
      {
        EditorGUI.BeginDisabledGroup(!select.IsSelectPoint);
        var width = maxWidth - padding.x * 2;
        var size = new Vector2(width, 20);
        position.x += padding.x;
        position.y += padding.y;

        var selectPoint = select.GetSelectPoint();
        var roll = EditorGUI.FloatField(new Rect(position, size), "Roll", selectPoint.GetRoll());
        select.SetSelectPointRoll(roll);
      }
    }

    struct GuiSlide : DrawStack
    {
      private readonly Rect rect;

      public float Depth => 0;
      public float Layer => 1;

      public GuiSlide(Rect rect)
      {
        this.rect = rect;
      }

      public int CompareTo(DrawStack other)
      {
        var layerCompare = Layer.CompareTo(other.Layer);
        return (layerCompare != 0) ? layerCompare : Depth.CompareTo(other.Depth);
      }

      public void Draw()
      {
        BezierCurveEditor.Distance =
        GUI.HorizontalSlider(rect, BezierCurveEditor.Distance, 1, 5);
      }
    }
  }
}