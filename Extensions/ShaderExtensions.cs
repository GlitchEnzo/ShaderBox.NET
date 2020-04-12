using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System;
using System.Numerics;

namespace ShaderBox
{
    public static class ShaderExtensions
    {
        public static VertexShader LoadVertexShader(Device device, string vsPath, out ShaderBytecode vertexShaderBytecode)
        {
            var vsCompilationResult = ShaderBytecode.CompileFromFile("Shaders//" + vsPath, "main", "vs_5_0", ShaderFlags.Debug);
            vertexShaderBytecode = vsCompilationResult.Bytecode;
            var vertexShader = new VertexShader(device, vsCompilationResult.Bytecode);
            return vertexShader;
        }

        public static PixelShader LoadPixelShader(Device device, string psPath)
        {
            var psCompilationResult = ShaderBytecode.CompileFromFile("Shaders//" + psPath, "main", "ps_5_0", ShaderFlags.Debug);
            var pixelShader = new PixelShader(device, psCompilationResult.Bytecode);
            return pixelShader;
        }
    }
}
