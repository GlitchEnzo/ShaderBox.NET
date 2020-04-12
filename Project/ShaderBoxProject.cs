using SharpDX.Direct3D11;
using System.Collections.Generic;

namespace ShaderBox
{
    public class ShaderBoxProject
    {
        public List<ShaderBoxPass> Passes = new List<ShaderBoxPass>();

        public List<ShaderBoxMesh> Meshes = new List<ShaderBoxMesh>();

        public List<ShaderBoxTexture> Textures = new List<ShaderBoxTexture>();

        public void Initialize(Device device)
        {
            foreach (var mesh in Meshes)
            {
                mesh.Initialize(device);
            }

            foreach (var texture in Textures)
            {
                texture.Initialize(device);
            }
        }
    }
}
