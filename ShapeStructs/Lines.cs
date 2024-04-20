using System.Runtime.InteropServices;
using System.Numerics;


namespace Paprika;

[StructLayout(LayoutKind.Explicit)]
public ref struct Lines(in Vector2 p1, in Vector2 p2, in Vector2 p3)
{
    [FieldOffset(0)]
    public Vector2 L0 = p1 - p2;

    [FieldOffset(8)]
    public Vector2 L1 = p2 - p3;

    [FieldOffset(16)]
    public Vector2 L2 = p3 - p1;
}
