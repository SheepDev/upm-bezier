using UnityEngine;

namespace SheepDev.Extension
{
  public static class FloatExtension
  {
    public static bool IsApproximately(this float a, float b, float margin)
    {
      return Mathf.Abs(a - b) < margin;
    }
  }
}