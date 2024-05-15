using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Paprika;

// [StructLayout(LayoutKind.Sequential, Pack = 16)]
public readonly struct RenderBuffer<T> : IRenderBuffer, IDisposable where T: unmanaged
{
    public RenderBuffer(int width, int height, int alignment = 32)
    {
        Size = new(width, height);
        Console.WriteLine($"Render buffer: Creating new internal buffer of length: {width * height}");
        Buffer = new(width * height, alignment);
    }



    public ref T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Buffer[index];
    }




    public void Dispose()
    {
        Buffer.Dispose();
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix4x4 GetViewportMatrix()
    {
        return Matrix4x4.CreateTranslation(1, 1, 0) *
            Matrix4x4.CreateScale(0.5f * Size.Width, 0.5f * Size.Height, 1);
        // return Matrix4x4.CreatePerspective(Size.Width, Size.Height, nearClip, farClip);

        // return new(
        //     Size.Width / 2f, 0, 0, Size.Width / 2f,
        //     0, -Size.Height / 2f, 0, Size.Height / 2f,
        //     0, 0, 1, 0,
        //     0, 0, 0, 1);
    }



    public readonly DumbBuffer<T> Buffer;
    public readonly Size2D Size { get; init; }
    public readonly Type BufferType => typeof(T);
}
