using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.CompilerServices;
using System.Numerics;


namespace Paprika;

[StructLayout(LayoutKind.Explicit)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public ref struct QuickVec2(in Vector2 vec)
{
    [FieldOffset(0)]
    public Vector2 Vec2 = vec;

    [FieldOffset(0)]
    public Vector64<float> Vec64;
}
