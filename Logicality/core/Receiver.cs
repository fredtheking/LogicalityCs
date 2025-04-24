using System.Numerics;
using Logicality.utils.interfaces;
using Raylib_cs;

namespace Logicality.core;

public class Receiver : IScript
{
  public bool State { get; set; }
  public bool IsOutput { get; set; }
  public Vector2 Offset;
  public Receiver? Connector { get; set; }
  public LogicBox Parent { get; set; }
  public WireLine? Wire { get; set; }
  public Hitbox Hitbox { get; set; }
  
  public Receiver(LogicBox parent, Vector2 offset, bool isOutput) 
  {
    (Parent, Offset, IsOutput) = (parent, offset, isOutput);
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
  }

  public void Render()
  {
    Hitbox.Render();
  }
}