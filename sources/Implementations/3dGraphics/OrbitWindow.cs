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

public class OrbitWindow : GameWindow
{
    private readonly Dictionary<string, List<Vector3>> trajectories;
    private readonly Dictionary<string, Vector3> finalPositions;
    
    private readonly Dictionary<string, Color4> planetColors = new()
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
    private float yaw = 45;
    private float pitch = -20;
    private float distance = 3f;
    private Vector2 lastMouse;
    private bool rotating;

    private Shader shader;
    private int lineVao;
    private int lineVbo;

    private SphereMesh sphereMesh;
    private int sphereVao;
    private int sphereVbo;
    private int sphereEbo;

    public OrbitWindow(
        GameWindowSettings gws,
        NativeWindowSettings nws,
        Dictionary<string, List<Vector3>> traj,
        Dictionary<string, Vector3> names)
        : base(gws, nws)
    {
        trajectories = traj;
        finalPositions = names;
    }

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

        shader = new Shader(vertPath, fragPath);
        
        sphereMesh = SphereMeshGenerator.CreateSphere(24, 24);

        // Linha VAO/VBO
        lineVao = GL.GenVertexArray();
        lineVbo = GL.GenBuffer();

        // Esfera VAO/VBO/EBO
        sphereVao = GL.GenVertexArray();
        sphereVbo = GL.GenBuffer();
        sphereEbo = GL.GenBuffer();

        GL.BindVertexArray(sphereVao);

        GL.BindBuffer(BufferTarget.ArrayBuffer, sphereVbo);
        GL.BufferData(BufferTarget.ArrayBuffer,
            sphereMesh.Vertices.Length * sizeof(float),
            sphereMesh.Vertices,
            BufferUsageHint.StaticDraw);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, sphereEbo);
        GL.BufferData(BufferTarget.ElementArrayBuffer,
            sphereMesh.Indices.Length * sizeof(uint),
            sphereMesh.Indices,
            BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        Console.WriteLine("Use trackpad para orbitar / zoom. Pressione F12 para screenshot.");
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        if (e.Button == MouseButton.Left)
        {
            rotating = true;
            lastMouse = MousePosition;
        }

        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        if (e.Button == MouseButton.Left)
            rotating = false;

        base.OnMouseUp(e);
    }

    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
        if (!rotating) return;

        var delta = e.Position - lastMouse;
        lastMouse = e.Position;

        yaw += delta.X * 0.3f;
        pitch += delta.Y * 0.3f;

        base.OnMouseMove(e);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        distance -= e.OffsetY * 5f;
        if (distance < 10) distance = 10;

        base.OnMouseWheel(e);
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        shader.Use();

        var projection = Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(45),
            Size.X / (float)Size.Y,
            0.1f,
            10000f);

        var cameraPos = new Vector3(
            distance * MathF.Cos(MathHelper.DegreesToRadians(pitch)) *
            MathF.Cos(MathHelper.DegreesToRadians(yaw)),
            distance * MathF.Sin(MathHelper.DegreesToRadians(pitch)),
            distance * MathF.Cos(MathHelper.DegreesToRadians(pitch)) *
            MathF.Sin(MathHelper.DegreesToRadians(yaw))
        );

        var view = Matrix4.LookAt(cameraPos, Vector3.Zero, Vector3.UnitY);

        shader.SetMatrix4("projection", projection);
        shader.SetMatrix4("view", view);

        DrawAllTrajectories();
        DrawAllBodies();

        SwapBuffers();
    }

    private void DrawAllTrajectories()
    {
        var identity = Matrix4.Identity;
        shader.SetMatrix4("model", identity);

        foreach (var track in trajectories.Values)
        {
            float[] vertices = track.SelectMany(v => new[] { v.X, v.Y, v.Z }).ToArray();

            GL.BindVertexArray(lineVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, lineVbo);
            GL.BufferData(BufferTarget.ArrayBuffer,
                vertices.Length * sizeof(float), vertices,
                BufferUsageHint.DynamicDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            var name = trajectories.First(t => t.Value == track).Key;
            Color4 c = planetColors.ContainsKey(name) ? planetColors[name] : Color4.White;

            shader.SetVector3("uColor", new Vector3(c.R, c.G, c.B));

            GL.DrawArrays(PrimitiveType.LineStrip, 0, track.Count);        
        }
    }

    private void DrawAllBodies()
    {
        GL.BindVertexArray(sphereVao);

        foreach (var pos in finalPositions.Values)
        {
            var model = Matrix4.CreateTranslation(pos);
            shader.SetMatrix4("model", model);

            GL.DrawElements(PrimitiveType.Triangles, sphereMesh.Indices.Length, DrawElementsType.UnsignedInt, 0);
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