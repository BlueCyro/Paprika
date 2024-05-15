using System.Runtime.InteropServices;
using Paprika;

namespace PaprikaBenchmarks;

public class BenchmarkOutput : IRenderOutput<PaprikaRenderer, int>
{
    public ref RenderBuffer<int> PixelBuffer => ref bufferSwap ? ref pixelBuffer1 : ref pixelBuffer2;
    RenderBuffer<int> pixelBuffer1;
    RenderBuffer<int> pixelBuffer2;
    


    public Size2D Resolution { get; private set; }



    public ref PaprikaRenderer CurrentRenderer => ref currentRenderer;
    PaprikaRenderer currentRenderer;



    private bool bufferSwap = false;


    
    public PaprikaCamera MainCamera;



    public BenchmarkOutput(int width = 1280, int height = 1024, int alignment = 32)
    {
        Console.WriteLine($"Setting up bench for: {width}, {height}");
        pixelBuffer1 = new(width, height, alignment);
        pixelBuffer2 = new(width, height, alignment);

        currentRenderer.ValidateBuffers(pixelBuffer1, alignment);
    }



    public void StartRender() => throw new NotImplementedException("Stub!");



    public void Update()
    {
        CurrentRenderer.RenderFrame(PixelBuffer, MainCamera);
        bufferSwap = !bufferSwap;
    }


    public void ResolutionResize()
    {

    }
}