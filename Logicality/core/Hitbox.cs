using Logicality.utils;
using Logicality.utils.interfaces;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Logicality.core;

public class Hitbox : IScript
{
  public Rectangle Rect { get; set; }
  public bool Hover { get; set; }
  public readonly bool[] Click = [false, false, false];
  public readonly bool[] Press = [false, false, false];
  public readonly bool[] Down = [false, false, false];
  public readonly bool[] Hold = [false, false, false];
  public readonly bool[] Drag = [false, false, false];
  
  public void Init()
  {
    
  }

  public void Enter()
  {
    
  }

  public void Update()
  {
    Hover = CheckCollisionPointRec(GetMousePosition(), Rect);
    for (int i = 0; i < 3; i++)
    {
      Click[i] = IsMouseButtonPressed((MouseButton)i);
      Press[i] = Click[i] && Hover;
      Down[i] = IsMouseButtonDown((MouseButton)i);
      Hold[i] = Down[i] && Hover;
      Drag[i] = CalcDrag(i);
    }
  }

  public void Render()
  {
    if (Config.Debug)
      DrawRectangleRec(Rect, new Color(255, 0, 0, 100));
  }
  
  
  private bool CalcDrag(int i){
    if (!Drag[i] && Press[i]) return true;
    if (Drag[i] && !Down[i]) return false;
    return Drag[i];
  }
}