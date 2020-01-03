using System;

public interface DrawStack : IComparable<DrawStack>
{
  float Layer { get; }
  float Depth { get; }
  void Draw();
}