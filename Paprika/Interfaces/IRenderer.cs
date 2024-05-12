namespace Paprika;

public interface IRenderer<TPixel>

where TPixel: unmanaged

{
    public RenderBuffer<TPixel> OutputBuffer { get; }
    public ICamera<TPixel> MainCamera { get; }
    public bool CanUploadGeometry { get; }
    public void RenderFrame();
}
