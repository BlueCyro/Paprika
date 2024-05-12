// using System.Numerics;
// using System.Runtime.CompilerServices;
// using System.Runtime.Intrinsics;
// using BepuUtilities;


// namespace Paprika;


// public partial class PaprikaRenderer_OLD
// {
//     public static readonly Vector<int> FullByte = new(255);
//     public static readonly Vector<int> zeroInt = new();
//     public const int AllBits = unchecked((int)uint.MaxValue);
//     public static readonly Vector3 Red = new(1f, 0f, 0f);
//     public static readonly Vector3 Green = new(0f, 1f, 0f);
//     public static readonly Vector3 Blue = new(0f, 0f, 1f);




//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public void DrawPinedaTriangleSIMD(
//         in Triangle tri,
//         int col,
//         in DumbBuffer<int> bufStart,
//         in DumbBuffer<float> zBufStart,
//         in Vector128<int> bbox,
//         in Vector3 oldZ/*,
//         ref EdgesVectorized edges*/)
//     {
//         Vector<int> wideCol = new(col);
//         EdgesVectorized edges = new(tri.A, tri.B, tri.C);
//         // edges.UpdateEdges(tri.A, tri.B, tri.C);

//         int minX = bbox[0];
//         int minY = bbox[1];
//         int maxX = bbox[2];
//         int maxY = bbox[3];


//         int frameWidth = FrameBufferSize.Width;
//         for (int y = maxY; y > minY; y--)
//         {
//             int row = y * frameWidth;
//             for (int x = maxX; x > minX; x -= Vector<int>.Count)
//             {
//                 int vecStart = x - Vector<int>.Count + row;
//                 // int vecFloatStart = x - Vector<float>.Count + row;

//                 ref int colStart = ref bufStart[vecStart];
//                 ref float zStart = ref zBufStart[vecStart];


//                 Vector<int> bufVec = Vector.LoadUnsafe(ref colStart);
//                 Vector<float> bufVecZ = Vector.LoadUnsafe(ref zStart);


//                 edges.IsInside(x, y, out Vector3Wide eN);


//                 RasterHelpers.InterpolateBarycentric(oldZ.X, oldZ.Y, oldZ.Z, in eN, out Vector<float> bigDepth);
//                 Vector<int> depthMask = Vector.LessThanOrEqual(bigDepth, bufVecZ);


//                 Vector<int> mask =
//                     Vector.GreaterThanOrEqual(eN.X, Vector<float>.Zero) &
//                     Vector.GreaterThanOrEqual(eN.Y, Vector<float>.Zero) &
//                     Vector.GreaterThanOrEqual(eN.Z, Vector<float>.Zero);

//                 Vector<int> colResult = Vector.ConditionalSelect(mask & depthMask, wideCol, bufVec);
//                 Vector<float> zResult = Vector.ConditionalSelect(mask & depthMask, bigDepth, bufVecZ);

//                 colResult.StoreUnsafe(ref colStart);
//                 zResult.StoreUnsafe(ref zStart);
//             }
//         }
//     }

//     public void FillRect(in Vector4 bbox)
//     {
//         for (int y = (int)bbox.W; y > bbox.Y; y--)
//         {
//             for (int x = (int)bbox.Z - 1; x > bbox.X; x--)
//             {
//                 SetPixel(x / Vector<int>.Count % 2 == 0 ? new QuickColor(128, 0, 0, 255).RGBA : new QuickColor(0, 128, 0, 255).RGBA, x, y);
//             }
//         }
//     }
// }



// // This is just a blatant clone of Bepu's version of this.
// // In theory, this should help the compiler inline better according to mister John Bepu (Ross)
// public interface IForLoop<T>
// {
//     void LoopBody(T iteration);
// }



// // public struct TriBoundsRowIterator : IForLoop<int>
// // {
// //     int row;
// //     TriBoundsColumnIterator columnIterator;



// //     public void LoopBody(int iteration)
// //     {

// //     }



// //     public void Reset()
// //     {
        
// //     }
// // }



// // public struct TriBoundsColumnIterator : IForLoop<int>
// // {

// //     public void LoopBody(int iteration)
// //     {

// //     }
// // }