using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Numerics;


namespace Paprika;

[StructLayout(LayoutKind.Explicit)]
public ref struct QuickVec3(in Vector3 target)
{
    [FieldOffset(0)]
    public Vector3 V3 = target;
    
    [FieldOffset(0)]
    public Vector128<float> V128;
}
