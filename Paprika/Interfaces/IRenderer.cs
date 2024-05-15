namespace Paprika;

public interface IRenderer<TPixel>

where TPixel: unmanaged
{
    public void RenderFrame<TCamera>(in RenderBuffer<TPixel> buffer, in TCamera mainCam) where TCamera: struct, ICamera<TPixel>;
}
