using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Paprika;

public class Texture : IDisposable
{
    private uint _handle;
    private GL _gl;

    public unsafe Texture(GL gl, string path)
    {
        _gl = gl;

        _handle = _gl.GenTexture();
        Bind();

        using (var img = Image.Load<Rgba32>(path))
        {
            gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, (uint) img.Width, (uint) img.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);

            img.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    fixed (void* data = accessor.GetRowSpan(y))
                    {
                        gl.TexSubImage2D(TextureTarget.Texture2D, 0, 0, y, (uint) accessor.Width, 1, PixelFormat.Rgba, PixelType.UnsignedByte, data);
                    }
                }
            });
        }

        SetParameters();
    }

    public unsafe Texture(GL gl, Span<byte> data, uint width, uint height)
    {
        _gl = gl;
        Console.WriteLine($"Date length is: {data.Length}");
        _handle = _gl.GenTexture();
        Bind();

        fixed (void* d = &data[0])
        {
            _gl.TexImage2D(TextureTarget.Texture2D, 0, (int) InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, d);
            SetParameters();
        }
    }

    private void SetParameters()
    {
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) GLEnum.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) GLEnum.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) GLEnum.Nearest);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) GLEnum.Nearest);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
        //_gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 8);
        // _gl.GenerateMipmap(TextureTarget.Texture2D);
    }

    public unsafe void Update(Span<byte> data, uint width, uint height)
    {
        fixed (byte* newImg = &data[0])
        {
            // _gl.TexImage2D(GLEnum.Texture2D, 0, InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, (void*) 0);
            _gl.TexImage2D(GLEnum.Texture2D, 0, InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, newImg);
            // SetParameters();
        }
    }


    public unsafe void Update<T>(Span<T> data, uint width, uint height)

    where T: unmanaged
    {
        fixed (void* newImg = &data[0])
        {
            // _gl.TexImage2D(GLEnum.Texture2D, 0, InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, (void*) 0);
            _gl.TexImage2D(GLEnum.Texture2D, 0, InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, newImg);
            // SetParameters();
        }
    }



    public unsafe void Update(void* data, uint width, uint height)
    {
        _gl.TexImage2D(GLEnum.Texture2D, 0, InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
    }

    public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
    {
        _gl.ActiveTexture(textureSlot);
        _gl.BindTexture(TextureTarget.Texture2D, _handle);
    }

    public void Unbind(TextureUnit textureSlot = TextureUnit.Texture0)
    {
        _gl.ActiveTexture(textureSlot);
        _gl.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void Dispose()
    {
        _gl.DeleteTexture(_handle);
    }
}
