using OxyPlot.Annotations;

namespace OrbitalSimulator;

using OxyPlot;
using OxyPlot.Series;
using OxyPlot.SkiaSharp;
using System.IO;

public class Plotter
{
    private readonly PlotModel _plotModel = new() { Title = "Orbital Simulation" };
    
    public void AddBodyName(string bodyName, (double x, double y) position)
    {
        var textAnnotation = new TextAnnotation
        {
            Text = bodyName,
            TextHorizontalAlignment = HorizontalAlignment.Center,
            TextVerticalAlignment = VerticalAlignment.Middle,
            TextPosition = new DataPoint(position.x, position.y),
            Background = OxyColor.FromAColor(200, OxyColors.White),
            Stroke = OxyColors.Black,
            StrokeThickness = 1,
        };
        _plotModel.Annotations.Add(textAnnotation);
    }


    public void AddTrajectory(string bodyName, List<(double x, double y)> trajectory)
    {
        LineSeries series = new () { Title = bodyName };
        foreach ((double x, double y) point in trajectory)
        {
            series.Points.Add(new DataPoint(point.x, point.y));
        }
        _plotModel.Series.Add(series);
    }

    public void Save(string fileName)
    {
        string rootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        string outputDirectory = Path.Combine(rootPath, "Plots");

        Directory.CreateDirectory(outputDirectory);

        string filePath = Path.Combine(outputDirectory, fileName);

        using var stream = File.Create(filePath);
        var exporter = new PngExporter { Width = 800, Height = 600 };
        exporter.Export(_plotModel, stream);

        Console.WriteLine($"Arquivo salvo em: {filePath}");
    }
}