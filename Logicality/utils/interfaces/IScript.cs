namespace Logicality.utils.interfaces;

public interface IScript
{
  public void Init();
  public void Enter();
  public void Leave();
  public void Update();
  public void Render();
}