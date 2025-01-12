namespace OrbitalSimulator;

using System;
using System.Collections.Generic;
using System.Linq;

class Simulator
{
    private const double G = 6.67430e-11;
    private List<Body> Bodies { get; set; } = [];

    private void AddBody(Body body) => Bodies.Add(body);

    private void Simulate(double deltaTime, int steps, Plotter plotter)
    {
        Dictionary<string, List<(double x, double y)>> trajectories = Bodies.ToDictionary(body => body.Name, _ => new List<(double X, double Y)>());

        for (int step = 0; step < steps; step++)
        {
            List<(double X, double Y)> forces = CalculateForces();

            for (int i = 0; i < Bodies.Count; i++)
            {
                Body body = Bodies[i];
                (double fx, double fy) = forces[i];

                body.Velocity = (
                    body.Velocity.x + (fx / body.Mass) * deltaTime,
                    body.Velocity.y + (fy / body.Mass) * deltaTime
                );

                body.Position = (
                    body.Position.x + body.Velocity.x * deltaTime,
                    body.Position.y + body.Velocity.y * deltaTime
                );

                trajectories[body.Name].Add(body.Position);
            }
        }

        foreach (Body body in Bodies)
        {
            plotter.AddTrajectory(body.Name, trajectories[body.Name]);
            
            (double x, double y) finalPosition = trajectories[body.Name].Last();
            plotter.AddBodyName(body.Name, finalPosition);
        }
    }

    private List<(double X, double Y)> CalculateForces()
    {
        List<(double X, double Y)> forces = new ();

        foreach (Body body in Bodies)
        {
            double fx = 0, fy = 0;

            foreach (Body other in Bodies)
            {
                if (body == other) continue;

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
        Body sun = new("Sun", 1.989e30, (0, 0), (0, 0));

        Body mercury = new("Mercury", 3.285e23, (57.91e9, 0), (0, 47400));
        Body venus = new("Venus", 4.867e24, (108.2e9, 0), (0, 35020));
        Body earth = new("Earth", 5.972e24, (149.6e9, 0), (0, 29783));
        Body mars = new("Mars", 6.39e23, (227.9e9, 0), (0, 24007));
        Body jupiter = new("Jupiter", 1.898e27, (778.5e9, 0), (0, 13070));
        Body saturn = new("Saturn", 5.683e26, (1.434e12, 0), (0, 9680));
        Body uranus = new("Uranus", 8.681e25, (2.871e12, 0), (0, 6800));
        Body neptune = new("Neptune", 1.024e26, (4.495e12, 0), (0, 5430));

        Body moon = new("Moon", 7.348e22, (149.6e9 + 384400000, 0), (0, 29783 + 1022));

        Body io = new("Io", 8.931e22, (778.5e9 + 421700000, 0), (0, 13070 + 17320));
        Body europa = new("Europa", 4.799e22, (778.5e9 + 671100000, 0), (0, 13070 + 13740));
        Body ganymede = new("Ganymede", 1.482e23, (778.5e9 + 1070400000, 0), (0, 13070 + 10870));
        Body callisto = new("Callisto", 1.076e23, (778.5e9 + 1882700000, 0), (0, 13070 + 8200));

        Simulator simulator = new();
        simulator.AddBody(sun);
        simulator.AddBody(mercury);
        simulator.AddBody(venus);
        simulator.AddBody(earth);
        simulator.AddBody(mars);
        simulator.AddBody(jupiter);
        simulator.AddBody(saturn);
        simulator.AddBody(uranus);
        simulator.AddBody(neptune);
        // simulator.AddBody(moon);
        // simulator.AddBody(io);
        // simulator.AddBody(europa);
        // simulator.AddBody(ganymede);
        // simulator.AddBody(callisto);

        Plotter plotter = new();
        simulator.Simulate(86400, 365, plotter);
        plotter.Save("solar_system_simulation.png");
    }
}
