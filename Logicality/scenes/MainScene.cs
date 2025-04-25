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
  public Sound? DisappearSound;
  public float CameraZoom = 1;
  
  public override void Init()
  {
    Selector.Init();
    foreach (LogicBox box in LogicBox.Boxes)
      box.Init();
    Overlay += () => Selector.Render();
  }
  public override void Enter()
  {
    Globals.Camera.Offset = Globals.Resolution / 2;
    Globals.Camera.Target = Globals.MapSize / 2;
    Globals.Camera.Rotation = 0;
    Globals.Camera.Zoom = 1;
    CameraZoom = 1;
    
    PlaceSound = LoadSound("assets/sounds/place.ogg");
    DisappearSound = LoadSound("assets/sounds/disappear.ogg");
    
    Selector.Enter();
    foreach (LogicBox box in FinalBoxes)
      box.Enter();
  }
  public override void Leave()
  {
    Globals.Camera.Offset = Vector2.Zero;
    Globals.Camera.Target = Vector2.Zero;
    Globals.Camera.Rotation = 0;
    Globals.Camera.Zoom = 1;
    
    UnloadSound(PlaceSound!.Value);
    PlaceSound = null;
    UnloadSound(DisappearSound!.Value);
    DisappearSound = null;
    
    Selector.Leave();
    foreach (LogicBox box in FinalBoxes)
      box.Leave();
  }
  public override void Update()
  {
    Selector.Update();
    if (IsMouseButtonPressed(MouseButton.Right))
      if (LogicBox.Create(new LogicBox(Selector.Pick(), GetScreenToWorld2D(GetMousePosition(), Globals.Camera) - new Vector2(60, 50))))
      {
        RotationShock = 1;
        PlaySound(PlaceSound!.Value);
      }

    CameraShakeHandle();
    ChangeSelectorFromNumbers();
    MoveCamera();
    
    foreach (LogicBox box in FinalBoxes)
    {
      box.Update();
      if (box.Destroy)
        PlaySound(DisappearSound!.Value);
    }
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
    for (int i = 0; i < Globals.MapSize.X / step + 1; ++i)
      DrawLineV(new Vector2((float)(i * step + Math.Sin(GetTime()*2) * intensity), 0), Globals.MapSize with { X = (float)(i * step + Math.Sin(GetTime()*1.2f) * intensity) }, purpa);
    for (int i = 0; i < Globals.MapSize.Y / step + 1; ++i)
      DrawLineV(new Vector2(0, (float)(i * step + Math.Sin(GetTime()*1.2f) * intensity)), Globals.MapSize with { Y = (float)(i * step + Math.Sin(GetTime()*2) * intensity) }, purpa);
  }

  private void CameraShakeHandle()
  {
    if (RotationShock > 0) RotationShock -= 5 * GetFrameTime();
    if (RotationShock < 0) RotationShock = 0;
    
    Globals.Camera.Rotation = (float)((new Random().NextDouble() * 2 - 1) * RotationShock);
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
      Globals.Camera.Target -= GetMouseDelta() / Globals.Camera.Zoom;
    CameraZoom += GetMouseWheelMoveV().Y * 0.1f;
    CameraZoom = Math.Clamp(CameraZoom, 0.4f, 5f);
    
    Globals.Camera.Zoom = Raymath.Lerp(Globals.Camera.Zoom, CameraZoom, 16f * GetFrameTime());
    Globals.Camera.Target.X = Math.Clamp(Globals.Camera.Target.X, 0, Globals.MapSize.X);
    Globals.Camera.Target.Y = Math.Clamp(Globals.Camera.Target.Y, 0, Globals.MapSize.Y);
  }
}