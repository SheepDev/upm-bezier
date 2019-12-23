using UnityEngine;

namespace Input
{
  public static class InputEditor
  {
    public static bool GetKeyDown(KeyCode key, out Event currentEvent)
    {
      currentEvent = Event.current;
      return currentEvent.type == EventType.KeyDown && currentEvent.keyCode == key;
    }

    public static bool GetKeyUp(KeyCode key, out Event currentEvent)
    {
      currentEvent = Event.current;
      return currentEvent.type == EventType.KeyUp && currentEvent.keyCode == key;
    }

    public static bool GetMouseDown(int mouse, out Event currentEvent)
    {
      currentEvent = Event.current;
      return currentEvent.type == EventType.MouseDown && currentEvent.button == mouse;
    }
  }
}