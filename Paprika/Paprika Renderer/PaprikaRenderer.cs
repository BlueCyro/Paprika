// using System.Numerics;
// using System.Runtime.CompilerServices;
// using System.Runtime.Intrinsics;
// using BepuUtilities;
// using bottlenoselabs.C2CS.Runtime;
// using Tracy;


// namespace Paprika;


// public partial class PaprikaRenderer_OLD
// {
//     const int red = 255 << 24;
//     private DumbBuffer<TriangleWide> geometry;

//     public void DumpUploadGeometry(DumbBuffer<TriangleWide> buffer)
//     {
//         geometry = buffer;
//     }

//     // private void ResizeBuffer()
//     // {
//     //     pixelBuffer1 = new int[FrameBufferSize.Length1D];
//     //     pixelBuffer2 = new int[FrameBufferSize.Length1D];
//     // }




//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public void SetPixel(in int data, in int x, in int y)
//     {
//         SetPixel(data, x + y * FrameBufferSize.Width);
//     }



//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public void SetPixel(in int data, in float x, in float y)
//     {
//         SetPixel(data, (int)x + (int)y * FrameBufferSize.Width);
//     }



//     [MethodImpl(MethodImplOptions.AggressiveInlining)]
//     public void SetPixel(in int data, in int index)
//     {
//         RenderOutput.PixelBuffer.Buffer.Span[Math.Clamp(index, 0, FrameBufferSize.Length1D - 1)] = data;
//     }



//     public void RenderFrame()
//     {
//         // var profileScope = Profiler.BeginEvent("Wide projection", colorType: Profiler.ColorType.NavyBlue);
//         // var triWideScope = Profiler.BeginEvent("TriWideLoop", colorType: Profiler.ColorType.NavyBlue);
//         // DumbBuffer<TriangleWide> triArray = Program.WideUploaded;
//         // TriangleWide[] triArray = Program.WideUploaded;

        
//         Matrix4x4 mvpMatrix = MainCamera.ViewProjectionMatrix * viewportMatrix; // Camera view projection -> viewport
//         Matrix4x4Wide.Broadcast(mvpMatrix, out Matrix4x4Wide mvpMatrixWide); // Broadcast to wide matrix for triangle transformation
//         Vector3Wide wideCameraPos = Vector3Wide.Broadcast(MainCamera.Position); // Broadcast to wide camera pos

//         Int4Wide.Broadcast(Vector128.Create(FrameBufferSize.Width, FrameBufferSize.Height - 1, FrameBufferSize.Width, FrameBufferSize.Height - 1), out Int4Wide frameBounds);
//         // Vector4Wide zero = new();
//         Int4Wide zero = new();
//         // EdgesVectorized edges = new();
//         DumbBuffer<int> colBuf = RenderOutput.PixelBuffer.Buffer;
//         DumbBuffer<float> zBuf = RenderOutput.ZBuffer.Buffer;

//         // float dot;

        

//         for (int i = 0; i < geometry.Length; i++)
//         {
//             // var triBatchProject = Profiler.BeginEvent("Triangle batch screen project", Profiler.ColorType.Orange);

//             // TriangleWide curTri = geometry[i];

//             TriangleWide.GetCenter(in geometry[i], out Vector3Wide center);
//             TriangleWide.GetNormal(in geometry[i], out Vector3Wide normal);

//             Vector3Wide.Subtract(in wideCameraPos, in center, out Vector3Wide diff);
//             Vector3Wide normalized = Vector3Wide.Normalize(diff);
//             Vector3Wide.Dot(in normal, in normalized, out Vector<float> dots);

//             if (Vector.LessThanOrEqualAll(dots, Vector<float>.Zero))
//                 continue;

//             TriangleWide.Transform(in geometry[i], in mvpMatrixWide, out TriangleWide transformed);
//             TriangleWide.ZDivide(in transformed, out Vector3Wide oldZWide, out transformed);
//             // TriangleWide.GetBounds(transformed, out Vector3Wide bboxMinWide, out Vector3Wide bboxMaxWide);
//             TriangleWide.Get2DBounds(in transformed, out Int4Wide bbox);
            

//             // Console.WriteLine($"V4 bbox: {result}");
//             // bool bboxCheck = 
//             //     Vector.GreaterThanOrEqualAll(bbox.X, zero.X) &&
//             //     Vector.GreaterThanOrEqualAll(bbox.Y, zero.Y) &&
//             //     Vector.LessThanOrEqualAll(bbox.Z, frameBounds.Z) &&
//             //     Vector.LessThanOrEqualAll(bbox.W, frameBounds.W);


//             // if (!bboxCheck)
//             //     continue;

//             // Vector4Wide.Scale(bbox, vecWidthRecip, out bbox);
//             // VectorHelpers.AlignWidth(bbox, out bbox);
//             // Vector4Wide.Scale(bbox, vecWidth, out bbox);
//             // VectorHelpers.VectorAlignWidth(bbox, out bbox);


//             Int4Wide.Clamp(in bbox, in zero, in frameBounds, out Int4Wide result);
//             // Int4Wide.Max(bbox, zero, out Int4Wide result);
//             // Int4Wide.Min(result, frameBounds, out result);
//             // bboxMinWide.X = Vector.Min(Vector.Max(Vector<float>.Zero, bboxMinWide.X), wideFrameWidth);
//             // bboxMinWide.Y = Vector.Min(Vector.Max(Vector<float>.Zero, bboxMinWide.Y), wideFrameHeight);
//             // bboxMaxWide.X = Vector.Min(Vector.Max(Vector<float>.Zero, bboxMaxWide.X), wideFrameWidth);
//             // bboxMaxWide.Y = Vector.Min(Vector.Max(Vector<float>.Zero, bboxMaxWide.Y), wideFrameHeight);

            
//             // triBatchProject.Dispose();


//             for (int j = 0; j < Vector<float>.Count; j++)
//             {
//                 // var triNarrowProcess = Profiler.BeginEvent("Narrow triangle extract & process");
//                 // if (j != 7 || i != 0)
//                 //     continue;

//                 // dot = dots[j];

//                 // if (dot <= 0f)
//                 //     continue;

//                 TriangleWide.ReadSlot(ref transformed, j, out Triangle narrowTri);
//                 // Vector3 vec = Vector3.One * dot;
//                 // QuickColor.PackedFromVector3(vec, out int color);
//                 Int4Wide.ReadSlot(ref result, j, out Vector128<int> bboxNarrow);
//                 Vector3Wide.ReadSlot(ref oldZWide, j, out Vector3 oldZ);

                
//                 DrawPinedaTriangleSIMD(in narrowTri, 255, in colBuf, in zBuf, in bboxNarrow, in oldZ/*, ref edges*/);
//                 // triNarrowProcess.Dispose();
//             }

//         }
//         // triWideScope.Dispose();
//         // Thread.Sleep(500);
//     }
// }