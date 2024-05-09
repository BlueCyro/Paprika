using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BepuUtilities;


namespace Paprika;


// [StructLayout(LayoutKind.Sequential, Size = 512)]
public readonly ref struct EdgesVectorized
{
    public EdgesVectorized(in Vector4 a, in Vector4 b, in Vector4 c)
    {
        float a1 = b.Y - c.Y;
        float b1 = c.X - b.X;
        float c1 = b.X * c.Y - c.X * b.Y;


        float a2 = c.Y - a.Y;
        float b2 = a.X - c.X;
        float c2 = c.X * a.Y - a.X * c.Y;


        float a3 = a.Y - b.Y;
        float b3 = b.X - a.X;
        float c3 = a.X * b.Y - b.X * a.Y;



        A1 = new(a1);
        B1 = new(b1);
        C1 = new(c1);

        A2 = new(a2);
        B2 = new(b2);
        C2 = new(c2);

        A3 = new(a3);
        B3 = new(b3);
        C3 = new(c3);
    }


    public readonly Vector<float> A1;
    public readonly Vector<float> B1;
    public readonly Vector<float> C1;



    public readonly Vector<float> A2;
    public readonly Vector<float> B2;
    public readonly Vector<float> C2;



    public readonly Vector<float> A3;
    public readonly Vector<float> B3;
    public readonly Vector<float> C3;



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void IsInside(int startX, int startY, out Vector3Wide eN)
    {
        // Unsafe.SkipInit(out Vector<float> Row);
        Vector<float> Row = new();

        switch (Vector<float>.Count)
        {
            case 16:
                Row = Vector512.Create(15f, 14f, 13f, 12f, 11f, 10f, 9f, 8f, 7f, 6f, 5f, 4f, 3f, 2f, 1f, 0f).AsVector();
                break;
            
            case 8:
                Row = Vector256.Create(7f, 6f, 5f, 4f, 3f, 2f, 1f, 0f).AsVector();
                break;
            
            case 4:
                Row = Vector128.Create(3f, 2f, 1f, 0f).AsVector();
                break;
        }

        // Unsafe.SkipInit(out eN);
        Vector<float> startXV = new Vector<float>(startX) - Row;
        Vector<float> startYV = new Vector<float>(startY);

        if (Fma.IsSupported && Vector<float>.Count == 8)
        {
            eN.X = Fma.MultiplyAdd(A1.AsVector256(), startXV.AsVector256(), Fma.MultiplyAdd(B1.AsVector256(), startYV.AsVector256(), C1.AsVector256())).AsVector();
            eN.Y = Fma.MultiplyAdd(A2.AsVector256(), startXV.AsVector256(), Fma.MultiplyAdd(B2.AsVector256(), startYV.AsVector256(), C2.AsVector256())).AsVector();
            eN.Z = Fma.MultiplyAdd(A3.AsVector256(), startXV.AsVector256(), Fma.MultiplyAdd(B3.AsVector256(), startYV.AsVector256(), C3.AsVector256())).AsVector();
        }
        else if (Fma.IsSupported && Vector<float>.Count == 4)
        {
            eN.X = Fma.MultiplyAdd(A1.AsVector128(), startXV.AsVector128(), Fma.MultiplyAdd(B1.AsVector128(), startYV.AsVector128(), C1.AsVector128())).AsVector();
            eN.Y = Fma.MultiplyAdd(A2.AsVector128(), startXV.AsVector128(), Fma.MultiplyAdd(B2.AsVector128(), startYV.AsVector128(), C2.AsVector128())).AsVector();
            eN.Z = Fma.MultiplyAdd(A3.AsVector128(), startXV.AsVector128(), Fma.MultiplyAdd(B3.AsVector128(), startYV.AsVector128(), C3.AsVector128())).AsVector();
        }
        else
        {
            eN.X = A1 * startXV + B1 * startY + C1;
            eN.Y = A2 * startXV + B2 * startY + C2;
            eN.Z = A3 * startXV + B3 * startY + C3;
        }

        Vector<float> scalar = MathHelper.FastReciprocal(eN.X + eN.Y + eN.Z);

        Vector3Wide.Scale(in eN, in scalar, out eN);
    }



    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // public void UpdateEdges(in Vector4 a, in Vector4 b, in Vector4 c)
    // {
    //     float a1 = b.Y - c.Y;
    //     float b1 = c.X - b.X;
    //     float c1 = b.X * c.Y - c.X * b.Y;


    //     float a2 = c.Y - a.Y;
    //     float b2 = a.X - c.X;
    //     float c2 = c.X * a.Y - a.X * c.Y;


    //     float a3 = a.Y - b.Y;
    //     float b3 = b.X - a.X;
    //     float c3 = a.X * b.Y - b.X * a.Y;



    //     A1 = new(a1);
    //     B1 = new(b1);
    //     C1 = new(c1);

    //     A2 = new(a2);
    //     B2 = new(b2);
    //     C2 = new(c2);

    //     A3 = new(a3);
    //     B3 = new(b3);
    //     C3 = new(c3);
    // }
}




// public readonly ref struct EdgesBundled(in TriangleWide triangle)
// {
//     public static readonly Vector<float> Row = new(Enumerable.Range(0, Vector<float>.Count).Select(i => (float)i).Reverse().ToArray());



//     public readonly Vector<float> A1 = triangle.B.Y - triangle.C.Y;
//     public readonly Vector<float> B1 = triangle.C.X - triangle.B.X;
//     public readonly Vector<float> C1 = triangle.B.X * triangle.C.Y - triangle.C.X * triangle.B.Y;



//     public readonly Vector<float> A2 = triangle.C.Y - triangle.A.Y;
//     public readonly Vector<float> B2 = triangle.A.X - triangle.C.X;
//     public readonly Vector<float> C2 = triangle.C.X * triangle.A.Y - triangle.A.X * triangle.C.Y;



//     public readonly Vector<float> A3 = triangle.A.Y - triangle.B.Y;
//     public readonly Vector<float> B3 = triangle.B.X - triangle.A.X;
//     public readonly Vector<float> C3 = triangle.A.X * triangle.B.Y - triangle.B.X * triangle.A.Y;



//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public readonly void IsInside(in int startX, in int startY, out Vector3Wide eN, out Vector<float> magnitude)
//     {
//         //Vector<float> startXV = (Vector<float>.One * startX) - Row;
//         // var startYV = One * startY;


//         eN.X = A1 * startX + B1 * startY + C1;
//         eN.Y = A2 * startX + B2 * startY + C2;
//         eN.Z = A3 * startX + B3 * startY + C3;


//         magnitude = eN.X + eN.Y + eN.Z;
//         eN /= magnitude;
//     }
// }