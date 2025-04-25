using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;
namespace Logicality.utils;

public static class VolumeUtils
{
  private static Vector2 Position = new(Globals.Resolution.X / 2 - 200, -10);
  private static Vector2 SmoothedPosition;
  private static double ShowBarCountdown;
  
  public static void Update()
  {
    void Decrement(bool viceVersa)
    {
      Globals.Volume += viceVersa ? 10 : -10;
      ShowBarCountdown = 4;
    }
    
    SetMasterVolume(Globals.Volume / 100f);

    if (IsKeyPressed(KeyboardKey.Minus)) Decrement(false);
    if (IsKeyPressed(KeyboardKey.Equal)) Decrement(true);
    
    if (ShowBarCountdown > 0) ShowBarCountdown -= GetFrameTime();
    
    Globals.Volume = Math.Clamp(Globals.Volume, 0, 100);
    ShowBarCountdown = Math.Clamp(ShowBarCountdown, 0, 4);
    Position.Y = ShowBarCountdown > 0 ? 20 : -10;
    
    SmoothedPosition = Vector2.Lerp(SmoothedPosition, Position, 16f * GetFrameTime());
  }

  public static void Render()
  {
    float width = 400f * (Globals.Volume / 100f);
    DrawLineEx(SmoothedPosition, SmoothedPosition with { X = SmoothedPosition.X + 400 }, 6, new Color(123, 0, 0, 255));
    DrawLineEx(SmoothedPosition, SmoothedPosition with { X = SmoothedPosition.X + width }, 6, Color.Lime);
  }
}