using Core.Expansions;
using Emgu;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using Emgu.CV.Util;

namespace TestApp
{
    class Program
    {
        const string DefPic = "Images\\def.png";

        const string DefClearPic = "Images\\def_clear.png";

        static void Main(string[] args)
        {           
            //TestCutBmp();

            using Image<Bgr, byte> inputImg = new Image<Bgr, byte>(DefPic);
            using Image<Gray, byte> grayImg = inputImg.Convert<Gray, byte>().ThresholdBinary(new Gray(150), new Gray(255));
            grayImg.Save("gray.jpg");
            var conturs = new VectorOfVectorOfPoint();
            var hi = new Mat();

            CvInvoke.FindContours(grayImg, conturs, hi, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

            var res = new Image<Gray, byte>(grayImg.Width, grayImg.Height, new Gray(0));

            CvInvoke.DrawContours(res, conturs, -1, new MCvScalar(255, 0, 0));

            res.Save("conturs.jpg");

            using var bmp = new Bitmap(grayImg.Width, grayImg.Height);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.Black);

            for (var i = 0; i < conturs.Size; ++i) 
            {
                var contur = conturs[i];

                if (contur.Size > 1)
                {
                    for (var j = 0; j < contur.Size - 1; ++j)
                    {
                        g.DrawLine(Pens.Red, contur[j], contur[j + 1]);
                    }
                }
                else
                {
                    var p = contur[0];
                    g.DrawEllipse(Pens.Beige, p.X - 5, p.Y - 5, 10, 10);
                }
            }

            bmp.Save("conturstest.jpg");
        }

        private static void TestCutBmp() 
        {
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
