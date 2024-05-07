using Silk.NET.OpenGL;
using System.Numerics;

namespace Paprika;

public class Shader : IDisposable
{
    public const string DefaultVertex = @"
        #version 330 core

        layout (location = 0) in vec3 aPosition;
        layout (location = 1) in vec2 aTextureCoord;

        out vec2 frag_texCoords;

        void main()
        {
            gl_Position = vec4(aPosition, 1.0);
            frag_texCoords = aTextureCoord;
        }";

    
    
    public const string DefaultFrag = @"
        #version 330 core

        in vec2 frag_texCoords;
        out vec4 out_color;

        uniform sampler2D uTexture;

        void main()
        {
            // out_color = vec4(frag_texCoords.x, frag_texCoords.y, 0, 1.0);
            out_color = texture(uTexture, frag_texCoords);
        }";



    private uint _handle;
    private GL _gl;

    public Shader(GL gl, string vertexPath, string fragmentPath)
    {
        _gl = gl;

        uint vertex = LoadShader(ShaderType.VertexShader, vertexPath);
        uint fragment = LoadShader(ShaderType.FragmentShader, fragmentPath);
        _handle = _gl.CreateProgram();
        _gl.AttachShader(_handle, vertex);
        _gl.AttachShader(_handle, fragment);
        _gl.LinkProgram(_handle);
        _gl.GetProgram(_handle, GLEnum.LinkStatus, out var status);
        if (status == 0)
        {
            throw new Exception($"Program failed to link with error: {_gl.GetProgramInfoLog(_handle)}");
        }
        _gl.DetachShader(_handle, vertex);
        _gl.DetachShader(_handle, fragment);
        _gl.DeleteShader(vertex);
        _gl.DeleteShader(fragment);
    }

    public void Use()
    {
        _gl.UseProgram(_handle);
    }

    public void SetUniform(string name, int value)
    {
        int location = _gl.GetUniformLocation(_handle, name);
        if (location == -1)
        {
            throw new Exception($"{name} uniform not found on shader.");
        }
        _gl.Uniform1(location, value);
    }

    public unsafe void SetUniform(string name, Matrix4x4 value)
    {
        //A new overload has been created for setting a uniform so we can use the transform in our shader.
        int location = _gl.GetUniformLocation(_handle, name);
        if (location == -1)
        {
            throw new Exception($"{name} uniform not found on shader.");
        }
        _gl.UniformMatrix4(location, 1, false, (float*) &value);
    }

    public void SetUniform(string name, float value)
    {
        int location = _gl.GetUniformLocation(_handle, name);
        if (location == -1)
        {
            throw new Exception($"{name} uniform not found on shader.");
        }
        _gl.Uniform1(location, value);
    }

    public void Dispose()
    {
        _gl.DeleteProgram(_handle);
    }

    // private uint LoadShader(ShaderType type, string path)
    // {
    //     string src = File.ReadAllText(path);
    //     uint handle = _gl.CreateShader(type);
    //     _gl.ShaderSource(handle, src);
    //     _gl.CompileShader(handle);
    //     string infoLog = _gl.GetShaderInfoLog(handle);
    //     if (!string.IsNullOrWhiteSpace(infoLog))
    //     {
    //         throw new Exception($"Error compiling shader of type {type}, failed with error {infoLog}");
    //     }

    //     return handle;
    // }


    private uint LoadShader(ShaderType type, string src)
    {
        uint handle = _gl.CreateShader(type);
        _gl.ShaderSource(handle, src);
        _gl.CompileShader(handle);
        string infoLog = _gl.GetShaderInfoLog(handle);
        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            throw new Exception($"Error compiling shader of type {type}, failed with error {infoLog}");
        }

        return handle;
    }
}
