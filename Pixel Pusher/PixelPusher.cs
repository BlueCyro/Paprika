using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;


namespace Paprika;

using static VectorHelpers;

public partial class PixelPusher
{
    public Size2D FrameBufferSize;

    private ulong frame;

    public RenderBuffer<int> PixelBuffer => bufferSwap ? pixelBuffer2 : pixelBuffer1;
    public Span<byte> PixelBufferBytes => MemoryMarshal.Cast<int, byte>(PixelBuffer.Buffer.AsSpan());

    // public event EventHandler<RenderEventArgs> OnUpdate;


    private GL gl;
    private Shader shader;
    private IWindow window;
    private IInputContext input;
    private Texture pixelView;
    private BufferObject<uint> elementBufferObject;
    private BufferObject<float> vertexBufferObject;
    private VertexArrayObject<float, uint> vertexArrayObject;

    public readonly Camera MainCamera;
    public readonly Matrix4x4 ViewportMatrix;
    private double avg;



    private bool bufferSwap = false;
    private RenderBuffer<int> pixelBuffer1;
    private RenderBuffer<int> pixelBuffer2;
    public readonly RenderBuffer<float> ZBuffer;



#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public PixelPusher(string name, int width, int height)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        FrameBufferSize = new(width, height);
        var opts = WindowOptions.Default;
        opts.Title = name;
        opts.Size = FrameBufferSize;
        window = Window.Create(opts);

        MainCamera = new Camera(0.1f, 100f, 60f, new(width, height), Vector3.Zero, Quaternion.Identity, true);
        ViewportMatrix =
            Matrix4x4.CreateTranslation(1, 1, 0) *
            Matrix4x4.CreateScale(0.5f * FrameBufferSize.Width, 0.5f * FrameBufferSize.Height, 1);


        pixelBuffer1 = new(width, height);
        pixelBuffer2 = new(width, height);
        ZBuffer = new(width, height);


        window.Load += Bootstrap;
        window.Render += Render;
        window.Update += Update;
        window.FramebufferResize += s => {
            gl!.Viewport(s);
            // Size = Vector64.Create(s.X, s.Y);
            // ResizeBuffer();
        };
    }



    // private void ResizeBuffer()
    // {
    //     pixelBuffer1 = new int[FrameBufferSize.Length1D];
    //     pixelBuffer2 = new int[FrameBufferSize.Length1D];
    // }



    public void StartRender()
    {
        window.Run();
        window.Dispose();
    }



    private void Bootstrap()
    {
        gl = window.CreateOpenGL();
        input = window.CreateInput();

        input.Mice[0].MouseMove += MouseMove;

        var col = System.Drawing.Color.BlueViolet;
        gl.ClearColor(col); // Clear to a specified color


        elementBufferObject = new(gl, ScreenQuad.Indices, BufferTargetARB.ElementArrayBuffer);
        vertexBufferObject = new(gl, ScreenQuad.Vertices, BufferTargetARB.ArrayBuffer);
        vertexArrayObject = new(gl, vertexBufferObject, elementBufferObject);

        
        vertexArrayObject.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 5, 0);
        vertexArrayObject.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 5, 3);


        shader = new(gl, Shader.DefaultVertex, Shader.DefaultFrag);
        pixelView = new(gl, PixelBufferBytes, FrameBufferSize.UWidth, FrameBufferSize.UHeight);
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



    public void MouseMove(IMouse mouse, Vector2 delta)
    {
        MainCamera.Rotation = Quaternion.CreateFromYawPitchRoll(-delta.X / FrameBufferSize.WidthSingle * MathF.Tau, delta.Y / FrameBufferSize.HeightSingle * MathF.PI - (MathF.PI / 2f), 0f);
    }



    private void Update(double delta)
    {
        var board = input.Keyboards[0];
        float shiftMod = 1f;

        if (board.IsKeyPressed(Key.ShiftLeft))
            shiftMod = 5f;
        
        if (board.IsKeyPressed(Key.Space))
            MainCamera.Position += MainCamera.Up * shiftMod * (float)delta;
        
        if (board.IsKeyPressed(Key.E))
            MainCamera.Position += -MainCamera.Up * shiftMod * (float)delta;
        

        if (board.IsKeyPressed(Key.W))
            MainCamera.Position += MainCamera.Forward * shiftMod * (float)delta;
        
        if (board.IsKeyPressed(Key.S))
            MainCamera.Position += -MainCamera.Forward * shiftMod * (float)delta;

        if (board.IsKeyPressed(Key.A))
            MainCamera.Position += MainCamera.Right * shiftMod * (float)delta;

        if (board.IsKeyPressed(Key.D))
            MainCamera.Position += -MainCamera.Right * shiftMod * (float)delta;

        

        Array.Clear(PixelBuffer);
        Array.Fill(ZBuffer, float.MaxValue);
        DoRender(frame++);
        PushPixels();
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetPixel(in int data, in int x, in int y)
    {
        SetPixel(data, x + y * FrameBufferSize.Width);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetPixel(in int data, in int x, in int y, in float depth = float.MaxValue)
    {
        // int index = Math.Clamp(x + y * size[0], 0, PixelBuffer.Length - 1);
        int index = x + y * FrameBufferSize.Width;
        if (ZBuffer[index] <= depth)
        {
            SetPixelUnsafe(data, index);
            SetPixelZUnsafe(depth, index);
        }
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetPixel(in int data, in float x, in float y)
    {
        SetPixel(data, (int)x + (int)y * FrameBufferSize.Width);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetPixel(in int data, in int index)
    {
        PixelBuffer.Buffer[Math.Clamp(index, 0, FrameBufferSize.Length1D - 1)] = data;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetPixelZ(in float data, in int index)
    {
        SetPixelZUnsafe(data, Math.Clamp(index, 0, ZBuffer.Buffer.Length - 1));
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetPixelUnsafe(in int data, in int index)
    {
        PixelBuffer.Buffer[index] = data;
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
        pixelView.Update(PixelBufferBytes, FrameBufferSize.UWidth, FrameBufferSize.UHeight);
        bufferSwap = !bufferSwap;
    }



    public void DoRender(ulong frame)
    {
        var pusher = this;   
        Stopwatch timer = Program.Timer;
        timer.Start();

        Triangle[] triArray = Program.Uploaded;
        
        Span<int> pixelBuf = pusher.PixelBuffer.Buffer.AsSpan();
        Span<float> zBuf = pusher.ZBuffer.Buffer.AsSpan();
        

        Matrix4x4 mvpMatrix = pusher.MainCamera.ViewProjectionMatrix * pusher.ViewportMatrix;


        for (int i = 0; i < triArray.Length; i++)
        {
            Triangle curTri = triArray[i];
            
            Vector3 diff = pusher.MainCamera.Position - curTri.Center;
            

            float dot = Vector3.Dot(curTri.Normal, Vector3.Normalize(diff));
            bool condition = dot >= 0f;
            

            // if (i == 256)
            if (!condition)
                continue;
            


            curTri.Transform(mvpMatrix);

            // curTri.PerspectiveProject(out var _);



            // pusher.DrawBounds(tri, defaultCol);
            var white = (new QuickColor(Program.firstCol) * dot).RGBA;
            pusher.DrawPinedaTriangleSIMD(ref curTri, white, pixelBuf, zBuf);
            // pusher.DrawBounds(curTri, firstCol);
        }

        timer.Stop();
        avg += timer.Elapsed.TotalMilliseconds;
        ulong frameCount = 20;
        if (frame % frameCount == 0)
        {
            // Console.WriteLine(timer.Elapsed.TotalMilliseconds);
            Console.WriteLine($"[Paprika] Took (avg): {avg / frameCount:F3}ms, Frame: {frame}");
            avg = 0;
        }
        // Console.WriteLine(timer.Elapsed.TotalMilliseconds);
        timer.Reset();
    }
}


public class RenderEventArgs(double delta, ulong frame) : EventArgs
{
    public double Delta = delta;
    public ulong Frame = frame;
}