using System.Numerics;
using System.Runtime.CompilerServices;
using BepuUtilities;


namespace Paprika;


public struct Triangle
{
    public Vector3 A;
    public Vector3 B;
    public Vector3 C;



    public Triangle(Vector3 a, Vector3 b, Vector3 c)
    {
        A = a;
        B = b;
        C = c;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void GetBounds(out Vector3 min, out Vector3 max)
    {
        min = Vector3.Min(Vector3.Min(A, B), C);
        max = Vector3.Max(Vector3.Max(A, B), C);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Transform(in Matrix4x4 transform)
    {
        A = Vector3.Transform(A, transform);
        B = Vector3.Transform(B, transform);
        C = Vector3.Transform(C, transform);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ZDivide(out Vector3 oldZ)
    {
        oldZ = new(A.Z, B.Z, C.Z);


        A /= MathF.Max(A.Z, 0f);
        B /= MathF.Max(B.Z, 0f);
        C /= MathF.Max(C.Z, 0f);
    }



    public readonly Vector3 Normal => Vector3.Normalize(Vector3.Cross(B - A, C - A));
    public readonly Vector3 Center => (A + B + C) / 3f;
}



public struct TriangleWide
{
    public Vector3Wide A;
    public Vector3Wide B;
    public Vector3Wide C;



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void GetBounds(out Vector3Wide min, out Vector3Wide max)
    {
        Vector3Wide.Min(A, B, out min);
        Vector3Wide.Min(C, min, out min);

        Vector3Wide.Max(A, B, out max);
        Vector3Wide.Max(C, max, out max);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WidenFrom(in Triangle triangle)
    {
        Vector3Wide.Broadcast(triangle.A, out Vector3Wide a);
        Vector3Wide.Broadcast(triangle.B, out Vector3Wide b);
        Vector3Wide.Broadcast(triangle.C, out Vector3Wide c);

        A = a;
        B = b;
        C = c;
    }



    public readonly Vector3Wide Normal
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            Vector3Wide.Cross(B - A, C - A, out Vector3Wide crossed);
            Vector3Wide.Normalize(crossed, out Vector3Wide normalized);
            return normalized;
        }
    }


    public readonly Vector3Wide Center
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            Vector3Wide.Scale(A + B + C, new(1f / 3f), out Vector3Wide scaled);
            return scaled;
        }
    }
}