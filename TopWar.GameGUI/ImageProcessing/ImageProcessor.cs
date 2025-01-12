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
        //private readonly string _mapName;
        //private readonly MemoryMappedFile? _mmf;
        //private readonly MemoryMappedViewAccessor? _accessor = null;

        private readonly MemoryMappedViewAccessor _accessor;
        private readonly ShareMemory _shareMemory; // 持有对 ShareMemory 的引用

        public ImageProcessor(IShareMemory shareMemory)
        //public ImageProcessor(string serverID)
        {
            //_mapName = $"SharedMemoryScreenshot{serverID}";
            //_mmf = MemoryMappedFile.CreateOrOpen(_mapName, 5 * 1024 * 1024);
            //_accessor = _mmf.CreateViewAccessor();
            //_accessor = shareMemory.MemoryAccessor;

            _shareMemory = (ShareMemory)shareMemory; // 保持对 ShareMemory 的引用
            _accessor = shareMemory.MemoryAccessor;
            //为什么必须加上面的一行 光下面的 不行 会丢失引用
            //首先，让我们理解每一行代码的作用和必要性。

            //_shareMemory = (ShareMemory)shareMemory;
            //这行代码执行了一个显式转换（也称为强制转换）。这里，shareMemory 变量原本是一个 IShareMemory 类型的引用（基于变量名和后文的上下文推断）。通过强制转换，您告诉编译器：“我知道 shareMemory 实际上引用的是一个 ShareMemory 类型的对象，所以请将它当作 ShareMemory 类型来处理。”

            //这种转换是必要的，因为 C# 不允许隐式地将接口引用转换为具体实现类型的引用，除非您确信这种转换是安全的（即接口引用实际上指向的是具体实现类型的实例）。

            //_accessor = shareMemory.MemoryAccessor;
            //这行代码从 shareMemory 对象中获取 MemoryAccessor 属性（或方法返回值，取决于它是属性还是方法）。重要的是要注意这里使用的是转换前的 shareMemory 变量，它是一个 IShareMemory 类型的引用。

            //如果 MemoryAccessor 是一个属性，并且它的返回类型不是 IShareMemory 或其接口中定义的其他类型，而是一个具体类型或与 ShareMemory 类紧密相关的类型，那么您可能会认为没有必要进行前面的显式转换。然而，这里的关键在于理解 MemoryAccessor 返回的对象是否与 shareMemory 的具体实现类型（ShareMemory）有直接的关联。

            //现在，来解释为什么“光下面的不行”：

            //如果您只写第二行代码，并且后续代码中需要使用 _shareMemory 变量来引用 ShareMemory 类型的对象，那么您将无法做到，因为 shareMemory 变量是一个 IShareMemory 类型的引用，不能直接当作 ShareMemory 类型来使用。
            //另外，即使 MemoryAccessor 返回的对象与 ShareMemory 类有关，但如果没有第一行代码，您也无法通过 _shareMemory 变量来访问 ShareMemory 类的任何特定于实现的方法或属性（除非这些方法或属性也定义在 IShareMemory 接口中）。
            //“会丢失引用”这个说法可能有些误导。实际上，您并没有丢失对 shareMemory 所引用对象的引用；您只是没有以正确的类型（ShareMemory）来引用它。这意味着您无法调用 ShareMemory 类中定义的任何非接口方法或访问其非接口属性。
            //因此，第一行代码是必要的，如果您需要在后续代码中以 ShareMemory 类型的身份来处理 shareMemory 所引用的对象。
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
        private void 测试截图然后写入共享内存()
        {
            while (true)
            {
                WriteToSharedMemory(false);
                
                Thread.Sleep(2000);
            }
        }
        private void 测试读取共享内存数据然后写入本地文件(int serverID)
        {
            string mapName = $"SharedMemoryScreenshot{serverID}";
            long lengthToRead = 2918454; // 要读取的字节数
            try
            {
                // 打开现有的内存映射文件
                using (var mmf = System.IO.MemoryMappedFiles.MemoryMappedFile.OpenExisting(mapName))
                {
                    // 创建一个视图访问器，用于读取数据
                    using (var accessor = mmf.CreateViewAccessor(0, lengthToRead, System.IO.MemoryMappedFiles.MemoryMappedFileAccess.Read))
                    {
                        // 创建一个字节数组来存储读取的数据
                        byte[] byteArray = new byte[lengthToRead];

                        // 从内存映射文件中读取数据到字节数组中
                        int bytesRead = accessor.ReadArray(0, byteArray, 0, byteArray.Length);

                        // 注意：对于 ReadArray 方法，上面的调用实际上是不正确的，因为 ReadArray 
                        // 并不是 MemoryMappedViewAccessor 的一个方法。正确的做法是使用 Read 方法，如下：
                        // int bytesRead = accessor.Read(0, byteArray, 0, byteArray.Length);
                        // 但由于 MemoryMappedViewAccessor 没有 ReadArray 方法，上面的 ReadArray 调用会编译失败。
                        // 使用下面的正确 Read 方法调用替换上面的 ReadArray 调用。

                        // 正确的 Read 方法调用（已经替换上面的错误调用）
                        // 注意：实际上，MemoryMappedViewAccessor 没有直接读取到 byte[] 的 ReadArray 方法，
                        // 这里应该使用非泛型的 Read 方法，并传入 byte 数组和相应的参数。
                        // 但由于 MemoryMappedViewAccessor 的 Read 方法是用于读取基本数据类型的单个值，
                        // 而不是直接读取到 byte[] 中，我们需要使用另一个类：MemoryMappedViewStream。

                        // 然而，为了说明如果有一个类似 ReadArray 的方法（实际上不存在），
                        // 下面的代码假设了这样一个方法的存在，并展示了如何调用它（但实际上你应该使用 MemoryMappedViewStream）。

                        // 由于 MemoryMappedViewAccessor 没有这样的方法，下面的代码将不会编译。
                        // 正确的做法是使用 MemoryMappedViewStream，如下面的替代方案所示。

                        // 假设的 ReadArray 方法调用（实际不存在，仅用于说明）
                        // bytesRead 应该等于 byteArray.Length，如果读取成功的话。
                        // 但由于这个方法不存在，我们需要使用其他方法。

                        // 正确的替代方案是使用 MemoryMappedViewStream，如下：

                        // 使用 MemoryMappedViewStream 读取数据到 byte[]
                        using (var stream = mmf.CreateViewStream(0, lengthToRead, System.IO.MemoryMappedFiles.MemoryMappedFileAccess.Read))
                        {
                            int bytesReadStream = stream.Read(byteArray, 0, byteArray.Length);

                            System.IO.File.WriteAllBytes("测试读取共享内存数据然后写入本地文件.bmp", byteArray);
                            // 此时，byteArray 包含了从共享内存中读取的数据
                            // bytesReadStream 应该等于 byteArray.Length，如果读取成功的话。
                        }

                        // 注意：由于我们使用了 MemoryMappedViewStream，上面的 ReadArray 调用已经被替换。
                        // bytesRead 变量在这里不再使用，因为我们已经使用了 bytesReadStream 来接收读取的字节数。

                        // 在这里处理 byteArray，例如保存到文件等。
                    }
                }
            }
            catch (System.IO.FileNotFoundException)
            {
                Console.WriteLine($"内存映射文件 '{mapName}' 不存在。");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"没有足够的权限访问内存映射文件 '{mapName}'。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"读取共享内存时出错: {ex.Message}");
            }
        }

        public int WriteToSharedMemory(bool is32Bit = true)
        {
            byte[]? imageBytes = is32Bit ? GetBitmapData(1280, 760, true) : GetBitmapData(1280, 760, false);

            if (imageBytes == null)
            {
                return 0;
            }
            else
            {
                int length = imageBytes.Length;

                //先写入长度,后面加上数据
                //_shareMemoryAccessor!.Write(0, length); // 写入数据长度
                //_shareMemoryAccessor.WriteArray(sizeof(int), imageBytes, 0, length);

                //直接写入数据,头部不加数据长度了
                //string str = "abcdefg";
                //byte[] make = Encoding.UTF8.GetBytes(str);
                //_shareMemoryAccessor!.WriteArray(0, make, 0, make.Length);
                //System.IO.File.WriteAllBytes("测试截图然后写入共享内存.bmp", make);
                _accessor.WriteArray(0, imageBytes, 0, length);
                //System.IO.File.WriteAllBytes("测试共享内存imageBytes.bmp", imageBytes);
                //测试读取共享内存数据然后写入本地文件(3606);

                //string _mapName = $"SharedMemoryScreenshot3606";
                //MemoryMappedFile _mmf = MemoryMappedFile.CreateOrOpen(_mapName, 5 * 1024 * 1024);
                //MemoryMappedViewAccessor _accessor = _mmf.CreateViewAccessor();
                //_accessor!.WriteArray(0, make, 0, make.Length);

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
