using Logicality.managers;
using Logicality.scenes;
using Logicality.utils;
using static Raylib_cs.Raylib;
using Raylib_cs;

// PRE-INITIALISATION
SceneManager sceneManager = new();
sceneManager.Add(new MainScene());
sceneManager.Change(nameof(MainScene));

// WINDOW SETUP
SetConfigFlags(ConfigFlags.AlwaysRunWindow | ConfigFlags.HighDpiWindow);
InitWindow(1920, 1080, "Logicality");

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
  ClearBackground(new Color(18, 5, 21, 255));
  
  // RENDER
  sceneManager.Current.Render();
  // GLOBAL-RENDER
  if (Config.Debug)
    DrawFPS(10, 10);    
  
  EndDrawing();
  
  // CHANGE SCENE IF NEEDED
  sceneManager.ActualChange();
}

CloseWindow();