using System.Numerics;
using Logicality.utils;
using Logicality.utils.interfaces;
using Raylib_cs;
using Steamworks;
using static Raylib_cs.Raylib;

namespace Logicality.core;

public class Receiver : IScript
{
  public static List<Receiver> Receivers { get; } = [];
  public bool State { get; set; }
  public bool StartedDragging { get; set; }
  public bool IsOutput { get; set; }
  public Vector2 Offset;
  public Vector2 StartWireOffset;
  public Receiver? Connector { get; set; }
  public LogicBox Parent { get; set; }
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
    bool dragging = Hitbox.Drag[(int)MouseButton.Left];
    StartedDragging = dragging;
    Parent.StartedDragging += dragging ? 1 : 0;

    if (Hitbox.Release[(int)MouseButton.Left])
    {
      List<Receiver> OtherReceivers = new(Receivers);
      if (Parent.Input1 is not null) OtherReceivers.Remove(Parent.Input1);
      if (Parent.Input2 is not null) OtherReceivers.Remove(Parent.Input2);
      if (Parent.Output is not null) OtherReceivers.Remove(Parent.Output);
      foreach (Receiver receiver in OtherReceivers)
      {
        if (CheckCollisionPointRec(GetScreenToWorld2D(GetMousePosition(), Globals.Camera), receiver.Hitbox.Rect))
        {
          Console.WriteLine("FOUND MY DAD!!");
          break;
        }
      }
    }
  }

  public void Render()
  {
    if (StartedDragging && Wire is null)
    {
      DrawLineBezier(Parent.SmoothedGriddedPosition + StartWireOffset, GetScreenToWorld2D(GetMousePosition(), Globals.Camera), 2, Color.RayWhite);
    }
    Hitbox.Render();
  }
}