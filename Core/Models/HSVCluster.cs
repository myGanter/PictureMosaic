using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using Core.Expansions;

namespace Core.Models
{
    public class HSVCluster : Cluster, IEquatable<HSVCluster>
    {
        public ColorHSV[] Colors { get; }

        public HSVCluster(ColorHSV[] Colors) 
        {
            if (Colors == null || Colors.Length == 0)
                throw new Exception("Colors empty");

            this.Colors = Colors;
        }
        public override Color GetAvColor()
        {
            int r = 0, g = 0, b = 0;

            foreach (var i in Colors.Select(x => x.ToARGBColor()))
            {
                r += i.R;
                g += i.G;
                b += i.B;
            }

            var colLen = Colors.Length;

            return Color.FromArgb(r / colLen, g / colLen, b / colLen);
        }

        public bool Equals([AllowNull] HSVCluster Obj)
        {
            if (Obj == null || Obj.Colors.Length != Colors.Length)
                return false;

            var objCols = Obj.Colors;
            for (var i = 0; i < Colors.Length; ++i) 
            {
                if (!Colors[i].Equals(objCols[i]))
                    return false;
            }

            return true;
        }

        public override bool Equals([AllowNull] Cluster Obj)
        {
            return Obj is HSVCluster hsvO && Equals(hsvO);
        }

        public override int GetHashCode()
        {
            long sum = 0;
            for (var i = 0; i < Colors.Length; ++i) 
            {
                sum += Colors[i].GetHashCode();
            }

            return (int)(sum / Colors.Length);
        }
    }
}
