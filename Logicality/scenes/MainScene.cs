using System.Numerics;
using Logicality.core;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Logicality.scenes;

public class MainScene : Scene
{
  public override string Name => nameof(MainScene);
  public LogicBoxSelector selector = new();
  public List<LogicBox> boxes = [];
  public static List<LogicBox> newBoxes = [];
  
  public override void Init()
  {
    LogicBox.Create(newBoxes, new LogicBox(newBoxes, LogicStates.Wire, new Vector2(100, 100)));
    LogicBox.Create(newBoxes, new LogicBox(newBoxes, LogicStates.Not, new Vector2(100, 300)));
    LogicBox.Create(newBoxes, new LogicBox(newBoxes, LogicStates.And, new Vector2(100, 500)));
    LogicBox.Create(newBoxes, new LogicBox(newBoxes, LogicStates.Or, new Vector2(100, 700)));
    LogicBox.Create(newBoxes, new LogicBox(newBoxes, LogicStates.Xor, new Vector2(300, 100)));
    LogicBox.Create(newBoxes, new LogicBox(newBoxes, LogicStates.Battery, new Vector2(300, 300)));
    LogicBox.Create(newBoxes, new LogicBox(newBoxes, LogicStates.Receive, new Vector2(300, 500)));
    
    foreach (LogicBox box in newBoxes)
      box.Init();
    selector.Init();
  }
  public override void Enter()
  {
    foreach (LogicBox box in boxes)
      box.Enter();
    selector.Enter();
  }
  public override void Update()
  {
    selector.Update();
    if (IsMouseButtonPressed(MouseButton.Middle))
      LogicBox.Create(newBoxes, new LogicBox(newBoxes, selector.Pick(), GetMousePosition() - new Vector2(60, 50)));

    foreach (LogicBox box in boxes)
      box.Update();
    boxes = new List<LogicBox>(newBoxes);
  }
  public override void Render()
  {
    DrawSceneGrid();
    foreach (LogicBox box in newBoxes)
      box.Render();
    selector.Render();
  }


  private void DrawSceneGrid()
  {
    Vector2 res = new Vector2(GetRenderWidth(), GetRenderHeight());
    int step = 20;
    float intensity = 0.67f;
    Color purpa = new(23, 8, 25, 255);
    for (int i = 0; i < res.X / step + 1; ++i)
      DrawLineV(new Vector2((float)(i * step + Math.Sin(GetTime() * 1.3 * i*2) * intensity), 0), res with { X = (float)(i * step + Math.Sin(GetTime() * 3.5 * i*2) * intensity) }, purpa);
    for (int i = 0; i < res.Y / step + 1; ++i)
      DrawLineV(new Vector2(0, (float)(i * step + Math.Sin(GetTime() * 3.5 * i*2) * intensity)), res with { Y = (float)(i * step + Math.Sin(GetTime() * 1.3 * i*2) * intensity) }, purpa);
  }
}