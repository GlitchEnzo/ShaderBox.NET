using SharpDX;
using SharpDX.Direct3D11;
using System.Drawing;
using System.Drawing.Imaging;
using Rectangle = System.Drawing.Rectangle;

namespace ShaderBox
{
    public static class TextureExtensions
    {
        public static Texture2D FromFile(Device device, string filepath)
        {
            //if (!System.IO.File.Exists(filepath))
            //{
            //    string possibleFullPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, filepath);
            //    if (System.IO.File.Exists(possibleFullPath))
            //    {
            //        filepath = possibleFullPath;
            //    }
            //}

            Texture2D texture;

            using (var bitmap = (Bitmap)Image.FromFile(filepath))
            {
                if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
                {
                    //bitmap = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), PixelFormat.Format32bppArgb);
                    System.Console.WriteLine("Possible format issue");
                }

                var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                texture = new SharpDX.Direct3D11.Texture2D(device, new SharpDX.Direct3D11.Texture2DDescription()
                {
                    Width = bitmap.Width,
                    Height = bitmap.Height,
                    ArraySize = 1,
                    BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource,
                    Usage = SharpDX.Direct3D11.ResourceUsage.Immutable,
                    CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                    Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                    MipLevels = 1,
                    OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
                    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                }, new SharpDX.DataRectangle(data.Scan0, data.Stride));
                bitmap.UnlockBits(data);
            }

            return texture;
        }

        // TODO: Fix the R and B channels being swapped.
        // The method here doesn't appear to work: https://github.com/sharpdx/SharpDX-Samples/blob/master/Desktop/Direct3D11.1/ScreenCapture/Program.cs
        public static Bitmap CopyTextureToBitmap(Device device, Texture2D gpuTexture)
        {
            var gpuDesc = gpuTexture.Description;

            var cpuDesc = new Texture2DDescription()
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = gpuDesc.Format, // Make this the proper swapped channels?
                //Format = Format.B8G8R8A8_UNorm,
                Width = gpuDesc.Width,
                Height = gpuDesc.Height,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Staging
            };

            var cpuTexture = new Texture2D(device, cpuDesc);

            device.ImmediateContext.CopyResource(gpuTexture, cpuTexture);
            var mappedSubresource = device.ImmediateContext.MapSubresource(cpuTexture, 0, MapMode.Read, MapFlags.None);
            var mappedSubresourceDataPointer = mappedSubresource.DataPointer;

            try
            {
                Bitmap bitmap = new Bitmap(gpuDesc.Width, gpuDesc.Height, PixelFormat.Format32bppArgb);
                BitmapData bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, gpuDesc.Width, gpuDesc.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);

                try
                {
                    Utilities.CopyMemory(bitmapData.Scan0, mappedSubresourceDataPointer, gpuDesc.Width * gpuDesc.Height * 4);
                    return bitmap;
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }
            }
            finally
            {
                device.ImmediateContext.UnmapSubresource(cpuTexture, 0);
            }
        }
    }
}
