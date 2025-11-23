using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace OrbitalSimulator.Implementations._3dGraphics;

public class Shader
{
    public int Handle { get; }

    public Shader(string vertexPath, string fragmentPath)
    {
        string vertCode = File.ReadAllText(vertexPath);
        string fragCode = File.ReadAllText(fragmentPath);

        int vert = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vert, vertCode);
        GL.CompileShader(vert);

        int frag = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(frag, fragCode);
        GL.CompileShader(frag);

        Handle = GL.CreateProgram();
        GL.AttachShader(Handle, vert);
        GL.AttachShader(Handle, frag);
        GL.LinkProgram(Handle);

        GL.DeleteShader(vert);
        GL.DeleteShader(frag);
    }

    public void Use() => GL.UseProgram(Handle);

    public void SetMatrix4(string name, Matrix4 matrix)
    {
        int loc = GL.GetUniformLocation(Handle, name);
        GL.UniformMatrix4(loc, false, ref matrix);
    }
    
    public void SetVector3(string name, Vector3 vector)
    {
        int loc = GL.GetUniformLocation(Handle, name);
        GL.Uniform3(loc, vector);
    }
}