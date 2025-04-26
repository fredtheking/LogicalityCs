using System.Numerics;
using Logicality.utils.interfaces;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Logicality.core;

public class Button : IScript
{
  public Vector2 Offset { get; set; }
  public Rectangle Rect;
  public Hitbox? Hitbox { get; set; }
  public LogicBox Parent { get; set; }
  public bool Selected { get; set; }
  public Color BgColor { get; set; }
  public Color SelectBgColor { get; set; }

  public Button(LogicBox parent, Vector2 offset, Vector2 size, bool createHitbox = true)
  {
    BgColor = Color.White;
    SelectBgColor = new Color(240, 240, 255,255);
    
    (Parent, Offset) = (parent, offset);
    Rect.Size = size;
    if (createHitbox) Hitbox = new Hitbox(new Rectangle(Parent.SmoothedGriddedPosition + Rect.Position, Rect.Size));
  }

  public void Init()
  {
    Hitbox?.Init();
  }

  public void Enter() 
  {
    Hitbox?.Enter();
  }

  public void Leave() 
  {
    Hitbox?.Leave();
  }

  public void Update() 
  {
    Hitbox?.Update();
    Selected = Hitbox!.Drag[(int)MouseButton.Left];
  }

  public void Render() 
  {
    Rectangle selectionRect = new(Parent.SmoothedGriddedPosition + Offset, Rect.Size);
     
    DrawRectangleRec(selectionRect, Selected && !Parent.Inactive ? SelectBgColor : BgColor);
    if (!Parent.Inactive && Hitbox!.Hover && Selected)
      DrawRectangleLinesEx(selectionRect, 2, Color.DarkBlue);
    else if (!Parent.Inactive && Hitbox!.Hover && !Selected)
      DrawRectangleLinesEx(selectionRect, 2, Color.Blue);
    else
      DrawRectangleLinesEx(selectionRect, 1, Color.Black);
    
    Hitbox?.Render();
  }
}