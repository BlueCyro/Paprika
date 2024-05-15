using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;


namespace Paprika;

public class SilkWindow
{
    #region Silk.NET OpenGL
    
    private GL gl;
    private Shader shader;
    private IWindow window;
    private IInputContext input;
    private Texture screen;
    private BufferObject<uint> elementBufferObject;
    private BufferObject<float> vertexBufferObject;
    private VertexArrayObject<float, uint> vertexArrayObject;

    #endregion

    public GLOutput? Output;
    public Size2D Resolution;


    double avg;

    Task counter;

    int frames;



    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public SilkWindow(string name, int width, int height)
    #pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        Resolution = new(width, height);
        var opts = WindowOptions.Default;
        opts.Title = name;
        opts.Size = Resolution;
        window = Window.Create(opts);


        window.Load += Bootstrap;
        window.Render += Render;
        window.Update += Update;
    }



    private void Bootstrap()
    {
        // Console.WriteLine($"Bootstrap function, reading buffer length as: {pixelBuffer1.Buffer.Length}");
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

        Span<byte> empty = new byte[Resolution.Length1D * Unsafe.SizeOf<int>()];
        screen = new(gl, empty, Resolution.UWidth, Resolution.UHeight);
    }



    private unsafe void Render(double delta)
    {
        vertexBufferObject.Bind();
        vertexArrayObject.Bind();
        screen.Bind();


        shader.Use();
        shader.SetUniform("uTexture", 0);
        gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, null);
    }



    public void Update(double delta)
    {
        if (Output != null)
        {
            var board = input.Keyboards[0];
            float shiftMod = 1f;

            if (board.IsKeyPressed(Key.ShiftLeft))
                shiftMod = 5f;
            
            if (board.IsKeyPressed(Key.Space))
                Output.MainCamera.Position += Output.MainCamera.Up * shiftMod * (float)delta;
            
            if (board.IsKeyPressed(Key.E))
                Output.MainCamera.Position += -Output.MainCamera.Up * shiftMod * (float)delta;
            

            if (board.IsKeyPressed(Key.W))
                Output.MainCamera.Position += Output.MainCamera.Forward * shiftMod * (float)delta;
            
            if (board.IsKeyPressed(Key.S))
                Output.MainCamera.Position += -Output.MainCamera.Forward * shiftMod * (float)delta;

            if (board.IsKeyPressed(Key.A))
                Output.MainCamera.Position += Output.MainCamera.Right * shiftMod * (float)delta;

            if (board.IsKeyPressed(Key.D))
                Output.MainCamera.Position += -Output.MainCamera.Right * shiftMod * (float)delta;
        }

        
        PushPixels();
    }



    public void PushPixels()
    {
        if (Output != null)
        {
            long then = Stopwatch.GetTimestamp();
            Output.Update();
            long now = Stopwatch.GetTimestamp();
            avg += (now - then) / (double)Stopwatch.Frequency;

            if (frames == 20)
            {
                Console.WriteLine($"Took: {(avg / 20) * 1000:f3}ms");
                avg = 0f;
                frames = 0;
            }

            frames++;
            screen.Update(Output.PixelBuffer.Buffer.Span, Resolution.UWidth, Resolution.UHeight);
            
        }
    }



     public void StartRender()
    {
        window.Run();
        window.Dispose();
    }



    public void MouseMove(IMouse mouse, Vector2 delta)
    {
        // if (Output != null)
        //     Output.MainCamera.Rotation = Quaternion.CreateFromYawPitchRoll(
        //         (Output.Resolution.WidthSingle / 2f  - delta.X) / Output.Resolution.WidthSingle * MathF.Tau,
        //         (Output.Resolution.HeightSingle / 2f - delta.Y) * -1f / Output.Resolution.HeightSingle * MathF.PI,
        //         0f);
    }
}
