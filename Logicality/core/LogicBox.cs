using System.Numerics;
using Logicality.scenes;
using Logicality.utils;
using Logicality.utils.interfaces;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Logicality.core;

public class LogicBox : IScript
{
  public static List<LogicBox> Boxes = [];
  public static Dictionary<LogicBox, Vector2> Occipied = [];
  public Rectangle RealRect;
  public Vector2 GriddedPosition;
  public Vector2 SmoothedGriddedPosition;
  public Rectangle DragRectOffset;
  public Vector2 CenterOffset;
  public Vector2 TextOffset;

  public Receiver? Input1;
  public Receiver? Input2;
  public Receiver? Output;
  
  public bool AllowDrawBorder { get; private set; }
  public bool Destroy { get; private set; }
  public bool Inactive { get; init; }
  public LogicStates State { get; private set; }
  public Hitbox? Hitbox { get; private set; }
  public Hitbox? DeletionHitbox { get; private set; }
  public Color Color { get; init; }
  public bool Selected { get; private set; }
  public int StartedDragging { get; set; }
  
  public LogicBox(LogicStates state, Vector2 position, bool defaultInactive = false)
  {
    RealRect = new Rectangle(position, 120, 100);
    GridPosition();

    if (GridsIntersects(Occipied))
    {
      Destroy = true;
      return;
    }
    
    State = state;
    Inactive = defaultInactive;
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
    if (!Inactive)
    {
      Hitbox = new Hitbox(new Rectangle(position + DragRectOffset.Position, DragRectOffset.Size));
      DeletionHitbox = new Hitbox(new Rectangle(GriddedPosition, RealRect.Size));
      DeletionHitbox.Color = new Color(0, 0, 255, 100);
    }

    if (State is not LogicStates.Battery)
    {
      Vector2 offset = new(30, CenterOffset.Y + 41);
      Input1 = new Receiver(this, offset, offset - Vector2.UnitX * 30, false);
    }
    if (State is LogicStates.And or LogicStates.Or or LogicStates.Xor)
    {
      Input1!.Offset.Y -= 7;
      Input1!.StartWireOffset.Y -= 7;
      Input1!.Hitbox.Rect.Y -= 7;
      Vector2 offset = new(30, CenterOffset.Y + 48);
      Input2 = new Receiver(this, offset, offset - Vector2.UnitX * 30, false);
    }
    if (State is not LogicStates.Receive)
    {
      Vector2 offset = new(RealRect.Width - 30, CenterOffset.Y + 41);
      Output = new Receiver(this, offset, offset + Vector2.UnitX * 30, true);
    }

    if (Input1 is not null && State is LogicStates.Wire or LogicStates.Not or LogicStates.Receive)
      Input1.Offset.Y = Output?.Offset.Y ?? CenterOffset.Y + 41;
  }

  public static bool Create(LogicBox box)
  {
    if (!box.Destroy)
    {
      box.Init();
      if (box.Input1 != null) Receiver.Receivers.Add(box.Input1);
      if (box.Input2 != null) Receiver.Receivers.Add(box.Input2);
      if (box.Output != null) Receiver.Receivers.Add(box.Output);
      Boxes.Add(box);
      Occipied.Add(box, box.GriddedPosition);
      return true;
    }
    return false;
  }

  private Vector2 CenterText()
  {
    Vector2 size = MeasureTextEx(GetFontDefault(), State.ToString().ToUpper(), 18, 2);
    return CenterOffset - size / 2;
  }
  
  public void Init()
  {
    SmoothedGriddedPosition = GriddedPosition - GetMouseDelta();
    Hitbox?.Init(); DeletionHitbox?.Init();
    Input1?.Init(); Input2?.Init(); Output?.Init();
    if (State is LogicStates.Battery)
      Output!.State = true;
    GridEtc();
  }

  public void Enter()
  {
    Hitbox?.Enter(); DeletionHitbox?.Enter();
    Input1?.Enter(); Input2?.Enter(); Output?.Enter();
  }
  
  public void Leave()
  {
    Hitbox?.Leave(); DeletionHitbox?.Leave();
    Input1?.Leave(); Input2?.Leave(); Output?.Leave();

    Destroy = true;
    DestroyCheck();
  }
  
  private bool DestroyCheck()
  {
    if (!Destroy) return false;
    if (Input1 != null) Receiver.Receivers.Remove(Input1);
    if (Input2 != null) Receiver.Receivers.Remove(Input2);
    if (Output != null) Receiver.Receivers.Remove(Output);
    Occipied.Remove(this);
    Boxes.Remove(this);
    return true;
  }

  public void Update()
  {
    SmoothedGriddedPosition = Vector2.Lerp(SmoothedGriddedPosition, GriddedPosition, 20f * GetFrameTime());
    if (DestroyCheck()) return;
    
    Occipied[this] = GriddedPosition;
    Hitbox?.Update(); DeletionHitbox?.Update();
    Input1?.Update(); Input2?.Update(); Output?.Update();
    
    if (!Inactive)
    {
      // Deletion
      if (DeletionHitbox!.Press[(int)MouseButton.Right])
      {
        Destroy = true;
        return;
      }
      
      // Selection
      Selected = Hitbox!.Drag[(int)MouseButton.Left];
      if (Selected)
      {
        RealRect.Position += GetMouseDelta() / Globals.Camera.Zoom;

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
      
      // Wires
      if (StartedDragging > 0 || Selected)
      {
        Boxes.Remove(this);
        Boxes.Add(this);
      }

      StartedDragging = 0;
    }
  }

  public void Render()
  {
    Rectangle griddedRectangle = new Rectangle(SmoothedGriddedPosition, RealRect.Size);
    DrawRectangleRec(griddedRectangle, Color);
    if (Inactive) DrawRectangleLinesEx(new Rectangle(griddedRectangle.Position - Vector2.One * 1, griddedRectangle.Size + Vector2.One * 2), 2, Color.Yellow);
    DrawRectangleLinesEx(griddedRectangle, 1, Color.Black);

    Rectangle selectionRect = new(SmoothedGriddedPosition + DragRectOffset.Position, DragRectOffset.Size);
    DrawRectangleRec(selectionRect, Selected && !Inactive ? new Color(240, 240, 255,255) : Color.White);
    if (!Inactive && Hitbox!.Hover && Selected)
      DrawRectangleLinesEx(selectionRect, 2, Color.DarkBlue);
    else if (!Inactive && Hitbox!.Hover && !Selected)
      DrawRectangleLinesEx(selectionRect, 2, Color.Blue);
    else
      DrawRectangleLinesEx(selectionRect, 1, Color.Black);
    
    if (Input1 is not null)
    {
      if (Input1.State) DrawCircleGradient((int)(SmoothedGriddedPosition.X + Input1.Offset.X), (int)(SmoothedGriddedPosition.Y + Input1.Offset.Y), 7, Color.DarkGreen, new Color(255, 255, 255, 36));
      DrawCircleV(SmoothedGriddedPosition + Input1.Offset, 5, Input1.State ? new Color(230, 230, 230, 255) : new Color(0, 0, 0, 100));
      DrawRectangleRoundedLinesEx(new Rectangle(SmoothedGriddedPosition + Input1.Offset - Vector2.One * 5, 10, 10), 1, 10, 1.01f, Color.Black);
      DrawLineEx(SmoothedGriddedPosition + Input1.Offset - Vector2.UnitX * 30, SmoothedGriddedPosition + Input1.Offset - Vector2.UnitX * 5, 1.01f, Color.Black);
    }
    if (Input2 is not null)
    {
      if (Input2.State) DrawCircleGradient((int)(SmoothedGriddedPosition.X + Input2.Offset.X), (int)(SmoothedGriddedPosition.Y + Input2.Offset.Y), 7, Color.DarkGreen, new Color(255, 255, 255, 36));
      DrawCircleV(SmoothedGriddedPosition + Input2.Offset, 5, Input2.State ? new Color(230, 230, 230, 255) : new Color(0, 0, 0, 100));
      DrawRectangleRoundedLinesEx(new Rectangle(SmoothedGriddedPosition + Input2.Offset - Vector2.One * 5, 10, 10), 1, 10, 1.01f, Color.Black);
      DrawLineEx(SmoothedGriddedPosition + Input2.Offset - Vector2.UnitX * 30, SmoothedGriddedPosition + Input2.Offset - Vector2.UnitX * 5, 1.01f, Color.Black);
    }
    if (Output is not null)
    {
      if (Output.State) DrawCircleGradient((int)(SmoothedGriddedPosition.X + Output.Offset.X), (int)(SmoothedGriddedPosition.Y + Output.Offset.Y), 7, Color.DarkGreen, new Color(255, 255, 255, 36));
      DrawCircleV(SmoothedGriddedPosition + Output.Offset, 5, Output.State ? new Color(230, 230, 230, 255) : new Color(0, 0, 0, 100));
      DrawRectangleRoundedLinesEx(new Rectangle(SmoothedGriddedPosition + Output.Offset - Vector2.One * 5, 10, 10), 1, 10, 1.01f, Color.Black);
      DrawLineEx(SmoothedGriddedPosition + Output.Offset + Vector2.UnitX * 30, SmoothedGriddedPosition + Output.Offset + Vector2.UnitX * 5, 1.01f, Color.Black);
    }
    
    DrawTextEx(GetFontDefault(), State.ToString().ToUpper(), SmoothedGriddedPosition + TextOffset, 18, 1, Color.Black);
    
    if (AllowDrawBorder)
    {
      DrawRectangleLinesEx(RealRect, 2, Color.DarkGray);
      DrawRectangleLinesEx(RealRect, 1, Color);
    }
    
    DeletionHitbox?.Render(); Hitbox?.Render();
    Input1?.Render(); Input2?.Render(); Output?.Render();
  }

  private void GridEtc()
  {
    if (!Inactive)
    {
      DeletionHitbox!.Rect.Position = GriddedPosition;
      Hitbox!.Rect.Position = GriddedPosition + DragRectOffset.Position;
    }
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
  
  private bool GridsIntersects(Dictionary<LogicBox, Vector2> occ)
  {
    Rectangle smallerRect = new(RealRect.Position + Vector2.One * 10, RealRect.Size - Vector2.One * 20);
    
    // Checks whether it overlaps with any other box
    bool first = occ.Where(p => p.Key != this).Any(p =>
      CheckCollisionRecs(new Rectangle(p.Value, 120, 100), smallerRect));
    // Checks whether the box is NOT in scene boundaries (from RectSize to MapSize-RectSize*2)
    bool second = !CheckCollisionRecs(smallerRect, new Rectangle(100, 80, Globals.MapSize - new Vector2(100, 80) * 2));
    
    // Both of them are "invalid position", so either of them should be true
    return first || second;
  }
}