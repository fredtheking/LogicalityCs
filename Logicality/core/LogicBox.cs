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
  public Vector2 CenterOffset;
  public Vector2 TextOffset;

  public static event Action<LogicBox>? AdditionalInit;
  public Button DragButton { get; init; }
  public Button? SwitchButton { get; init; }
  public bool SwitchState { get; private set; } = true;
  
  public Receiver?[] Receivers = new Receiver[3];

  public bool AllowDrawBorder { get; private set; }
  public bool Destroy { get; private set; }
  public bool Inactive { get; init; }
  public LogicStates State { get; private set; }
  public Hitbox? DeletionHitbox { get; private set; }
  public Color Color { get; init; }
  public int StartedDragging { get; set; }
  
  public LogicBox(LogicStates state, Vector2 position, bool defaultInactive = false)
  {
    RealRect = new Rectangle(position, 120, 100);
    GridPosition();

    if (GridsIntersects(Occipied) || WireLine.Wires.Any(x => x.Hitbox.Hover))
    {
      Destroy = true;
      return;
    }
    
    State = state;
    Inactive = defaultInactive;
    Color = state switch
    {
      LogicStates.Switch => Color.LightGray,
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
    DragButton = new Button(this, CenterOffset - new Vector2(50, 20), new Vector2(100, 40), Inactive);
    if (!Inactive)
    {
      DeletionHitbox = new Hitbox(new Rectangle(GriddedPosition, RealRect.Size)) 
        { Color = new Color(0, 0, 255, 100) };
    }
    if (State is LogicStates.Switch)
    {
      SwitchButton = new Button(this, new Vector2(44, CenterOffset.Y + 28), new Vector2(34, 28));
      DetermineSwitchColor();
    }

    if (State is not LogicStates.Battery)
    {
      Vector2 offset = new(30, CenterOffset.Y + 41);
      Receivers[0] = new Receiver(this, offset, offset - Vector2.UnitX * 30, false);
    }
    if (State is LogicStates.And or LogicStates.Or or LogicStates.Xor)
    {
      Receivers[0]!.Offset.Y -= 7;
      Receivers[0]!.StartWireOffset.Y -= 7;
      Receivers[0]!.Hitbox.Rect.Y -= 7;
      Vector2 offset = new(30, CenterOffset.Y + 48);
      Receivers[1] = new Receiver(this, offset, offset - Vector2.UnitX * 30, false);
    }
    if (State is not LogicStates.Receive)
    {
      Vector2 offset = new(RealRect.Width - 30, CenterOffset.Y + 41);
      Receivers[2] = new Receiver(this, offset, offset + Vector2.UnitX * 30, true);
    }

    if (Receivers[0] is not null && State is LogicStates.Switch or LogicStates.Not or LogicStates.Receive)
      Receivers[0]!.Offset.Y = Receivers[2]?.Offset.Y ?? CenterOffset.Y + 41;
  }

  public static bool Create(LogicBox box)
  {
    if (box.Destroy) return false;
    box.Init();
    Boxes.Add(box);
    Occipied.Add(box, box.GriddedPosition);
    return true;
  }

  private Vector2 CenterText()
  {
    Vector2 size = MeasureTextEx(GetFontDefault(), State.ToString().ToUpper(), 18, 2);
    return CenterOffset - size / 2;
  }
  
  public void Init()
  {
    SmoothedGriddedPosition = GriddedPosition - GetMouseDelta();
    DragButton.Init(); SwitchButton?.Init(); DeletionHitbox?.Init();
    foreach (Receiver receiver in Receivers)
      receiver?.Init();
    if (State is LogicStates.Battery)
      Receivers[2]!.State = true;
    GridEtc();
    AdditionalInit?.Invoke(this);
  }

  public void Enter()
  {
    DragButton.Enter(); SwitchButton?.Enter(); DeletionHitbox?.Enter();
    foreach (Receiver receiver in Receivers)
      receiver?.Enter();
  }
  
  public void Leave()
  {
    DragButton.Leave(); SwitchButton?.Leave(); DeletionHitbox?.Leave();
    foreach (Receiver receiver in Receivers)
      receiver?.Leave();

    Destroy = true;
    DestroyCheck();
  }
  
  private bool DestroyCheck()
  {
    if (!Destroy) return false;
    Occipied.Remove(this);
    Boxes.Remove(this);
    foreach (Receiver receiver in Receivers.Where(x => x is not null))
      receiver!.Parent = null;
    return true;
  }

  public void Update()
  {
    SmoothedGriddedPosition = Vector2.Lerp(SmoothedGriddedPosition, GriddedPosition, 20f * GetFrameTime());
    if (DestroyCheck()) return;
    
    Occipied[this] = GriddedPosition;
    DragButton.Update(); 
    SwitchButton?.Update(); 
    DeletionHitbox?.Update();
    foreach (Receiver receiver in Receivers)
      receiver?.Update();

    if (Receivers[2] is not null)
      Receivers[2]!.State = State switch
      {
        LogicStates.Switch when SwitchState => Receivers[0]!.State,
        LogicStates.Not => !Receivers[0]!.State,
        LogicStates.And => Receivers[0]!.State && Receivers[1]!.State,
        LogicStates.Or => Receivers[0]!.State || Receivers[1]!.State,
        LogicStates.Xor => Receivers[0]!.State ^ Receivers[1]!.State,
        LogicStates.Battery => true,
        _ => false
      };

    if (SwitchButton is not null)
    {
      if (SwitchButton.Hitbox!.Press[(int)MouseButton.Left])
      {
        PlaySound(Globals.InteractionSetSound!.Value);
        SwitchState = !SwitchState;
        DetermineSwitchColor();
      }
    }

    if (Inactive) return;
    switch (DragButton.Selected)
    {
      // Deletion
      case false when DeletionHitbox!.Press[(int)MouseButton.Right]:
        Destroy = true;
        return;
      // Selection
      case true:
      {
        RealRect.Position += GetMouseDelta() / Globals.Camera.Zoom;

        foreach (Receiver receiver in Receivers)
        {
          if (receiver?.Wire is not null)
            receiver.Wire!.UpdateBezierEaseCubicInOutPoints(6);
        }

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

        break;
      }
      default:
        AllowDrawBorder = false;
        RealRect.Position = GriddedPosition;
        break;
    }

    // Wires
    if (StartedDragging > 0 || DragButton.Selected)
    {
      Boxes.Remove(this);
      Boxes.Add(this);
    }

    StartedDragging = 0;
  }

  public void Render()
  {
    Rectangle griddedRectangle = new Rectangle(SmoothedGriddedPosition, RealRect.Size);

    if (!CheckCollisionRecs(new Rectangle(GetScreenToWorld2D(Vector2.Zero, Globals.Camera), Globals.Resolution / Globals.Camera.Zoom), griddedRectangle))
      return;
    
    DrawRectangleRec(griddedRectangle, Color);
    if (Inactive) DrawRectangleLinesEx(new Rectangle(griddedRectangle.Position - Vector2.One * 1, griddedRectangle.Size + Vector2.One * 2), 2, Color.Yellow);
    DrawRectangleLinesEx(griddedRectangle, 1, Color.Black);
    
    DeletionHitbox?.Render(); DragButton.Render(); SwitchButton?.Render();
    
    DrawTextEx(GetFontDefault(), State.ToString().ToUpper(), SmoothedGriddedPosition + TextOffset, 18, 1, Color.Black);
    
    foreach (Receiver receiver in Receivers)
      receiver?.Render();
    foreach (Receiver receiver in Receivers)
      receiver?.RollbackWires();
    
    if (AllowDrawBorder)
    {
      DrawRectangleLinesEx(RealRect, 2, Color.DarkGray);
      DrawRectangleLinesEx(RealRect, 1, Color);
    }
  }

  private void GridEtc()
  {
    if (!Inactive)
    {
      DeletionHitbox!.Rect.Position = GriddedPosition;
      DragButton.Hitbox!.Rect.Position = GriddedPosition + DragButton.Offset;
      if (SwitchButton is not null)
        SwitchButton.Hitbox!.Rect.Position = GriddedPosition + SwitchButton.Offset;
    }
    foreach (Receiver receiver in Receivers)
      if (receiver is not null)
        receiver.Hitbox.Rect.Position = GriddedPosition + receiver.Offset - Vector2.One * 5;
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

  private void DetermineSwitchColor()
  {
    switch (SwitchState)
    {
      case true:
        SwitchButton!.BgColor = Color.Green;
        SwitchButton.SelectBgColor = new Color(0, 174, 22, 255);
        break;
      case false:
        SwitchButton!.BgColor = new Color(190, 0, 0, 255);
        SwitchButton.SelectBgColor = new Color(160, 0, 0, 255);
        break;
    }
  }
}