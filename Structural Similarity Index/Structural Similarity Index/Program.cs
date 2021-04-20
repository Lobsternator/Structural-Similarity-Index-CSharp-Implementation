using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StructuralSimilarityIndex
{
    class Program
    {
        static void Main(string[] args)
        {
            var img1 = new Bitmap("Images/1.png");
            var img2 = new Bitmap("Images/2.png");
            var comparer = new Comparer(img1, img2, 64, 8);
            var similarity = comparer.CalculateSimilarity();

            Console.WriteLine($"Image 1 and image 2 have a {similarity * 100}% similarity.");
            Console.Read();
        }
    }
}
