using Logicality.core;

namespace Logicality.managers;

public class SceneManager
{
  public List<Scene> All { get; private set; }
  private string? _changeSceneTo { get; set; }
  public Scene Current { get; private set; }
  public bool Changed { get; set; }
  
  public void Add(params Scene[] scenes) =>
    All = scenes.ToList();

  public void Change(string sceneName) =>
    _changeSceneTo = sceneName;

  public void ActualChange()
  {
    if (_changeSceneTo == null) return;
    Current = All.First(x => x.Name == _changeSceneTo);
    Changed = true;
    _changeSceneTo = null;
  }
}