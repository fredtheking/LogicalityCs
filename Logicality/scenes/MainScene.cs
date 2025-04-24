using System.Numerics;
using Logicality.core;
using Logicality.utils;
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
    Overlay += () => Selector.Render();
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
    if (IsMouseButtonPressed(MouseButton.Right))
      LogicBox.Create(new LogicBox(Selector.Pick(), GetScreenToWorld2D(GetMousePosition(), Config.Camera) - new Vector2(60, 50)));

    KeyboardKey keyPressed = (KeyboardKey)GetKeyPressed();
    if (keyPressed is >= KeyboardKey.One and <= KeyboardKey.Seven)
      Selector.Selected = keyPressed - KeyboardKey.One;
    
    if (IsMouseButtonDown(MouseButton.Middle))
      Config.Camera.Target -= GetMouseDelta() / Config.Camera.Zoom;
    Config.Camera.Zoom += GetMouseWheelMoveV().Y * 200 * GetFrameTime();
    
    Config.Camera.Zoom = Math.Clamp(Config.Camera.Zoom, 0.1f, 5f);
    
    foreach (LogicBox box in FinalBoxes)
      box.Update();
    FinalBoxes = new List<LogicBox>(LogicBox.Boxes);
  }
  public override void Render()
  {
    DrawSceneGrid();
    foreach (LogicBox box in FinalBoxes)
      box.Render();
  }

  private void DrawSceneGrid()
  {
    int step = 20;
    float intensity = 0.67f;
    Color purpa = new(28, 13, 29, 255);
    for (int i = 0; i < Config.Resolution.X / step + 1; ++i)
      DrawLineV(new Vector2((float)(i * step + Math.Sin(GetTime() * 1.3 * i*2) * intensity), 0), Config.Resolution with { X = (float)(i * step + Math.Sin(GetTime() * 3.5 * i*2) * intensity) }, purpa);
    for (int i = 0; i < Config.Resolution.Y / step + 1; ++i)
      DrawLineV(new Vector2(0, (float)(i * step + Math.Sin(GetTime() * 3.5 * i*2) * intensity)), Config.Resolution with { Y = (float)(i * step + Math.Sin(GetTime() * 1.3 * i*2) * intensity) }, purpa);
  }
}