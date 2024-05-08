using System.Runtime.InteropServices;
using Paprika;

namespace PaprikaBenchmarks;

public class BenchmarkOutput<T> : IRenderOutput<T> where T: IRenderer
{
    public RenderBuffer<int> PixelBuffer => bufferSwap ? pixelBuffer2 : pixelBuffer1;
    public Span<byte> PixelBufferBytes => MemoryMarshal.Cast<int, byte>(PixelBuffer.Buffer.Span);
    public RenderBuffer<float> ZBuffer { get; private set; }
    public RenderBuffer<int> pixelBuffer1;
    public RenderBuffer<int> pixelBuffer2;
    
    
    public Size2D FrameBufferSize { get; private set; }
    public T CurrentRenderer { get; private set; }
    public IRenderer CurrentIRenderer => CurrentRenderer;


    private bool bufferSwap = false;



    public BenchmarkOutput(int width = 1280, int height = 1024, int alignment = 32)
    {
        Console.WriteLine($"Setting up bench for: {width}, {height}");
        pixelBuffer1 = new(width, height, alignment);
        pixelBuffer2 = new(width, height, alignment);
        ZBuffer = new(width, height, alignment);

        CurrentRenderer = (T?)Activator.CreateInstance(typeof(T), [ this ]) ?? throw new Exception($"Failed to initialize instance of {typeof(T)}!");
    }



    public void StartRender() => throw new NotImplementedException("Stub!");



    public void Update()
    {
        PixelBuffer.Buffer.Clear();
        ZBuffer.Buffer.Fill(float.MaxValue);

        CurrentRenderer.RenderFrame();
        bufferSwap = !bufferSwap;
    }
}