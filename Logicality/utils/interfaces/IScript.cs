namespace Logicality.utils.interfaces;

public interface IScript
{
  public abstract void Init();
  public abstract void Enter();
  public abstract void Update();
  public abstract void Render();
}