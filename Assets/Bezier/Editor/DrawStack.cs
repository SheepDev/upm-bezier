using System;

public interface DrawStack : IComparable<DrawStack>
{
  float layer { get; }
  void Draw();
}