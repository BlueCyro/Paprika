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
        // int pack = typeof(T).StructLayoutAttribute?.Pack ?? 0;

        // int alignment = pack > 0 ? pack : 16; // I need to research memory alignment better, will test this on other systems
        // int alignedByteSize = (int)(Math.Ceiling(sizeof(T) / (double)alignment) * alignment);
        int alignment = typeof(T).IsPrimitive ? sizeof(T) : 256;
        
        int byteLen = sizeof(T) * Length;
        // LengthBytes = (nuint)(alignedByteSize * Length);
        LengthBytes = (nuint)byteLen;
        // LengthBytes = (nuint)(Math.Ceiling((float)byteLen / Vector<T>.) * alignment);
        Console.WriteLine($"Instantiating {typeof(T)} buffer with length of: {Length}, (Type size: {sizeof(T)}), (Bytes: {byteLen}), (Real: {LengthBytes})");

        // nuint alignment = 64;
        // nuint alignment = (nuint)Math.Pow(2, Math.Round(Math.Log(Unsafe.SizeOf<T>()) / Math.Log(2)));


        Pointer = (T*)NativeMemory.AlignedAlloc(LengthBytes, (nuint)alignment);
        IntPtr pointerView = new IntPtr(Pointer);
        Console.WriteLine($"Aligning on: {pointerView} (Hex: {pointerView:x}) ({alignment}, {pointerView % alignment})");
    }



    public void Dispose()
    {
        GC.SuppressFinalize(this);
        NativeMemory.AlignedFree(Pointer);
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