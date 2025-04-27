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
  public bool Inactive = false; 

  public Button(LogicBox parent, Vector2 offset, Vector2 size, bool Inactive = false)
  {
    BgColor = Color.White;
    SelectBgColor = new Color(240, 240, 255,255);
    
    (Parent, Offset) = (parent, offset);
    Rect.Size = size;
    if (!Inactive) Hitbox = new Hitbox();
  }

  public void Init()
  {
    Hitbox?.Init();
    if (Hitbox is not null) Hitbox.Rect = new Rectangle(Parent.SmoothedGriddedPosition + Offset, Rect.Size);
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
    Selected = Hitbox?.Drag[(int)MouseButton.Left] ?? false;
  }

  public void Render() 
  {
    Rectangle selectionRect = new(Parent.SmoothedGriddedPosition + Offset, Rect.Size);
     
    DrawRectangleRec(selectionRect, Selected && !Inactive ? SelectBgColor : BgColor);
    if (Inactive || !Selected) 
      // Idle state
      DrawRectangleLinesEx(selectionRect, 1, Color.Black);
    if (Hitbox is not null)
    {
      switch (Hitbox!.Hover)
      {
        // Selected hitbox
        case true when Selected:
          DrawRectangleLinesEx(selectionRect, 2, Color.DarkBlue);
          break;
        //Hovered hitbox
        case true when !Selected:
          DrawRectangleLinesEx(selectionRect, 2, Color.Blue);
          break;
      }
    }
    
    Hitbox?.Render();
  }
}