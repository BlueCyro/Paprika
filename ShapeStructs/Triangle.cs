using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.CompilerServices;
using System.Numerics;


namespace Paprika;


[StructLayout(LayoutKind.Explicit)]
public struct Triangle
{
    public Triangle(Vertex[] vertices)
    {
        m1N = new(vertices[0].Position, 1f);
        m2N = new(vertices[1].Position, 1f);
        m3N = new(vertices[2].Position, 1f);
        m4N = new(0f, 0f, 1f, 0f);


        uv1 = vertices[0].UV;
        uv2 = vertices[1].UV;
        uv3 = vertices[2].UV;
    }

    
    public Triangle(in Triangle oldTri, in Matrix4x4 triMat)
    {
        uv1 = oldTri.uv1;
        uv2 = oldTri.uv2;
        uv3 = oldTri.uv3;
        TriMatrix = triMat;
    }
    

    [FieldOffset(0)]
    public Vector2 v1;
    
    [FieldOffset(16)]
    public Vector2 v2;

    [FieldOffset(32)]
    public Vector2 v3;

    
    
    [FieldOffset(0)]
    public Vector3 p1;
    
    [FieldOffset(16)]
    public Vector3 p2;
    
    [FieldOffset(32)]
    public Vector3 p3;



    [FieldOffset(0)]
    public Vector4 m1N;

    [FieldOffset(16)]
    public Vector4 m2N;

    [FieldOffset(32)]
    public Vector4 m3N;

    [FieldOffset(48)]
    public Vector4 m4N;


    [FieldOffset(64)]
    public readonly Vector2 uv1;

    [FieldOffset(72)]
    public readonly Vector2 uv2;

    [FieldOffset(80)]
    public readonly Vector2 uv3;


    [field: FieldOffset(0)]
    public Matrix4x4 TriMatrix
    {
        get;
        set;
    }



    public readonly Vector3 Normal
    {
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Vector3.Normalize(Vector3.Cross(p2 - p1, p3 - p1));
    }

    
    public readonly Vector3 Center
    {
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (p1 + p2 + p3) / 3f;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Triangle Transform(in Matrix4x4 transform)
    {
        return this with { TriMatrix = this.TriMatrix * transform };
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
}
