using Logicality.utils.interfaces;
using static Raylib_cs.Raylib;
using Raylib_cs;

namespace Logicality.core;

public class LogicBoxSelector : IScript
{
  private string[] Names = ["W", "N", "A", "O", "X", "B", "R"];
  List<SelectorOption> Options = [];

  public int Selected
  {
    get
    {
      return Options.FindIndex(x => x.Selected);
    }
    set
    {
      Options.ForEach(x => x.Selected = false); 
      Options[value].Selected = true;
    }
  }

  public void Init()
  {
    for (int i = 0; i < Names.Length; i++) 
      Options.Add(new SelectorOption(new(33 * i + 300, 0, 30, 30), Names[i]));
    Options[0].Selected = true;
  }
  public void Enter() { }
  public void Update()
  {
    Selected = Math.Clamp(Selected, 0, Options.Count - 1);
    for (int i = 0; i < Options.Count; i++)
    {
      Options[i].Update();
      if (Options[i].Hitbox.Press[(int)MouseButton.Left])
      {
        Selected = i;
        Options.ForEach(x => x.Selected = false);
        Options[i].Selected = true;
      }
    }
  }
  public void Render()
  {
    foreach (SelectorOption option in Options) 
      option.Render();
  }


  public LogicStates Pick()
  {
    return Options[Selected].Name switch
    {
      "W" => LogicStates.Wire,
      "N" => LogicStates.Not,
      "A" => LogicStates.And,
      "O" => LogicStates.Or,
      "X" => LogicStates.Xor,
      "B" => LogicStates.Battery,
      "R" => LogicStates.Receive,
      _ => throw new KeyNotFoundException()
    };
  }
}