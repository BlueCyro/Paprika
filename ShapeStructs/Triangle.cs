using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.CompilerServices;
using System.Numerics;


namespace Paprika;

[StructLayout(LayoutKind.Explicit)]
public struct Triangle
{
    [FieldOffset(0)]
    public readonly Vector2 v1;
    
    [FieldOffset(16)]
    public readonly Vector2 v2;

    [FieldOffset(32)]
    public readonly Vector2 v3;

    
    
    [FieldOffset(0)]
    public Vector3 p1;
    
    [FieldOffset(16)]
    public Vector3 p2;
    
    [FieldOffset(32)]
    public Vector3 p3;

    
    
    [FieldOffset(0)]
    public Matrix4x4 TriMatrix;

    [FieldOffset(0)]
    public Vector256<float> Vec256;

    public readonly Vector3 Normal => Vector3.Normalize(Vector3.Cross(p2 - p1, p3 - p1));
    public readonly Vector3 Center => (p1 + p2 + p3) / 3f;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Transform(in Matrix4x4 transform)
    {
        TriMatrix *= transform;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector3 Bary(in int x, in int y, in Lines lines)
    {
        float e1 = (x - v1.X) * lines.L0.Y - (y - v1.Y) * lines.L0.X;
        float e2 = (x - v2.X) * lines.L1.Y - (y - v2.Y) * lines.L1.X;
        float e3 = (x - v3.X) * lines.L2.Y - (y - v3.Y) * lines.L2.X;
        float area = e1 + e2 + e3;


        return new Vector3(e2, e3, e1) / area;
    }


    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Vector3 Bary(in float x, in float y, in Lines lines)
    {
        float e1 = (x - v1.X) * lines.L0.Y - (y - v1.Y) * lines.L0.X;
        float e2 = (x - v2.X) * lines.L1.Y - (y - v2.Y) * lines.L1.X;
        float e3 = (x - v3.X) * lines.L2.Y - (y - v3.Y) * lines.L2.X;
        float area = e1 + e2 + e3;


        return new Vector3(e2, e3, e1) / area;
    }
}