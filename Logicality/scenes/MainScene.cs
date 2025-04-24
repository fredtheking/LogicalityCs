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
  public float RotationShock = 0;
  public Sound? PlaceSound;
  
  public override void Init()
  {
    LogicBox.Create(new LogicBox(LogicStates.Not, Vector2.One * 70, true));
    
    Selector.Init();
    foreach (LogicBox box in LogicBox.Boxes)
      box.Init();
    Overlay += () => Selector.Render();
  }
  public override void Enter()
  {
    Config.Camera.Offset = Config.Resolution / 2;
    Config.Camera.Target = Config.MapSize / 2;
    Config.Camera.Rotation = 0;
    Config.Camera.Zoom = 1;
    
    PlaceSound = LoadSound("assets/sounds/place.ogg");
    Selector.Enter();
    foreach (LogicBox box in FinalBoxes)
      box.Enter();
  }
  public override void Leave()
  {
    Config.Camera.Offset = Vector2.Zero;
    Config.Camera.Target = Vector2.Zero;
    Config.Camera.Rotation = 0;
    Config.Camera.Zoom = 1;
    
    UnloadSound(PlaceSound!.Value);
    PlaceSound = null;
    
    Selector.Leave();
    foreach (LogicBox box in FinalBoxes)
      box.Leave();
  }
  public override void Update()
  {
    Selector.Update();
    if (IsMouseButtonPressed(MouseButton.Right))
      if (LogicBox.Create(new LogicBox(Selector.Pick(), GetScreenToWorld2D(GetMousePosition(), Config.Camera) - new Vector2(60, 50))))
      {
        RotationShock = 1;
        PlaySound(PlaceSound!.Value);
      }

    CameraShakeHandle();
    ChangeSelectorFromNumbers();
    MoveCamera();
    
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
    float intensity = 2f;
    Color purpa = new(58, 30, 54, 255);
    for (int i = 0; i < Config.MapSize.X / step + 1; ++i)
      DrawLineV(new Vector2((float)(i * step + Math.Sin(GetTime()*2) * intensity), 0), Config.MapSize with { X = (float)(i * step + Math.Sin(GetTime()*1.2f) * intensity) }, purpa);
    for (int i = 0; i < Config.MapSize.Y / step + 1; ++i)
      DrawLineV(new Vector2(0, (float)(i * step + Math.Sin(GetTime()*1.2f) * intensity)), Config.MapSize with { Y = (float)(i * step + Math.Sin(GetTime()*2) * intensity) }, purpa);
  }

  private void CameraShakeHandle()
  {
    if (RotationShock > 0) RotationShock -= 5 * GetFrameTime();
    if (RotationShock < 0) RotationShock = 0;
    
    Config.Camera.Rotation = (float)((new Random().NextDouble() * 2 - 1) * RotationShock);
  }

  private void ChangeSelectorFromNumbers()
  {
    KeyboardKey keyPressed = (KeyboardKey)GetKeyPressed();
    if (keyPressed is >= KeyboardKey.One and <= KeyboardKey.Seven)
      Selector.Selected = keyPressed - KeyboardKey.One;
  }

  private void MoveCamera()
  {
    if (IsMouseButtonDown(MouseButton.Middle) && !IsMouseButtonDown(MouseButton.Left))
      Config.Camera.Target -= GetMouseDelta() / Config.Camera.Zoom;
    Config.Camera.Zoom += GetMouseWheelMoveV().Y * 200 * GetFrameTime();
    
    Config.Camera.Zoom = Math.Clamp(Config.Camera.Zoom, 0.1f, 5f);
    
    Config.Camera.Target.X = Math.Clamp(Config.Camera.Target.X, 0, Config.MapSize.X);
    Config.Camera.Target.Y = Math.Clamp(Config.Camera.Target.Y, 0, Config.MapSize.Y);
  }
}