namespace Paprika;

public interface IRenderOutput<T> where T: IRenderer
{
    public RenderBuffer<int> PixelBuffer { get; }
    public Span<byte> PixelBufferBytes { get; }
    public RenderBuffer<float> ZBuffer { get; }
    public Size2D FrameBufferSize { get; }
    public T CurrentRenderer { get; }
    public IRenderer CurrentIRenderer { get; }
    public void StartRender();
}
