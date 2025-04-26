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
          Wire = receiver.Wire = new WireLine(IsOutput ? receiver : this, IsOutput ? this : receiver);
          
          break;
        }
      }
    }
    
    StartedDragging = dragging;
  }

  public void Render()
  {
    if (State) DrawCircleGradient((int)(Parent.SmoothedGriddedPosition.X + Offset.X), (int)(Parent.SmoothedGriddedPosition.Y + Offset.Y), 7, Color.DarkGreen, new Color(255, 255, 255, 36));
    DrawCircleV(Parent.SmoothedGriddedPosition + Offset, 5, State ? new Color(230, 230, 230, 255) : new Color(0, 0, 0, 100));
    DrawRectangleRoundedLinesEx(new Rectangle(Parent.SmoothedGriddedPosition + Offset - Vector2.One * 5, 10, 10), 1, 10, 1.01f, Color.Black);
    if (IsOutput)
      DrawLineEx(Parent.SmoothedGriddedPosition + Offset + Vector2.UnitX * 30, Parent.SmoothedGriddedPosition + Offset + Vector2.UnitX * 5, 1.01f, Color.Black);
    else
      DrawLineEx(Parent.SmoothedGriddedPosition + Offset - Vector2.UnitX * 30, Parent.SmoothedGriddedPosition + Offset - Vector2.UnitX * 5, 1.01f, Color.Black);
    
    if (StartedDragging && Wire is null)
    {
      DrawLineBezier(Parent.SmoothedGriddedPosition + StartWireOffset, GetScreenToWorld2D(GetMousePosition(), Globals.Camera), 4, Color.DarkPurple);
      DrawLineBezier(Parent.SmoothedGriddedPosition + StartWireOffset, GetScreenToWorld2D(GetMousePosition(), Globals.Camera), 2, Color.RayWhite);
    }
    
    if (Parent is not null && Connector?.Parent is not null)
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
}