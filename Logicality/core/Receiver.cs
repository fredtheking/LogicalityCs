using System.Numerics;

namespace Logicality.core;

public class Receiver
{
  public bool State { get; set; }
  public bool IsOutput { get; set; }
  public Vector2 Offset;
  public Receiver? Connector { get; set; }
  public LogicBox Parent { get; set; }
  public WireLine? Wire { get; set; }
  
  public Receiver(LogicBox parent, Vector2 offset, bool isOutput) 
  {
    (Parent, Offset, IsOutput) = (parent, offset, isOutput);
  }
}