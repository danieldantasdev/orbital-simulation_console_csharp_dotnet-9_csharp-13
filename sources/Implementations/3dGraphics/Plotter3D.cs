using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OrbitalSimulator.Interfaces;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace OrbitalSimulator.Implementations._3dGraphics;

public class Plotter3D : IPlotter
{
    private readonly Dictionary<string, List<Vector3>> trajectories = new();
    private readonly Dictionary<string, Vector3> finalPositions = new();
    const float SCALE = 1e-9f; // reduz de metros para unidades "astronômicas"

    public void AddBodyName(string name, (double x, double y) position)
    {
        finalPositions[name] = new Vector3(
            (float)(position.x * SCALE),
            (float)(position.y * SCALE),
            0);    
    }
    
    public void AddTrajectory(string name, List<(double x, double y)> raw)
    {
        trajectories[name] = raw
            .Select(p => new Vector3(
                (float)(p.x * SCALE),
                (float)(p.y * SCALE),
                0))
            .ToList();
    }

    public void Save(string fileName)
    {
        Console.WriteLine("Screenshot disponível na janela 3D com F12.");
    }

    public void RunWindow()
    {
        var native = new NativeWindowSettings()
        {
            Title = "Orbital Simulator 3D",
            Size = new Vector2i(1280, 720),
            APIVersion = new Version(3, 3),
            Profile = ContextProfile.Core
        };

        using var win = new OrbitWindow(GameWindowSettings.Default, native, trajectories, finalPositions);
        win.Run();
    }
}