using static Raylib_cs.Raylib;
namespace Logicality.utils;

public static class MusicUtils
{
  private static int LastPlayedMusic = 0;
  
  public static void RollNew()
  {
    if (Globals.CurrentGameMusic is not null)
    {
      StopMusicStream(Globals.CurrentGameMusic.Value);
      UnloadMusicStream(Globals.CurrentGameMusic.Value);
    }

    int CurrentId;
    do
    {
      CurrentId = GetRandomValue(1, 5);
    } while (LastPlayedMusic == CurrentId);
    LastPlayedMusic = CurrentId;
    
    Globals.CurrentGameMusic = LoadMusicStream($"assets/music/game/{CurrentId}.ogg");
    PlayMusicStream(Globals.CurrentGameMusic.Value);
  }

  public static void Update()
  {
    if (Globals.CurrentGameMusic is null || !IsMusicStreamPlaying(Globals.CurrentGameMusic.Value)) RollNew();
    if (Globals.CurrentGameMusic is not null) UpdateMusicStream(Globals.CurrentGameMusic.Value);
  }
}