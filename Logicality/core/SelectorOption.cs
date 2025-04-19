using Logicality.utils.interfaces;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Logicality.core;

public class SelectorOption : IScript
{
  public Rectangle Rect;
  public string Name;
  public Hitbox Hitbox;
  public bool Selected;

  public SelectorOption(Rectangle rect, string name)
  {
    Rect = rect;
    Name = name;
    Hitbox = new(Rect);
  }

  public void Init()
  {
    
  }

  public void Enter()
  {
    
  }

  public void Update()
  {
    Hitbox.Update();
  }

  public void Render()
  {
    DrawRectangleRec(Rect, Color.RayWhite);
    DrawText(Name, (int)Rect.X + 9, (int)Rect.Y + 5, 20, Color.Black);
      
    Color borderColor = Selected ? Color.Green : Color.Black;
    DrawRectangleLinesEx(Rect, 2, borderColor);
    
    Hitbox.Render();
  }
}