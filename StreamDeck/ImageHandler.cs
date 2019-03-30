using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using ImageMagick;

namespace StreamDeck
{
    public class ImageHandler
    {
        public static Bitmap Base64ToImage(string base64)
        {
            byte[] imageBytes = Convert.FromBase64String(base64);

            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                Bitmap image = new Bitmap(Image.FromStream(ms, true));
                return image;
            }
        }

        public static string BitmapToBase64(Bitmap bitmap)
        {
            using (var ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png);
                byte[] imageBytes = ms.ToArray();
                
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public static Bitmap CropImage(Image image, int cropX, int cropY, int cropWidth, int cropHeight)
        {
            Rectangle rect = new Rectangle(cropX, cropY, cropWidth, cropHeight);
            Bitmap cropped = null;
            using (Bitmap bm = new Bitmap(image))
            {
                Bitmap tmp = bm;
                cropped = tmp.Clone(rect, bm.PixelFormat);
                tmp.Dispose();
            }
            return cropped;
        }

        public static void Scale(ref Bitmap bm, int width, int height)
        {
            if (bm.Width < width && bm.Height < height)
            {
                if (width - bm.Width >= height - bm.Height)
                {
                    bm = ResizeImage(bm, width, (int)Math.Round((double)bm.Height * width / bm.Width));
                }
                else
                {
                    bm = ResizeImage(bm, (int)Math.Round((double)bm.Width * height / bm.Height), height);
                }
            }
            else if (bm.Width < width)
            {
                bm = ResizeImage(bm, width, (int)Math.Round((double)bm.Height * width / bm.Width));
            }
            else if (bm.Height < height)
            {
                bm = ResizeImage(bm, (int)Math.Round((double)bm.Width * height / bm.Height), height);
            }
            else if (bm.Width >= width && bm.Height >= height)
            {
                if (bm.Width - width >= bm.Height - height)
                {
                    bm = ResizeImage(bm, (int)Math.Round((double)bm.Width * height / bm.Height), height);
                }
                else
                {
                    bm = ResizeImage(bm, width, (int)Math.Round((double)bm.Height * width / bm.Width));
                }
            }
            else if (bm.Width >= width)
            {
                bm = ResizeImage(bm, (int)Math.Round((double)bm.Width * height / bm.Height), height);
            }
            else if (bm.Height >= height)
            {
                bm = ResizeImage(bm, width, (int)Math.Round((double)bm.Height * width / bm.Width));
            }
        }

        public static Bitmap ImageSuperHandle(string path, int buttonWidth, int buttonHeight)
        {
            using (Image bg = Image.FromFile(path))
            {
                Bitmap bm = null;
                Bitmap upscaled = new Bitmap(bg);

                double buttonRatio = Math.Round((double)buttonWidth / buttonHeight);
                double imageRatio = Math.Round((double)bg.Width / bg.Height);
                
                Scale(ref upscaled, buttonWidth, buttonHeight);

                int padW = (int)Math.Round((upscaled.Width - buttonWidth) / 2.0);
                int padH = (int)Math.Round((upscaled.Height - buttonHeight) / 2.0);
                bm = CropImage(upscaled, padW, padH, buttonWidth, buttonHeight);
                
                upscaled.Dispose();

                return bm;
            }
        }

        public static string GifToBase64(string path, int buttonWidth, int buttonHeight)
        {
            using (var collection = new MagickImageCollection(new FileInfo(path)))
            {
                collection.Coalesce();
                foreach (var image in collection)
                {
                    image.Resize(buttonWidth, buttonHeight);
                }
                return collection.ToBase64();
            }
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}
