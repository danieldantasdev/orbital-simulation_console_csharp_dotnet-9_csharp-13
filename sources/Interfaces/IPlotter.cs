namespace OrbitalSimulator.Interfaces;

public interface IPlotter
{
    void AddBodyName(string bodyName, (double x, double y) position);
    void AddTrajectory(string bodyName, List<(double x, double y)> trajectory);
    void Save(string fileName);
}