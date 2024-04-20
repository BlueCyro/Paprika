using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Numerics;


namespace Paprika;

[StructLayout(LayoutKind.Explicit)]
public readonly ref struct Bounds2D(Vector2 min, Vector2 max)
{
    public readonly Vector128<int> BoundsInt => Vector128.ConvertToInt32(Bounds);


    [FieldOffset(0)]
    public readonly Vector2 Min = min;
    
    [FieldOffset(8)]
    public readonly Vector2 Max = max;

    [FieldOffset(0)]
    public readonly Vector128<float> Bounds;

    [FieldOffset(0)]
    public readonly Vector4 BoundsVec4;
}
