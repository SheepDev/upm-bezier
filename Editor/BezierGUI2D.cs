using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Bezier
{
  public class BezierGUI2D : EditorBehaviour
  {
    private Vector2 position;
    private float speed;
    private float maxWidth;

    public BezierGUI2D(Vector2 position, float speed = 20, float maxWidth = 250)
    {
      this.position = position;
      this.speed = speed;
      this.maxWidth = maxWidth;
    }

    public override void OnUpdate(float deltaTime)
    {
    }

    public override void OnSceneGUI(SceneView view)
    {
      Handles.BeginGUI();
      GUI2D(position);
      Handles.EndGUI();
    }

    private void GUI2D(Vector2 position)
    {
      var draws = new List<DrawStack>();
      var curveEditor = BezierCurveEditor.activeCurve;

      var titlePosition = position + new Vector2(10, 10); // left & top padding
      draws.Add(new GuiTitle(new Rect(titlePosition, new Vector2(maxWidth, 20)), curveEditor.Name, TextAnchor.MiddleCenter));
      position.y += 30;

      var buttonPosition = position + new Vector2(0, 10); // left & top padding
      draws.Add(new GuiEditButton(maxWidth, buttonPosition, new Vector2(200, 50)));
      position.y += 60;

      if (curveEditor.IsEdit)
      {
        draws.Add(new GuiTangentType(maxWidth, position, new Vector2(10, 20)));
        position.y += 60;
      }

      var totalHeight = position.y + 10;
      draws.Add(new GuiBackground(new Rect(this.position, new Vector2(maxWidth, totalHeight)), new Color(1, 1, 1, .3f)));

      draws.Sort();
      foreach (var draw in draws)
      {
        draw.Draw();
      }
    }

    struct GuiBackground : DrawStack
    {
      private Rect rect;
      private Color color;

      public float layer => 0;

      public GuiBackground(Rect rect, Color color)
      {
        this.rect = rect;
        this.color = color;
      }

      public int CompareTo(DrawStack other)
      {
        return layer.CompareTo(other.layer);
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
      private Rect rect;
      private string text;
      private TextAnchor anchor;

      public float layer => 4;

      public GuiTitle(Rect rect, string text, TextAnchor anchor)
      {
        this.rect = rect;
        this.text = text;
        this.anchor = anchor;
      }

      public int CompareTo(DrawStack other)
      {
        return layer.CompareTo(other.layer);
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
      private float maxWidth;
      private Vector2 size;
      private Vector2 position;

      public float layer => 5;

      public GuiEditButton(float maxWidth, Vector2 position, Vector2 size)
      {
        this.size = size;
        this.maxWidth = maxWidth;
        this.position = position;

        this.size.x = Mathf.Min(maxWidth, size.x);
      }

      public int CompareTo(DrawStack other)
      {
        return layer.CompareTo(other.layer);
      }

      public void Draw()
      {
        var curve = BezierCurveEditor.activeCurve;
        var text = (curve.IsEdit) ? "Finish Edit" : "Start Edit";
        position.x += (maxWidth / 2) - (size.x / 2);

        var isPress = GUI.Button(new Rect(position, size), text);
        if (isPress)
        {
          curve.EditToggle();
        }
      }
    }

    struct GuiTangentType : DrawStack
    {
      private float maxWidth;
      private Vector2 position;
      private Vector2 padding;

      public float layer => 7;

      public GuiTangentType(float maxWidth, Vector2 position, Vector2 padding)
      {
        this.maxWidth = maxWidth;
        this.position = position;
        this.padding = padding;
      }

      public int CompareTo(DrawStack other)
      {
        return layer.CompareTo(other.layer);
      }

      public void Draw()
      {
        var curve = BezierCurveEditor.activeCurve;
        var point = curve.ActivePoint;

        EditorGUI.BeginDisabledGroup(!curve.IsActivePointIndex);
        var width = maxWidth - padding.x * 2;
        var size = new Vector2(width, 10);
        position.x += padding.x;
        position.y += padding.y;

        var newTypeTangentStart =
          (TangentType)EditorGUI.EnumPopup(new Rect(position, size), "Type Tangent Start", point.TangentStart.type);
        position.y += padding.y;
        var newTypeTangentEnd =
          (TangentType)EditorGUI.EnumPopup(new Rect(position, size), "Type Tangent End", point.TangentEnd.type);
        EditorGUI.EndDisabledGroup();

        var isModify = point.TangentStart.type != newTypeTangentStart || point.TangentEnd.type != newTypeTangentEnd;
        if (isModify)
        {
          point.SetTangentType(newTypeTangentStart, TangentSpace.Start);
          point.SetTangentType(newTypeTangentEnd, TangentSpace.End);
          curve.SetActivePoint(point);
        }
      }
    }
  }
}