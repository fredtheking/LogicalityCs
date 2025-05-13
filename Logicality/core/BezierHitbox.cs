using System.Numerics;
using Logicality.utils;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Logicality.core;

public class BezierHitbox : Hitbox
{
  public Vector2[] Points = null!;
  public Vector2 Start;
  public Vector2 End;

  public BezierHitbox() { }
  public BezierHitbox(Vector2[] points) => Points = points;
  
  public override bool HoverHandle()
  {
    List<bool> hover = [];
    Vector2 mousePoint = GetScreenToWorld2D(GetMousePosition(), Globals.Camera);

    for (int i = 0; i < Points.Length - 2; i += 2)
    {
      Vector2 p0 = Points[i];
      Vector2 p1 = Points[i + 1];
      Vector2 p2 = Points[i + 2];
      Vector2 p3 = Points[i + 3];

      //  p0 -- p2
      //  |  \/  |
      //  p1 -- p3
      hover.Add(CheckCollisionPointTriangle(mousePoint, p0, p1, p2) ||
                CheckCollisionPointTriangle(mousePoint, p1, p2, p3));
    }

    bool verdict = hover.Any(x => x);
    return verdict;
  }

  public override void Render()
  {
    if (Globals.Debug)
      DrawLineBezier(Start, End, 6, new Color(255, 0, 0, 100));
  }
}