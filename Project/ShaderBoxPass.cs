using System.Collections.Generic;

namespace ShaderBox
{
    public class ShaderBoxPass
    {
        public string VertexShader;
        public string GeometryShader;
        public string HullShader;
        public string DomainShader;
        public string ComputeShader;

        // TODO: This should just a be a list of IDs that point to a global list of meshes for the entire project
        //public List<ShaderBoxMesh> Meshes = new List<ShaderBoxMesh>();

        public List<ShaderBoxBuffer> Buffers = new List<ShaderBoxBuffer>();

        // TODO: This should just a be a list of IDs that point to a global list of textures for the entire project
        //public List<ShaderBoxTexture> Textures = new List<ShaderBoxTexture>();
        
        // ImGui tree node
        //public bool isOpen;

        public string name = "Pass";

        //public Dictionary<ShaderType, >

        public List<ShaderBoxTextureBinding> TextureBindings = new List<ShaderBoxTextureBinding>();

        public void Serialize()
        {
            var testPass = new ShaderBoxPass();
            testPass.VertexShader = @"hello
this is a 
multiline test";
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(testPass);
        }
    }
}
