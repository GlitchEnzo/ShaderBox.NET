using ImGuiNET;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DirectInput;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace ShaderBox
{
    public class ImGuiDx11
    {
        private int textureId;
        private IntPtr fontTextureId;
        private Dictionary<IntPtr, ShaderResourceView> loadedTextures = new Dictionary<IntPtr, ShaderResourceView>();

        //private ImDrawVert[] vertexData = new ImDrawVert[1024 * 16];
        private byte[] vertexData = new byte[1024 * 16 * 20]; //sizeof(ImDrawVert) = 20
        Buffer vertexBuffer;
        VertexBufferBinding vertexBufferBinding;
        //int vertexCount;

        //ushort[] indices = new ushort[1024 * 32];
        byte[] indices = new byte[1024 * 32 * 2]; //sizeof(ushort) = 16
        Buffer indexBuffer;
        int indexCount;

        VertexShader vertexShader;
        PixelShader pixelShader;
        InputLayout inputLayout;

        ImGuiConstants constants;
        Buffer constantBuffer;

        ShaderResourceView textureView;
        Device device;

        Dictionary<Key, char> keyToChar = new Dictionary<Key, char>()
        {
            { Key.A, 'a' },
            { Key.B, 'b' },
            { Key.C, 'c' },
            { Key.D, 'd' },
            { Key.E, 'e' },
            { Key.F, 'f' },
            { Key.G, 'g' },
            { Key.H, 'h' },
            { Key.I, 'i' },
            { Key.J, 'j' },
            { Key.K, 'k' },
            { Key.L, 'i' },
            { Key.M, 'm' },
            { Key.N, 'n' },
            { Key.O, 'o' },
            { Key.P, 'p' },
            { Key.Q, 'q' },
            { Key.R, 'r' },
            { Key.S, 's' },
            { Key.T, 't' },
            { Key.U, 'u' },
            { Key.V, 'v' },
            { Key.W, 'w' },
            { Key.X, 'x' },
            { Key.Y, 'y' },
            { Key.Z, 'z' },
            { Key.Back, (char)8 },
            { Key.Escape, (char)27 },
            { Key.Return, (char)13 },
        };

        public ImGuiDx11()
        {
            var imGuiContext = ImGui.CreateContext();
            ImGui.SetCurrentContext(imGuiContext);

            // init input - even needed?
        }

        public unsafe void Initialize(Device device)
        {
            this.device = device;

            // build the font atlas
            var io = ImGui.GetIO();
            io.Fonts.AddFontDefault();
            io.Fonts.GetTexDataAsRGBA32(out byte* pixelData, out int width, out int height, out int bytesPerPixel);

            //var pixels = new byte[width * height * bytesPerPixel];
            //Marshal.Copy(new IntPtr(pixelData), pixels, 0, pixels.Length);

            //GCHandle pinnedArray = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            //IntPtr pointer = pinnedArray.AddrOfPinnedObject();

            // TODO: determine best way to create a new texture from this data
            //var image = System.Drawing.Image.FromFile("Textures//" + "debug_grid.jpg");
            //var bitmap = new System.Drawing.Bitmap(image);
            //var data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var tex2d = new Texture2D(device, new Texture2DDescription()
            {
                Width = width,
                Height = height,
                ArraySize = 1,
                BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource,
                Usage = SharpDX.Direct3D11.ResourceUsage.Immutable,
                CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                //Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                MipLevels = 1,
                OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
            //}, new SharpDX.DataRectangle(data.Scan0, data.Stride));
            }, new SharpDX.DataRectangle(new IntPtr(pixelData), width * bytesPerPixel));
            //bitmap.UnlockBits(data);

            // DEBUG: Save out the font texture to disk
            //var fontBitmap = TextureExtensions.CopyTextureToBitmap(device, tex2d);
            //fontBitmap.Save("fontTexture.png", System.Drawing.Imaging.ImageFormat.Png);

            textureView = new ShaderResourceView(device, tex2d);

            fontTextureId = BindTexture(tex2d);

            io.Fonts.SetTexID(fontTextureId);
            io.Fonts.ClearTexData();

            //ImGuiIOPtr io = ImGui.GetIO();
            io.KeyMap[(int)ImGuiKey.Tab] = (int)Key.Tab;
            io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Key.Left;
            io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Key.Right;
            io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Key.Up;
            io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Key.Down;
            io.KeyMap[(int)ImGuiKey.PageUp] = (int)Key.PageUp;
            io.KeyMap[(int)ImGuiKey.PageDown] = (int)Key.PageDown;
            io.KeyMap[(int)ImGuiKey.Home] = (int)Key.Home;
            io.KeyMap[(int)ImGuiKey.End] = (int)Key.End;
            io.KeyMap[(int)ImGuiKey.Delete] = (int)Key.Delete;
            io.KeyMap[(int)ImGuiKey.Backspace] = (int)Key.Back;
            io.KeyMap[(int)ImGuiKey.Enter] = (int)Key.Return;
            io.KeyMap[(int)ImGuiKey.Escape] = (int)Key.Escape;
            io.KeyMap[(int)ImGuiKey.A] = (int)Key.A;
            io.KeyMap[(int)ImGuiKey.A] = (int)Key.A;
            io.KeyMap[(int)ImGuiKey.C] = (int)Key.C;
            io.KeyMap[(int)ImGuiKey.V] = (int)Key.V;
            io.KeyMap[(int)ImGuiKey.X] = (int)Key.X;
            io.KeyMap[(int)ImGuiKey.Y] = (int)Key.Y;
            io.KeyMap[(int)ImGuiKey.Z] = (int)Key.Z;

            //io.KeyRepeatDelay = 0.05f;
            //io.KeyRepeatRate = 0.005f;

            // create buffers
            //vertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, vertexData);
            vertexBuffer = BufferExtensions.CreateDynamicVertexBuffer(device, vertexData);
            vertexBufferBinding = new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<ImDrawVert>(), 0);
            //indexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, indices);
            indexBuffer = BufferExtensions.CreateDynamicIndexBuffer(device, indices);
            indexCount = indices.Length;

            ShaderBytecode vertexShaderBytecode;
            vertexShader = ShaderExtensions.LoadVertexShader(device, "imgui_vs.hlsl", out vertexShaderBytecode);
            pixelShader = ShaderExtensions.LoadPixelShader(device, "imgui_ps.hlsl");

            var elements = new[]
            {
                new InputElement("POSITION", 0, Format.R32G32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, 8, 0, InputClassification.PerVertexData, 0),
                new InputElement("COLOR", 0, Format.R8G8B8A8_UNorm, 16, 0, InputClassification.PerVertexData, 0)
            };
            inputLayout = new InputLayout(device, vertexShaderBytecode, elements);

            io.DisplaySize = new System.Numerics.Vector2(1280, 720);

            constants.Model = Matrix.Identity;
            constants.View = Matrix.Identity;
            constants.Projection = Matrix.OrthoOffCenterLH(0f,
                                                            io.DisplaySize.X,
                                                            io.DisplaySize.Y,
                                                            0.0f,
                                                            -1.0f,
                                                            1.0f);

            var viewProj = Matrix.Multiply(constants.View, constants.Projection);
            viewProj.Transpose();
            constants.ModelViewProjection = viewProj;

            constantBuffer = BufferExtensions.CreateConstantBufferFromStruct(device, constants);
        }

        public IntPtr BindTexture(Texture2D texture)
        {
            var id = new IntPtr(textureId++);
            var srv = new ShaderResourceView(device, texture);
            loadedTextures.Add(id, srv);
            return id;
        }

        public void UnbindTexture(IntPtr textureId)
        {
            loadedTextures.Remove(textureId);
        }

        int _scrollWheelValue = 0;
        public void StartFrame(float deltaSeconds, Mouse mouse, Vector2 mousePosition, Keyboard keyboard)
        {
            var io = ImGui.GetIO();
            io.DeltaTime = deltaSeconds;

            mouse.Poll();
            var mouseState = mouse.GetCurrentState();
            io.MousePos = new System.Numerics.Vector2(mousePosition.X, mousePosition.Y);

            io.MouseDown[0] = mouseState.Buttons[0];
            io.MouseDown[1] = mouseState.Buttons[1];
            io.MouseDown[2] = mouseState.Buttons[2];

            var scrollDelta = mouseState.Z - _scrollWheelValue;
            io.MouseWheel = scrollDelta > 0 ? 1 : scrollDelta < 0 ? -1 : 0;
            _scrollWheelValue = mouseState.Z;

            var keyboardState = keyboard.GetCurrentState();
            //io.KeysDown[] = keyboardState.AllKeys[(int)Key.Return]
            io.KeysDown[(int)Key.Tab] = keyboardState.IsPressed(Key.Tab);
            io.KeysDown[(int)Key.Left] = keyboardState.IsPressed(Key.Left);
            io.KeysDown[(int)Key.Right] = keyboardState.IsPressed(Key.Right);
            io.KeysDown[(int)Key.Up] = keyboardState.IsPressed(Key.Up);
            io.KeysDown[(int)Key.Down] = keyboardState.IsPressed(Key.Down);
            io.KeysDown[(int)Key.PageUp] = keyboardState.IsPressed(Key.PageUp);
            io.KeysDown[(int)Key.PageDown] = keyboardState.IsPressed(Key.PageDown);
            io.KeysDown[(int)Key.Home] = keyboardState.IsPressed(Key.Home);
            io.KeysDown[(int)Key.End] = keyboardState.IsPressed(Key.End);
            io.KeysDown[(int)Key.Delete] = keyboardState.IsPressed(Key.Delete);
            io.KeysDown[(int)Key.Back] = keyboardState.IsPressed(Key.Back);
            io.KeysDown[(int)Key.Return] = keyboardState.IsPressed(Key.Return);
            io.KeysDown[(int)Key.Escape] = keyboardState.IsPressed(Key.Escape);
            io.KeysDown[(int)Key.A] = keyboardState.IsPressed(Key.A);
            io.KeysDown[(int)Key.C] = keyboardState.IsPressed(Key.C);
            io.KeysDown[(int)Key.V] = keyboardState.IsPressed(Key.V);
            io.KeysDown[(int)Key.X] = keyboardState.IsPressed(Key.X);
            io.KeysDown[(int)Key.Y] = keyboardState.IsPressed(Key.Y);
            io.KeysDown[(int)Key.Z] = keyboardState.IsPressed(Key.Z);

            //for (int i = 0; i < keyboardState.PressedKeys.Count; i++)
            //{
            //    if (keyToChar.ContainsKey(keyboardState.PressedKeys[i]))
            //    {
            //        //char c = (char)keyboardState.PressedKeys[i];
            //        char c = keyToChar[keyboardState.PressedKeys[i]];
            //        //io.AddInputCharacter(c);
            //    }
            //}

            // TODO: Remove hard-coded size and use the scale factor
            io.DisplaySize = new System.Numerics.Vector2(1280, 720);
            io.DisplayFramebufferScale = new System.Numerics.Vector2(1, 1);

            ImGui.NewFrame();
        }

        public unsafe void EndFrame(Device device)
        {
            ImGui.Render();

            var drawData = ImGui.GetDrawData();

            drawData.ScaleClipRects(ImGui.GetIO().DisplayFramebufferScale);

            // update buffers -----------------
            if (drawData.TotalVtxCount == 0)
                return;

            int vtxOffset = 0;
            int idxOffset = 0;

            for (int n = 0; n < drawData.CmdListsCount; n++)
            {
                var cmdList = drawData.CmdListsRange[n];

                //int sizeOfT = Unsafe.SizeOf<ImDrawListPtr>();
                //var ptr = (byte*)drawData.CmdListsRange.Data + sizeOfT * i;
                //var cmdList = Unsafe.AsRef<ImDrawListPtr>(ptr);

                //var data = (byte*)drawData.CmdListsRange.Data;
                //var ptr = data + sizeof(void*) * 0;
                //var cmdList = Unsafe.Read<ImDrawListPtr>(ptr);

                if (vtxOffset * sizeof(ImDrawVert) + cmdList.VtxBuffer.Size * sizeof(ImDrawVert) > vertexData.Length)
                    Console.WriteLine("Too many verts...");
                if (idxOffset * sizeof(ushort) + cmdList.IdxBuffer.Size * sizeof(ushort) > indices.Length)
                    Console.WriteLine("Too many verts...");

                fixed (void* vtxDstPtr = &vertexData[vtxOffset * sizeof(ImDrawVert)])
                fixed (void* idxDstPtr = &indices[idxOffset * sizeof(ushort)])
                {
                    System.Buffer.MemoryCopy((void*)cmdList.VtxBuffer.Data, vtxDstPtr, vertexData.Length, cmdList.VtxBuffer.Size * sizeof(ImDrawVert));
                    System.Buffer.MemoryCopy((void*)cmdList.IdxBuffer.Data, idxDstPtr, indices.Length, cmdList.IdxBuffer.Size * sizeof(ushort));
                }

                //fixed (void* vtxDstPtr = &vertexData[vtxOffset * sizeof(ImDrawVert)])
                //fixed (void* idxDstPtr = &indices[idxOffset * sizeof(ushort)])
                //{
                //    System.Buffer.MemoryCopy((void*)cmdList.VtxBuffer.Data, vtxDstPtr, vertexData.Length * sizeof(ImDrawVert), cmdList.VtxBuffer.Size * sizeof(ImDrawVert));
                //    System.Buffer.MemoryCopy((void*)cmdList.IdxBuffer.Data, idxDstPtr, indices.Length * sizeof(ushort), cmdList.IdxBuffer.Size * sizeof(ushort));

                //    //var vertSize = cmdList.VtxBuffer.Size * sizeof(ImDrawVert);
                //    //var indSize = cmdList.IdxBuffer.Size * sizeof(ushort);
                //    //System.Buffer.MemoryCopy((void*)cmdList.VtxBuffer.Data, vtxDstPtr, vertSize, vertSize);
                //    //System.Buffer.MemoryCopy((void*)cmdList.IdxBuffer.Data, idxDstPtr, indSize, indSize);
                //}

                vtxOffset += cmdList.VtxBuffer.Size;
                idxOffset += cmdList.IdxBuffer.Size;
            }

            BufferExtensions.UpdateBuffer(device, vertexBuffer, vertexData);
            BufferExtensions.UpdateBuffer(device, indexBuffer, indices);

            // render command lists ---------------------

            device.ImmediateContext.VertexShader.Set(vertexShader);
            device.ImmediateContext.PixelShader.Set(pixelShader);

            device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            device.ImmediateContext.InputAssembler.InputLayout = inputLayout;
            device.ImmediateContext.InputAssembler.SetVertexBuffers(0, vertexBufferBinding);
            device.ImmediateContext.InputAssembler.SetIndexBuffer(indexBuffer, Format.R16_UInt, 0);
            device.ImmediateContext.VertexShader.SetConstantBuffer(0, constantBuffer);
            device.ImmediateContext.UpdateSubresource(ref constants, constantBuffer);
            //device.ImmediateContext.PixelShader.SetShaderResource(0, textureView);

            vtxOffset = 0;
            idxOffset = 0;

            for (int n = 0; n < drawData.CmdListsCount; n++)
            {
                var cmdList = drawData.CmdListsRange[n];

                for (int cmdi = 0; cmdi < cmdList.CmdBuffer.Size; cmdi++)
                {
                    var drawCmd = cmdList.CmdBuffer[cmdi];

                    if (!loadedTextures.ContainsKey(drawCmd.TextureId))
                    {
                        throw new InvalidOperationException($"Could not find a texture with id '{drawCmd.TextureId}', please check your bindings");
                    }

                    // Set scissor rect
                    device.ImmediateContext.Rasterizer.SetScissorRectangle((int)drawCmd.ClipRect.X,
                        (int)drawCmd.ClipRect.Y,
                        //(int)(drawCmd.ClipRect.Z - drawCmd.ClipRect.X),
                        //(int)(drawCmd.ClipRect.W - drawCmd.ClipRect.Y));
                        (int)drawCmd.ClipRect.Z,
                        (int)drawCmd.ClipRect.W);

                    // Update shader/material with texture
                    device.ImmediateContext.PixelShader.SetShaderResource(0, loadedTextures[drawCmd.TextureId]);

                    device.ImmediateContext.DrawIndexed((int)drawCmd.ElemCount, idxOffset, vtxOffset);

                    idxOffset += (int)drawCmd.ElemCount;
                }

                vtxOffset += cmdList.VtxBuffer.Size;
            }
        }
    }
}
