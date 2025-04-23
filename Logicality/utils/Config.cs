using System.Numerics;
using Raylib_cs;

namespace Logicality.utils;

public static class Config
{
  public static bool Debug = true;
  public static Vector2 Resolution = new (1920, 1080);
  public static Camera2D Camera = new(){Offset = Vector2.Zero, Target = Vector2.Zero, Rotation = 0, Zoom = 1};
}