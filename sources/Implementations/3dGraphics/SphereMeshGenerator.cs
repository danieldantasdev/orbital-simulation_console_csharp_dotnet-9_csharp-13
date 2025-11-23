using OrbitalSimulator.Models;

namespace OrbitalSimulator.Implementations._3dGraphics;

public static class SphereMeshGenerator
{
    public static SphereMesh CreateSphere(int latSegments, int lonSegments)
    {
        List<float> vertices = new();
        List<uint> indices = new();

        for (int lat = 0; lat <= latSegments; lat++)
        {
            float theta = lat * MathF.PI / latSegments;
            float sinTheta = MathF.Sin(theta);
            float cosTheta = MathF.Cos(theta);

            for (int lon = 0; lon <= lonSegments; lon++)
            {
                float phi = lon * 2f * MathF.PI / lonSegments;
                float sinPhi = MathF.Sin(phi);
                float cosPhi = MathF.Cos(phi);

                float x = cosPhi * sinTheta;
                float y = cosTheta;
                float z = sinPhi * sinTheta;

                vertices.Add(x);
                vertices.Add(y);
                vertices.Add(z);
            }
        }

        for (uint lat = 0; lat < latSegments; lat++)
        {
            for (uint lon = 0; lon < lonSegments; lon++)
            {
                uint first = lat * (uint)(lonSegments + 1) + lon;
                uint second = first + (uint)(lonSegments + 1);

                indices.Add(first);
                indices.Add(second);
                indices.Add(first + 1);

                indices.Add(second);
                indices.Add(second + 1);
                indices.Add(first + 1);
            }
        }

        return new SphereMesh(vertices.ToArray(), indices.ToArray());
    }
}