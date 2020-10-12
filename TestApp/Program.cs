using Core.Expansions;
using System;
using System.Drawing;

namespace TestApp
{
    class Program
    {
        const string DefPic = "Images\\def.png";

        const string DefClearPic = "Images\\def_clear.png";

        static void Main(string[] args)
        {
            using (var b = new Bitmap(DefPic))
            {
                using var nbmp = new Bitmap(b, b.Width * 5, b.Height * 5);
                nbmp.Save("Images\\defNewSize.png");
            }

            TestCutBmp(new Bitmap(DefPic), 300, 600, "Images\\defS.png");
            TestCutBmp(new Bitmap(DefClearPic), 300, 600, "Images\\def_clearS.png");
        }

        private static void TestCutBmp(Bitmap Bmp, int NewW, int NewH, string SaveFileName)
        {
            using (Bmp)
            {
                using (var newBmp = Bmp.CutBmpToCenter(NewW, NewH))
                {
                    newBmp.Save(SaveFileName);
                }
            }
        }
    }
}
