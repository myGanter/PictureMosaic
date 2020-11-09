using Core.Expansions;
using Emgu;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using Emgu.CV.Util;
using System.Linq;
using Emgu.CV.Aruco;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TestApp
{
    class Program
    {
        const string DefPic = "Images\\def.png";

        const string DefClearPic = "Images\\def_clear.png";

        const string CatPic = "Images\\cat.jpg";

        static void Main(string[] args)
        {           
            //TestCutBmp();

            using Image<Bgr, byte> inputImg = new Image<Bgr, byte>(DefPic);
            using Image<Gray, byte> grayImg = inputImg.Convert<Gray, byte>().ThresholdBinary(new Gray(150), new Gray(255));
            grayImg.Save("gray.jpg");
            var conturs = new VectorOfVectorOfPoint();
            var hi = new Mat();

            CvInvoke.FindContours(grayImg, conturs, hi, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

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

            var hh = new Point[4] { new Point(0, 0), new Point(grayImg.Width, 0), new Point(0, grayImg.Height), new Point(grayImg.Width, grayImg.Height) };

            var filteredConturs = conturs.ToArrayOfArray().Select(x => x.Where(y => !hh.Contains(y)).ToArray()).Where(x => x.Length > 1).ToList();
            var vecss = new List<Dictionary<Point, Point>>();

            foreach (var c in filteredConturs)
            {
                var first = c[0];
                var second = c[1];
                var vec = new Point(second.X - first.X, second.Y - first.Y);
                var secondVecLen = (float)Math.Sqrt(vec.X * vec.X + vec.Y * vec.Y);
                var normVec = new PointF(vec.X / secondVecLen, vec.Y / secondVecLen);
                vecss.Add(new Dictionary<Point, Point>());
                var vecs = vecss.Last();
                vecs.Add(first, vec);

                for (var i = 2; i < c.Length; ++i)
                {
                    second = c[i];
                    var nextVec = new Point(second.X - first.X, second.Y - first.Y);
                    var nextVecLen = (float)Math.Sqrt(nextVec.X * nextVec.X + nextVec.Y * nextVec.Y);
                    var normNextVec = new PointF(nextVec.X / nextVecLen, nextVec.Y / nextVecLen);

                    var vecsLen = Math.Abs(Math.Sqrt(Math.Pow(normVec.X - normNextVec.X, 2) + Math.Pow(normVec.Y - normNextVec.Y, 2)));

                    if (vecsLen > 0.4)
                    {
                        first = c[i - 1];
                        second = c[i];
                        vec = new Point(second.X - first.X, second.Y - first.Y);
                        secondVecLen = (float)Math.Sqrt(vec.X * vec.X + vec.Y * vec.Y);
                        normVec = new PointF(vec.X / secondVecLen, vec.Y / secondVecLen);

                        if (vecs.ContainsKey(first))
                            vecs[first] = vec;
                        else
                            vecs.Add(first, vec);
                    }
                    else
                    {
                        vecs[first] = nextVec;
                    }
                }

                //DrawLines(c, "orig.bmp", grayImg.Width, grayImg.Height);
                //DrawLines(vecs.Aggregate(new List<Point>(), (a, v) => { a.Add(v.Key); a.Add(new Point(v.Key.X + v.Value.X, v.Key.Y + v.Value.Y)); return a; }).ToArray(), grayImg.Width, grayImg.Height);
                //DrawLines(vecs.Aggregate(new List<Point>(), (a, v) => { a.Add(v.Key); a.Add(new Point(v.Key.X + v.Value.X, v.Key.Y + v.Value.Y)); return a; }).ToArray(), "neorig.bmp", grayImg.Width, grayImg.Height);
            }

            var selM = vecss.SelectMany(x => x, (x, v) => v);
            var avval = selM.Select(x => Math.Sqrt(x.Value.X * x.Value.X + x.Value.Y * x.Value.Y)).Average();
            var resVv = selM.Select(x => new { Value = x, Len = Math.Sqrt(x.Value.X * x.Value.X + x.Value.Y * x.Value.Y) }).Where(x => x.Len > (avval / 100 * 550)).Select(x => x.Value).ToList();

            var listt = resVv;//vecss.Select(x => x.Select(y => y)).Aggregate(new List<KeyValuePair<Point, Point>>(), (a, v) => { a.AddRange(v); return a; }); 

            var ct = typeof(Color);
            var props = ct.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .Where(x => x.Name != "Empty" && x.PropertyType == ct)
                .ToList();

            var rnd = new Random();
            using var resBmp3 = new Bitmap(grayImg.Width, grayImg.Height);
            using var gr3 = Graphics.FromImage(resBmp3);
            gr3.Clear(Color.Black);
            foreach (var i in listt)
            {
                var rndCol = (Color)props[rnd.Next(props.Count)].GetValue(null);

                gr3.DrawLine(new Pen(new SolidBrush(rndCol), 2), i.Key, new Point(i.Key.X + i.Value.X, i.Key.Y + i.Value.Y));
            }

            resBmp3.Save("resBmp3.bmp");
        }

        static Bitmap resBmp4;
        static Graphics gr4;
        private static void DrawLines(Point[] Points, int W, int H)
        {
            if (resBmp4 == null)
            {
                resBmp4 = new Bitmap(W, H);
                gr4 = Graphics.FromImage(resBmp4);
                gr4.Clear(Color.Black);
            }

            var ct = typeof(Color);
            var props = ct.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .Where(x => x.Name != "Empty" && x.PropertyType == ct)
                .ToList();
            var rnd = new Random();
            for (var i = 0; i < Points.Length - 1; ++i)
            {
                var rndCol = (Color)props[rnd.Next(props.Count)].GetValue(null);

                gr4.DrawLine(new Pen(new SolidBrush(rndCol), 2), Points[i], Points[i + 1]);
            }

            
        }

        private static void DrawLines(Point[] Points, string FName, int W, int H)
        {
            using var resBmp3 = new Bitmap(W, H);
            using var gr3 = Graphics.FromImage(resBmp3);
            gr3.Clear(Color.Black);

            var ct = typeof(Color);
            var props = ct.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .Where(x => x.Name != "Empty" && x.PropertyType == ct)
                .ToList();
            var rnd = new Random();
            for (var i = 0; i < Points.Length - 1; ++i)
            {
                var rndCol = (Color)props[rnd.Next(props.Count)].GetValue(null);

                gr3.DrawLine(new Pen(new SolidBrush(rndCol), 2), Points[i], Points[i + 1]);
            }

            resBmp3.Save(FName);
        }

        private static void TestCutBmp() 
        {
            TestCutBmp(new Bitmap(CatPic), 1000, 100, "Images\\catS1.png");
            TestCutBmp(new Bitmap(DefPic), 300, 600, "Images\\defS.png");
            TestCutBmp(new Bitmap(DefClearPic), 300, 600, "Images\\def_clearS.png");
        }

        private static void TestCutBmp(Bitmap Bmp, int NewW, int NewH, string SaveFileName)
        {
            using (Bmp)
            {
                using (var newBmp = Bmp.ScaleCutBmpCenter(NewW, NewH))
                {
                    newBmp.Save(SaveFileName);
                }
            }
        }
    }
}
