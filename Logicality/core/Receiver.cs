using System.Numerics;
using Logicality.utils;
using Logicality.utils.interfaces;
using Raylib_cs;
using Steamworks;
using static Raylib_cs.Raylib;

namespace Logicality.core;

public class Receiver : IScript
{
  public bool State { get; set; }
  public bool StartedDragging { get; set; }
  public bool IsOutput { get; set; }
  public Vector2 Offset;
  public Vector2 StartWireOffset;
  public Receiver? Connector { get; set; }
  public LogicBox? Parent { get; set; }
  public WireLine? Wire { get; set; }
  public Hitbox Hitbox { get; set; }
  public event Action? ConnectionEstablishedTrigger;
  
  public Receiver(LogicBox parent, Vector2 offset, Vector2 startWireOffset, bool isOutput) 
  {
    (Parent, Offset, StartWireOffset, IsOutput) = (parent, offset, startWireOffset, isOutput);
    Hitbox = new(new Rectangle(parent.RealRect.Position + offset - Vector2.One * 5, 10, 10));
  }
  
  public void Init()
  {
    Hitbox.Init();
  }

  public void Enter()
  {
    Hitbox.Enter();
  }
  
  public void Leave()
  {
    Hitbox.Leave();
  }

  public void Update()
  {
    Hitbox.Update();
    Wire?.Update();
    
    bool dragging = Hitbox.Drag[(int)MouseButton.Left];
    Parent!.StartedDragging += dragging ? 1 : 0;
    
    if (Hitbox.Press[(int)MouseButton.Left])
      PlayInteractionSound();

    if (Hitbox.Release[(int)MouseButton.Left] && StartedDragging)
    {
      if (Hitbox.Hover)
        PlayInteractionSound();
      List<Receiver> OtherReceivers = LogicBox.Boxes.Where(x => x != Parent).SelectMany(x => x.Receivers).Where(x => x is not null).Cast<Receiver>().ToList();
      
      foreach (Receiver receiver in OtherReceivers)
      {
        if (receiver.IsOutput != IsOutput && Wire is null && Connector is null && receiver.Wire is null && receiver.Connector is null && CheckCollisionPointRec(GetScreenToWorld2D(GetMousePosition(), Globals.Camera), receiver.Hitbox.Rect))
        {
          Console.WriteLine($"FOUND - Parent: {Parent.GriddedPosition}, Receiver: {receiver.StartWireOffset}");

          Connector = receiver;
          receiver.Connector = this;
          Wire = receiver.Wire = new WireLine(IsOutput ? this : receiver, IsOutput ? receiver : this);
          ConnectionEstablishedTrigger?.Invoke();
          
          break;
        }
      }
    }
    
    StartedDragging = dragging;
  }

  public void Render()
  {
    if (State) DrawCircleV(Parent!.SmoothedGriddedPosition + Offset, 7, new Color(255, 255, 255, 36));
    
    Vector2 ConnectionSpace = Connector is not null ? Vector2.UnitX : Vector2.Zero;
    Color HalfBlack = new Color(0, 0, 0, 100);
    if (IsOutput)
    {
      DrawLineEx(Parent!.SmoothedGriddedPosition + Offset + Vector2.UnitX * 29 + ConnectionSpace, Parent.SmoothedGriddedPosition + Offset + Vector2.UnitX * 5, 4, Color.Black);
      DrawLineEx(Parent.SmoothedGriddedPosition + Offset + Vector2.UnitX * 29 + ConnectionSpace, Parent.SmoothedGriddedPosition + Offset + Vector2.UnitX * 5, 2, State ? Color.RayWhite : Parent.Color);
      if (!State)
        DrawRectangleGradientEx(new Rectangle(Parent!.SmoothedGriddedPosition + Offset + Vector2.UnitX * 5 - Vector2.UnitY + ConnectionSpace, 24, 2), HalfBlack, HalfBlack, Color.DarkGray, Color.DarkGray);
    }
    else
    {
      DrawLineEx(Parent!.SmoothedGriddedPosition + Offset - Vector2.UnitX * 29 - ConnectionSpace, Parent.SmoothedGriddedPosition + Offset - Vector2.UnitX * 5, 4, Color.Black);
      DrawLineEx(Parent.SmoothedGriddedPosition + Offset - Vector2.UnitX * 29 - ConnectionSpace, Parent.SmoothedGriddedPosition + Offset - Vector2.UnitX * 5, 2, State ? Color.RayWhite : Parent.Color);
      if (!State)
        DrawRectangleGradientEx(new Rectangle(Parent!.SmoothedGriddedPosition + Offset - Vector2.UnitX * 29 - Vector2.UnitY - ConnectionSpace, 24 + ConnectionSpace.X, 2), Color.DarkGray, Color.DarkGray, HalfBlack, HalfBlack);
    }
    
    if (StartedDragging && Wire is null)
    {
      Console.WriteLine(Random.Shared.Next());
      DrawLineBezier(Parent.SmoothedGriddedPosition + StartWireOffset, GetScreenToWorld2D(GetMousePosition(), Globals.Camera), 4, new Color(0, 0, 0, 100));
      DrawLineBezier(Parent.SmoothedGriddedPosition + StartWireOffset, GetScreenToWorld2D(GetMousePosition(), Globals.Camera), 2, new Color(245, 245, 245, 100));
    }
    
    DrawCircleV(Parent.SmoothedGriddedPosition + Offset, 5, State ? new Color(230, 230, 230, 255) : new Color(0, 0, 0, 100));
    DrawRectangleRoundedLinesEx(new Rectangle(Parent.SmoothedGriddedPosition + Offset - Vector2.One * 5, 10, 10), 1, 10, 1.01f, Color.Black);
    
    if (Parent is not null && Connector?.Parent is not null && !BoxesTooClose())
      Wire!.Render();
    
    Hitbox.Render();
  }

  public void RollbackWiresRender()
  {
    if (Wire is not null)
      Wire.Drawed = false;
  }

  private void PlayInteractionSound()
  {
    PlaySound(Globals.InteractionSetSound!.Value);
  }

  private bool BoxesTooClose()
  {
    Vector2 thisPos = Parent!.SmoothedGriddedPosition;
    Vector2 connectorPos = Connector!.Parent!.SmoothedGriddedPosition;

    return thisPos.X - connectorPos.X is >= -121 and <= 121 && Math.Abs(thisPos.Y - connectorPos.Y) < 1;
  }
}