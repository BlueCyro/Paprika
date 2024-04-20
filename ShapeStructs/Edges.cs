using System.Numerics;


namespace Paprika;

// [StructLayout(LayoutKind.Explicit)]
public readonly ref struct Edges(in Vector2 p1, in Vector2 p2, in Vector2 p3)
{
    public readonly float A1 = p1.Y - p2.Y;
    public readonly float B1 = p2.X - p1.X;
    public readonly float C1 = p1.X * p2.Y - p2.X * p1.Y;


    public readonly float A2 = p2.Y - p3.Y;
    public readonly float B2 = p3.X - p2.X;
    public readonly float C2 = p2.X * p3.Y - p3.X * p2.Y;


    public readonly float A3 = p3.Y - p1.Y;
    public readonly float B3 = p1.X - p3.X;
    public readonly float C3 = p3.X * p1.Y - p1.X * p3.Y;



    public readonly void IsInside(int x, int y, out float e1, out float e2, out float e3)
    {
        e1 = A1 * x + B1 * y + C1;
        e2 = A2 * x + B2 * y + C2;
        e3 = A3 * x + B3 * y + C3;
    }
}
