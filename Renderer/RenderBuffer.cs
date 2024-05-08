using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Paprika;

// [StructLayout(LayoutKind.Sequential, Pack = 16)]
public readonly struct RenderBuffer<T> : IRenderBuffer, IDisposable where T: unmanaged
{
    public ref T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Buffer[index];
    }



    public RenderBuffer(int width, int height, int alignment = 32)
    {
        Size = new(width, height);
        Buffer = new(width * height, alignment);
    }



    public void Dispose()
    {
        Buffer.Dispose();
    }



    public readonly DumbBuffer<T> Buffer;
    public readonly Size2D Size { get; init; }
    public readonly Type BufferType => typeof(T);
}
