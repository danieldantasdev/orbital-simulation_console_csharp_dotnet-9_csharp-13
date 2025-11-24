using OrbitalSimulator.Implementations._2dGraphics;
using OrbitalSimulator.Implementations._3dGraphics;
using OrbitalSimulator.Interfaces;
using OrbitalSimulator.Models;

namespace OrbitalSimulator.Implementations.Calcules;

public class Simulator
{
    private const double G = 6.67430e-11;
    private readonly List<Body> _bodies = [];

    private void AddBody(Body body) => _bodies.Add(body);
    
    private void AddDefaultBodies()
    {
        AddBody(new Body("Sun", 1.989e30, (0, 0), (0, 0)));
        AddBody(new Body("Mercury", 3.285e23, (57.91e9, 0), (0, 47400)));
        AddBody(new Body("Venus", 4.867e24, (108.2e9, 0), (0, 35020)));
        AddBody(new Body("Earth", 5.972e24, (149.6e9, 0), (0, 29783)));
        AddBody(new Body("Mars", 6.39e23, (227.9e9, 0), (0, 24007)));
        AddBody(new Body("Jupiter", 1.898e27, (778.5e9, 0), (0, 13070)));
        AddBody(new Body("Saturn", 5.683e26, (1.434e12, 0), (0, 9680)));
        AddBody(new Body("Uranus", 8.681e25, (2.871e12, 0), (0, 6800)));
        AddBody(new Body("Neptune", 1.024e26, (4.495e12, 0), (0, 5430)));
        
        // AddBody(new Body("Moon", 7.348e22, (149.6e9 + 384400000, 0), (0, 29783 + 1022)));
        //
        // AddBody(new Body("Io", 8.931e22, (778.5e9 + 421700000, 0), (0, 13070 + 17320)));
        // AddBody(new Body("Europa", 4.799e22, (778.5e9 + 671100000, 0), (0, 13070 + 13740)));
        // AddBody(new Body("Ganymede", 1.482e23, (778.5e9 + 1070400000, 0), (0, 13070 + 10870)));
        // AddBody(new Body("Callisto", 1.076e23, (778.5e9 + 1882700000, 0), (0, 13070 + 8200)));
    }
    
    private List<(double X, double Y)> CalculateForces()
    {
        var forces = new (double X, double Y)[_bodies.Count];

        for (int i = 0; i < _bodies.Count; i++)
        {
            double fx = 0, fy = 0;
            var body = _bodies[i];

            for (int j = 0; j < _bodies.Count; j++)
            {
                if (i == j) continue;

                var other = _bodies[j];
                double dx = other.Position.x - body.Position.x;
                double dy = other.Position.y - body.Position.y;
                double distSq = dx * dx + dy * dy;
                double dist = Math.Sqrt(distSq);
                if (dist == 0) continue;

                double f = G * body.Mass * other.Mass / distSq;
                fx += f * dx / dist;
                fy += f * dy / dist;
            }

            forces[i] = (fx, fy);
        }

        return forces.ToList();
    }

    private void Simulate(double deltaTime, int steps, IPlotter plotter)
    {
        var trajectories = _bodies.ToDictionary(b => b.Name, _ => new List<(double X, double Y)>());

        for (int step = 0; step < steps; step++)
        {
            var forces = CalculateForces();

            for (int i = 0; i < _bodies.Count; i++)
            {
                var body = _bodies[i];
                var (fx, fy) = forces[i];

                body.Velocity = (body.Velocity.x + fx / body.Mass * deltaTime,
                    body.Velocity.y + fy / body.Mass * deltaTime);

                body.Position = (body.Position.x + body.Velocity.x * deltaTime,
                    body.Position.y + body.Velocity.y * deltaTime);

                trajectories[body.Name].Add(body.Position);
            }
        }

        foreach (var body in _bodies)
        {
            plotter.AddTrajectory(body.Name, trajectories[body.Name]);
            plotter.AddBodyName(body.Name, trajectories[body.Name].Last());
        }
    }

    private static void Render(IPlotter plotter)
    {
        if (plotter is Plotter3D p3d)
        {
            Console.WriteLine("Abrindo janela 3D...");
            p3d.RunWindow();
            return;
        }

        string fileName = $"solar_system_simulation_2d_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        plotter.Save(fileName);
        Console.WriteLine($"Plot 2D salvo: {fileName}");
    }
    
    private void Execute(string plotterType = "2D", double deltaTime = 86400, int steps = 365)
    {
        IPlotter plotter = plotterType switch
        {
            "3D" => new Plotter3D(),
            _ => new Plotter2D()
        };

        AddDefaultBodies();

        Console.WriteLine("Simulando...");
        Simulate(deltaTime, steps, plotter);

        Render(plotter);
    }
    
    public void ExecuteInteractive()
    {
        Console.WriteLine("Selecione o tipo de plot:");
        Console.WriteLine("1 - Plot 2D (OxyPlot)");
        Console.WriteLine("2 - Plot 3D (OpenTK)");
        Console.Write("Opção: ");

        string? option = Console.ReadLine();
        string plotterType = option == "2" ? "3D" : "2D";

        Execute(plotterType);
    }
}