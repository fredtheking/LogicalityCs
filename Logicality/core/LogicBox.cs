using System.Numerics;
using Logicality.scenes;
using Logicality.utils;
using Logicality.utils.interfaces;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Logicality.core;

public class LogicBox : IScript
{
  public List<LogicBox> Boxes;
  public Dictionary<LogicBox, Vector2> Occipied;
  public Rectangle RealRect;
  public Vector2 GriddedPosition;
  public Rectangle DragRectOffset;
  public Vector2 CenterOffset;
  public Vector2 TextOffset;

  public Receiver? Input1;
  public Receiver? Input2;
  public Receiver? Output;
  
  public bool AllowDrawBorder { get; private set; }
  public bool Destroy { get; private set; }
  public LogicStates State { get; private set; }
  public Hitbox Hitbox { get; private set; }
  public Color Color { get; private set; }
  public bool Selected { get; private set; }
  
  public LogicBox(List<LogicBox> boxes, Dictionary<LogicBox, Vector2> occipied, LogicStates state, Vector2 position)
  {
    RealRect = new Rectangle(position, 120, 100);
    GridPosition();

    if (GridsIntersects(occipied))
    {
      Destroy = true;
      return;
    }
      
    Boxes = boxes;
    Occipied = occipied;
    State = state;
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
    
    CenterOffset = RealRect.Size / 2 - Vector2.UnitY * 14;
    TextOffset = CenterText();
    DragRectOffset = new Rectangle(CenterOffset - new Vector2(50, 20), new Vector2(100, 40));
    Hitbox = new Hitbox(new Rectangle(position + DragRectOffset.Position, DragRectOffset.Size));
    
    if (State is not LogicStates.Battery)
      Input1 = new Receiver(this, new Vector2(30, CenterOffset.Y + 41), false);
    if (State is LogicStates.And or LogicStates.Or or LogicStates.Xor)
    {
      Input1!.Offset.Y -= 7;
      Input1!.Hitbox.Rect.Y -= 7;
      Input2 = new Receiver(this, new Vector2(30, CenterOffset.Y + 48), false);
    }
    if (State is not LogicStates.Receive)
      Output = new Receiver(this, new Vector2(RealRect.Width - 30, CenterOffset.Y + 41), true);

    if (Input1 is not null && State is LogicStates.Wire or LogicStates.Not or LogicStates.Receive)
      Input1.Offset.Y = Output?.Offset.Y ?? CenterOffset.Y + 41;
  }

  public static void Create(LogicBox box)
  {
    if (!box.Destroy)
    {
      box.Init();
      box.Boxes.Add(box);
      box.Occipied.Add(box, box.GriddedPosition);
    }
  }

  private Vector2 CenterText()
  {
    Vector2 size = MeasureTextEx(GetFontDefault(), State.ToString().ToUpper(), 18, 2);
    return CenterOffset - size / 2;
  }
  
  public void Init()
  {
    Hitbox.Init();
    Input1?.Init(); Input2?.Init(); Output?.Init();
    if (State is LogicStates.Battery)
      Output!.State = true;
    GridEtc();
  }

  public void Enter()
  {
    Hitbox.Enter();
    Input1?.Enter(); Input2?.Enter(); Output?.Enter();
  }

  public void Update()
  {
    Occipied[this] = GriddedPosition;
    Hitbox.Update();
    Input1?.Update(); Input2?.Update(); Output?.Update();

    Selected = Hitbox.Drag[(int)MouseButton.Left];
    if (Selected)
    {
      RealRect.Position += GetMouseDelta();

      if (!GridsIntersects(Occipied))
      {
        AllowDrawBorder = false;
        GridPosition();
        GridEtc();
      }
      else
      {
        AllowDrawBorder = true;
      }
    }
    else
    {
      AllowDrawBorder = false;
      RealRect.Position = GriddedPosition;
    }
  }

  public void Render()
  {
    Rectangle griddedRectangle = new Rectangle(GriddedPosition, RealRect.Size);
    DrawRectangleRec(griddedRectangle, Color);
    DrawRectangleLinesEx(griddedRectangle, 2, Color.Black);
    if (AllowDrawBorder) DrawRectangleLinesEx(new Rectangle(RealRect.Position, RealRect.Size), 1, Color.Brown);

    Rectangle selectionRect = new(GriddedPosition + DragRectOffset.Position, DragRectOffset.Size);
    DrawRectangleRec(selectionRect, Selected ? new Color(240, 240, 255,255) : Color.White);
    if (Hitbox.Hover && Selected)
      DrawRectangleLinesEx(selectionRect, 2, Color.DarkBlue);
    else if (Hitbox.Hover && !Selected)
      DrawRectangleLinesEx(selectionRect, 2, Color.Blue);
    else
      DrawRectangleLinesEx(selectionRect, 1, Color.Black);
    
    if (Input1 is not null)
    {
      if (Input1.State) DrawCircleGradient((int)(GriddedPosition.X + Input1.Offset.X), (int)(GriddedPosition.Y + Input1.Offset.Y), 7, Color.DarkGreen, new Color(255, 255, 255, 36));
      DrawCircleV(GriddedPosition + Input1.Offset, 5, Input1.State ? new Color(230, 230, 230, 255) : new Color(0, 0, 0, 100));
      DrawCircleLinesV(GriddedPosition + Input1.Offset, 5, Color.Black);
      DrawLineV(GriddedPosition + Input1.Offset - Vector2.UnitX * 30, GriddedPosition + Input1.Offset - Vector2.UnitX * 5, Color.Black);
    }
    if (Input2 is not null)
    {
      if (Input2.State) DrawCircleGradient((int)(GriddedPosition.X + Input2.Offset.X), (int)(GriddedPosition.Y + Input2.Offset.Y), 7, Color.DarkGreen, new Color(255, 255, 255, 36));
      DrawCircleV(GriddedPosition + Input2.Offset, 5, Input2.State ? new Color(230, 230, 230, 255) : new Color(0, 0, 0, 100));
      DrawCircleLinesV(GriddedPosition + Input2.Offset, 5, Color.Black);
      DrawLineV(GriddedPosition + Input2.Offset - Vector2.UnitX * 30, GriddedPosition + Input2.Offset - Vector2.UnitX * 5, Color.Black);
    }
    if (Output is not null)
    {
      if (Output.State) DrawCircleGradient((int)(GriddedPosition.X + Output.Offset.X), (int)(GriddedPosition.Y + Output.Offset.Y), 7, Color.DarkGreen, new Color(255, 255, 255, 36));
      DrawCircleV(GriddedPosition + Output.Offset, 5, Output.State ? new Color(230, 230, 230, 255) : new Color(0, 0, 0, 100));
      DrawCircleLinesV(GriddedPosition + Output.Offset, 5, Color.Black);
      DrawLineV(GriddedPosition + Output.Offset + Vector2.UnitX * 30, GriddedPosition + Output.Offset + Vector2.UnitX * 5, Color.Black);
    }
    
    DrawTextEx(GetFontDefault(), State.ToString().ToUpper(), GriddedPosition + TextOffset, 18, 2, Color.Black);
    
    Hitbox.Render();
    Input1?.Render(); Input2?.Render(); Output?.Render();
  }

  private void GridEtc()
  {
    Hitbox.Rect.Position = GriddedPosition + DragRectOffset.Position;
    if (Input1 is not null) Input1.Hitbox.Rect.Position = GriddedPosition + Input1.Offset - Vector2.One * 5;
    if (Input2 is not null) Input2.Hitbox.Rect.Position = GriddedPosition + Input2.Offset - Vector2.One * 5;
    if (Output is not null) Output.Hitbox.Rect.Position = GriddedPosition + Output.Offset - Vector2.One * 5;
  }

  private void GridPosition()
  {
    GriddedPosition = new Vector2(
      (int)Math.Round(RealRect.X / 20) * 20,
      (int)Math.Round(RealRect.Y / 20) * 20
    );
  }

  private bool GridsIntersects(Dictionary<LogicBox, Vector2> occ) =>
    occ.Where(p => p.Key != this).Any(p =>
      CheckCollisionRecs(new Rectangle(p.Value, 120, 100), new Rectangle(RealRect.Position, 120, 100)));
}