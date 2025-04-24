using System.Diagnostics;
using Logicality.managers;
using Logicality.scenes;
using Logicality.utils;
using static Raylib_cs.Raylib;
using Raylib_cs;

// PRE-INITIALISATION
SceneManager sceneManager = new();
sceneManager.Add(new MainScene());
sceneManager.Add(new MenuScene());
sceneManager.Change(nameof(MainScene));

// WINDOW SETUP
SetConfigFlags(ConfigFlags.AlwaysRunWindow | ConfigFlags.HighDpiWindow);
InitWindow((int)Config.Resolution.X, (int)Config.Resolution.Y, "Logicality");
InitAudioDevice();
//SetTargetFPS(30);

// POST-INITIALISATION
sceneManager.All.ForEach(x => x.Init());
sceneManager.ActualChange();

// MAIN LOOP
while (!WindowShouldClose())
{
  // ENTER CHECK
  if (sceneManager.Changed)
  {
    // ENTER
    sceneManager.Current.Enter();
    sceneManager.Changed = false;
    // GLOBAL-ENTER
    // ...
  }
  
  
  
  // UPDATE
  sceneManager.Current.Update();
  // GLOBAL-UPDATE
  if (IsKeyPressed(KeyboardKey.F3) || IsKeyPressed(KeyboardKey.Grave))
    Config.Debug = !Config.Debug;
  
  
  
  BeginDrawing();
  ClearBackground(new(43, 21, 41, 255));
  BeginMode2D(Config.Camera);
  
  // RENDER
  sceneManager.Current.Render();
  // GLOBAL-RENDER
  // ...
  
  EndMode2D();
  sceneManager.Current.Overlay?.Invoke();
  if (Config.Debug)
  {
    DrawFPS(10, 10);
    DrawText(sceneManager.Current.Name, 10, 30, 20, Color.White);
  }
  EndDrawing();
  
  // CHANGE SCENE IF NEEDED
  sceneManager.ActualChange();
}

CloseAudioDevice();
CloseWindow();