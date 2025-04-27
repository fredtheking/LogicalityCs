using static Raylib_cs.Raylib;
namespace Logicality.utils;

public static class MusicUtils
{
  private static int LastPlayedMusic;
  
  public static void RollNew()
  {
    if (Globals.CurrentGameMusic is not null)
    {
      StopMusicStream(Globals.CurrentGameMusic.Value);
      Unload();
    }

    int currentId;
    do
    {
      currentId = GetRandomValue(1, 5);
    } while (LastPlayedMusic == currentId);
    LastPlayedMusic = currentId;
    
    Globals.CurrentGameMusic = LoadMusicStream($"assets/music/game/{currentId}.ogg");
    PlayMusicStream(Globals.CurrentGameMusic.Value);
  }

  public static void Update()
  {
    if (Globals.CurrentGameMusic is null || !IsMusicStreamPlaying(Globals.CurrentGameMusic.Value)) RollNew();
    if (Globals.CurrentGameMusic is not null) UpdateMusicStream(Globals.CurrentGameMusic.Value);
  }

  public static void Unload()
  {
    UnloadMusicStream(Globals.CurrentGameMusic!.Value);
  }
}