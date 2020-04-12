using SharpDX;
using SharpDX.Direct3D11;
using System.Runtime.InteropServices;

namespace ShaderBox
{
    public static class BufferExtensions
    {
        public static Buffer CreateConstantBufferFromStruct<T>(Device device, T bufferData) where T : struct
        {
            int size = Marshal.SizeOf(typeof(T));

            Buffer buffer = new Buffer(device, size, ResourceUsage.Default, 
                                            BindFlags.ConstantBuffer, 
                                            CpuAccessFlags.None, 
                                            ResourceOptionFlags.None, 0);

            device.ImmediateContext.UpdateSubresource(ref bufferData, buffer);
            return buffer;
        }

        /// <summary>
        /// Create a GPU buffer represending an HLSL constant buffer using the given CPU struct.
        /// See here: https://gamedev.stackexchange.com/questions/71172/how-to-set-shader-global-variable-in-sharpdx-without-using-effect-class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="device"></param>
        /// <param name="cpuData"></param>
        /// <returns></returns>
        public static Buffer CreateConstantBuffer<T>(Device device, T cpuData) where T : struct
        {
            var structSize = Marshal.SizeOf(default(T));

            // Verify that the incoming size is a multiple of 16 bytes, since that is a requirement for constant buffers
            System.Diagnostics.Trace.Assert(structSize % 16 == 0, string.Format("The given struct ({0}) is not a multiple of 16 bytes.", typeof(T)));

            var desc = new BufferDescription()
            {
                BindFlags = BindFlags.ConstantBuffer,// | BindFlags.ShaderResource,
                StructureByteStride = 0,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = structSize,
                Usage = ResourceUsage.Dynamic,
                CpuAccessFlags = CpuAccessFlags.Write
            };

            var buffer = Buffer.Create(device, ref cpuData, desc);

            return buffer;
        }

        public static Buffer CreateVertexBuffer(Device device, ObjModel model)
        {
            Buffer buffer = Buffer.Create(device, BindFlags.VertexBuffer, model.VertexData);
            return buffer;
        }

        public static Buffer CreateDynamicVertexBuffer<T>(Device device, T[] cpuData) where T : struct
        {
            var structSize = Marshal.SizeOf(default(T));

            var desc = new BufferDescription()
            {
                BindFlags = BindFlags.VertexBuffer,
                StructureByteStride = 0,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = structSize * cpuData.Length, // TODO: Set to full size of the array?
                Usage = ResourceUsage.Dynamic,
                CpuAccessFlags = CpuAccessFlags.Write
            };

            var buffer = Buffer.Create(device, cpuData, desc);

            return buffer;
        }
        
        public static void UpdateBuffer<T>(Device device, Buffer resource, T cpuData) where T : struct
        {
            var mappedSubresource = device.ImmediateContext.MapSubresource(resource, 0, MapMode.WriteDiscard, MapFlags.None);

            try
            {
                Utilities.Write(mappedSubresource.DataPointer, ref cpuData);

                //device.ImmediateContext.UpdateSubresource(ref cpuData, resource);
                //device.ImmediateContext.UpdateSubresource(ref cpuData, resource, 0, Marshal.SizeOf(default(T)), 0, null);
            }
            finally
            {
                device.ImmediateContext.UnmapSubresource(resource, 0);
            }
        }

        public static void UpdateBuffer<T>(Device device, Buffer resource, T[] cpuData) where T : struct
        {
            var mappedSubresource = device.ImmediateContext.MapSubresource(resource, 0, MapMode.WriteDiscard, MapFlags.None);

            try
            {
                Utilities.Write(mappedSubresource.DataPointer, cpuData, 0, cpuData.Length);
            }
            finally
            {
                device.ImmediateContext.UnmapSubresource(resource, 0);
            }
        }

        public static Buffer CreateIndexBuffer(Device device, ObjModel model)
        {
            Buffer buffer = Buffer.Create(device, BindFlags.IndexBuffer, model.Indices);
            return buffer;
        }

        public static Buffer CreateDynamicIndexBuffer<T>(Device device, T[] cpuData) where T : struct
        {
            var structSize = Marshal.SizeOf(default(T));

            var desc = new BufferDescription()
            {
                BindFlags = BindFlags.IndexBuffer,
                StructureByteStride = 0,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = structSize * cpuData.Length,
                Usage = ResourceUsage.Dynamic,
                CpuAccessFlags = CpuAccessFlags.Write
            };

            var buffer = Buffer.Create(device, cpuData, desc);

            return buffer;
        }
    }
}
