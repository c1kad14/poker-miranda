using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using miranda.ui;

namespace TessTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var man = new CardRecognizer();

            var image = Bitmap.FromFile("./1.bmp") as Bitmap;
            var txt = man.RecognizeText(image);
            //man.RecognizePlayer(image, "", 0);
            Console.ReadLine();
        }
    }
}
