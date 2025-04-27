using Logicality.managers;
using Logicality.scenes;
using Logicality.utils;
using static Raylib_cs.Raylib;
using Raylib_cs;
using Steamworks;

// PRE-INITIALISATION
try
{
  Console.WriteLine(SteamAPI.Init() ? "Steam API initialised successfully!" : "Steam API not initialised.");
}
catch (Exception ex)
{
  Console.WriteLine($"Steam API Error: {ex.Message}");
}
SceneManager sceneManager = new();
sceneManager.Add(new MainScene(), new MenuScene());
sceneManager.Change(nameof(MainScene));
float elapsedTime = 0f;
int framesCounter = 0;

// WINDOW SETUP
SetConfigFlags(ConfigFlags.AlwaysRunWindow | ConfigFlags.HighDpiWindow);
InitWindow((int)Globals.Resolution.X, (int)Globals.Resolution.Y, "Logicality");
InitAudioDevice();
//SetTargetFPS(30);

// POST-INITIALISATION
sceneManager.All.ForEach(x => x.Init());
sceneManager.ActualChange(true);
Console.WriteLine(SteamAPI.IsSteamRunning());

// RESOURCES LOAD
Globals.InteractionSetSound = LoadSound("assets/sounds/click.ogg");


// MAIN LOOP
while (!WindowShouldClose())
{
  // UPDATE
  sceneManager.Current.Update();
  // GLOBAL-UPDATE
  Globals.MouseOnBezier = 0;
  VolumeUtils.Update();
  if (Globals.Volume > 0) 
    MusicUtils.Update();
  FpsProcess();
  if (IsKeyPressed(KeyboardKey.F3) || IsKeyPressed(KeyboardKey.Grave))
    Globals.Debug = !Globals.Debug;
  if (IsKeyPressed(KeyboardKey.F4))
    MusicUtils.RollNew();
  if (IsKeyPressed(KeyboardKey.F1))
    sceneManager.Previous();
  else if (IsKeyPressed(KeyboardKey.F2))
    sceneManager.Next();
  
  
  
  BeginDrawing();
  ClearBackground(new Color(43, 21, 41, 255));
  BeginMode2D(Globals.Camera);
  
  // RENDER
  sceneManager.Current.Render();
  // GLOBAL-RENDER
  // ...
  
  EndMode2D();
  sceneManager.Current.Overlay?.Invoke();
  VolumeUtils.Render();
  DrawText($"FPS: {Globals.FPS}", 10, 10, 20, Color.Green);
  DrawText(sceneManager.Current.Name, 10, 30, 20, Color.White);
  EndDrawing();
  
  // CHANGE SCENE IF NEEDED
  sceneManager.ActualChange();
}

sceneManager.Current.Leave();
MusicUtils.Unload();
UnloadSound(Globals.InteractionSetSound.Value);
SteamAPI.Shutdown();
CloseAudioDevice();
CloseWindow();

void FpsProcess()
{
  elapsedTime += GetFrameTime();
  framesCounter++;

  if (elapsedTime >= 0.5f)
  {
    Globals.FPS = framesCounter;
    framesCounter = 0;
    elapsedTime = 0f;
  }
}