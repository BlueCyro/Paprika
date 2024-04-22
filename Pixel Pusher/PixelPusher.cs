
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;


namespace Paprika;

public partial class PixelPusher
{
    public Vector64<int> Size
    {
        get
        {
            return size;
        }

        private set
        {
            size = value;
            SizeFloat = Vector64.Create<float>([value[0], value[1]]);
            SizeVec2 = new(value[0], value[1]);
            SizeVec4 = new(value[0], value[1], value[0], value[1]);
            TotalLength = value[0] * value[1];
        }
    }

    public Vector64<float> SizeFloat { get; private set; }
    public Vector2 SizeVec2 { get; private set; }
    public Vector4 SizeVec4 { get; private set; }
    public int TotalLength { get; private set; }
    private Vector64<int> size;
    private ulong frame;

    public int[] PixelBuffer => bufferSwap ? pixelBuffer2 : pixelBuffer1;
    public Span<byte> PixelBufferBytes => MemoryMarshal.Cast<int, byte>(PixelBuffer.AsSpan());

    public event EventHandler<RenderEventArgs> OnUpdate;


    private GL gl;
    private Shader shader;
    private IWindow window;
    private Texture pixelView;
    private BufferObject<uint> elementBufferObject;
    private BufferObject<float> vertexBufferObject;
    private VertexArrayObject<float, uint> vertexArrayObject;



    private bool bufferSwap = false;
    private int[] pixelBuffer1;
    private int[] pixelBuffer2;
    public readonly float[] ZBuffer;



#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public PixelPusher(string name, Vector64<int> size)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        var opts = WindowOptions.Default;
        opts.Title = name;
        opts.Size = new Vector2D<int>(size[0], size[1]);
        // opts.VSync = false;
        window = Window.Create(opts);
        Size = size;

        pixelBuffer1 = new int[size[0] * size[1]];
        pixelBuffer2 = new int[pixelBuffer1.Length];
        ZBuffer = new float[pixelBuffer1.Length];

        window.Load += Bootstrap;
        window.Render += Render;
        window.Update += Update;
        window.FramebufferResize += s => {
            gl!.Viewport(s);
            // Size = Vector64.Create(s.X, s.Y);
            // ResizeBuffer();
        };
    }



    private void ResizeBuffer()
    {
        pixelBuffer1 = new int[Size[0] * Size[1]];
        pixelBuffer2 = new int[pixelBuffer1.Length];
    }



    public void StartRender()
    {
        window.Run();
        window.Dispose();
    }



    private void Bootstrap()
    {
        gl = window.CreateOpenGL();
        var col = System.Drawing.Color.BlueViolet;
        gl.ClearColor(col); // Clear to a specified color


        elementBufferObject = new(gl, ScreenQuad.Indices, BufferTargetARB.ElementArrayBuffer);
        vertexBufferObject = new(gl, ScreenQuad.Vertices, BufferTargetARB.ArrayBuffer);
        vertexArrayObject = new(gl, vertexBufferObject, elementBufferObject);

        
        vertexArrayObject.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 5, 0);
        vertexArrayObject.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 5, 3);


        shader = new(gl, Shader.DefaultVertex, Shader.DefaultFrag);
        pixelView = new(gl, PixelBufferBytes, (uint)Size[0], (uint)Size[1]);
    }



    private unsafe void Render(double delta)
    {
        vertexBufferObject.Bind();
        vertexArrayObject.Bind();
        pixelView.Bind();


        shader.Use();
        shader.SetUniform("uTexture", 0);
        gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, null);
    }



    private void Update(double delta)
    {
        Array.Clear(PixelBuffer);
        Array.Fill(ZBuffer, float.MaxValue);
        OnUpdate?.Invoke(this, new(delta, frame++));
        PushPixels();
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void SetPixelUnsafe(in int data, in float x, in float y)
    {
        fixed (int* ptr = PixelBuffer)
        {
            *(ptr + (int)x + (int)y * size[0]) = data;
        }
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetPixel(in int data, in int x, in int y)
    {
        SetPixel(data, x + y * size[0]);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetPixel(in int data, in int x, in int y, in float depth = float.MaxValue)
    {
        // int index = Math.Clamp(x + y * size[0], 0, PixelBuffer.Length - 1);
        int index = x + y * size[0];
        if (ZBuffer[index] <= depth)
        {
            SetPixelUnsafe(data, index);
            SetPixelZUnsafe(depth, index);
        }
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetPixel(in int data, in float x, in float y)
    {
        SetPixel(data, (int)x + (int)y * size[0]);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetPixel(in int data, in int index)
    {
        PixelBuffer[Math.Clamp(index, 0, PixelBuffer.Length - 1)] = data;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetPixelZ(in float data, in int index)
    {
        SetPixelZUnsafe(data, Math.Clamp(index, 0, ZBuffer.Length - 1));
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetPixelUnsafe(in int data, in int index)
    {
        PixelBuffer[index] = data;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetPixelZUnsafe(in float data, in int index)
    {
        ZBuffer[index] = data;
    }
    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // public void SetPixel(byte r, byte g, byte b, byte a, int x, int y)
    // {
    //     int col = r | (g << 8) | (b << 16) | (a << 24);

    //     SetPixel(col, x, y);
    // }



    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // public void SetPixel(int r, int g, int b, int a, int x, int y)
    // {
    //     SetPixel((byte)r, (byte)g, (byte)b, (byte)a, x, y);
    // }



    public void PushPixels()
    {
        pixelView.Update(PixelBufferBytes, (uint)Size[0], (uint)Size[1]);
        bufferSwap = !bufferSwap;
    }
}


public class RenderEventArgs(double delta, ulong frame) : EventArgs
{
    public double Delta = delta;
    public ulong Frame = frame;
}