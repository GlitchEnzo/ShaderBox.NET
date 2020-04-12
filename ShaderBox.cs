using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DirectInput;
using SharpDX.DXGI;
using SharpDX.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using ImGuiNET;
using System.Windows.Forms;

namespace ShaderBox
{
    /// <summary>
    /// Draw triangle without buffers.
    /// See here: https://docs.microsoft.com/en-us/windows/desktop/direct3d11/d3d10-graphics-programming-guide-input-assembler-stage-no-buffers
    /// </summary>
    class Program : IDisposable
    {
        static void Main(string[] args)
        {
            //ShaderBoxPass pass = new ShaderBoxPass();
            //pass.Serialize();

            Program program = new Program();
            program.Run();
        }

        RenderForm renderForm;
        const int Width = 1280;
        const int Height = 720;

        Device device;
        DeviceContext context;
        SwapChain swapChain;
        RenderTargetView renderTargetView;
        SharpDX.Mathematics.Interop.RawColor4 clearColor = new SharpDX.Mathematics.Interop.RawColor4(100 / 255.0f, 149 / 255.0f, 237 / 255.0f, 1.0f);

        Stopwatch stopwatch = Stopwatch.StartNew();
        double time;

        Vector2 mousePosition;

        VertexShader vertexShader;
        PixelShader pixelShader;
        GeometryShader geometryShader;

        Viewport viewport;

        Keyboard keyboard;
        Mouse mouse;

        ObjModel model;
        ShaderBoxConstants constants;
        Buffer constantBuffer;
        Buffer vertexBuffer;
        VertexBufferBinding vertexBufferBinding;
        Buffer indexBuffer;
        ShaderBytecode vertexShaderBytecode;
        InputLayout inputLayout;

        ImGuiDx11 imGuiRenderer;

        Texture2D renderOutput;
        RenderTargetView renderOutputView;
        IntPtr renderOutputId;

        ShaderBoxProject currentProject;

        //public static char GetChar(KeyEventArgs args)
        //{
        //    int keyValue = args.KeyValue;
        //    if (!args.Shift && keyValue >= (int)Keys.A  && keyValue <= (int)Keys.Z)
        //    {
        //        return (char)(keyValue + 32);
        //    }
        //    return (char)keyValue;
        //}

        public Program()
        {
            currentProject = new ShaderBoxProject();
            currentProject.Textures.Add(new ShaderBoxTexture("Textures\\debug_grid.jpg"));
            currentProject.Meshes.Add(new ShaderBoxMesh("Models//bunny.obj"));
            var pass = new ShaderBoxPass();
            var textureBinding = new ShaderBoxTextureBinding();
            textureBinding.shaderType = ShaderType.Pixel;
            textureBinding.slotId = 0;
            textureBinding.texture = currentProject.Textures[0];
            pass.TextureBindings.Add(textureBinding);
            currentProject.Passes.Add(pass);

            renderForm = new RenderForm("ShaderBox")
            {
                ClientSize = new System.Drawing.Size(Width, Height),
                AllowUserResizing = false
            };

            renderForm.MouseMove += (sender, args) =>
            {
                mousePosition.X = args.X;
                mousePosition.Y = args.Y;
            };

            //renderForm.KeyDown += (sender, args) =>
            //{
            //    var io = ImGui.GetIO();
            //    io.AddInputCharacter((uint)GetChar(args));
            //};

            renderForm.KeyPress += (sender, args) =>
            {
                var io = ImGui.GetIO();
                io.AddInputCharacter((uint)args.KeyChar);
            };

            InitializeDeviceResources();
            InitializeShaders("vs.hlsl", "ps.hlsl");

            // load a simple OBJ model and initialize all of the needed buffers and matrices
            model = ObjModel.LoadObj("Models//bunny.obj");
            //var bounds = ObjModel.FindBounds(model);

            constants.Model = Matrix.Identity;
            constants.View = Matrix.LookAtLH(new Vector3(0, 0.85f, -1.85f), new Vector3(0, 0.85f, 0), new Vector3(0, 1, 0));
            constants.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 3f, Width / (float)Height, 0.01f, 1000.0f);

            var viewProj = Matrix.Multiply(constants.View, constants.Projection);
            viewProj.Transpose();
            constants.ModelViewProjection = viewProj;

            constantBuffer = BufferExtensions.CreateConstantBufferFromStruct(device, constants);
            vertexBuffer = BufferExtensions.CreateVertexBuffer(device, model);
            vertexBufferBinding = new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<VertexPositionTextureNormal>(), 0);
            indexBuffer = BufferExtensions.CreateIndexBuffer(device, model);

            var elements = new[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0, InputClassification.PerVertexData, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, 24, 0, InputClassification.PerVertexData, 0),
                //new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 12, 0),
            };
            inputLayout = new InputLayout(device, vertexShaderBytecode, elements);

            // ImGUI init
            imGuiRenderer = new ImGuiDx11();
            imGuiRenderer.Initialize(device);

            //Texture2D testTexure = TextureExtensions.FromFile(device, "Textures\\debug_grid.jpg");

            currentProject.Initialize(device);
            foreach (var texture in currentProject.Textures)
            {
                texture.bindingID = imGuiRenderer.BindTexture(texture.texture);
            }

            //testTextureId = imGuiRenderer.BindTexture(testTexure);
            renderOutputId = imGuiRenderer.BindTexture(renderOutput);

            filePicker = ImGuiFilePicker.GetFilePicker(file, @"C:\Users\oneze\source\repos\ShaderBox\ShaderBox\bin\Debug");
        }

        private void InitializeDeviceResources()
        {
            ModeDescription backBufferDesc = new ModeDescription(Width, Height, new Rational(60, 1), Format.R8G8B8A8_UNorm);

            SwapChainDescription swapChainDesc = new SwapChainDescription()
            {
                ModeDescription = backBufferDesc,
                SampleDescription = new SampleDescription(1, 0),
                Usage = Usage.RenderTargetOutput,
                BufferCount = 1,
                OutputHandle = renderForm.Handle,
                IsWindowed = true
            };

            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.Debug, swapChainDesc, out device, out swapChain);
            context = device.ImmediateContext;

            viewport = new Viewport(0, 0, Width, Height);
            context.Rasterizer.SetViewport(viewport);

            var rasteriserDesc = new RasterizerStateDescription
            {
                //CullMode = CullMode.Back,
                CullMode = CullMode.None,
                //CullMode = CullMode.Front,
                FillMode = FillMode.Solid,
                IsDepthClipEnabled = true,
                IsFrontCounterClockwise = true,
                IsScissorEnabled = true,
                IsMultisampleEnabled = false,
                IsAntialiasedLineEnabled = false,
                DepthBias = 0,
                DepthBiasClamp = 0f,
                SlopeScaledDepthBias = 0f,
            };
            context.Rasterizer.State = new RasterizerState(device, rasteriserDesc);

            // set up alpha blending
            var blendStateDescription = new BlendStateDescription();
            blendStateDescription.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            blendStateDescription.RenderTarget[0].BlendOperation = BlendOperation.Add;
            blendStateDescription.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
            blendStateDescription.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
            blendStateDescription.RenderTarget[0].IsBlendEnabled = true;
            blendStateDescription.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            blendStateDescription.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
            blendStateDescription.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
            blendStateDescription.AlphaToCoverageEnable = false;
            blendStateDescription.IndependentBlendEnable = false;

            var blendState = new BlendState(device, blendStateDescription);
            context.OutputMerger.SetBlendState(blendState, new SharpDX.Mathematics.Interop.RawColor4(), -1);

            using (Texture2D backBuffer = swapChain.GetBackBuffer<Texture2D>(0))
            {
                renderTargetView = new RenderTargetView(device, backBuffer);

                var desc = backBuffer.Description;
                desc.BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource;
                renderOutput = new Texture2D(device, desc);
                renderOutputView = new RenderTargetView(device, renderOutput);
            }

            var directInput = new DirectInput();
            keyboard = new Keyboard(directInput);
            keyboard.Properties.BufferSize = 128;
            keyboard.Acquire();

            mouse = new Mouse(directInput);
            mouse.Properties.AxisMode = DeviceAxisMode.Absolute;
            mouse.Properties.BufferSize = 128;
            mouse.Acquire();
        }

        void InitializeShaders(string vsPath = "vs.hlsl", string psPath = "ps.hlsl", string gsPath = null)
        {
            vertexShaderText = System.IO.File.ReadAllText("Shaders//" + vsPath);
            var vsCompilationResult = ShaderBytecode.Compile(vertexShaderText, "main", "vs_5_0", ShaderFlags.Debug);
            //var vsCompilationResult = ShaderBytecode.CompileFromFile("Shaders//" + vsPath, "main", "vs_5_0", ShaderFlags.Debug);
            vertexShaderBytecode = vsCompilationResult.Bytecode;
            vertexShader = new VertexShader(device, vsCompilationResult.Bytecode);

            pixelShaderText = System.IO.File.ReadAllText("Shaders//" + psPath);
            var psCompilationResult = ShaderBytecode.Compile(pixelShaderText, "main", "ps_5_0", ShaderFlags.Debug);
            //var psCompilationResult = ShaderBytecode.CompileFromFile("Shaders//" + psPath, "main", "ps_5_0", ShaderFlags.Debug);
            pixelShader = new PixelShader(device, psCompilationResult.Bytecode);

            if (!string.IsNullOrEmpty(gsPath))
            {
                var gsCompilationResult = ShaderBytecode.CompileFromFile("Shaders//" + gsPath, "main", "gs_5_0", ShaderFlags.Debug);
                geometryShader = new GeometryShader(device, gsCompilationResult.Bytecode);
            }
        }

        string errorText = "";

        public void CompileVertexShader()
        {
            try
            {
                errorText = string.Empty;

                vertexShader.Dispose();

                var vsCompilationResult = ShaderBytecode.Compile(vertexShaderText, "main", "vs_5_0", ShaderFlags.Debug);
                vertexShader = new VertexShader(device, vsCompilationResult.Bytecode);
            }
            catch (Exception e)
            {
                errorText = e.Message;
            }
        }

        public void CompilePixelShader()
        {
            try
            {
                errorText = string.Empty;

                pixelShader.Dispose();

                var psCompilationResult = ShaderBytecode.Compile(pixelShaderText, "main", "ps_5_0", ShaderFlags.Debug);
                //var psCompilationResult = ShaderBytecode.CompileFromFile("Shaders//" + psPath, "main", "ps_5_0", ShaderFlags.Debug);
                pixelShader = new PixelShader(device, psCompilationResult.Bytecode);
            }
            catch (Exception e)
            {
                errorText = e.Message;
            }
        }

        public void Run()
        {
            RenderLoop.Run(renderForm, RenderCallback);
        }

        private void RenderCallback()
        {
            // exit when the escape key is pressed
            keyboard.Poll();
            var keyboardBufferData = keyboard.GetBufferedData();
            if (keyboardBufferData.Any(x => x.Key == Key.Escape))
            {
                renderForm.Close();
            }

            // update time
            var oldTime = time;
            time = stopwatch.Elapsed.TotalSeconds;
            var frameTime = (time - oldTime);

            // update framerate in the window title
            renderForm.Text = string.Format("ShaderBox - {0}", (1.0f / frameTime).ToString("F1"));

            DrawRabbit();

            context.OutputMerger.SetRenderTargets(renderTargetView);
            context.ClearRenderTargetView(renderTargetView, clearColor);

            DrawUI((float)frameTime);

            swapChain.Present(0, PresentFlags.None);
        }

        private void DrawRabbit()
        {
            context.OutputMerger.SetRenderTargets(renderOutputView);
            context.ClearRenderTargetView(renderOutputView, clearColor);

            context.VertexShader.Set(vertexShader);
            context.PixelShader.Set(pixelShader);

            if (geometryShader != null)
            {
                context.GeometryShader.Set(geometryShader);
            }

            //context.InputAssembler.InputLayout = null; // create no buffers and have the vertex shader output verts based on the SV_VertexID

            // draw triangles
            //context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            //context.Draw(3, 0); // triangle = 3 verts
            //context.Draw(6, 0); // quad = 2 triangles
            //context.Draw(36, 0); // cube = 12 triangles

            // draw lines
            //context.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
            //context.Draw(6, 0);

            // draw lines with adjacency
            //context.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineListWithAdjacency;
            //context.Draw(12, 0);

            context.Rasterizer.SetScissorRectangle(0, 0, Width, Height);

            // draw indexed triangles
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.InputAssembler.InputLayout = inputLayout;
            context.InputAssembler.SetVertexBuffers(0, vertexBufferBinding);
            context.InputAssembler.SetIndexBuffer(indexBuffer, Format.R32_UInt, 0);
            context.VertexShader.SetConstantBuffer(0, constantBuffer);
            context.UpdateSubresource(ref constants, constantBuffer);
            context.DrawIndexed(model.Indices.Length, 0, 0);
        }

        bool showRenderOutput = true;
        bool showErrorWindow = true;
        bool showShaderWindow = true;
        bool showProjectWindow = true;
        bool showTexturesWindow = true;
        bool showDemoWindow = false;

        private void DrawUI(float frameTime)
        {
            imGuiRenderer.StartFrame((float)frameTime, mouse, mousePosition, keyboard);

            ImGui.StyleColorsLight();

            ImGui.SetNextWindowPos(new System.Numerics.Vector2(0, 0), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(1280, 720), ImGuiCond.Always);
            ImGui.Begin("Demo Window", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoBringToFrontOnFocus);
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("New")) { }
                    if (ImGui.MenuItem("Open", "Ctrl+O")) { }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Edit"))
                {
                    if (ImGui.MenuItem("Undo", "CTRL+Z")) { }
                    if (ImGui.MenuItem("Redo", "CTRL+Y", false, false)) { }  // Disabled item
                    ImGui.Separator();
                    if (ImGui.MenuItem("Cut", "CTRL+X")) { }
                    if (ImGui.MenuItem("Copy", "CTRL+C")) { }
                    if (ImGui.MenuItem("Paste", "CTRL+V")) { }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Window"))
                {
                    if (ImGui.MenuItem("Render Output")) { showRenderOutput = !showRenderOutput; }
                    if (ImGui.MenuItem("Shaders")) { showShaderWindow = !showShaderWindow; }
                    if (ImGui.MenuItem("Errors")) { showErrorWindow = !showErrorWindow; }
                    if (ImGui.MenuItem("Project")) { showProjectWindow = !showProjectWindow; }
                    if (ImGui.MenuItem("Textures")) { showTexturesWindow = !showTexturesWindow; }
                    if (ImGui.MenuItem("Demo Window")) { showDemoWindow = !showDemoWindow; }
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }
            //if (ImGui.Button("Hello World"))
            //    showDemoWindow = !showDemoWindow;
            //ImGui.Image(renderOutputId, new System.Numerics.Vector2(512, 512), System.Numerics.Vector2.Zero, System.Numerics.Vector2.One, System.Numerics.Vector4.One, System.Numerics.Vector4.One);
            ImGui.End();

            if (showErrorWindow)
                DrawErrorWindow();

            if (showShaderWindow)
                DrawShaderWindow();

            if (showProjectWindow)
                DrawProjectWindow();

            if (showTexturesWindow)
                DrawTexturesWindow();

            if (showDemoWindow)
                ImGui.ShowDemoWindow(ref showDemoWindow);

            //ImGui.Begin("Second Window");
            //if (ImGui.Button("Button 2"))
            //    showDemoWindow = !showDemoWindow;
            ////ImGui.Separator();
            //DrawSplitter(true, 50, ref topSize, ref bottomSize, 10, 10);
            //if (ImGui.Button("Button 3"))
            //    showDemoWindow = !showDemoWindow;
            //ImGui.End();

            //ImGui.Begin("Splitter test");
            //    float w = 200.0f;
            //    float h = 300.0f;
            //    ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new System.Numerics.Vector2(0, 0));
            //    ImGui.BeginChild("child1", new System.Numerics.Vector2(w, h), true);
            //    ImGui.EndChild();
            //    ImGui.SameLine();
            //    ImGui.InvisibleButton("vsplitter", new System.Numerics.Vector2(8.0f, h));
            //    if (ImGui.IsItemActive())
            //        w += ImGui.GetIO().MouseDelta.X;
            //    ImGui.SameLine();
            //    ImGui.BeginChild("child2", new System.Numerics.Vector2(0, h), true);
            //    ImGui.EndChild();
            //    ImGui.InvisibleButton("hsplitter", new System.Numerics.Vector2(-1, 8.0f));
            //    if (ImGui.IsItemActive())
            //        h += ImGui.GetIO().MouseDelta.Y;
            //    ImGui.BeginChild("child3", new System.Numerics.Vector2(0, 0), true);
            //    ImGui.EndChild();
            //    ImGui.PopStyleVar();
            //ImGui.End();

            //if (showDemoWindow)
            //    ImGui.ShowDemoWindow(ref showDemoWindow);

            if (showRenderOutput)
                DrawRenderOutputWindow();

            //ImGui.BeginTabBar("TestTab", ImGuiTabBarFlags.None);
            //ImGui.EndTabBar();

            imGuiRenderer.EndFrame(device);
        }

        //float topSize = 50;
        //float bottomSize = 50;

        string vertexShaderText = "";
        string pixelShaderText = "";

        private void DrawMainMenu()
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("New")) { }
                    if (ImGui.MenuItem("Open", "Ctrl+O")) { }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Edit"))
                {
                    if (ImGui.MenuItem("Undo", "CTRL+Z")) { }
                    if (ImGui.MenuItem("Redo", "CTRL+Y", false, false)) { }  // Disabled item
                    ImGui.Separator();
                    if (ImGui.MenuItem("Cut", "CTRL+X")) { }
                    if (ImGui.MenuItem("Copy", "CTRL+C")) { }
                    if (ImGui.MenuItem("Paste", "CTRL+V")) { }
                    ImGui.EndMenu();
                }
                ImGui.EndMainMenuBar();
            }
        }

        private void DrawSplitter(bool split_vertically, float thickness, ref float size0, ref float size1, float min_size0, float min_size1, float size = -1.0f)
        {
            var backup_pos = ImGui.GetCursorPos();

            if (split_vertically)
                ImGui.SetCursorPosY(backup_pos.Y + size0);
            else
                ImGui.SetCursorPosX(backup_pos.X + size0);

            ImGui.PushStyleColor(ImGuiCol.Button, System.Numerics.Vector4.Zero);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, System.Numerics.Vector4.Zero);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new System.Numerics.Vector4(0.6f, 0.6f, 0.6f, 0.1f));

            ImGui.Button("##Splitter", new System.Numerics.Vector2(split_vertically ? thickness : size, !split_vertically ? thickness : size));
            ImGui.PopStyleColor(3);

            ImGui.SetItemAllowOverlap(); // This is to allow having other buttons OVER our splitter. 

            if (ImGui.IsItemActive())
            {
                float mouse_delta = split_vertically ? ImGui.GetMouseDragDelta().Y : ImGui.GetMouseDragDelta().X;

                // Minimum pane size
                if (mouse_delta < min_size0 - size0)
                    mouse_delta = min_size0 - size0;
                if (mouse_delta > size1 - min_size1)
                    mouse_delta = size1 - min_size1;

                // Apply resize
                size0 += mouse_delta;
                size1 -= mouse_delta;
            }
            ImGui.SetCursorPos(backup_pos);
        }

        private void DrawShaderWindow()
        {
            if (ImGui.Begin("Shaders"))
            {
                if (ImGui.BeginTabBar("Shaders"))
                {
                    if (ImGui.BeginTabItem("Vertex Shader"))
                    {
                        if (ImGui.Button("Compile"))
                        {
                            CompileVertexShader();
                        }
                        float width = ImGui.GetWindowWidth();
                        float height = ImGui.GetWindowHeight();
                        ImGui.InputTextMultiline("##vertexShader", ref vertexShaderText, 1024 * 128, new System.Numerics.Vector2(width - 10, height - 75), ImGuiInputTextFlags.AllowTabInput | ImGuiInputTextFlags.Multiline);
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Pixel Shader"))
                    {
                        if (ImGui.Button("Compile"))
                        {
                            CompilePixelShader();
                        }
                        float width = ImGui.GetWindowWidth();
                        float height = ImGui.GetWindowHeight();
                        ImGui.InputTextMultiline("##pixelShader", ref pixelShaderText, 1024 * 128, new System.Numerics.Vector2(width - 10, height - 75), ImGuiInputTextFlags.AllowTabInput | ImGuiInputTextFlags.Multiline);
                        ImGui.EndTabItem();
                    }

                    ImGui.EndTabBar();
                }
                ImGui.End();
            }
        }

        private void DrawErrorWindow()
        {
            ImGui.Begin("Errors");
            if (ImGui.Button("Clear"))
            {
                errorText = string.Empty;
            }
            float width = ImGui.GetWindowWidth();
            float height = ImGui.GetWindowHeight();
            //ImGui.InputTextMultiline(string.Empty, ref pixelShaderText, 1024 * 128, new System.Numerics.Vector2(width - 10, height - 75), ImGuiInputTextFlags.AllowTabInput | ImGuiInputTextFlags.Multiline);
            ImGui.Text(errorText);
            ImGui.End();
        }

        private void DrawRenderOutputWindow()
        {
            ImGui.Begin("Render Output");
            float width = ImGui.GetWindowWidth();
            float height = ImGui.GetWindowHeight();
            // TODO: Use correct aspect ratio instead of stretching
            ImGui.Image(renderOutputId, new System.Numerics.Vector2(width, height - 40), System.Numerics.Vector2.Zero, System.Numerics.Vector2.One, System.Numerics.Vector4.One, System.Numerics.Vector4.One);
            ImGui.End();
        }

        private void DrawProjectWindow()
        {
            ImGui.Begin("Project");
            DrawCurrentProject();
            //bool open = ImGui.TreeNode("Pass 1");
            //if (ImGui.BeginPopupContextItem())
            //{
            //    if (ImGui.MenuItem("Add New")) { }
            //    ImGui.EndPopup();
            //}
            //if (open)
            //{
            //    if (ImGui.TreeNode("Meshes"))
            //    {
            //        ImGui.TreePop();
            //    }
            //    if (ImGui.TreeNode("Textures"))
            //    {
            //        ImGui.TreePop();
            //    }
            //    if (ImGui.TreeNode("Buffers"))
            //    {
            //        ImGui.TreePop();
            //    }
            //    ImGui.TreePop();
            //}

            //if (ImGui.TreeNode("Pass 2"))
            //{
            //    if (ImGui.TreeNode("Meshes"))
            //    {
            //        ImGui.TreePop();
            //    }
            //    if (ImGui.TreeNode("Textures"))
            //    {
            //        ImGui.TreePop();
            //    }
            //    if (ImGui.TreeNode("Buffers"))
            //    {
            //        ImGui.TreePop();
            //    }
            //    ImGui.TreePop();
            //}
            //ImGui.TreeNode("Pass 3");

            ImGui.End();
        }

        private void DrawCurrentProject()
        {
            foreach (var pass in currentProject.Passes)
            {
                var isPassOpen = ImGui.TreeNode(pass.name);
                if (ImGui.BeginPopupContextItem())
                {
                    if (ImGui.MenuItem("Add New"))
                    {
                    }
                    ImGui.EndPopup();
                }
                if (isPassOpen)
                {
                    //if (ImGui.TreeNode("Meshes"))
                    //{
                    //    ImGui.TreePop();
                    //}
                    var isTexturesNodeOpen = ImGui.TreeNode("Bound Textures");
                    if (ImGui.BeginPopupContextItem())
                    {
                        if (ImGui.MenuItem("Bind New Texture"))
                        {
                            var textureBinding = new ShaderBoxTextureBinding();
                            textureBinding.shaderType = ShaderType.Pixel;
                            textureBinding.slotId = 0;
                            textureBinding.texture = currentProject.Textures[0];
                            pass.TextureBindings.Add(textureBinding);
                        }
                        ImGui.EndPopup();
                    }
                    if (isTexturesNodeOpen)
                    {
                        //foreach (var textureBinding in pass.TextureBindings)
                        for (int i = 0; i < pass.TextureBindings.Count; i++)
                        {
                            var textureBinding = pass.TextureBindings[i];

                            ImGui.PushID(i);

                            ImGui.SetNextItemWidth(100);
                            int currentItem = (int)textureBinding.shaderType;
                            if (ImGui.Combo("##shaderType", ref currentItem, ShaderTypeEx.GetShaderTypeNamesAsSingleStringSeparatedByZeroes()))
                            {
                                textureBinding.shaderType = (ShaderType)currentItem;
                            }

                            ImGui.SameLine();
                            ImGui.SetNextItemWidth(75);
                            int slotId = textureBinding.slotId;
                            if (ImGui.InputInt("##slotId", ref slotId))
                            {
                                textureBinding.slotId = slotId;
                            }

                            ImGui.SameLine();
                            //ImGui.Image(textureBinding.texture.bindingID, new System.Numerics.Vector2(32, 32), System.Numerics.Vector2.Zero, System.Numerics.Vector2.One, System.Numerics.Vector4.One, System.Numerics.Vector4.One);
                            if (ImGui.ImageButton(textureBinding.texture.bindingID, new System.Numerics.Vector2(32, 32)))
                            {

                            }

                            ImGui.PopID();
                        }
                        ImGui.TreePop();
                    }
                    //if (ImGui.TreeNode("Buffers"))
                    //{
                    //    ImGui.TreePop();
                    //}
                    ImGui.TreePop();
                }
            }
        }

        object file = new object();
        ImGuiFilePicker filePicker;
        string selectedFile;

        int selectedTexture;

        private void DrawTexturesWindow()
        {
            ImGui.Begin("Textures");

            filePicker.Draw(ref selectedFile);
            ImGui.SameLine();
            if (ImGui.Button("Add Selected Texture"))
            {
                var sbTexture = new ShaderBoxTexture(selectedFile);
                sbTexture.Initialize(device);

                sbTexture.bindingID = imGuiRenderer.BindTexture(sbTexture.texture);

                currentProject.Textures.Add(sbTexture);

            }

            //foreach (var texture in currentProject.Textures)
            for (int i = 0; i < currentProject.Textures.Count; i++)
            {
                ImGui.PushID(i);

                var texture = currentProject.Textures[i];
                //ImGui.PushStyleVar(ImGuiStyleVar.SelectableTextAlign, new System.Numerics.Vector2(0.5f, 0.5f));
                var cursorPos = ImGui.GetCursorPos();
                var width = ImGui.GetWindowWidth();
                if (ImGui.Selectable("##selectableTexture", i == selectedTexture, ImGuiSelectableFlags.None, new System.Numerics.Vector2(width, 150)))
                {
                    selectedTexture = i;
                }
                //ImGui.SameLine();
                ImGui.SetCursorPos(cursorPos);
                ImGui.Text(texture.filepath);
                //ImGui.SameLine();
                ImGui.Image(texture.bindingID, new System.Numerics.Vector2(128, 128), System.Numerics.Vector2.Zero, System.Numerics.Vector2.One, System.Numerics.Vector4.One, System.Numerics.Vector4.One);
                //ImGui.PopStyleVar();

                ImGui.PopID();
            }

            ImGui.End();
        }

        public void Dispose()
        {
            renderForm.Dispose();
        }
    }
}
