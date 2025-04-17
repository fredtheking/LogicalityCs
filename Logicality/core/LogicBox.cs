using System.Numerics;
using Logicality.utils.interfaces;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Logicality.core;

public class LogicBox : IScript
{
  public Rectangle Rect;
  public Rectangle DragRectOffset;
  public Vector2 CenterOffset;
  public Vector2 TextOffset;

  public bool InputOneBool;
  public Vector2 InputOneOffset;
  
  public bool InputTwoBool;
  public Vector2 InputTwoOffset;
  
  public bool OutputBool;
  public Vector2 OutputOffset;
  
  public LogicStates State { get; private set; }
  public Hitbox Hitbox { get; private set; }
  public Color Color { get; private set; }
  public bool Selected { get; private set; }
  
  public LogicBox(LogicStates state, Vector2 position)
  {
    State = state;
    Rect = new Rectangle(position.X, position.Y, 120, 100);
    Color = state switch
    {
      LogicStates.Wire => Color.LightGray,
      LogicStates.Not => Color.Red,
      LogicStates.And => Color.Lime,
      LogicStates.Or => Color.Yellow,
      LogicStates.Xor => Color.Orange,
      _ => Color.Blank
    };
    Hitbox = new();
    
    CenterOffset = Rect.Size / 2 - Vector2.UnitY * 14;
    TextOffset = CenterText();
    DragRectOffset = new Rectangle(CenterOffset - new Vector2(50, 20), new Vector2(100, 40));
    
    InputOneOffset = new Vector2(30, CenterOffset.Y + 34);
    InputTwoOffset = new Vector2(30, CenterOffset.Y + 48);
    
    OutputOffset = new Vector2(Rect.Width - 30, CenterOffset.Y + 41);
    if (State is LogicStates.Wire or LogicStates.Not)
      InputOneOffset.Y = OutputOffset.Y;
  }

  private Vector2 CenterText()
  {
    Vector2 size = MeasureTextEx(GetFontDefault(), State.ToString().ToUpper(), 20, 2);
    return CenterOffset - size / 2;
  }
  
  public void Init()
  {
    Hitbox.Init();
  }

  public void Enter()
  {
    Hitbox.Enter();
  }

  public void Update()
  {
    Hitbox.Update();

    Selected = Hitbox.Drag[(int)MouseButton.Left];
    if (Selected) 
      Rect.Position += GetMouseDelta();
    
    Hitbox.Rect = new(Rect.Position + DragRectOffset.Position, DragRectOffset.Size);
  }

  public void Render()
  {
    DrawRectangleRec(Rect, Color);
    DrawRectangleLinesEx(Rect, 2, Color.Black);

    Rectangle selectionRect = new(Rect.Position + DragRectOffset.Position, DragRectOffset.Size);
    DrawRectangleRec(selectionRect, Selected ? new Color(240, 240, 255,255) : Color.White);
    if (Hitbox.Hover && Selected)
      DrawRectangleLinesEx(selectionRect, 2, Color.DarkBlue);
    else if (Hitbox.Hover && !Selected)
      DrawRectangleLinesEx(selectionRect, 2, Color.Blue);
    else
      DrawRectangleLinesEx(selectionRect, 1, Color.Black);
    
    DrawCircleV(Rect.Position + InputOneOffset, 5, InputOneBool ? Color.White : new Color(0, 0, 0, 100));
    DrawCircleLinesV(Rect.Position + InputOneOffset, 5, Color.Black);
    DrawLineV(Rect.Position + InputOneOffset - Vector2.UnitX * 30, Rect.Position + InputOneOffset - Vector2.UnitX * 5, Color.Black);
    if (State is LogicStates.And or LogicStates.Or or LogicStates.Xor)
    {
      DrawCircleV(Rect.Position + InputTwoOffset, 5, InputTwoBool ? Color.White : new Color(0, 0, 0, 100));
      DrawCircleLinesV(Rect.Position + InputTwoOffset, 5, Color.Black);
      DrawLineV(Rect.Position + InputTwoOffset - Vector2.UnitX * 30, Rect.Position + InputTwoOffset - Vector2.UnitX * 5, Color.Black);
    }
    DrawCircleV(Rect.Position + OutputOffset, 5, OutputBool ? Color.White : new Color(0, 0, 0, 100));
    DrawCircleLinesV(Rect.Position + OutputOffset, 5, Color.Black);
    DrawLineV(Rect.Position + OutputOffset + Vector2.UnitX * 30, Rect.Position + OutputOffset + Vector2.UnitX * 5, Color.Black);
    
    DrawTextEx(GetFontDefault(), State.ToString().ToUpper(), Rect.Position + TextOffset, 20, 2, Color.Black);
    
    Hitbox.Render();
  }
}