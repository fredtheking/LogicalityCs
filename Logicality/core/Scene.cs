using Logicality.utils.interfaces;

namespace Logicality.core;

public abstract class Scene : IScript
{
  public abstract string Name { get; }
  public Action? Overlay;
  
  public abstract void Init();
  public abstract void Enter();
  public abstract void Update();
  public abstract void Render();
}