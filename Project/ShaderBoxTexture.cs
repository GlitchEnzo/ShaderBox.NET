using SharpDX.Direct3D11;
using System;

namespace ShaderBox
{
    public class ShaderBoxTexture
    {
        /// <summary>
        /// The loaded DirectX Texture object
        /// </summary>
        public Texture2D texture;

        // ImGui binding id
        public IntPtr bindingID;

        // must be relative to project
        public string filepath;

        public ShaderBoxTexture(string filepath)
        {
            this.filepath = filepath;
        }

        public void Initialize(Device device)
        {
            // for now assume they are all loadable via .NET Bitmaps 
            texture = TextureExtensions.FromFile(device, filepath);
        }
    }
}
