using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


namespace Paprika;

// [StructLayout(LayoutKind.Sequential, Pack = 32)]
public readonly unsafe struct DumbBuffer<T> : IDisposable where T: unmanaged
{
    public readonly int Start
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        init;
    }


    public readonly int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        init;
    }



    public readonly nuint LengthBytes
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        init;
    }


    // public readonly IntPtr StartAddress
    // {
    //     [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //     get;
    //     init;
    // }


    public readonly T* Pointer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        init;
    }


    public readonly Span<T> Span
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(Pointer, Length);
    }


    public readonly T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => *(Pointer + index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => *(Pointer + index) = value;
    }



    public DumbBuffer(int count)
    {
        Length = count;
        LengthBytes = (nuint)(count * sizeof(T));
        nuint alignment = (nuint)Vector<byte>.Count;
        // nuint alignment = 32;
        // if (typeof(T).IsAssignableFrom(typeof(ValueType)))
        //     alignment = (nuint)Vector<T>.Count;
        // else
        //     alignment = (nuint)Vector<float>.Count;

        Pointer = (T*)NativeMemory.AlignedAlloc(LengthBytes, alignment);

        // StartAddress = IntPtr.
    }



    public void Dispose()
    {
        GC.SuppressFinalize(this);
        NativeMemory.AlignedFree(Pointer);
        // Marshal.FreeHGlobal(StartAddress);
    }


    
    public void Clear()
    {
        NativeMemory.Clear(Pointer, LengthBytes);
    }



    public void Fill(T value)
    {
        new Span<T>(Pointer, Length).Fill(value);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator T*(DumbBuffer<T> buffer) => buffer.Pointer;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T* operator +(DumbBuffer<T> left, int right) => left.Pointer + right;
}