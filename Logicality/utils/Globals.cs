using System.Numerics;
using Raylib_cs;

namespace Logicality.utils;

public static class Globals
{
  public static bool Debug = true;
  public static Vector2 Resolution = new (1920, 1080);
  public static Vector2 MapSize = new (800);
  public static Camera2D Camera = new();
  public static Sound? VolumePitchSound = null!;
  
  public static Music? CurrentGameMusic = null!;
  public static int Volume = 100;
}