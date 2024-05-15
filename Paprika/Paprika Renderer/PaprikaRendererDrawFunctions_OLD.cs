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