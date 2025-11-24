using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OrbitalSimulator.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

namespace OrbitalSimulator.Implementations._3dGraphics;

public class OrbitWindow(
    GameWindowSettings gws,
    NativeWindowSettings nws,
    Dictionary<string, List<Vector3>> traj,
    Dictionary<string, Vector3> names)
    : GameWindow(gws, nws)
{
    private readonly Dictionary<string, Color4> _planetColors = new()
    {
        ["Mercury"] = Color4.Gray,
        ["Venus"] = Color4.Orange,
        ["Earth"] = Color4.Blue,
        ["Mars"] = Color4.Red,
        ["Jupiter"] = Color4.Brown,
        ["Saturn"] = Color4.Yellow,
        ["Uranus"] = Color4.Green,
        ["Neptune"] = Color4.Cyan,
        ["Sun"] = Color4.White
    };

    // Câmera Orbit
    private float _yaw = 45;
    private float _pitch = -20;
    private float _distance = 3f;
    private Vector2 _lastMouse;
    private bool _rotating;

    private Shader _shader;
    private int _lineVao;
    private int _lineVbo;

    private SphereMesh _sphereMesh;
    private int _sphereVao;
    private int _sphereVbo;
    private int _sphereEbo;

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.ClearColor(0, 0, 0, 1);
        GL.Enable(EnableCap.DepthTest);

        string rootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));

        string shadersPath = Path.Combine(rootPath, "Shaders");

        string vertPath = Path.Combine(shadersPath, "basic.vert");
        string fragPath = Path.Combine(shadersPath, "basic.frag");

        Console.WriteLine("Vertex Shader: " + vertPath);
        Console.WriteLine("Fragment Shader: " + fragPath);

        _shader = new Shader(vertPath, fragPath);
        
        _sphereMesh = SphereMeshGenerator.CreateSphere(24, 24);

        // Linha VAO/VBO
        _lineVao = GL.GenVertexArray();
        _lineVbo = GL.GenBuffer();

        // Esfera VAO/VBO/EBO
        _sphereVao = GL.GenVertexArray();
        _sphereVbo = GL.GenBuffer();
        _sphereEbo = GL.GenBuffer();

        GL.BindVertexArray(_sphereVao);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _sphereVbo);
        GL.BufferData(BufferTarget.ArrayBuffer,
            _sphereMesh.Vertices.Length * sizeof(float),
            _sphereMesh.Vertices,
            BufferUsageHint.StaticDraw);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _sphereEbo);
        GL.BufferData(BufferTarget.ElementArrayBuffer,
            _sphereMesh.Indices.Length * sizeof(uint),
            _sphereMesh.Indices,
            BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        Console.WriteLine("Use trackpad para orbitar / zoom. Pressione F12 para screenshot.");
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        if (e.Button == MouseButton.Left)
        {
            _rotating = true;
            _lastMouse = MousePosition;
        }

        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        if (e.Button == MouseButton.Left)
            _rotating = false;

        base.OnMouseUp(e);
    }

    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
        if (!_rotating) return;

        var delta = e.Position - _lastMouse;
        _lastMouse = e.Position;

        _yaw += delta.X * 0.3f;
        _pitch += delta.Y * 0.3f;

        base.OnMouseMove(e);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        _distance -= e.OffsetY * 5f;
        if (_distance < 10) _distance = 10;

        base.OnMouseWheel(e);
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _shader.Use();

        var projection = Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(45),
            Size.X / (float)Size.Y,
            0.1f,
            10000f);

        var cameraPos = new Vector3(
            _distance * MathF.Cos(MathHelper.DegreesToRadians(_pitch)) *
            MathF.Cos(MathHelper.DegreesToRadians(_yaw)),
            _distance * MathF.Sin(MathHelper.DegreesToRadians(_pitch)),
            _distance * MathF.Cos(MathHelper.DegreesToRadians(_pitch)) *
            MathF.Sin(MathHelper.DegreesToRadians(_yaw))
        );

        var view = Matrix4.LookAt(cameraPos, Vector3.Zero, Vector3.UnitY);

        _shader.SetMatrix4("projection", projection);
        _shader.SetMatrix4("view", view);

        DrawAllTrajectories();
        DrawAllBodies();

        SwapBuffers();
    }

    private void DrawAllTrajectories()
    {
        var identity = Matrix4.Identity;
        _shader.SetMatrix4("model", identity);

        foreach (var track in traj.Values)
        {
            float[] vertices = track.SelectMany(v => new[] { v.X, v.Y, v.Z }).ToArray();

            GL.BindVertexArray(_lineVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _lineVbo);
            GL.BufferData(BufferTarget.ArrayBuffer,
                vertices.Length * sizeof(float), vertices,
                BufferUsageHint.DynamicDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            var name = traj.First(t => t.Value == track).Key;
            Color4 c = _planetColors.ContainsKey(name) ? _planetColors[name] : Color4.White;

            _shader.SetVector3("uColor", new Vector3(c.R, c.G, c.B));

            GL.DrawArrays(PrimitiveType.LineStrip, 0, track.Count);        
        }
    }

    private void DrawAllBodies()
    {
        GL.BindVertexArray(_sphereVao);

        foreach (var pos in names.Values)
        {
            var model = Matrix4.CreateTranslation(pos);
            _shader.SetMatrix4("model", model);

            GL.DrawElements(PrimitiveType.Triangles, _sphereMesh.Indices.Length, DrawElementsType.UnsignedInt, 0);
        }
    }

    protected override void OnKeyDown(KeyboardKeyEventArgs e)
    {
        if (e.Key == Keys.F12)
            SaveScreenshot();

        base.OnKeyDown(e);
    }
    
    private void SaveScreenshot()
    {
        string rootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        string outputDirectory = Path.Combine(rootPath, "Plots");

        Directory.CreateDirectory(outputDirectory);

        string filePath = Path.Combine(outputDirectory,
            $"solar_system_simulation_3d_{DateTime.Now:yyyyMMdd_HHmmss}.png");

        int w = Size.X;
        int h = Size.Y;

        byte[] pixels = new byte[w * h * 3];
        GL.ReadPixels(0, 0, w, h, PixelFormat.Rgb, PixelType.UnsignedByte, pixels);

        using var img = Image.LoadPixelData<Rgb24>(pixels, w, h);

        // OpenGL salva invertido
        img.Mutate(x => x.Flip(FlipMode.Vertical));

        img.Save(filePath);

        Console.WriteLine($"[3D] Screenshot salvo em: {filePath}");
    }
}