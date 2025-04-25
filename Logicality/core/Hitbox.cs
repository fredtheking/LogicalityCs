using Logicality.utils;
using Logicality.utils.interfaces;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Logicality.core;

public class Hitbox : IScript
{
  public bool OverlayHitbox = false;
  public Rectangle Rect;
  public Color Color = new Color(255, 0, 0, 100);
  public bool Hover { get; set; }
  public readonly bool[] Click = [false, false, false];
  public readonly bool[] Press = [false, false, false];
  public readonly bool[] Down = [false, false, false];
  public readonly bool[] Hold = [false, false, false];
  public readonly bool[] Release = [false, false, false];
  public readonly bool[] Drag = [false, false, false];

  public Hitbox() { }
  public Hitbox(Rectangle rect) => Rect = rect;
  
  public void Init() { }

  public void Enter() { }
  
  public void Leave() { }

  public void Update()
  {
    
    Hover = CheckCollisionPointRec(OverlayHitbox ? GetMousePosition() : GetScreenToWorld2D(GetMousePosition(), Globals.Camera), Rect);
    for (int i = 0; i < 3; i++)
    {
      Click[i] = IsMouseButtonPressed((MouseButton)i);
      Press[i] = Click[i] && Hover;
      Down[i] = IsMouseButtonDown((MouseButton)i);
      Hold[i] = Down[i] && Hover;
      Release[i] = IsMouseButtonReleased((MouseButton)i);
      Drag[i] = CalcDrag(i);
    }
  }

  public void Render()
  {
    if (Globals.Debug)
      DrawRectangleRec(Rect, Color);
  }
  
  
  private bool CalcDrag(int i){
    if (!Drag[i] && Press[i]) return true;
    if (Drag[i] && !Down[i]) return false;
    return Drag[i];
  }
}