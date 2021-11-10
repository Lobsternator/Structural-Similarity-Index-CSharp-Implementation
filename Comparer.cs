using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace StructuralSimilarityIndex
{
    class Comparer
    {
        Bitmap img1;
        Bitmap img2;

        int rescaleSize;
        int windowSize;
        int windowsPerLine;
        int pixelsPerWindow;
        int numWindows;

        PixelFormat rescaleFormat;

        double c1;
        double c2;

        public Comparer(Bitmap img1, Bitmap img2, int rescaleSize, PixelFormat rescaleFormat, int windowSize, double k1 = 0.01, double k2 = 0.03)
        {
            this.img1          = img1;
            this.img2          = img2;
            this.rescaleSize   = rescaleSize;
            this.rescaleFormat = rescaleFormat;
            this.windowSize    = windowSize;

            windowsPerLine  = rescaleSize / windowSize;
            pixelsPerWindow = windowSize * windowSize;
            numWindows      = windowsPerLine * windowsPerLine;

            int bytesPerPixel   = Image.GetPixelFormatSize(this.rescaleFormat) / 8;
            double dynamicRange = Math.Pow(2f, bytesPerPixel) - 1;
            c1                  = Math.Pow(k1 * dynamicRange, 2);
            c2                  = Math.Pow(k2 * dynamicRange, 2);
        }

        Bitmap RescaleBitmap(Bitmap bmp)
        {
            var destRect = new Rectangle(0, 0, rescaleSize, rescaleSize);
            var destBmp  = new Bitmap(rescaleSize, rescaleSize, rescaleFormat);

            destBmp.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);

            using (var graphics = Graphics.FromImage(destBmp))
            {
                graphics.CompositingMode    = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.GammaCorrected;
                graphics.InterpolationMode  = InterpolationMode.Bicubic;
                graphics.SmoothingMode      = SmoothingMode.AntiAlias;
                graphics.PixelOffsetMode    = PixelOffsetMode.Half;

                using (ImageAttributes attributes = new ImageAttributes())
                {
                    attributes.SetWrapMode(WrapMode.TileFlipXY);

                    graphics.DrawImage(bmp, destRect, 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, attributes);
                }
            }

            return destBmp;
        }

        public double CalculateSimilarity()
        {
            img1 = RescaleBitmap(img1);
            img2 = RescaleBitmap(img2);

            double MSSIM = 0;

            for (int wy = 0; wy < windowsPerLine; wy++)
            {
                for (int wx = 0; wx < windowsPerLine; wx++)
                {
                    double LAverage1   = 0;
                    double LAverage2   = 0;
                    double LVariance1  = 0;
                    double LVariance2  = 0;
                    double LCovariance = 0;

                    for (int x = 0; x < windowSize; x++)
                    {
                        for (int y = 0; y < windowSize; y++)
                        {
                            Color color1 = img1.GetPixel(wx * windowSize + x, wy * windowSize + y);
                            Color color2 = img2.GetPixel(wx * windowSize + x, wy * windowSize + y);

                            // ITU-R-recommendation BT.601 for luma values.
                            double L1 = 0.299 * color1.R + 0.587 * color1.G + 0.114 * color1.B;
                            double L2 = 0.299 * color2.R + 0.587 * color2.G + 0.114 * color2.B;
                            L1 /= 255; L2 /= 255;

                            LAverage1 += L1;
                            LAverage2 += L2;
                        }
                    }

                    LAverage1 /= pixelsPerWindow;
                    LAverage2 /= pixelsPerWindow;

                    for (int y = 0; y < windowSize; y++)
                    {
                        for (int x = 0; x < windowSize; x++)
                        {
                            Color color1 = img1.GetPixel(wx * windowSize + x, wy * windowSize + y);
                            Color color2 = img2.GetPixel(wx * windowSize + x, wy * windowSize + y);

                            // ITU-R-rekommendation BT.601 for luma värden.
                            double L1 = 0.299 * color1.R + 0.587 * color1.G + 0.114 * color1.B;
                            double L2 = 0.299 * color2.R + 0.587 * color2.G + 0.114 * color2.B;
                            L1 /= 255; L2 /= 255;

                            LVariance1  += (L1 - LAverage1) * (L1 - LAverage1);
                            LVariance2  += (L2 - LAverage2) * (L2 - LAverage2);
                            LCovariance += (L1 - LAverage1) * (L2 - LAverage2);
                        }
                    }

                    LVariance1  /= pixelsPerWindow;
                    LVariance2  /= pixelsPerWindow;
                    LCovariance /= pixelsPerWindow;

                    double numer =                 (2 * LAverage1 * LAverage2 + c1) * (2 * LCovariance + c2);
                    double denom = (LAverage1 * LAverage1 + LAverage2 * LAverage2 + c1) * (LVariance1 + LVariance2 + c2);

                    MSSIM += numer / denom;
                }
            }

            MSSIM /= numWindows;

            return MSSIM;
        }
    }
}
