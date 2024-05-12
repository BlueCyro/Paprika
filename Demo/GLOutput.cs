using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using bottlenoselabs.C2CS.Runtime;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Tracy;


namespace Paprika;




// public class GLOutput<T> : IRenderOutput<T> where T: IRenderer
// {
//     public RenderBuffer<int> PixelBuffer
//     {
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         get => bufferSwap ? pixelBuffer2 : pixelBuffer1;
//     }
//     public RenderBuffer<float> ZBuffer
//     {
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         get => zBuffer;
//         private set => zBuffer = value;
//     }
//     public Span<byte> PixelBufferBytes => MemoryMarshal.Cast<int, byte>(PixelBuffer.Buffer.Span);
//     public Size2D FrameBufferSize { get; private set; }
//     public T CurrentRenderer { get; private set; }
//     public IRenderer CurrentIRenderer => CurrentRenderer;


//     private GL gl;
//     private Shader shader;
//     private IWindow window;
//     private IInputContext input;
//     private Texture pixelView;
//     private BufferObject<uint> elementBufferObject;
//     private BufferObject<float> vertexBufferObject;
//     private VertexArrayObject<float, uint> vertexArrayObject;


//     private bool bufferSwap = false;
//     private RenderBuffer<int> pixelBuffer1;
//     private RenderBuffer<int> pixelBuffer2;
//     private RenderBuffer<float> zBuffer;



// #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
//     public GLOutput(string name, int width, int height)
// #pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
//     {
//         FrameBufferSize = new(width, height);
//         var opts = WindowOptions.Default;
//         opts.Title = name;
//         opts.Size = FrameBufferSize;
//         window = Window.Create(opts);


//         window.Load += Bootstrap;
//         window.Render += Render;
//         window.Update += Update;
//         window.FramebufferResize += s => {
//             gl!.Viewport(s);
//             // Size = Vector64.Create(s.X, s.Y);
//             // ResizeBuffer();
//         };


//         pixelBuffer1 = new(width, height);
//         pixelBuffer2 = new(width, height);
//         ZBuffer = new(width, height);

//         CurrentRenderer = (T?)Activator.CreateInstance(typeof(T), [ this ]) ?? throw new Exception($"Failed to initialize instance of {typeof(T)}!");
//     }



//     public void StartRender()
//     {
//         window.Run();
//         window.Dispose();
//     }



//     public void PushPixels()
//     {
//         pixelView.Update(PixelBufferBytes, FrameBufferSize.UWidth, FrameBufferSize.UHeight);
//         bufferSwap = !bufferSwap;
//     }



//     private void Bootstrap()
//     {
//         gl = window.CreateOpenGL();
//         input = window.CreateInput();

//         input.Mice[0].MouseMove += MouseMove;

//         var col = System.Drawing.Color.BlueViolet;
//         gl.ClearColor(col); // Clear to a specified color


//         elementBufferObject = new(gl, ScreenQuad.Indices, BufferTargetARB.ElementArrayBuffer);
//         vertexBufferObject = new(gl, ScreenQuad.Vertices, BufferTargetARB.ArrayBuffer);
//         vertexArrayObject = new(gl, vertexBufferObject, elementBufferObject);

        
//         vertexArrayObject.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 5, 0);
//         vertexArrayObject.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 5, 3);


//         shader = new(gl, Shader.DefaultVertex, Shader.DefaultFrag);
//         pixelView = new(gl, PixelBufferBytes, FrameBufferSize.UWidth, FrameBufferSize.UHeight);
//     }



//     private unsafe void Render(double delta)
//     {
//         vertexBufferObject.Bind();
//         vertexArrayObject.Bind();
//         pixelView.Bind();


//         shader.Use();
//         shader.SetUniform("uTexture", 0);
//         gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, null);
//     }


//     // double avg;
//     // ulong frame;
//     // int renderedTris;
//     private void Update(double delta)
//     {
//         // Program.DEBUG_ReAllocateDumbBuffer();
//         // var board = input.Keyboards[0];
//         // float shiftMod = 1f;

//         // if (board.IsKeyPressed(Key.ShiftLeft))
//         //     shiftMod = 5f;
        
//         // if (board.IsKeyPressed(Key.Space))
//         //     MainCamera.Position += MainCamera.Up * shiftMod * (float)delta;
        
//         // if (board.IsKeyPressed(Key.E))
//         //     MainCamera.Position += -MainCamera.Up * shiftMod * (float)delta;
        

//         // if (board.IsKeyPressed(Key.W))
//         //     MainCamera.Position += MainCamera.Forward * shiftMod * (float)delta;
        
//         // if (board.IsKeyPressed(Key.S))
//         //     MainCamera.Position += -MainCamera.Forward * shiftMod * (float)delta;

//         // if (board.IsKeyPressed(Key.A))
//         //     MainCamera.Position += MainCamera.Right * shiftMod * (float)delta;

//         // if (board.IsKeyPressed(Key.D))
//         //     MainCamera.Position += -MainCamera.Right * shiftMod * (float)delta;

        

//         PixelBuffer.Buffer.Clear();
//         ZBuffer.Buffer.Fill(float.MaxValue);

//         // Program.Timer.Start();
//         // CString render = (CString)"RenderFrame";
//         // PInvoke.TracyAllocSrcloc(175, (CString)"Update", )
//         // PInvoke.TracyEmitZoneBegin();
//         // PInvoke.TracyEmitFrameMarkStart(render);
//         CurrentRenderer.RenderFrame();
//         // PInvoke.TracyEmitFrameMarkEnd(render);
//         // Console.WriteLine("HUH");
//         // render.Dispose();
        
//         // Program.Timer.Stop();

//         // avg += Program.Timer.ElapsedMilliseconds;
//         // ulong frameCount = 20;
//         // if (++frame % frameCount == 0)
//         // {
//         //     Console.WriteLine($"[Paprika] Took (avg): {avg / frameCount:F3}ms, Frame: {frame}, Tris: {renderedTris}");
//         //     // Console.WriteLine($"Camera pos: {MainCamera.Position}, Camera rot {MainCamera.Rotation}");
//         //     avg = 0;
//         // }

//         PushPixels();
//         // Program.Timer.Reset();
//         // Profiler.ProfileFrame("Update");
//     }



//     public void MouseMove(IMouse mouse, Vector2 delta)
//     {
//         // MainCamera.Rotation = Quaternion.CreateFromYawPitchRoll(-delta.X / FrameBufferSize.WidthSingle * MathF.Tau, delta.Y / FrameBufferSize.HeightSingle * MathF.PI - (MathF.PI / 2f), 0f);
//     }
// }