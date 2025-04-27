using System.Numerics;
using Logicality.utils;
using static Raylib_cs.Raylib;
using Raylib_cs;
using Logicality.utils.interfaces;

namespace Logicality.core;

public record WireLine(Receiver From, Receiver To) : IScript
{
  public static List<WireLine> Wires = [];
  const int SPLINE_SEGMENT_DIVISIONS = 24;
  public bool Updated { private get; set; }
  public bool Drawed { private get; set; }
  public Vector2[] bezierPoints = new Vector2[2 * SPLINE_SEGMENT_DIVISIONS + 2];
  public BezierHitbox Hitbox = new();
  public event Action? DestroyTrigger;
  
  static float EaseCubicInOut(float t, float b, float c, float d)
  {
    float result = 0.0f;

    if ((t /= 0.5f*d) < 1) result = 0.5f*c*t*t*t + b;
    else
    {
      t -= 2;
      result = 0.5f*c*(t*t*t + 2.0f) + b;
    }

    return result;
  }

  public void Init()
  {
    Wires.Add(this);
    Hitbox.Init();
    UpdateBezierEaseCubicInOutPoints(6);
  }

  public void Enter()
  {
    Hitbox.Enter();
  }

  public void Leave()
  {
    Hitbox.Leave();
  }

  public void Update()
  {
    Hitbox.Update();
    To.State = From.State;
    if (From.Parent is null || To.Parent is null || Hitbox.Press[(int)MouseButton.Right]) 
      Destroy();
  }

  public void UpdateBezierEaseCubicInOutPoints(float thick)
  {
    // Part that I stole from og raylib lib and modified cuz im too lazy to think myself :)

    if (From.Parent is null || To.Parent is null || Updated) return;
    
    Vector2 startPos = From.Parent!.GriddedPosition + From.StartWireOffset;
    Vector2 previous = startPos;
    Vector2 endPos = To.Parent!.GriddedPosition + To.StartWireOffset;
    Vector2 current = Vector2.Zero;

    Hitbox.Start = startPos;
    Hitbox.End = endPos;

    Array.Clear(bezierPoints);

    for (int i = 1; i <= SPLINE_SEGMENT_DIVISIONS; i++)
    {
      current.Y = EaseCubicInOut(i, startPos.Y, endPos.Y - startPos.Y, SPLINE_SEGMENT_DIVISIONS);
      current.X = previous.X + (endPos.X - startPos.X)/SPLINE_SEGMENT_DIVISIONS;

      float dy = current.Y - previous.Y;
      float dx = current.X - previous.X;
      float size = (float)(0.5f*thick/Math.Sqrt(dx*dx+dy*dy));

      if (i == 1)
      {
        bezierPoints[0].X = previous.X + dy*size;
        bezierPoints[0].Y = previous.Y - dx*size;
        bezierPoints[1].X = previous.X - dy*size;
        bezierPoints[1].Y = previous.Y + dx*size;
      }

      bezierPoints[2*i + 1].X = current.X - dy*size;
      bezierPoints[2*i + 1].Y = current.Y + dx*size;
      bezierPoints[2*i].X = current.X + dy*size;
      bezierPoints[2*i].Y = current.Y - dx*size;

      previous = current;
    }

    Hitbox.Points = bezierPoints;
  }

  private void Destroy()
  {
    From.Wire = To.Wire = null;
    From.Connector = To.Connector = null;
    To.State = false;
    Wires.Remove(this);
    DestroyTrigger?.Invoke();
  }

  public void Render()
  {
    if (Drawed) return;
    DrawLineBezier(From.Parent!.SmoothedGriddedPosition + From.StartWireOffset, To.Parent!.SmoothedGriddedPosition + To.StartWireOffset, 4, Color.Black);
    DrawLineBezier(From.Parent.SmoothedGriddedPosition + From.StartWireOffset, To.Parent.SmoothedGriddedPosition + To.StartWireOffset, 2, From.State ? Color.RayWhite : Color.DarkGray);
    
    Hitbox.Render();
    Drawed = true;
  }
}