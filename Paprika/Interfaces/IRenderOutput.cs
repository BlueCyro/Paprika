using System.Runtime.CompilerServices;

namespace Paprika;

public interface IRenderOutput<TRenderer, TPixel>

where TRenderer: struct, IRenderer<TPixel>
where TPixel: unmanaged

{
    public Size2D Resolution { get; }
    public RenderBuffer<TPixel> PixelBuffer { get; }
    public TRenderer CurrentRenderer { get; }
    public void Update();
    public void ResolutionResize();
}
