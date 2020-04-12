using SharpDX;
using System.Runtime.InteropServices;

namespace ShaderBox
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ShaderBoxConstants
    {
        public Matrix Model;
        public Matrix View;
        public Matrix Projection;
        //public Matrix ModelView;
        //public Matrix ViewProjection;
        public Matrix ModelViewProjection;
    }
}