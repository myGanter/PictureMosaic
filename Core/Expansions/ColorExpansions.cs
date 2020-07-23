using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Core.Models;

namespace Core.Expansions
{
    public static class ColorExpansions
    {
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
