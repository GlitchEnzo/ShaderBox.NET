using System;
using System.Numerics;

namespace ShaderBox
{
    public static class ObjModelExtensions
    {
        public static void Draw(this ObjModel model)
        {
            // is this even worthwhile?
        }

        /// <summary>
        /// This is likely very broken, so don't trust it...
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static Tuple<Vector3, Vector3> FindBounds(this ObjModel model)
        {
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

            foreach (var vertex in model.VertexData)
            {
                var vert = vertex.Position.ToVector3();
                if (vert.AllLess(min))
                {
                    min = vert;
                }

                if (vert.AllGreater(max))
                {
                    max = vert;
                }
            }

            return new Tuple<Vector3, Vector3>(min, max);
        }
    }
}
