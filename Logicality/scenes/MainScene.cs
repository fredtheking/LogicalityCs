using System.Numerics;
using Logicality.core;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Logicality.scenes;

public class MainScene : Scene
{
  public override string Name => nameof(MainScene);
  public LogicBoxSelector Selector = new();
  public List<LogicBox> FinalBoxes = [];
  
  public override void Init()
  {
    Selector.Init();
    foreach (LogicBox box in LogicBox.Boxes)
      box.Init();
  }
  public override void Enter()
  {
    Selector.Enter();
    foreach (LogicBox box in FinalBoxes)
      box.Enter();
  }
  public override void Update()
  {
    Selector.Update();
    if (IsMouseButtonPressed(MouseButton.Middle))
      LogicBox.Create(new LogicBox(Selector.Pick(), GetMousePosition() - new Vector2(60, 50)));

    KeyboardKey keyPressed = (KeyboardKey)GetKeyPressed();
    if (keyPressed is >= KeyboardKey.One and <= KeyboardKey.Seven)
      Selector.Selected = keyPressed - KeyboardKey.One;
    
    foreach (LogicBox box in FinalBoxes)
      box.Update();
    FinalBoxes = new List<LogicBox>(LogicBox.Boxes);
  }
  public override void Render()
  {
    DrawSceneGrid();
    foreach (LogicBox box in FinalBoxes)
      box.Render();
    Selector.Render();
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