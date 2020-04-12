using System.Numerics;

namespace ShaderBox
{
    public struct VertexPositionTextureColor
    {
        public Vector4 Position;

        public Vector3 TextureCoordinates;

        public Vector4 Color;

        public VertexPositionTextureColor(Vector4 position, Vector3 texCoords, Vector4 color)
        {
            Position = position;
            TextureCoordinates = texCoords;
            Color = color;
        }
    }
}