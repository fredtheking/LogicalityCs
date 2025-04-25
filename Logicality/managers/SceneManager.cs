using Logicality.core;

namespace Logicality.managers;

public class SceneManager
{
  public List<Scene> All { get; } = [];
  private string? _changeSceneTo { get; set; }
  public Scene Current { get; private set; } = null!;

  public void Add(params Scene[] scenes) =>
    scenes.ToList().ForEach(s => All.Add(s));

  public void Change(string sceneName) =>
    _changeSceneTo = sceneName;
  
  public void Previous() =>
    Change(All[(All.IndexOf(Current) - 1 + All.Count) % All.Count].Name);
  
  public void Next() =>
    Change(All[(All.IndexOf(Current) + 1) % All.Count].Name);

  public void ActualChange(bool ignoreLeave = false)
  {
    if (_changeSceneTo == null) return;
    if (!ignoreLeave) Current.Leave();
    Current = All.First(x => x.Name == _changeSceneTo);
    _changeSceneTo = null;
    Current.Enter();
  }
}