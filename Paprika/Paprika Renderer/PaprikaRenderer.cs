using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using BepuUtilities;

namespace Paprika;



public struct PaprikaRenderer : IRenderer<int>
{
    DumbBuffer<float> ZBuffer;


    public void RenderFrame<TCamera>(in RenderBuffer<int> renderBuffer, in TCamera mainCam) where TCamera: struct, ICamera<int>
    {
        // ValidateBuffers(in renderBuffer);

        renderBuffer.Buffer.Clear();
        ZBuffer.Fill(float.MaxValue);
        
        DumbBuffer<TriangleWide> geo = GeometryHolder<TriangleWide>.Geometry;
        Matrix4x4 projectionMatrix = CameraHelpers.GetPerspectiveProjection<TCamera, int>(mainCam, renderBuffer.Size);
        
        Matrix4x4 viewportMatrix = renderBuffer.GetViewportMatrix();

        Matrix4x4Wide wideScreenProject = Matrix4x4Wide.Broadcast(mainCam.ViewMatrix * projectionMatrix * viewportMatrix);


        Int4Wide bounds = Int4Wide.Broadcast(renderBuffer.Size.Width, renderBuffer.Size.Height - 1, renderBuffer.Size.Width, renderBuffer.Size.Height - 1);
        Vector3Wide wideCamPos = Vector3Wide.Broadcast(mainCam.Position);



        for (int i = 0; i < geo.Length; i++)
        {
            bool cull = ShouldBackfaceCull(geo[i], wideCamPos, out Vector<float> wideDot);
            if (cull)
                 continue;

            // ShouldBackfaceCull(geo[i], wideCamPos, out Vector<float> wideDot);

            
            TriangleWide projected = ProjectTriangleWide(geo[i], wideScreenProject, bounds, out Int4Wide wideBounds, out Vector3Wide oldZWide);


            for (int j = 0; j < Vector<float>.Count; j++)
            {
                TriangleWide.ReadSlot(ref projected, j, out Triangle narrowTri);
                Int4Wide.ReadSlot(ref wideBounds, j, out Vector128<int> narrowBounds);
                Vector3Wide.ReadSlot(ref oldZWide, j, out Vector3 narrowZ);
                

                QuickColor.PackedFromVector3(Vector3.One * wideDot[j], out int color);
                narrowTri.DrawSIMD(ref renderBuffer.Buffer[0], ref ZBuffer[0], narrowBounds, narrowZ, renderBuffer.Size, color);
            }
        }

    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TriangleWide ProjectTriangleWide(
        in TriangleWide triWide,
        in Matrix4x4Wide wideScreenProject,
        in Int4Wide frameBounds,
        out Int4Wide bounds,
        out Vector3Wide oldZWide)
    {
        TriangleWide transformed = TriangleWide.Transform(triWide, wideScreenProject);

        transformed = TriangleWide.ZDivide(transformed, out oldZWide);

        bounds = Int4Wide.Clamp(TriangleWide.Get2DBounds(transformed), Int4Wide.Zero, frameBounds);

        return transformed;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ShouldBackfaceCull(in TriangleWide triWide, in Vector3Wide wideCamPos, out Vector<float> wideDot)
    {
        TriangleWide.GetCenter(triWide, out Vector3Wide center);
        TriangleWide.GetNormal(triWide, out Vector3Wide normal);

        wideDot = Vector3Wide.Dot(Vector3Wide.Normalize(wideCamPos - center), normal);

        return Vector.LessThanOrEqualAll(
            wideDot,
            Vector<float>.Zero);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ValidateBuffers(in RenderBuffer<int> renderBuffer, int alignment = 32)
    {
        // Console.WriteLine($"Validating buffer: Input length is {renderBuffer.Buffer.Length}, ZBuffer length is {ZBuffer.Length}");
        if (ZBuffer.Length != renderBuffer.Buffer.Length)
            ZBuffer = new(renderBuffer.Buffer.Length, alignment);
    }
}