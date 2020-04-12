using System.Collections.Generic;

namespace ShaderBox
{
    /// <summary>
    /// Represents a pixel point along an edge, in normalized screenspace coordinates.
    /// </summary>
    public struct EdgePoint
    {
        public uint IndexA;
        public uint IndexB;
        public float X;
        public float Y;
        // possible edge type to have different styles
    }

    public static class EdgePointTest
    {
        public static IEnumerable<EdgePoint> GenerateRandomPoints(int count, int seed = 1337)
        {
            System.Random random = new System.Random(seed);

            List<EdgePoint> points = new List<EdgePoint>();

            for (int i = 0; i < count; i++)
            {
                EdgePoint point = new EdgePoint();
                point.X = (float)random.NextDouble() * 2 - 1;
                point.Y = (float)random.NextDouble() * 2 - 1;
                points.Add(point);
            }

            return points;
        }

        //public static IDictionary<>
    }
}