using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Windows;
using System.Windows.Media.Imaging;
using TopWar.GameGUI.ShareMemoryS;

namespace TopWar.GameGUI.ImageProcessing
{
    public class ImageProcessor : IImageProcessor
    {
        private readonly MemoryMappedViewAccessor? _shareMemoryAccessor;
        public ImageProcessor(IShareMemory shareMemory)
        {
            _shareMemoryAccessor = shareMemory.MemoryAccessor;
        }
        public static byte[] CropImage(byte[] imageBytes, int x1, int y1, int x2, int y2)
        {
            using var ms = new MemoryStream(imageBytes);
            using var originalImage = new Bitmap(ms);
            // 计算切割区域的宽度和高度
            int width = x2 - x1;
            int height = y2 - y1;

            // 创建一个新的 Bitmap 来存储切割后的图像
            using var croppedImage = new Bitmap(width, height);
            using (var g = Graphics.FromImage(croppedImage))
            {
                // 在新的 Bitmap 上绘制切割后的图像
                g.DrawImage(originalImage, new Rectangle(0, 0, width, height), new Rectangle(x1, y1, width, height), GraphicsUnit.Pixel);
            }

            // 将切割后的图像转换为字节数组
            using var resultStream = new MemoryStream();
            croppedImage.Save(resultStream, ImageFormat.Bmp);
            return resultStream.ToArray();
        }
        public int WriteToSharedMemory(bool is32Bit = true)
        {
            byte[]? imageBytes = is32Bit ? GetBitmapData(1280, 760, true) : GetBitmapData(1280, 760, false);

            if (imageBytes == null)
                return 0;
            else
            {
                int length = imageBytes.Length;
                _shareMemoryAccessor!.Write(0, length); // 写入数据长度
                _shareMemoryAccessor.WriteArray(sizeof(int), imageBytes, 0, length);
                return imageBytes.Length;
            }
        }
        public static byte[]? GetBitmapData(int width, int height, bool is32Bit)
        {
            byte[]? pixelData = null;

            // 使用 Application.Current.Dispatcher 确保我们在主UI线程上执行
            Application.Current.Dispatcher.Invoke(() =>
            {
                var element = Application.Current.MainWindow;

                // 创建位图并渲染
                var bitmap = new RenderTargetBitmap(width, height, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
                bitmap.Render(element);

                // 转换位图
                pixelData = is32Bit ? ConvertTo32BitBmp(bitmap) : ConvertTo24BitBmp(bitmap);
            });

            return pixelData;
        }

        private static byte[] ConvertTo32BitBmp(RenderTargetBitmap bitmap)
        {
            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (var memoryStream = new System.IO.MemoryStream())
            {
                encoder.Save(memoryStream);
                return memoryStream.ToArray();
            }
        }

        private static byte[] ConvertTo24BitBmp(RenderTargetBitmap renderTargetBitmap)
        {
            // 将 RenderTargetBitmap 转换为 Bitmap
            Bitmap bitmap;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                encoder.Save(memoryStream);
                memoryStream.Position = 0;
                bitmap = new Bitmap(memoryStream);
            }

            // 创建一个新的 24 位 Bitmap 并绘制 32 位 Bitmap
            Bitmap bitmap24bpp = new Bitmap(bitmap.Width, bitmap.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(bitmap24bpp))
            {
                g.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
            }

            // 将 24 位 Bitmap 转换为字节数组
            using (MemoryStream resultStream = new MemoryStream())
            {
                bitmap24bpp.Save(resultStream, ImageFormat.Bmp);
                return resultStream.ToArray();
            }
        }


    }
}
