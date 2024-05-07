using System.Numerics;
using System.Runtime.CompilerServices;


namespace Paprika;

public abstract class PixelRenderer<T> where T: IRenderer
{
    public Size2D FrameBufferSize
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => RenderOutput.FrameBufferSize;
    }
    public Camera MainCamera { get; private set; }
    public IRenderOutput<T> RenderOutput { get; private set; }


    public PixelRenderer(IRenderOutput<T> renderOutput)
    {
        RenderOutput = renderOutput;
        MainCamera = new Camera(0.1f, 100f, 60f, new(renderOutput.FrameBufferSize.Width, renderOutput.FrameBufferSize.Height), Vector3.Zero, Quaternion.Identity, true);
        viewportMatrix =
            Matrix4x4.CreateTranslation(1, 1, 0) *
            Matrix4x4.CreateScale(0.5f * FrameBufferSize.Width, 0.5f * FrameBufferSize.Height, 1);
    }


    internal Matrix4x4 viewportMatrix;
}
