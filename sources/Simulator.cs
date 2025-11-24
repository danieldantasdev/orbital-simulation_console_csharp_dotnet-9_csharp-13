using OrbitalSimulator.Implementations._2dGraphics;
using OrbitalSimulator.Implementations._3dGraphics;
using OrbitalSimulator.Interfaces;
using OrbitalSimulator.Models;

namespace OrbitalSimulator;

using System;
using System.Collections.Generic;
using System.Linq;

public class Simulator
{
    private const double G = 6.67430e-11;
    private List<Body> Bodies { get; set; } = [];

    private void AddBody(Body body) => Bodies.Add(body);

    private void Simulate(double deltaTime, int steps, IPlotter plotter)
    {
        var trajectories = Bodies.ToDictionary(
            body => body.Name,
            _ => new List<(double X, double Y)>()
        );

        for (int step = 0; step < steps; step++)
        {
            List<(double X, double Y)> forces = CalculateForces();

            for (int i = 0; i < Bodies.Count; i++)
            {
                Body body = Bodies[i];
                (double fx, double fy) = forces[i];

                // Atualiza velocidades
                body.Velocity = (
                    body.Velocity.x + (fx / body.Mass) * deltaTime,
                    body.Velocity.y + (fy / body.Mass) * deltaTime
                );

                // Atualiza posições
                body.Position = (
                    body.Position.x + body.Velocity.x * deltaTime,
                    body.Position.y + body.Velocity.y * deltaTime
                );

                // Guarda trajetória
                trajectories[body.Name].Add(body.Position);
            }
        }

        // Envia dados ao plotter
        foreach (Body body in Bodies)
        {
            plotter.AddTrajectory(body.Name, trajectories[body.Name]);

            var finalPos = trajectories[body.Name].Last();
            plotter.AddBodyName(body.Name, finalPos);
        }
    }

    private List<(double X, double Y)> CalculateForces()
    {
        List<(double X, double Y)> forces = new();

        foreach (var body in Bodies)
        {
            double fx = 0, fy = 0;

            foreach (var other in Bodies)
            {
                if (other == body) continue;

                double dx = other.Position.x - body.Position.x;
                double dy = other.Position.y - body.Position.y;
                double distance = Math.Sqrt(dx * dx + dy * dy);

                if (distance == 0) continue;

                double force = G * body.Mass * other.Mass / (distance * distance);

                fx += force * (dx / distance);
                fy += force * (dy / distance);
            }

            forces.Add((fx, fy));
        }

        return forces;
    }

    public void Execute()
    {
        Console.WriteLine("Selecione o tipo de plot:");
        Console.WriteLine("1 - Plot 2D (OxyPlot)");
        Console.WriteLine("2 - Plot 3D (OpenTK)");
        Console.Write("Opção: ");

        string? option = Console.ReadLine();
        IPlotter plotter = option == "2" ? new Plotter3D() : new Plotter2D();

        // --------------------- ADD BODIES ---------------------

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

        // -------------------------------------------------------

        Console.WriteLine("Simulando...");
        Simulate(86400, 365, plotter);

        if (plotter is Plotter3D p3d)
        {
            Console.WriteLine("Abrindo janela 3D...");
            p3d.RunWindow();
            return;
        }

        // Modo 2D salva diretamente
        plotter.Save($"solar_system_simulation_2d_{DateTime.Now:yyyyMMdd_HHmmss}.png");
        Console.WriteLine("Plot 2D salvo!");
    }
}