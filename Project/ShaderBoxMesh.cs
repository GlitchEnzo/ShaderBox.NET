using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace ShaderBox
{
    public class ShaderBoxMesh
    {
        // for now, assume they are only .obj files
        ObjModel model;

        Buffer vertexBuffer;
        VertexBufferBinding vertexBufferBinding;
        Buffer indexBuffer;

        // must be relative to project
        string filepath;

        public ShaderBoxMesh(string filepath)
        {
            this.filepath = filepath;
        }

        public void Initialize(Device device)
        {
            model = ObjModel.LoadObj(filepath);

            vertexBuffer = BufferExtensions.CreateVertexBuffer(device, model);
            vertexBufferBinding = new VertexBufferBinding(vertexBuffer, SharpDX.Utilities.SizeOf<VertexPositionTextureNormal>(), 0);
            indexBuffer = BufferExtensions.CreateIndexBuffer(device, model);
        }
    }
}
