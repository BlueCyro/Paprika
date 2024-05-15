using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using bottlenoselabs.C2CS.Runtime;
using Tracy;


namespace Paprika;


public class GLOutput : IRenderOutput<PaprikaRenderer, int>
{



    #region RenderOutput specific

    public PaprikaCamera MainCamera;
    public Size2D Resolution { get; private set; }
    public ref PaprikaRenderer CurrentRenderer
    {
        get => ref currentRenderer;
    }
    private PaprikaRenderer currentRenderer;


    
    public ref RenderBuffer<int> PixelBuffer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref (bufferSwap ? ref pixelBuffer1 : ref pixelBuffer2);
    }
    private RenderBuffer<int> pixelBuffer1;
    private RenderBuffer<int> pixelBuffer2;
    private bool bufferSwap = false;

    private int align;
    
    #endregion
    



    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public GLOutput(int width, int height, int alignment = 32)
    #pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        align = alignment;
        Resolution = new(width, height);
        // window.FramebufferResize += s => {
        //     gl!.Viewport(s);
        //     // Size = Vector64.Create(s.X, s.Y);
        //     // ResizeBuffer();
        // };


        pixelBuffer1 = new(width, height, align);
        pixelBuffer2 = new(width, height, align);
        currentRenderer.ValidateBuffers(pixelBuffer1, align);

        Console.WriteLine($"GLOutput constructor pixel buffer 1 length: {pixelBuffer1.Buffer.Length}");
    }



    public void ResolutionResize()
    {

    }


    public void Update()
    {
        // PixelBuffer.Buffer.Clear();
        CurrentRenderer.RenderFrame(in PixelBuffer, in MainCamera);
        bufferSwap = !bufferSwap;
    }



    // double avg;
    // ulong frame;
    // int renderedTris;
    // public void Update()
    // {
    //     // Program.DEBUG_ReAllocateDumbBuffer();


    //     // Program.Timer.Start();
    //     // CString render = (CString)"RenderFrame";
    //     // PInvoke.TracyAllocSrcloc(175, (CString)"Update", )
    //     // PInvoke.TracyEmitZoneBegin();
    //     // PInvoke.TracyEmitFrameMarkStart(render);
    //     // CurrentRenderer.RenderFrame(in PixelBuffer, in MainCamera);
    //     // PInvoke.TracyEmitFrameMarkEnd(render);
    //     // Console.WriteLine("HUH");
    //     // render.Dispose();
        
    //     // Program.Timer.Stop();

    //     // avg += Program.Timer.ElapsedMilliseconds;
    //     // ulong frameCount = 20;
    //     // if (++frame % frameCount == 0)
    //     // {
    //     //     Console.WriteLine($"[Paprika] Took (avg): {avg / frameCount:F3}ms, Frame: {frame}, Tris: {renderedTris}");
    //     //     // Console.WriteLine($"Camera pos: {MainCamera.Position}, Camera rot {MainCamera.Rotation}");
    //     //     avg = 0;
    //     // }

    //     // PushPixels();
    //     // Program.Timer.Reset();
    //     // Profiler.ProfileFrame("Update");
    // }
}
