using System.Numerics;
using static Raylib_cs.Raylib;
using Raylib_cs;
using Logicality.utils.interfaces;

namespace Logicality.core;

public record WireLine(Receiver From, Receiver To) : IScript
{
  public bool Drawed { private get; set; }
  public event Action? DestroyTrigger;
  
  public void Init() { }
  public void Enter() { }
  public void Leave() { }

  public void Update()
  {
    To.State = From.State;
    if (From.Parent is null || To.Parent is null) 
      Destroy();
  }

  private void Destroy()
  {
    From.Wire = To.Wire = null;
    From.Connector = To.Connector = null;
    To.State = false;
    DestroyTrigger?.Invoke();
  }

  public void Render()
  {
    if (Drawed) return;
    DrawLineBezier(From.Parent!.SmoothedGriddedPosition + From.StartWireOffset, To.Parent!.SmoothedGriddedPosition + To.StartWireOffset, 4, Color.Black);
    DrawLineBezier(From.Parent.SmoothedGriddedPosition + From.StartWireOffset, To.Parent.SmoothedGriddedPosition + To.StartWireOffset, 2, From.State ? Color.RayWhite : Color.DarkGray);
    Drawed = true;
  }
}