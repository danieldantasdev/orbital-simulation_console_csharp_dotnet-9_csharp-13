namespace OrbitalSimulator;

public class Body(string name, double mass, (double x, double y) position, (double x, double y) velocity)
{
    public string Name { get; } = name;
    public double Mass { get; } = mass;
    public (double x, double y) Position { get; set; } = position;
    public (double x, double y) Velocity { get; set; } = velocity;
}