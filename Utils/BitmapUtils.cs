using MyPCL.Modules;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace MyPCL.Utils
{
    /// <summary>
    /// 一个万能的自动图片类型转换工具类
    /// </summary>
    public class BitmapUtils
    {
        /// <summary>
        /// 位图缓存。
        /// </summary>
        public static ConcurrentDictionary<string, BitmapUtils> BitmapCache = new ConcurrentDictionary<string, BitmapUtils>();

        /// <summary>
        /// 存储的图片
        /// </summary>
        public Bitmap Pic;

        /// <summary>
        /// 自动类型转换<br/>
        /// 支持的类：Image，ImageSource，Bitmap，ImageBrush，BitmapSource
        /// </summary>
        /// <param name="image"></param>
        public static implicit operator BitmapUtils(Image image)
        {
            if(image == null)
            {
                return null;
            }
            return new BitmapUtils(image);
        }
        public static implicit operator BitmapUtils(ImageSource image)
        {
            if (image == null)
            {
                return null;
            }
            return new BitmapUtils(image);
        }
        public static implicit operator BitmapUtils(ImageBrush image)
        {
            if (image == null) return null;
            return new BitmapUtils(image);
        }
        public static implicit operator BitmapUtils(Bitmap image)
        {
            if (image == null) return null;
            return new BitmapUtils(image);
        }
        public static implicit operator Bitmap(BitmapUtils image)
        {
            if (image == null)
            {
                return null;
            }
            return image.Pic;
        }
        public static implicit operator Image(BitmapUtils image)
        {
            if (image == null)
            {
                return null;
            }
            return image.Pic;
        }
        public static implicit operator ImageSource(BitmapUtils image)
        {
            if (image == null) return null;
            var bitmap = image.Pic;
            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            try
            {
                var size = rect.Width * rect.Height * 4;
                return BitmapSource.Create(
                    bitmap.Width,
                    bitmap.Height,
                    bitmap.HorizontalResolution,
                    bitmap.VerticalResolution,
                    PixelFormats.Bgra32,
                    null,
                    bitmapData.Scan0,
                    size,
                    bitmapData.Stride);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }
        public static implicit operator ImageBrush(BitmapUtils image)
        {
            if (image == null)
            {
                return null;
            }
            return new ImageBrush(image);
        }

        #region 构造方法
        public BitmapUtils(){ }
        public BitmapUtils(Image image)
        {
            Pic = (Bitmap)image;
        }

        public BitmapUtils(Bitmap image)
        {
            Pic = image;
        }

        public BitmapUtils(ImageSource image)
        {
            using (MemoryStream Ms = new MemoryStream())
            {
                BmpBitmapEncoder Encoder = new BmpBitmapEncoder();
                Encoder.Frames.Add(BitmapFrame.Create((BitmapSource)image));
                Encoder.Save(Ms);
                Pic = new Bitmap(Ms);
            }
        }

        public BitmapUtils(ImageBrush image)
        {
            using (MemoryStream Ms = new MemoryStream())
            {
                BmpBitmapEncoder Encoder = new BmpBitmapEncoder();
                Encoder.Frames.Add(BitmapFrame.Create((BitmapSource)image.ImageSource));
                Encoder.Save(Ms);
                Pic = new Bitmap(Ms);
            }
        }

        public BitmapUtils(string FilePathOrResourceName)
        {
            try
            {
                FilePathOrResourceName.Replace("pack://application:,,,/images/", ModBase.PathImage);
                if (FilePathOrResourceName.StartsWith(ModBase.PathImage))
                {
                    // 使用缓存
                    if (BitmapCache.ContainsKey(FilePathOrResourceName))
                        Pic = BitmapCache[FilePathOrResourceName].Pic;
                    else
                    {
                        var imageSource = (ImageSource)new ImageSourceConverter().ConvertFromString(FilePathOrResourceName);
                        Pic = new BitmapUtils(imageSource).Pic;
                        BitmapCache.TryAdd(FilePathOrResourceName, Pic);
                    }
                }
                else
                {
                    // 使用这种自己接管 FileStream 的方法加载才能解除文件占用
                    using (FileStream InputStream = new FileStream(FilePathOrResourceName, FileMode.Open))
                    {
                        // 判断是否为WebP文件头
                        byte[] header = new byte[2];
                        InputStream.Read(header, 0, 2);
                        InputStream.Seek(0, SeekOrigin.Begin);
                        if (header[0] == 82 && header[1] == 73)
                        {
                            // 读取 WebP
                            var FileBytes = new byte[InputStream.Length - 1];
                            InputStream.Read(FileBytes, 0, FileBytes.Length);
                            // 将代码隔离在另外一个类中，这样只要不走进这个分支就不会加载 Imazen.WebP.dll
                            Pic = WebPDecoder.DecodeFromBytes(FileBytes);
                        }
                        else
                        {
                            Pic = new Bitmap(InputStream);
                        }
                    }
                }
            }
            catch (Exception ex) 
            {
                Pic = Application.Current.TryFindResource(FilePathOrResourceName) as Bitmap;
                if(Pic == null)
                {
                    Pic = new Bitmap(1, 1);
                    throw new Exception($"加载 MyBitmap 失败（{FilePathOrResourceName}）", ex);
                }
                else
                {
                    // Log(ex, $"指定类型有误的 MyBitmap 加载（{FilePathOrResourceName}）", LogLevel.Developer)
                }
            }
        }
        #endregion

        /// <summary>
        /// 获取裁切的图片，这个方法不会导致原对象改变且会返回一个新的对象。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <returns></returns>
        public BitmapUtils Clip(int x,int y,int Width,int Height)
        {
            var bmp = new Bitmap(Width, Height,Pic.PixelFormat);
            bmp.SetResolution(Pic.HorizontalResolution, Pic.VerticalResolution);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.TranslateTransform(-x, -y);
                g.DrawImage(Pic, new Rectangle(0, 0, Pic.Width, Pic.Height));
            }
            return bmp;
        }

        /// <summary>
        /// 获取旋转或翻转后的图片，这个方法不会导致原对象改变且会返回一个新的对象。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public BitmapUtils RotateFlip(RotateFlipType type)
        {
            var bmp = new Bitmap(Pic);
            bmp.SetResolution(Pic.HorizontalResolution, Pic.VerticalResolution);
            bmp.RotateFlip(type);
            return bmp;
        }

        /// <summary>
        /// 将图像保存到文件。
        /// </summary>
        /// <param name="FilePath"></param>
        public void Save(string FilePath)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)this));
            using (FileStream fileStream = new FileStream(FilePath, FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }
    }
}
