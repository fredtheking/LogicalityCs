using System.Numerics;
using Logicality.scenes;
using Logicality.utils.interfaces;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Logicality.core;

public class LogicBox : IScript
{
  public List<LogicBox> Boxes;
  public Rectangle Rect;
  public Rectangle DragRectOffset;
  public Vector2 CenterOffset;
  public Vector2 TextOffset;

  public Receiver? Input1;
  public Receiver? Input2;
  public Receiver? Output;
  
  public LogicStates State { get; private set; }
  public Hitbox Hitbox { get; private set; }
  public Color Color { get; private set; }
  public bool Selected { get; private set; }
  
  public LogicBox(List<LogicBox> boxes, LogicStates state, Vector2 position)
  {
    Boxes = boxes;
    State = state;
    Rect = new Rectangle(position.X, position.Y, 120, 100);
    Color = state switch
    {
      LogicStates.Wire => Color.LightGray,
      LogicStates.Not => Color.Red,
      LogicStates.And => Color.Lime,
      LogicStates.Or => Color.Yellow,
      LogicStates.Xor => Color.Orange,
      LogicStates.Battery => Color.DarkPurple,
      LogicStates.Receive => Color.DarkBlue,
      _ => Color.Blank
    };
    Hitbox = new();
    
    CenterOffset = Rect.Size / 2 - Vector2.UnitY * 14;
    TextOffset = CenterText();
    DragRectOffset = new Rectangle(CenterOffset - new Vector2(50, 20), new Vector2(100, 40));
    
    if (State is not LogicStates.Battery)
      Input1 = new Receiver(this, new Vector2(30, CenterOffset.Y + 34), false);
    if (State is LogicStates.And or LogicStates.Or or LogicStates.Xor)
      Input2 = new Receiver(this, new Vector2(30, CenterOffset.Y + 48), false);
    if (State is not LogicStates.Receive)
      Output = new Receiver(this, new Vector2(Rect.Width - 30, CenterOffset.Y + 41), true);

    if (Input1 is not null && State is LogicStates.Wire or LogicStates.Not or LogicStates.Receive)
      Input1.Offset.Y = Output?.Offset.Y ?? CenterOffset.Y + 41;
  }

  private Vector2 CenterText()
  {
    Vector2 size = MeasureTextEx(GetFontDefault(), State.ToString().ToUpper(), 18, 2);
    return CenterOffset - size / 2;
  }
  
  public void Init()
  {
    Hitbox.Init();
    if (State is LogicStates.Battery)
      Output!.State = true;
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
    {
      Rect.Position += GetMouseDelta();
      Boxes.Remove(this);
      Boxes.Add(this);
    }
    
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
    
    if (Input1 is not null)
    {
      DrawCircleV(Rect.Position + Input1.Offset, 5, Input1.State ? new Color(230, 230, 230, 255) : new Color(0, 0, 0, 100));
      DrawCircleLinesV(Rect.Position + Input1.Offset, 5, Color.Black);
      DrawLineV(Rect.Position + Input1.Offset - Vector2.UnitX * 30, Rect.Position + Input1.Offset - Vector2.UnitX * 5, Color.Black);
    }
    if (Input2 is not null)
    {
      DrawCircleV(Rect.Position + Input2.Offset, 5, Input2.State ? new Color(230, 230, 230, 255) : new Color(0, 0, 0, 100));
      DrawCircleLinesV(Rect.Position + Input2.Offset, 5, Color.Black);
      DrawLineV(Rect.Position + Input2.Offset - Vector2.UnitX * 30, Rect.Position + Input2.Offset - Vector2.UnitX * 5, Color.Black);
    }
    if (Output is not null)
    {
      DrawCircleV(Rect.Position + Output.Offset, 5, Output.State ? new Color(230, 230, 230, 255) : new Color(0, 0, 0, 100));
      DrawCircleLinesV(Rect.Position + Output.Offset, 5, Color.Black);
      DrawLineV(Rect.Position + Output.Offset + Vector2.UnitX * 30, Rect.Position + Output.Offset + Vector2.UnitX * 5, Color.Black);
    }
    
    DrawTextEx(GetFontDefault(), State.ToString().ToUpper(), Rect.Position + TextOffset, 18, 2, Color.Black);
    
    Hitbox.Render();
  }
}