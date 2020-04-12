using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace ShaderBox
{
    public class DirectXModel
    {
        public ObjModel ObjModel;

        public Matrix ModelMatrix = Matrix.Identity;

        Buffer vertexBuffer;
        VertexBufferBinding vertexBufferBinding;
        Buffer indexBuffer;

        int indexCount;

        public DirectXModel(SharpDX.Direct3D11.Device device, string objFilepath)
        {
            ObjModel = ObjModel.LoadObj(objFilepath);

            vertexBuffer = BufferExtensions.CreateVertexBuffer(device, ObjModel);
            vertexBufferBinding = new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<VertexPositionTextureNormal>(), 0);
            indexBuffer = BufferExtensions.CreateIndexBuffer(device, ObjModel);
            indexCount = ObjModel.Indices.Length;
        }

        public void GenerateEdgeList()
        {

        }

        public void Draw(DeviceContext context)
        {
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            //context.InputAssembler.InputLayout = inputLayout;
            context.InputAssembler.SetVertexBuffers(0, vertexBufferBinding);
            context.InputAssembler.SetIndexBuffer(indexBuffer, Format.R32_UInt, 0);
            //context.VertexShader.SetConstantBuffer(0, constantBuffer);
            //context.UpdateSubresource(ref constants, constantBuffer);
            context.DrawIndexed(indexCount, 0, 0);
        }
    }
}
