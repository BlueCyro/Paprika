using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Paprika;

public readonly struct RenderBuffer<T> : IRenderBuffer, IDisposable where T: unmanaged
{
    public unsafe T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => *(Buffer.Pointer + index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => *(Buffer.Pointer + index) = value;
    }



    public RenderBuffer(int width, int height)
    {
        Size = new(width, height);
        Buffer = new(width * height);
    }



    public void Dispose()
    {
        Buffer.Dispose();
    }



    public readonly Size2D Size { get; init; }
    public readonly DumbBuffer<T> Buffer;
    public readonly Type BufferType => typeof(T);
}
