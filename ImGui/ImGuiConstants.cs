using SharpDX;
using System.Runtime.InteropServices;

namespace ShaderBox
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ImGuiConstants
    {
        public Matrix Model;
        public Matrix View;
        public Matrix Projection;
        public Matrix ModelViewProjection;
    }
}