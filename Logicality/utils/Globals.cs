using System.Numerics;
using Raylib_cs;

namespace Logicality.utils;

public static class Globals
{
  public static bool Debug = true;
  public static readonly Vector2 Resolution = new (1920, 1080);
  public static readonly Vector2 MapSize = new (1400);
  public static Camera2D Camera = new();
  public static Sound? InteractionSetSound = null!;
  
  public static Music? CurrentGameMusic = null!;
  public static int Volume = 10;
}