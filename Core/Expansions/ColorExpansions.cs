using System;
using System.Drawing;
using Core.Models;

namespace Core.Expansions
{
    public static class ColorExpansions
    {
        public static double CaclDistance(this Cluster Cl1, Cluster Cl) 
        {
            var p1 = Cl1.GetAvColor();
            var p2 = Cl.GetAvColor();

            return Math.Sqrt(Math.Pow(p1.R - p2.R, 2) + Math.Pow(p1.G - p2.G, 2) + Math.Pow(p1.B - p2.B, 2));
        }

        public static ColorHSV FindCluster(this ColorHSV Hsv, short H, short S, short V)
        {
            if (Hsv.V <= 10)
                return new ColorHSV(0, 0, 0);

            var h = Hsv.H / H * H;
            var s = Hsv.S / S * S;
            var v = Hsv.V / V * V;

            return new ColorHSV((short)h, (short)s, (short)v);
        }

        public static Color ToARGBColor(this ColorHSV Hsv)
        {
            int hi = Convert.ToInt32(Math.Floor(Hsv.H / 60d)) % 6;
            double f = Hsv.H / 60d - Math.Floor(Hsv.H / 60d);

            var value = Hsv.V / 100d * 255d;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - Hsv.S / 100d));
            int q = Convert.ToInt32(value * (1 - f * Hsv.S / 100d));
            int t = Convert.ToInt32(value * (1 - (1 - f) * Hsv.S / 100d));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }

        public static ColorHSV ToHSVColor(this Color Col) 
        {
            int max = Math.Max(Col.R, Math.Max(Col.G, Col.B));
            int min = Math.Min(Col.R, Math.Min(Col.G, Col.B));

            var hue = Col.GetHue();
            var saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            var value = max / 255d;

            var h = (short)Math.Round(hue);
            var s = (short)Math.Round(saturation * 100);
            var v = (short)Math.Round(value * 100);

            return new ColorHSV(h, s, v);
        }
    }
}
