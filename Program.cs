using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace StructuralSimilarityIndex
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
                return;

            Bitmap   img1       = new Bitmap(args[0]);
            Bitmap   img2       = new Bitmap(args[1]);
            Comparer comparer   = new Comparer(img1, img2, 64, PixelFormat.Format24bppRgb, 8);
            double   similarity = comparer.CalculateSimilarity();

            Console.WriteLine($"Image A and image B have a {similarity * 100}% similarity.");
            Console.Read();
        }
    }
}
